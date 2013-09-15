namespace Soundfingerprinting
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Soundfingerprinting.Audio.Strides;
	using Soundfingerprinting.Fingerprinting;
	using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;
	using Soundfingerprinting.Hashing;
	
	using Soundfingerprinting.Dao;
	using Soundfingerprinting.Dao.Entities;
	using Soundfingerprinting.DbStorage;
	using Soundfingerprinting.DbStorage.Entities;
	
	using Soundfingerprinting.SoundTools;

	/// <summary>
	/// Singleton class for repository container
	/// </summary>
	public class Repository
	{
		/// <summary>
		///   Min hasher
		/// </summary>
		private readonly MinHash minHash;

		/// <summary>
		///   Storage for min-hash permutations
		/// </summary>
		private readonly IPermutations permutations;
		
		private DatabaseService dbService;
		private FingerprintService fingerprintService;
		
		public Repository(IPermutations permutations, DatabaseService dbService, FingerprintService fingerprintService)
		{
			this.permutations = permutations;
			this.minHash = new MinHash(this.permutations);
			this.dbService = dbService;
			this.fingerprintService = fingerprintService;
		}

		/*
		public List<HashSignature> GetSignatures(IEnumerable<bool[]> fingerprints, Track track, int hashTables, int hashKeys)
		{
			List<HashSignature> signatures = new List<HashSignature>();
			foreach (bool[] fingerprint in fingerprints)
			{
				int[] signature = hasher.ComputeMinHashSignature(fingerprint); // Compute min-hash signature out of signature
				Dictionary<int, long> buckets = hasher.GroupMinHashToLSHBuckets(signature, hashTables, hashKeys); // Group Min-Hash signature into LSH buckets
				
				//long[] hashbuckets = buckets.Values.ToArray();
				
				int[] hashSignature = new int[buckets.Count];
				foreach (KeyValuePair<int, long> bucket in buckets)
				{
					hashSignature[bucket.Key] = (int)bucket.Value;
				}

				HashSignature hash = new HashSignature(track, hashSignature); // associate track to hash-signature
				signatures.Add(hash);
			}
			return signatures; // Return the signatures
		}
		 */

		public void Query(int trackid,
		                  int lshHashTables,
		                  int lshGroupsPerKey,
		                  int thresholdTables,
		                  WorkUnitParameterObject param) {
			
			int recognized = 0, verified = 0;

			// Get fingerprints
			List<bool[]> signatures = fingerprintService.CreateFingerprintsFromAudioSamples(param.AudioSamples, param);

			long elapsedMiliseconds = 0;
			Track actualTrack = dbService.ReadTrackById(trackid);
			
			Dictionary<int, QueryStats> allCandidates = QueryFingerprintManager.QueryOneSongMinHash(
				signatures,
				dbService,
				minHash,
				param.MillisecondsToProcess*1000,
				lshHashTables,
				lshGroupsPerKey,
				thresholdTables,
				ref elapsedMiliseconds); /*Query the database using Min Hash*/

			// Order by Hamming Similarity
			OrderedParallelQuery<KeyValuePair<int, QueryStats>> order = allCandidates.AsParallel() /*Using PLINQ*/
				.OrderBy((pair) => pair.Value.OrderingValue =
				         pair.Value.HammingDistance / pair.Value.NumberOfTotalTableVotes
				         + 0.4 * pair.Value.MinHammingDistance);

			Track recognizedTrack = null;
			bool found = false;

			if (order.Any())
			{
				KeyValuePair<int, QueryStats> item = order.ElementAt(0);
				recognizedTrack = dbService.ReadTrackById(item.Key);
				if (actualTrack.Id == recognizedTrack.Id)
				{
					recognized++;
					found = true;
				}

				verified++;
				if (!found)
				{
					// If the element is not found, search it in all candidates
					var query = order.Select((pair, indexAt) => new {Pair = pair, IndexAt = indexAt}).Where((a) => a.Pair.Key == actualTrack.Id);
					if (query.Any())
					{
						var anonymType = query.ElementAt(0);
						recognizedTrack = dbService.ReadTrackById(anonymType.Pair.Key);
					}
					else
					{
					}
				}
			}
		}
		
		/// <summary>
		/// Insert track into database
		/// </summary>
		/// <param name="track">Track</param>
		/// <param name="hashTables">Number of hash tables (e.g. 25)</param>
		/// <param name="hashKeys">Number of hash keys (e.g. 4)</param>
		/// <param name="param">WorkUnitParameterObject parameters</param>
		public void InsertTrackInDatabaseUsingSamples(Track track, int hashTables, int hashKeys, WorkUnitParameterObject param)
		{
			int count = 0;
			
			List<bool[]> images = fingerprintService.CreateFingerprintsFromAudioSamples(param.AudioSamples, param);
			List<Fingerprint> inserted = AssociateFingerprintsToTrack(images, track.Id);
			dbService.InsertFingerprint(inserted);
			count = inserted.Count;

			HashFingerprintsUsingMinHash(inserted, track, hashTables, hashKeys);
		}
		
		/// <summary>
		/// Associate fingerprint signatures with a specific track
		/// </summary>
		/// <param name = "fingerprintSignatures">Signatures built from one track</param>
		/// <param name = "trackId">Track id, which is the parent for this fingerprints</param>
		/// <returns>List of fingerprint entity objects</returns>
		private List<Fingerprint> AssociateFingerprintsToTrack(IEnumerable<bool[]> fingerprintSignatures, int trackId)
		{
			const int FakeId = -1;
			List<Fingerprint> fingers = new List<Fingerprint>();
			int c = 0;
			foreach (bool[] signature in fingerprintSignatures)
			{
				fingers.Add(new Fingerprint(FakeId, signature, trackId, c));
				c++;
			}

			return fingers;
		}
		
		/// <summary>
		/// Hash Fingerprints using Min-Hash algorithm
		/// </summary>
		/// <param name = "listOfFingerprintsToHash">List of fingerprints already inserted in the database</param>
		/// <param name = "track">Track of the corresponding fingerprints</param>
		/// <param name = "hashTables">Number of hash tables (e.g. 25)</param>
		/// <param name = "hashKeys">Number of hash keys (e.g. 4)</param>
		private void HashFingerprintsUsingMinHash(IEnumerable<Fingerprint> listOfFingerprintsToHash, Track track, int hashTables, int hashKeys)
		{
			List<HashBinMinHash> listToInsert = new List<HashBinMinHash>();
			foreach (Fingerprint fingerprint in listOfFingerprintsToHash)
			{
				int[] hashBins = minHash.ComputeMinHashSignature(fingerprint.Signature); //Compute Min Hashes
				Dictionary<int, long> hashTable = minHash.GroupMinHashToLSHBuckets(hashBins, hashTables, hashKeys);
				foreach (KeyValuePair<int, long> item in hashTable)
				{
					HashBinMinHash hash = new HashBinMinHash(-1, item.Value, item.Key, track.Id, fingerprint.Id);
					listToInsert.Add(hash);
				}
			}
			dbService.InsertHashBin(listToInsert); //Insert
		}
		
	}
}