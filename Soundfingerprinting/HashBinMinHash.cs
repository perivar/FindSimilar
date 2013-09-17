namespace Soundfingerprinting.Dao.Entities
{
	using System;

	using Soundfingerprinting.DbStorage.Entities;

	/// <summary>
	///   Bin for Min-Hash + LSH schema
	/// </summary>
	[Serializable]
	public class HashBinMinHash : HashBin
	{
		public HashBinMinHash()
		{
		}

		public HashBinMinHash(int id, long hashBin, int hashTable, int trackId, int hashedFingerprint)
			: base(id, hashBin, hashTable, trackId)
		{
			FingerprintId = hashedFingerprint;
		}

		public int FingerprintId { get; set; }
		
		public override string ToString() {
			return String.Format("id: {0}, hashBin: {1}, hashTable: {2}, trackId: {3}, fingerprintId: {4}", Id, Bin, HashTable, TrackId, FingerprintId);
		}
		
	}
}