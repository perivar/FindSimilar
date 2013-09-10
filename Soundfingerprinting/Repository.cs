namespace Soundfingerprinting.DuplicatesDetector.DataAccess
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Soundfingerprinting.Audio.Strides;
	using Soundfingerprinting.DuplicatesDetector.Model;
	using Soundfingerprinting.Fingerprinting;
	using Soundfingerprinting.Fingerprinting.WorkUnitBuilder;
	using Soundfingerprinting.Hashing;

	/// <summary>
	///   Singleton class for repository container
	/// </summary>
	public class Repository
	{
		/// <summary>
		///   Min hasher
		/// </summary>
		private readonly MinHash hasher;

		/// <summary>
		///   Storage for min-hash permutations
		/// </summary>
		private readonly IPermutations permutations;

		public Repository(IPermutations permutations)
		{
			this.permutations = permutations;
			this.hasher = new MinHash(this.permutations);
		}

		public List<HashSignature> GetSignatures(IEnumerable<bool[]> fingerprints, Track track, int hashTables, int hashKeys)
		{
			List<HashSignature> signatures = new List<HashSignature>();
			foreach (bool[] fingerprint in fingerprints)
			{
				int[] signature = hasher.ComputeMinHashSignature(fingerprint); /*Compute min-hash signature out of signature*/
				Dictionary<int, long> buckets = hasher.GroupMinHashToLSHBuckets(signature, hashTables, hashKeys); /*Group Min-Hash signature into LSH buckets*/
				
				//long[] hashbuckets = buckets.Values.ToArray();
				
				int[] hashSignature = new int[buckets.Count];
				foreach (KeyValuePair<int, long> bucket in buckets)
				{
					hashSignature[bucket.Key] = (int)bucket.Value;
				}

				HashSignature hash = new HashSignature(track, hashSignature); /*associate track to hash-signature*/
				signatures.Add(hash);
			}
			return signatures; /*Return the signatures*/
		}
	}
}