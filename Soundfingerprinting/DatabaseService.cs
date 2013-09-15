using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Soundfingerprinting.Dao.Entities;
using Soundfingerprinting.DbStorage.Entities;

namespace Soundfingerprinting.DbStorage
{
	// SQL Lite database class
	// perivar@nerseth.com
	public class DatabaseService
	{
		IDbConnection dbcon;

		// singleton instance
		private static DatabaseService instance;
		
		#region Singleton Patterns
		/// <summary>
		/// Return a DatabaseService Instance
		/// </summary>
		/// <returns>A DatabaseService Instance</returns>
		public static DatabaseService Instance
		{
			get {
				if (instance == null)
					instance = new DatabaseService();
				return instance;
			}
		}
		#endregion
		
		#region Constructor and Destructor
		protected DatabaseService()
		{
			string homedir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string dbdir = Path.Combine(homedir,".mirage");
			string dbfile = Path.Combine(dbdir, "soundfingerprinting.db");
			string sqlite = string.Format("Data Source={0};Version=3", dbfile);
			
			if (!Directory.Exists(dbdir)) {
				Directory.CreateDirectory(dbdir);
			}
			
			dbcon = (IDbConnection) new SQLiteConnection(sqlite);
			dbcon.Open();
		}
		
		~DatabaseService()
		{
			dbcon.Close();
		}
		#endregion
		
		#region Add and Remove the Fingerprint table
		public bool RemoveFingerprintTable()
		{
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "DROP TABLE IF EXISTS fingerprints";
			
			try {
				dbcmd.ExecuteNonQuery();
			} catch (SQLiteException){
				return false;
			}
			
			return true;
		}
		
		public bool AddFingerprintTable() {
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS fingerprints"
				+ " (id INTEGER PRIMARY KEY AUTOINCREMENT, trackid INTEGER, songorder INTEGER, signature BLOB)";
			
			try {
				dbcmd.ExecuteNonQuery();
			} catch (SQLiteException){
				return false;
			}
			
			return true;
		}
		#endregion
		
		#region Add and Remove the HashBin table
		public bool RemoveHashBinTable()
		{
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "DROP TABLE IF EXISTS hashbins";
			
			try {
				dbcmd.ExecuteNonQuery();
			} catch (SQLiteException){
				return false;
			}
			
			return true;
		}
		
		public bool AddHashBinTable() {
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS hashbins"
				+ " (id INTEGER PRIMARY KEY AUTOINCREMENT, hashbin INTEGER, hashtable INTEGER, trackid INTEGER, fingerprintid INTEGER)";

			try {
				dbcmd.ExecuteNonQuery();
			} catch (SQLiteException){
				return false;
			}
			
			return true;
		}
		#endregion
		
		#region Inserts
		public void InsertFingerprint(Fingerprint fingerprint)
		{
			IDbDataParameter dbTrackIdParam = new SQLiteParameter("@trackid", DbType.Int32);
			IDbDataParameter dbSongOrderParam = new SQLiteParameter("@songorder", DbType.Int32);
			IDbDataParameter dbSignatureParam = new SQLiteParameter("@signature", DbType.Binary);
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "INSERT INTO fingerprints (trackid, songorder, signature) " +
				"VALUES (@trackid, @songorder, @signature)";
			dbcmd.Parameters.Add(dbTrackIdParam);
			dbcmd.Parameters.Add(dbSongOrderParam);
			dbcmd.Parameters.Add(dbSignatureParam);

			dbTrackIdParam.Value = fingerprint.TrackId;
			dbSongOrderParam.Value = fingerprint.SongOrder;
			dbSignatureParam.Value = fingerprint.Signature;
			
			try {
				dbcmd.Prepare();
				dbcmd.ExecuteNonQuery();
			} catch (Exception e) {
				throw e;
			}
		}

		public void InsertFingerprint(IEnumerable<Fingerprint> collection)
		{
			IDbDataParameter dbTrackIdParam = new SQLiteParameter("@trackid", DbType.Int32);
			IDbDataParameter dbSongOrderParam = new SQLiteParameter("@songorder", DbType.Int32);
			IDbDataParameter dbSignatureParam = new SQLiteParameter("@signature", DbType.Binary);
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "INSERT INTO fingerprints (trackid, songorder, signature) " +
				"VALUES (@trackid, @songorder, @signature)";

			dbcmd.Parameters.Add(dbTrackIdParam);
			dbcmd.Parameters.Add(dbSongOrderParam);
			dbcmd.Parameters.Add(dbSignatureParam);
			dbcmd.Prepare();
			
			using (var transaction = dbcon.BeginTransaction())
			{
				try {
					foreach (var fingerprint in collection) {
						dbTrackIdParam.Value = fingerprint.TrackId;
						dbSongOrderParam.Value = fingerprint.SongOrder;
						dbSignatureParam.Value = BoolToByte(fingerprint.Signature);

						dbcmd.ExecuteNonQuery();
					}
					transaction.Commit();
					
				} catch (Exception e1) {
					// attempt to rollback the transaction
					try {
						transaction.Rollback();
					} catch (Exception e2) {
						// do nothing
					}
					throw e1;
				}
			}
		}

		public void InsertTrack(Track track)
		{
			//trackDao.Insert(track);
		}

		public void InsertTrack(IEnumerable<Track> collection)
		{
			//trackDao.Insert(collection);
		}

		public void InsertHashBin(HashBinMinHash hashBin)
		{
			IDbDataParameter dbHashBinParam = new SQLiteParameter("@hashbin", DbType.Int64);
			IDbDataParameter dbHashTableParam = new SQLiteParameter("@hashtable", DbType.Int32);
			IDbDataParameter dbTrackIdParam = new SQLiteParameter("@trackid", DbType.Int32);
			IDbDataParameter dbFingerprintIdParam = new SQLiteParameter("@fingerprintid", DbType.Int32);
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "INSERT INTO hashbins (hashbin, hashtable, trackid, fingerprintid) " +
				"VALUES (@hashbin, @hashtable, @trackid, @fingerprintid)";

			dbcmd.Parameters.Add(dbHashBinParam);
			dbcmd.Parameters.Add(dbHashTableParam);
			dbcmd.Parameters.Add(dbTrackIdParam);
			dbcmd.Parameters.Add(dbFingerprintIdParam);

			dbHashBinParam.Value = hashBin.Bin;
			dbHashTableParam.Value = hashBin.HashTable;
			dbTrackIdParam.Value = hashBin.TrackId;
			dbFingerprintIdParam.Value = hashBin.FingerprintId;
			
			try {
				dbcmd.Prepare();
				dbcmd.ExecuteNonQuery();
			} catch (Exception e) {
				throw e;
			}
		}

		public void InsertHashBin(IEnumerable<HashBinMinHash> collection)
		{
			IDbDataParameter dbHashBinParam = new SQLiteParameter("@hashbin", DbType.Int64);
			IDbDataParameter dbHashTableParam = new SQLiteParameter("@hashtable", DbType.Int32);
			IDbDataParameter dbTrackIdParam = new SQLiteParameter("@trackid", DbType.Int32);
			IDbDataParameter dbFingerprintIdParam = new SQLiteParameter("@fingerprintid", DbType.Int32);
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "INSERT INTO hashbins (hashbin, hashtable, trackid, fingerprintid) " +
				"VALUES (@hashbin, @hashtable, @trackid, @fingerprintid)";

			dbcmd.Parameters.Add(dbHashBinParam);
			dbcmd.Parameters.Add(dbHashTableParam);
			dbcmd.Parameters.Add(dbTrackIdParam);
			dbcmd.Parameters.Add(dbFingerprintIdParam);
			dbcmd.Prepare();
			
			using (var transaction = dbcon.BeginTransaction())
			{
				try {
					foreach (var hashBin in collection) {
						dbHashBinParam.Value = hashBin.Bin;
						dbHashTableParam.Value = hashBin.HashTable;
						dbTrackIdParam.Value = hashBin.TrackId;
						dbFingerprintIdParam.Value = hashBin.FingerprintId;
						
						dbcmd.ExecuteNonQuery();
					}
					transaction.Commit();
					
				} catch (Exception e1) {
					// attempt to rollback the transaction
					try {
						transaction.Rollback();
					} catch (Exception e2) {
						// do nothing
					}
					throw e1;
				}
			}
		}
		#endregion

		public IDictionary<Track, int> ReadDuplicatedTracks()
		{
			//return trackDao.ReadDuplicatedTracks();
			return null;
		}

		#region Reads
		public IList<Fingerprint> ReadFingerprints()
		{
			var fingerprints = new List<Fingerprint>();
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			
			dbcmd.CommandText = "SELECT id, trackid, songorder, signature FROM [fingerprints]";
			dbcmd.CommandType = CommandType.Text;

			IDataReader reader = dbcmd.ExecuteReader();
			while (reader.Read()) {
				Fingerprint fingerprint = new Fingerprint();
				fingerprint.Id = reader.GetInt32(0);
				fingerprint.TrackId = reader.GetInt32(1);
				fingerprint.SongOrder = reader.GetInt32(2);
				fingerprint.Signature = ByteToBool((byte[]) reader.GetValue(3));
				fingerprints.Add(fingerprint);
			}
			
			reader.Close();
			return fingerprints;
		}

		public IList<Fingerprint> ReadFingerprintsByTrackId(int trackId, int numberOfFingerprintsToRead)
		{
			var fingerprints = new List<Fingerprint>();
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			
			dbcmd.CommandText = "SELECT id, songorder, signature FROM [fingerprints] WHERE [trackid] = @trackid LIMIT @limit";
			dbcmd.Parameters.Add(new SQLiteParameter("@trackid") { Value = trackId });
			dbcmd.Parameters.Add(new SQLiteParameter("@limit") { Value = numberOfFingerprintsToRead });
			dbcmd.CommandType = CommandType.Text;
			dbcmd.Prepare();

			IDataReader reader = dbcmd.ExecuteReader();
			while (reader.Read()) {
				Fingerprint fingerprint = new Fingerprint();
				fingerprint.Id = reader.GetInt32(0);
				fingerprint.TrackId = trackId;
				fingerprint.SongOrder = reader.GetInt32(1);
				fingerprint.Signature = ByteToBool((byte[]) reader.GetValue(2));
				fingerprints.Add(fingerprint);
			}
			
			reader.Close();
			return fingerprints;
		}

		public IDictionary<int, IList<Fingerprint>> ReadFingerprintsByMultipleTrackId(
			IEnumerable<Track> tracks, int numberOfFingerprintsToRead)
		{
			var result = new Dictionary<int, IList<Fingerprint>>();
			var fingerprints = new List<Fingerprint>();
			
			String statementValueTags = String.Join(",", tracks.Select(x => x.Id));

			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			
			dbcmd.CommandText = "SELECT id, trackid, songorder, signature FROM [fingerprints] WHERE trackid in (@trackids) LIMIT @limit";
			dbcmd.Parameters.Add(new SQLiteParameter("@trackids") { Value = statementValueTags});
			dbcmd.Parameters.Add(new SQLiteParameter("@limit") { Value = numberOfFingerprintsToRead });
			dbcmd.CommandType = CommandType.Text;
			dbcmd.Prepare();

			IDataReader reader = dbcmd.ExecuteReader();
			
			int lastTrackId = -1;
			while (reader.Read()) {
				Fingerprint fingerprint = new Fingerprint();
				fingerprint.Id = reader.GetInt32(0);
				fingerprint.TrackId = reader.GetInt32(1);
				fingerprint.SongOrder = reader.GetInt32(2);
				fingerprint.Signature = ByteToBool((byte[]) reader.GetValue(3));
				
				if (lastTrackId == -1 || lastTrackId == fingerprint.TrackId) {
					// still processing same track
				} else {
					// new track
					// add fingerprints to dictionary and then reset fingerprints
					result.Add(lastTrackId, fingerprints);
					fingerprints.Clear();
					fingerprints.Add(fingerprint);
				}
				lastTrackId = fingerprint.TrackId;
			}
			if (lastTrackId != -1) {
				// add last fingerprints
				result.Add(lastTrackId, fingerprints);
			}
			
			reader.Close();
			return result;
			
			//return fingerprintDao.ReadFingerprintsByMultipleTrackId(tracks, numberOfFingerprintsToRead);
		}

		public Fingerprint ReadFingerprintById(int id)
		{
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}

			dbcmd.CommandText = "SELECT trackid, songorder, signature FROM [fingerprints] WHERE [id] = @id";
			dbcmd.Parameters.Add(new SQLiteParameter("@id") { Value = id });
			dbcmd.CommandType = CommandType.Text;
			dbcmd.Prepare();

			IDataReader reader = dbcmd.ExecuteReader();
			if (!reader.Read()) {
				return null;
			}
			
			Fingerprint fingerprint = new Fingerprint();
			fingerprint.Id = id;
			fingerprint.TrackId = reader.GetInt32(0);
			fingerprint.SongOrder = reader.GetInt32(1);
			fingerprint.Signature = ByteToBool((byte[]) reader.GetValue(2));
			
			reader.Close();
			return fingerprint;
		}
		
		public IList<Fingerprint> ReadFingerprintById(IEnumerable<int> ids)
		{
			var fingerprints = new List<Fingerprint>();
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			
			String statementValueTags = String.Join(",", ids);
			
			dbcmd.CommandText = "SELECT id, trackid, songorder, signature FROM [fingerprints] WHERE id in (@ids)";
			dbcmd.Parameters.Add(new SQLiteParameter("@ids") { Value = statementValueTags });
			dbcmd.CommandType = CommandType.Text;
			dbcmd.Prepare();

			IDataReader reader = dbcmd.ExecuteReader();
			while (reader.Read()) {
				Fingerprint fingerprint = new Fingerprint();
				fingerprint.Id = reader.GetInt32(0);
				fingerprint.TrackId = reader.GetInt32(1);
				fingerprint.SongOrder = reader.GetInt32(2);
				fingerprint.Signature = ByteToBool((byte[]) reader.GetValue(3));
				fingerprints.Add(fingerprint);
			}
			
			reader.Close();
			return fingerprints;
		}

		public virtual IList<Track> ReadTracks()
		{
			//return trackDao.Read();
			return null;
		}

		public Track ReadTrackById(int id)
		{
			//return trackDao.ReadById(id);
			return null;
		}

		public Track ReadTrackByArtistAndTitleName(string artist, string title)
		{
			//return trackDao.ReadTrackByArtistAndTitleName(artist, title);
			return null;
		}

		public IList<Track> ReadTrackByFingerprint(int id)
		{
			//return trackDao.ReadTrackByFingerprintId(id);
			return null;
		}

		public IDictionary<int, IList<HashBinMinHash>> ReadFingerprintsByHashBucketLsh(long[] hashBuckets)
		{
			IDbDataParameter dbHashBinParam = new SQLiteParameter("@hashbin", DbType.Int64);
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			
			dbcmd.CommandText = "SELECT id, hashbin, hashtable, trackid, fingerprintid FROM hashbins WHERE hashbin = @hashbin";

			// TODO: verify that this can go here and does not need to be within the foreach loop below
			dbcmd.Parameters.Add(dbHashBinParam);
			dbcmd.CommandType = CommandType.Text;
			dbcmd.Prepare();
			
			IDictionary<int, IList<HashBinMinHash>> result = new Dictionary<int, IList<HashBinMinHash>>();
			foreach (long hashBin in hashBuckets)
			{
				dbHashBinParam.Value = hashBin;
				IDataReader reader = dbcmd.ExecuteReader();
				var resultPerHashBucket = new Dictionary<int, HashBinMinHash>();
				while (reader.Read()) {
					int hashId = reader.GetInt32(0);
					long hashBin2 = reader.GetInt32(1);
					int hashTable = reader.GetInt32(2);
					int trackId = reader.GetInt32(3);
					int fingerprintId = reader.GetInt32(4);
					HashBinMinHash hash = new HashBinMinHash(hashId, hashBin2, hashTable, trackId, fingerprintId);
					resultPerHashBucket.Add(fingerprintId, hash);
				}
				reader.Close();
				
				foreach (var pair in resultPerHashBucket)
				{
					if (result.ContainsKey(pair.Key))
					{
						result[pair.Key].Add(pair.Value);
					}
					else
					{
						result.Add(pair.Key, new List<HashBinMinHash>(new[] { pair.Value }));
					}
				}
			}

			return result;
		}
		#endregion

		#region Deletes
		public int DeleteTrack(int trackId)
		{
			//return trackDao.DeleteTrack(trackId);
			return -1;
		}

		public int DeleteTrack(Track track)
		{
			//return DeleteTrack(track.Id);
			return -1;
		}

		public int DeleteTrack(IEnumerable<int> collection)
		{
			//return collection.Sum(trackId => trackDao.DeleteTrack(trackId));
			return -1;
		}

		public int DeleteTrack(IEnumerable<Track> collection)
		{
			//return DeleteTrack(collection.Select(track => track.Id));
			return -1;
		}
		#endregion
		
		#region Private Static Utils
		private static bool[] ByteToBool(byte[] byteArray) {
			// basic - same count
			bool[] boolArray = new bool[byteArray.Length];
			for (int i = 0; i < byteArray.Length; i++) {
				boolArray[i] = (byteArray[i] == 1 ? true: false);
			}
			return boolArray;
		}
		
		private static byte[] BoolToByte(bool[] boolArray) {
			// http://stackoverflow.com/questions/713057/convert-bool-to-byte-c-sharp
			// basic - same count
			byte[] byteArray = Array.ConvertAll(boolArray, b => b ? (byte)1 : (byte)0);
			return byteArray;
		}
		#endregion
	}
}
