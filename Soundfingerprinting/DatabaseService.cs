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
			string sqlite = string.Format("Data Source={0};Version=3;", dbfile);
			sqlite += "Count Changes=off;Journal Mode=off;";
			sqlite += "Pooling=true;Cache Size=10000;Page Size=4096;Synchronous=off";
			
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
				+ " (id INTEGER PRIMARY KEY AUTOINCREMENT, trackid INTEGER, songorder INTEGER, totalfingerprints INTEGER, signature BLOB)";
			
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

		#region Add and Remove the Track table
		public bool RemoveTrackTable()
		{
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "DROP TABLE IF EXISTS tracks";
			
			try {
				dbcmd.ExecuteNonQuery();
			} catch (SQLiteException){
				return false;
			}
			
			return true;
		}
		
		public bool AddTrackTable() {
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS tracks"
				+ " (id INTEGER PRIMARY KEY AUTOINCREMENT, albumid INTEGER, length INTEGER, artist TEXT, title TEXT, filepath TEXT, tags TEXT)";

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
			IDbDataParameter dbTotalFingerprintsParam = new SQLiteParameter("@totalfingerprints", DbType.Int32);
			IDbDataParameter dbSignatureParam = new SQLiteParameter("@signature", DbType.Binary);
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "INSERT INTO fingerprints (trackid, songorder, totalfingerprints, signature) " +
				"VALUES (@trackid, @songorder, @totalfingerprints, @signature); SELECT last_insert_rowid();";
			dbcmd.Parameters.Add(dbTrackIdParam);
			dbcmd.Parameters.Add(dbSongOrderParam);
			dbcmd.Parameters.Add(dbTotalFingerprintsParam);
			dbcmd.Parameters.Add(dbSignatureParam);

			dbTrackIdParam.Value = fingerprint.TrackId;
			dbSongOrderParam.Value = fingerprint.SongOrder;
			dbTotalFingerprintsParam.Value = fingerprint.TotalFingerprintsPerTrack;
			dbSignatureParam.Value = fingerprint.Signature;
			
			try {
				dbcmd.Prepare();
				//dbcmd.ExecuteNonQuery();
				fingerprint.Id = Convert.ToInt32(dbcmd.ExecuteScalar());
				dbcmd.Dispose();
			} catch (Exception e) {
				throw e;
			}
		}

		public bool InsertFingerprint(IEnumerable<Fingerprint> collection)
		{
			IDbDataParameter dbTrackIdParam = new SQLiteParameter("@trackid", DbType.Int32);
			IDbDataParameter dbSongOrderParam = new SQLiteParameter("@songorder", DbType.Int32);
			IDbDataParameter dbTotalFingerprintsParam = new SQLiteParameter("@totalfingerprints", DbType.Int32);
			IDbDataParameter dbSignatureParam = new SQLiteParameter("@signature", DbType.Binary);
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
				dbcmd.CommandText = "INSERT INTO fingerprints (trackid, songorder, totalfingerprints, signature) " +
					"VALUES (@trackid, @songorder, @totalfingerprints, @signature); SELECT last_insert_rowid();";

				dbcmd.Parameters.Add(dbTrackIdParam);
				dbcmd.Parameters.Add(dbSongOrderParam);
				dbcmd.Parameters.Add(dbTotalFingerprintsParam);
				dbcmd.Parameters.Add(dbSignatureParam);
				dbcmd.Prepare();
				
				int count = collection.Count();
				using (var transaction = dbcon.BeginTransaction())
				{
					try {
						foreach (var fingerprint in collection) {
							dbTrackIdParam.Value = fingerprint.TrackId;
							dbSongOrderParam.Value = fingerprint.SongOrder;
							dbTotalFingerprintsParam.Value = fingerprint.TotalFingerprintsPerTrack = count;
							dbSignatureParam.Value = BoolToByte(fingerprint.Signature);

							//dbcmd.ExecuteNonQuery();
							fingerprint.Id = Convert.ToInt32(dbcmd.ExecuteScalar());
						}
						transaction.Commit();
						dbcmd.Dispose();
						
					} catch (Exception e1) {
						// attempt to rollback the transaction
						try {
							transaction.Rollback();
						} catch (Exception e2) {
							// do nothing
						}
						throw e1;
						return false;
					}
				}
			}
			return true;
		}

		public bool InsertTrack(Track track)
		{
			IDbDataParameter dbAlbumIdParam = new SQLiteParameter("@albumid", DbType.Int64);
			IDbDataParameter dbLengthParam = new SQLiteParameter("@length", DbType.Int32);
			IDbDataParameter dbArtistParam = new SQLiteParameter("@artist", DbType.String);
			IDbDataParameter dbTitleParam = new SQLiteParameter("@title", DbType.String);
			IDbDataParameter dbFilePathParam = new SQLiteParameter("@filepath", DbType.String);
			IDbDataParameter dbTagsParam = new SQLiteParameter("@tags", DbType.String);
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "INSERT INTO tracks (albumid, length, artist, title, filepath, tags) " +
				"VALUES (@albumid, @length, @artist, @title, @filepath, @tags); SELECT last_insert_rowid();";

			dbcmd.Parameters.Add(dbAlbumIdParam);
			dbcmd.Parameters.Add(dbLengthParam);
			dbcmd.Parameters.Add(dbArtistParam);
			dbcmd.Parameters.Add(dbTitleParam);
			dbcmd.Parameters.Add(dbFilePathParam);
			dbcmd.Parameters.Add(dbTagsParam);

			dbAlbumIdParam.Value = track.AlbumId;
			dbLengthParam.Value = track.TrackLengthMs;
			dbArtistParam.Value = track.Artist;
			dbTitleParam.Value = track.Title;
			dbFilePathParam.Value = track.FilePath;
			dbTagsParam.Value = string.Join(";", track.Tags.Select(x => x.Key + "=" + x.Value));
			
			try {
				dbcmd.Prepare();
				//dbcmd.ExecuteNonQuery();
				track.Id = Convert.ToInt32(dbcmd.ExecuteScalar());
				dbcmd.Dispose();
			} catch (Exception e) {
				throw e;
				return false;
			}
			return true;
		}

		public void InsertTrack(IEnumerable<Track> collection)
		{
			IDbDataParameter dbAlbumIdParam = new SQLiteParameter("@albumid", DbType.Int64);
			IDbDataParameter dbLengthParam = new SQLiteParameter("@length", DbType.Int32);
			IDbDataParameter dbArtistParam = new SQLiteParameter("@artist", DbType.String);
			IDbDataParameter dbTitleParam = new SQLiteParameter("@title", DbType.String);
			IDbDataParameter dbFilePathParam = new SQLiteParameter("@filepath", DbType.String);
			IDbDataParameter dbTagsParam = new SQLiteParameter("@tags", DbType.String);
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "INSERT INTO tracks (albumid, length, artist, title, filepath, tags) " +
				"VALUES (@albumid, @length, @artist, @title, @filepath, @tags); SELECT last_insert_rowid();";
			
			dbcmd.Parameters.Add(dbAlbumIdParam);
			dbcmd.Parameters.Add(dbLengthParam);
			dbcmd.Parameters.Add(dbArtistParam);
			dbcmd.Parameters.Add(dbTitleParam);
			dbcmd.Parameters.Add(dbFilePathParam);
			dbcmd.Parameters.Add(dbTagsParam);
			dbcmd.Prepare();
			
			using (var transaction = dbcon.BeginTransaction())
			{
				try {
					foreach (var track in collection) {
						dbAlbumIdParam.Value = track.AlbumId;
						dbLengthParam.Value = track.TrackLengthMs;
						dbArtistParam.Value = track.Artist;
						dbTitleParam.Value = track.Title;
						dbFilePathParam.Value = track.FilePath;
						dbTagsParam.Value = string.Join(";", track.Tags.Select(x => x.Key + "=" + x.Value));
						
						//dbcmd.ExecuteNonQuery();
						track.Id = Convert.ToInt32(dbcmd.ExecuteScalar());
					}
					transaction.Commit();
					dbcmd.Dispose();
					
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
				dbcmd.Dispose();
			} catch (Exception e) {
				throw e;
			}
		}

		public bool InsertHashBin(IEnumerable<HashBinMinHash> collection)
		{
			IDbDataParameter dbHashBinParam = new SQLiteParameter("@hashbin", DbType.Int64);
			IDbDataParameter dbHashTableParam = new SQLiteParameter("@hashtable", DbType.Int32);
			IDbDataParameter dbTrackIdParam = new SQLiteParameter("@trackid", DbType.Int32);
			IDbDataParameter dbFingerprintIdParam = new SQLiteParameter("@fingerprintid", DbType.Int32);
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
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
						dbcmd.Dispose();
						
					} catch (Exception e1) {
						// attempt to rollback the transaction
						try {
							transaction.Rollback();
						} catch (Exception e2) {
							// do nothing
						}
						throw e1;
						return false;
					}
				}
			}
			return true;
		}
		#endregion

		#region Reads
		public IDictionary<Track, int> ReadDuplicatedTracks()
		{
			throw new NotImplementedException();
			//return trackDao.ReadDuplicatedTracks();
		}
		
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
			dbcmd.Dispose();
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
			dbcmd.Dispose();
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
			
			String query = String.Format("SELECT id, trackid, songorder, signature FROM [fingerprints] WHERE (trackid IN ({0})) LIMIT {0};", statementValueTags, numberOfFingerprintsToRead);
			dbcmd.CommandText = query;
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
			dbcmd.Dispose();
			return result;
			
			//return fingerprintDao.ReadFingerprintsByMultipleTrackId(tracks, numberOfFingerprintsToRead);
		}

		public Fingerprint ReadFingerprintById(int id)
		{
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}

			dbcmd.CommandText = "SELECT trackid, songorder, totalfingerprints, signature FROM [fingerprints] WHERE [id] = @id";
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
			fingerprint.TotalFingerprintsPerTrack = reader.GetInt32(2);
			fingerprint.Signature = ByteToBool((byte[]) reader.GetValue(3));
			
			reader.Close();
			dbcmd.Dispose();
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
			
			String query = String.Format("SELECT id, trackid, songorder, totalfingerprints, signature FROM [fingerprints] WHERE (id IN ({0}));", statementValueTags);
			dbcmd.CommandText = query;
			dbcmd.CommandType = CommandType.Text;
			dbcmd.Prepare();

			IDataReader reader = dbcmd.ExecuteReader();
			while (reader.Read()) {
				Fingerprint fingerprint = new Fingerprint();
				fingerprint.Id = reader.GetInt32(0);
				fingerprint.TrackId = reader.GetInt32(1);
				fingerprint.SongOrder = reader.GetInt32(2);
				fingerprint.TotalFingerprintsPerTrack = reader.GetInt32(3);
				fingerprint.Signature = ByteToBool((byte[]) reader.GetValue(4));
				fingerprints.Add(fingerprint);
			}
			
			reader.Close();
			dbcmd.Dispose();
			return fingerprints;
		}

		public IList<string> ReadTrackFilenames() {
			var filenames = new List<string>();
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			
			dbcmd.CommandText = "SELECT filepath FROM [tracks]";
			dbcmd.CommandType = CommandType.Text;

			IDataReader reader = dbcmd.ExecuteReader();
			while (reader.Read()) {
				string filename = reader.GetString(0);
				filenames.Add(filename);
			}
			
			reader.Close();
			dbcmd.Dispose();
			return filenames;
		}
		
		public IList<Track> ReadTracks()
		{
			var tracks = new List<Track>();
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			
			dbcmd.CommandText = "SELECT id, albumid, length, artist, title, filepath FROM [tracks]";
			dbcmd.CommandType = CommandType.Text;

			IDataReader reader = dbcmd.ExecuteReader();
			while (reader.Read()) {
				Track track = new Track();
				track.Id = reader.GetInt32(0);
				track.AlbumId = reader.GetInt32(1);
				track.TrackLengthMs = reader.GetInt32(2);
				if (!reader.IsDBNull(3)) {
					track.Artist = reader.GetString(3);
				}
				track.Title = reader.GetString(4);
				track.FilePath = reader.GetString(5);
				tracks.Add(track);
			}
			
			reader.Close();
			dbcmd.Dispose();
			return tracks;
		}

		public IList<Track> ReadTracks(string whereClause)
		{
			var tracks = new List<Track>();
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}

			string query = "SELECT id, albumid, length, artist, title, filepath FROM [tracks]";
			if (whereClause != null && whereClause != "") {
				query = string.Format("{0} {1}", query, whereClause);
			}
			
			dbcmd.CommandText = query;
			dbcmd.CommandType = CommandType.Text;

			IDataReader reader = dbcmd.ExecuteReader();
			while (reader.Read()) {
				Track track = new Track();
				track.Id = reader.GetInt32(0);
				track.AlbumId = reader.GetInt32(1);
				track.TrackLengthMs = reader.GetInt32(2);
				if (!reader.IsDBNull(3)) {
					track.Artist = reader.GetString(3);
				}
				track.Title = reader.GetString(4);
				track.FilePath = reader.GetString(5);
				tracks.Add(track);
			}
			
			reader.Close();
			dbcmd.Dispose();
			return tracks;
		}
		
		public Track ReadTrackById(int id)
		{
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}

			dbcmd.CommandText = "SELECT albumid, length, artist, title, filepath FROM [tracks] WHERE [id] = @id";
			dbcmd.Parameters.Add(new SQLiteParameter("@id") { Value = id });
			dbcmd.CommandType = CommandType.Text;
			dbcmd.Prepare();

			IDataReader reader = dbcmd.ExecuteReader();
			if (!reader.Read()) {
				return null;
			}
			
			Track track = new Track();
			track.Id = id;
			track.AlbumId = reader.GetInt32(0);
			track.TrackLengthMs = reader.GetInt32(1);
			if (!reader.IsDBNull(2)) {
				track.Artist = reader.GetString(2);
			}
			track.Title = reader.GetString(3);
			track.FilePath = reader.GetString(4);
			
			reader.Close();
			dbcmd.Dispose();
			return track;
		}

		public IList<Track> ReadTrackById(IEnumerable<int> ids)
		{
			var tracks = new List<Track>();
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			
			String statementValueTags = String.Join(",", ids);
			
			String query = String.Format("SELECT id, albumid, length, artist, title, filepath FROM [tracks] WHERE (id IN ({0}));", statementValueTags);
			dbcmd.CommandText = query;
			dbcmd.CommandType = CommandType.Text;
			dbcmd.Prepare();

			IDataReader reader = dbcmd.ExecuteReader();
			while (reader.Read()) {
				Track track = new Track();
				track.Id = reader.GetInt32(0);
				track.AlbumId = reader.GetInt32(1);
				track.TrackLengthMs = reader.GetInt32(2);
				if (!reader.IsDBNull(3)) {
					track.Artist = reader.GetString(3);
				}
				track.Title = reader.GetString(4);
				track.FilePath = reader.GetString(5);
				tracks.Add(track);
			}
			
			reader.Close();
			dbcmd.Dispose();
			return tracks;
		}
		
		public Track ReadTrackByArtistAndTitleName(string artist, string title)
		{
			throw new NotImplementedException();
		}

		public IList<Track> ReadTrackByFingerprint(int id)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Find fingerprints using hash-buckets (e.g. HashBins)
		/// </summary>
		/// <param name="hashBuckets"></param>
		/// <returns>Return dictionary with fingerprintids as keys and the corresponding hashbins as values</returns>
		public IDictionary<int, IList<HashBinMinHash>> ReadFingerprintsByHashBucketLshSlow(long[] hashBuckets)
		{
			IDbDataParameter dbHashBinParam = new SQLiteParameter("@hashbin", DbType.Int64);
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			
			dbcmd.CommandText = "SELECT id, hashbin, hashtable, trackid, fingerprintid FROM hashbins WHERE hashbin = @hashbin";

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
		
		/// <summary>
		/// Find fingerprints using hash-buckets (e.g. HashBins)
		/// </summary>
		/// <param name="hashBuckets"></param>
		/// <returns>Return dictionary with fingerprintids as keys and the corresponding hashbins as values</returns>
		public IDictionary<int, IList<HashBinMinHash>> ReadFingerprintsByHashBucketLsh(long[] hashBuckets) {
			
			IDictionary<int, IList<HashBinMinHash>> result = new Dictionary<int, IList<HashBinMinHash>>();
			
			String statementValueTags = String.Join(",", hashBuckets);

			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}

			String query = String.Format("SELECT id, hashbin, hashtable, trackid, fingerprintid FROM hashbins WHERE (hashbin IN ({0}))", statementValueTags);
			dbcmd.CommandText = query;
			dbcmd.CommandType = CommandType.Text;
			dbcmd.Prepare();
			
			IDataReader reader = dbcmd.ExecuteReader();
			while (reader.Read()) {
				HashBinMinHash hash = new HashBinMinHash();
				hash.Id = reader.GetInt32(0);
				hash.Bin = reader.GetInt64(1);
				hash.HashTable = reader.GetInt32(2);
				hash.TrackId = reader.GetInt32(3);
				hash.FingerprintId = reader.GetInt32(4);
				
				if (result.ContainsKey(hash.FingerprintId))
				{
					result[hash.FingerprintId].Add(hash);
				}
				else
				{
					result.Add(hash.FingerprintId, new List<HashBinMinHash>(new[] { hash }));
				}
			}
			
			reader.Close();
			return result;
		}
		#endregion

		#region Deletes
		public int DeleteTrack(int trackId)
		{
			throw new NotImplementedException();
		}

		public int DeleteTrack(Track track)
		{
			throw new NotImplementedException();
		}

		public int DeleteTrack(IEnumerable<int> collection)
		{
			throw new NotImplementedException();
		}

		public int DeleteTrack(IEnumerable<Track> collection)
		{
			throw new NotImplementedException();
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
