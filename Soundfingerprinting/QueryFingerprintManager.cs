namespace Soundfingerprinting.SoundTools
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using Soundfingerprinting.Audio.Services;
	using Soundfingerprinting.Audio.Strides;
	using Soundfingerprinting.Dao;
	using Soundfingerprinting.Dao.Entities;
	using Soundfingerprinting.DbStorage;
	using Soundfingerprinting.DbStorage.Entities;
	using Soundfingerprinting.Hashing;

	public static class QueryFingerprintManager
	{
		/// <summary>
		/// Query one specific song using MinHash algorithm.
		/// </summary>
		/// <param name="signatures">Signature signatures from a song</param>
		/// <param name="dbService">DatabaseService used to query the underlying database</param>
		/// <param name="lshHashTables">Number of hash tables from the database</param>
		/// <param name="lshGroupsPerKey">Number of groups per hash table</param>
		/// <param name="thresholdTables">Threshold percentage [0.07 for 20 LHash Tables, 0.17 for 25 LHashTables]</param>
		/// <param name="queryTime">Set buy the method, representing the query length</param>
		/// <returns>Dictionary with Tracks ID's and the Query Statistics</returns>
		public static Dictionary<Int32, QueryStats> QueryOneSongMinHash(
			IEnumerable<bool[]> signatures,
			DatabaseService dbService,
			MinHash minHash,
			int lshHashTables,
			int lshGroupsPerKey,
			int thresholdTables,
			ref long queryTime)
		{
			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();
			Dictionary<int, QueryStats> stats = new Dictionary<int, QueryStats>();
			foreach (bool[] signature in signatures)
			{
				if (signature == null)
				{
					continue;
				}

				// Compute Min Hash on randomly selected fingerprints
				int[] bin = minHash.ComputeMinHashSignature(signature);
				
				Dictionary<int, long> hashes = minHash.GroupMinHashToLSHBuckets(bin, lshHashTables, lshGroupsPerKey); /*Find all candidates by querying the database*/
				long[] hashbuckets = hashes.Values.ToArray();
				IDictionary<int, IList<HashBinMinHash>> candidates = dbService.ReadFingerprintsByHashBucketLsh(hashbuckets);
				Dictionary<int, IList<HashBinMinHash>> potentialCandidates = SelectPotentialMatchesOutOfEntireDataset(candidates, thresholdTables);
				if (potentialCandidates.Count > 0)
				{
					IList<Fingerprint> fingerprints = dbService.ReadFingerprintById(potentialCandidates.Keys);
					Dictionary<Fingerprint, int> finalCandidates = fingerprints.ToDictionary(finger => finger, finger => potentialCandidates[finger.Id].Count);
					ArrangeCandidatesAccordingToFingerprints(
						signature, finalCandidates, lshHashTables, lshGroupsPerKey, stats);
				}
			}

			stopWatch.Stop();
			queryTime = stopWatch.ElapsedMilliseconds; /*Set the query Time parameter*/
			return stats;
		}

		/// <summary>
		/// Arrange candidates according to the corresponding calculation between initial signature and actual signature
		/// </summary>
		/// <param name="f">Actual signature gathered from the song</param>
		/// <param name="potentialCandidates">Potential fingerprints returned from the database</param>
		/// <param name="lHashTables">Number of L Hash tables</param>
		/// <param name="kKeys">Number of keys per table</param>
		/// <param name="trackIdQueryStats">Result set</param>
		/// <returns>Result set</returns>
		private static Dictionary<Int32, QueryStats> ArrangeCandidatesAccordingToFingerprints(bool[] f, Dictionary<Fingerprint, int> potentialCandidates,
		                                                                                      int lHashTables, int kKeys, Dictionary<Int32, QueryStats> trackIdQueryStats)
		{
			// Most time consuming method while performing the necessary calculation
			foreach (KeyValuePair<Fingerprint, int> pair in potentialCandidates)
			{
				Fingerprint fingerprint = pair.Key;
				int tableVotes = pair.Value;
				
				// Compute Hamming Distance of actual and read signature
				int hammingDistance = MinHash.CalculateHammingDistance(f, fingerprint.Signature)*tableVotes;
				double jaqSimilarity = MinHash.CalculateJaqSimilarity(f, fingerprint.Signature);
				
				// Add to sample set
				Int32 trackId = fingerprint.TrackId;
				if (!trackIdQueryStats.ContainsKey(trackId))
					trackIdQueryStats.Add(trackId, new QueryStats(0, 0, 0, -1, -1, 0, Int32.MinValue, 0, Int32.MaxValue, Int32.MinValue, Int32.MinValue, Double.MaxValue));
				
				QueryStats stats = trackIdQueryStats[trackId];
				stats.HammingDistance += hammingDistance; 		// Sum hamming distance of each potential candidate
				stats.NumberOfTrackIdOccurences++;  			// Increment occurrence count
				stats.NumberOfTotalTableVotes += tableVotes; 	// Find total table votes
				stats.HammingDistanceByTrack += hammingDistance/tableVotes; // Find hamming distance by track id occurrence
				if (stats.MinHammingDistance > hammingDistance/tableVotes) 	// Find minimal hamming distance over the entire set
					stats.MinHammingDistance = hammingDistance/tableVotes;
				if (stats.MaxTableVote < tableVotes) // Find maximal table vote
					stats.MaxTableVote = tableVotes;
				if (stats.Similarity > jaqSimilarity)
					stats.Similarity = jaqSimilarity;
			}
			return trackIdQueryStats;
		}

		/// <summary>
		/// Select potential matches out of the entire dataset
		/// </summary>
		/// <param name="dataset">Dataset to consider</param>
		/// <param name="thresholdTables">Threshold tables</param>
		/// <returns>Sub dictionary</returns>
		public static Dictionary<int, IList<HashBinMinHash>> SelectPotentialMatchesOutOfEntireDataset(IDictionary<int, IList<HashBinMinHash>> dataset, int thresholdTables)
		{
			Dictionary<int, IList<HashBinMinHash>> result = new Dictionary<int, IList<HashBinMinHash>>();
			if (dataset == null)
			{
				return result;
			}

			foreach (var item in dataset)
			{
				if (item.Value.Count >= thresholdTables)
				{
					List<int> tables = new List<int>();
					foreach (HashBinMinHash hashes in item.Value)
					{
						if (!tables.Contains(hashes.HashTable))
						{
							tables.Add(hashes.HashTable);
						}
					}

					if (tables.Count >= thresholdTables)
					{
						result.Add(item.Key, item.Value);
					}
				}
			}

			return result;
		}
	}
}