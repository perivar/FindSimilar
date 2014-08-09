/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 * 
 * Copyright (C) 2007 Dominik Schnitzer <dominik@schnitzer.at>
 * Changed and enhanced by Per Ivar Nerseth <perivar@nerseth.com>
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA  02110-1301, USA.
 */

using System;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

using Comirva.Audio.Feature;

namespace Mirage
{
	// SQL Lite database class
	// Originally a part of the Mirage project
	// Heavily modified by perivar@nerseth.com
	public class Db
	{
		IDbConnection dbcon;

		#region Constructor and Destructor
		public Db()
		{
			string homedir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string dbdir = Path.Combine(homedir,".mirage");
			string dbfile = Path.Combine(dbdir, "mirage.db");
			string sqlite = string.Format("Data Source={0};Version=3", dbfile);
			
			if (!Directory.Exists(dbdir)) {
				Directory.CreateDirectory(dbdir);
			}
			
			dbcon = (IDbConnection) new SQLiteConnection(sqlite);
			dbcon.Open();
		}
		
		~Db()
		{
			dbcon.Close();
		}
		#endregion
		
		#region Add and Remove the table
		public bool RemoveTable()
		{
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "DROP TABLE IF EXISTS mirage";
			
			try {
				dbcmd.ExecuteNonQuery();
			} catch (SQLiteException){
				return false;
			}
			
			return true;
		}
		
		public bool AddTable() {
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS mirage"
				+ " (trackid INTEGER PRIMARY KEY, audioFeature BLOB, name TEXT, duration INTEGER, bitstring TEXT)";
			
			try {
				dbcmd.ExecuteNonQuery();
			} catch (SQLiteException){
				return false;
			}
			
			return true;
		}
		#endregion
		
		#region Has, Add and Remove Track
		/// <summary>
		/// Check whether we have a given filename already stored in the database
		/// </summary>
		/// <param name="name">filename</param>
		/// <param name="trackid">id returned if found</param>
		/// <returns>true if track was found</returns>
		public bool HasTrack(string name, out int trackid) {
			
			IDbDataParameter dbNameParam = new SQLiteParameter("@name", DbType.String);
			dbNameParam.Value = name;
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}

			dbcmd.CommandText = "SELECT trackid FROM mirage WHERE name=@name";
			dbcmd.Parameters.Add(dbNameParam);
			
			IDataReader reader = dbcmd.ExecuteReader();
			if (!reader.Read()) {
				trackid = -1;
				return false;
			}
			
			trackid = reader.GetInt32(0);
			
			reader.Close();
			return true;
		}
		
		/// <summary>
		/// Add a track to the database using the given track-id
		/// </summary>
		/// <param name="trackid">track-id to use</param>
		/// <param name="audioFeature">the audiofeature object</param>
		/// <returns>-1 if failed otherwise the track-id passed</returns>
		public int AddTrack(ref int trackid, AudioFeature audioFeature)
		{
			IDbDataParameter dbTrackIdParam = new SQLiteParameter("@trackid", DbType.Int64);
			IDbDataParameter dbAudioFeatureParam = new SQLiteParameter("@audioFeature", DbType.Binary);
			IDbDataParameter dbNameParam = new SQLiteParameter("@name", DbType.String);
			IDbDataParameter dbDurationParam = new SQLiteParameter("@duration", DbType.Int64);
			IDbDataParameter dbBitStringParam = new SQLiteParameter("@bitstring", DbType.String);
			
			dbTrackIdParam.Value = trackid;
			dbAudioFeatureParam.Value = audioFeature.ToBytes();
			dbNameParam.Value = audioFeature.Name;
			dbDurationParam.Value = audioFeature.Duration;
			dbBitStringParam.Value = audioFeature.BitString;
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "INSERT INTO mirage (trackid, audioFeature, name, duration, bitstring) " +
				"VALUES (@trackid, @audioFeature, @name, @duration, @bitstring)";
			dbcmd.Parameters.Add(dbTrackIdParam);
			dbcmd.Parameters.Add(dbAudioFeatureParam);
			dbcmd.Parameters.Add(dbNameParam);
			dbcmd.Parameters.Add(dbDurationParam);
			dbcmd.Parameters.Add(dbBitStringParam);
			
			try {
				dbcmd.ExecuteNonQuery();
			} catch (SQLiteException) {
				return -1;
			}
			
			//trackid++;
			return trackid;
		}

		/// <summary>
		/// Add a track to the database
		/// </summary>
		/// <param name="audioFeature">the audiofeature object</param>
		/// <returns>-1 if failed otherwise the track-id passed</returns>
		public int AddTrack(AudioFeature audioFeature)
		{
			IDbDataParameter dbAudioFeatureParam = new SQLiteParameter("@audioFeature", DbType.Binary);
			IDbDataParameter dbNameParam = new SQLiteParameter("@name", DbType.String);
			IDbDataParameter dbDurationParam = new SQLiteParameter("@duration", DbType.Int64);
			IDbDataParameter dbBitStringParam = new SQLiteParameter("@bitstring", DbType.String);
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "INSERT INTO mirage (audioFeature, name, duration, bitstring) " +
				"VALUES (@audioFeature, @name, @duration, @bitstring); SELECT last_insert_rowid();";
			
			dbcmd.Parameters.Add(dbAudioFeatureParam);
			dbcmd.Parameters.Add(dbNameParam);
			dbcmd.Parameters.Add(dbDurationParam);
			dbcmd.Parameters.Add(dbBitStringParam);

			dbAudioFeatureParam.Value = audioFeature.ToBytes();
			dbNameParam.Value = audioFeature.Name;
			dbDurationParam.Value = audioFeature.Duration;
			dbBitStringParam.Value = audioFeature.BitString;
			
			int trackid = -1;
			try {
				dbcmd.Prepare();
				//dbcmd.ExecuteNonQuery();
				trackid = Convert.ToInt32(dbcmd.ExecuteScalar());
				dbcmd.Dispose();
			} catch (Exception) {
				return -1;
			}
			return trackid;
		}

		/// <summary>
		/// Remove a track from the database using the given track-id
		/// </summary>
		/// <param name="trackid">track-id to delete</param>
		/// <returns>-1 if failed otherwise the track-id passed</returns>
		public int RemoveTrack(int trackid)
		{
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "DELETE FROM mirage WHERE trackid="+trackid;
			
			try {
				dbcmd.ExecuteNonQuery();
			} catch (SQLiteException){
				return -1;
			}
			
			return trackid;
		}
		#endregion

		/// <summary>
		/// Return the number of tracks in the database
		/// </summary>
		/// <returns>the number of tracks in the database</returns>
		public int GetTrackCount() {
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}

			dbcmd.CommandText = "SELECT Count(*) FROM mirage";
			int count = Convert.ToInt32(dbcmd.ExecuteScalar());
			
			dbcmd.Dispose();
			return count;
		}
		
		/// <summary>
		/// Return all trackids from the database
		/// </summary>
		/// <returns>and int array of the database tracks</returns>
		public int[] GetTrackIds()
		{
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}

			dbcmd.CommandText = "SELECT trackid FROM mirage";
			IDataReader reader = dbcmd.ExecuteReader();
			
			ArrayList tracks = new ArrayList();
			
			while (reader.Read()) {
				tracks.Add(reader.GetInt32(0));
			}
			reader.Close();
			
			int[] tracksInt = new int[tracks.Count];
			IEnumerator e = tracks.GetEnumerator();
			int i = 0;
			while (e.MoveNext()) {
				tracksInt[i] = (int)e.Current;
				i++;
			}
			return tracksInt;
		}
		
		/// <summary>
		/// Get a track from the database using its id
		/// </summary>
		/// <param name="trackid">id</param>
		/// <param name="analysisMethod">analysis method (SCMS or MandelEllis)</param>
		/// <returns>an AudioFeature object</returns>
		public AudioFeature GetTrack(int trackid, Analyzer.AnalysisMethod analysisMethod)
		{
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "SELECT audioFeature, name, duration, bitstring FROM mirage " +
				"WHERE trackid = " + trackid;
			IDataReader reader = dbcmd.ExecuteReader();
			if (!reader.Read()) {
				return null;
			}
			
			byte[] buf = (byte[]) reader.GetValue(0);
			string name = reader.GetString(1);
			long duration = reader.GetInt64(2);
			string bitstring = reader.GetString(3);
			
			reader.Close();
			
			AudioFeature audioFeature = null;
			switch (analysisMethod) {
				case Analyzer.AnalysisMethod.MandelEllis:
					audioFeature = MandelEllis.FromBytes(buf);
					break;
				case Analyzer.AnalysisMethod.SCMS:
					audioFeature = Scms.FromBytes(buf);
					break;
			}
			audioFeature.Name = name;
			audioFeature.Duration = duration;
			audioFeature.BitString = bitstring;
			
			return audioFeature;
		}
		
		/// <summary>
		/// Return all track filenames from the database
		/// </summary>
		/// <returns>a List of filenames.</returns>
		public IList<string> ReadTrackFilenames() {
			var filenames = new List<string>();
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			
			dbcmd.CommandText = "SELECT name FROM [mirage]";
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
		
		/// <summary>
		/// Return all tracks from the database
		/// </summary>
		/// <returns>a dictionary of filenames as Key. The Values are keyvaluepairs with the track-id as Key and duration as Value.</returns>
		public Dictionary<string, KeyValuePair<int, long>> GetTracks(string whereClause = null) {

			Dictionary<string, KeyValuePair<int, long>> trackNames = new Dictionary<string, KeyValuePair<int, long>>();
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			
			string query = "SELECT trackid, name, duration FROM mirage";
			if (whereClause != null && whereClause != "") {
				query = string.Format("{0} {1}", query, whereClause);
			}
			
			dbcmd.CommandText = query;
			IDataReader reader = dbcmd.ExecuteReader();
			
			while(reader.Read())
			{
				int trackid = reader.GetInt32(0);
				string filename = reader.GetString(1);
				long duration = reader.GetInt64(2);
				trackNames.Add(filename, new KeyValuePair<int, long>(trackid, duration));
			}
			
			reader.Close();
			return trackNames;
		}
		
		/// <summary>
		/// Return a defined number of (or all) tracks from the database specified by a optional where clause
		/// </summary>
		/// <param name="numberToRead">number of items to read</param>
		/// <param name="whereClause">where clause</param>
		/// <returns>a list with queryresult items</returns>
		public List<FindSimilar.QueryResult> GetTracksList(int numberToRead, string whereClause = null) {
			
			List<FindSimilar.QueryResult> queryResultList = new List<FindSimilar.QueryResult>();
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}

			string query = "SELECT trackid, name, duration FROM mirage";
			if (whereClause != null && whereClause != "") {
				query = string.Format("{0} {1}", query, whereClause);
			}
			
			if (numberToRead > 0) {
				query += " LIMIT @limit";
			}
			
			dbcmd.CommandText = query;
			dbcmd.Parameters.Add(new SQLiteParameter("@limit") { Value = numberToRead });
			dbcmd.CommandType = CommandType.Text;
			dbcmd.Prepare();

			IDataReader reader = dbcmd.ExecuteReader();
			while (reader.Read()) {
				FindSimilar.QueryResult queryResult = new FindSimilar.QueryResult();
				queryResult.Id = reader.GetInt32(0);
				queryResult.Path = reader.GetString(1);
				queryResult.Duration = reader.GetInt64(2);
				queryResultList.Add(queryResult);
			}
			
			reader.Close();
			dbcmd.Dispose();

			return queryResultList;
		}

		/// <summary>
		/// Return all tracks from the database except the trackids
		/// If duration is more than 0 and percentage is less than 1.0
		/// the tracks duration must not be more or less than the percentage given
		/// </summary>
		/// <param name="trackId">tracks ids to exclude</param>
		/// <param name="duration">duration (used if more than 0)</param>
		/// <param name="percentage">percentage below and above the duration in ms when querying (used if between 0.1 - 0.9)</param>
		/// <returns>a datareader pointer</returns>
		public IDataReader GetTracks(int[] trackId, long duration, double percentage)
		{
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			
			String trackSql = "";
			if ((trackId != null) && (trackId.Length > 0)) {
				trackSql = trackId[0].ToString();
				
				for (int i = 1; i < trackId.Length; i++) {
					trackSql = trackSql + ", " + trackId[i];
				}
			}
			
			dbcmd.CommandText = "SELECT audioFeature, trackid, name, duration, bitstring FROM mirage " +
				"WHERE trackid NOT in (" + trackSql + ")";

			if (duration > 0 && percentage < 1.0) {
				dbcmd.CommandText += " AND duration > " + (int)(duration * (1.0 - percentage)) + " AND duration < " + (int)(duration * (1.0 + percentage));
			}
			
			return dbcmd.ExecuteReader();
		}
		
		/// <summary>
		/// Using the passed datareader pointer, fill the audio feature tracks array with content
		/// </summary>
		/// <param name="tracksIterator">datareader pointer</param>
		/// <param name="tracks">AudioFeature array</param>
		/// <param name="mapping">array of trackids</param>
		/// <param name="len">number of tracks to return</param>
		/// <param name="analysisMethod">analysis method (SCMS or MandelEllis)</param>
		/// <returns>number of tracks returned</returns>
		public int GetNextTracks(ref IDataReader tracksIterator, ref AudioFeature[] tracks,
		                         ref int[] mapping, int len, Analyzer.AnalysisMethod analysisMethod)
		{
			int i = 0;

			while ((i < len) && tracksIterator.Read()) {
				
				AudioFeature audioFeature = null;
				switch (analysisMethod) {
					case Analyzer.AnalysisMethod.MandelEllis:
						audioFeature = MandelEllis.FromBytes((byte[]) tracksIterator.GetValue(0));
						break;
					case Analyzer.AnalysisMethod.SCMS:
						audioFeature = Scms.FromBytes((byte[]) tracksIterator.GetValue(0));
						break;
				}
				mapping[i] = tracksIterator.GetInt32(1);
				audioFeature.Name = tracksIterator.GetString(2);
				audioFeature.Duration = tracksIterator.GetInt64(3);
				audioFeature.BitString = tracksIterator.GetString(4);
				tracks[i] = audioFeature;
				i++;
			}

			if (i == 0) {
				tracksIterator.Close();
				tracksIterator = null;
			}
			
			return i;
		}
		
	}
}
