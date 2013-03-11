/*
 * Mirage - High Performance Music Similarity and Automatic Playlist Generator
 * http://hop.at/mirage
 * 
 * Copyright (C) 2007 Dominik Schnitzer <dominik@schnitzer.at>
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

		public Db()
		{
			//Console.WriteLine("Start");
			string homedir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string dbdir = Path.Combine(homedir,".mirage");
			string dbfile = Path.Combine(dbdir, "mirage.db");
			string sqlite = string.Format("Data Source={0};Version=3", dbfile);
			//Console.WriteLine(sqlite);
			
			if (!Directory.Exists(dbdir)) {
				Directory.CreateDirectory(dbdir);
			}
			
			dbcon = (IDbConnection) new SQLiteConnection(sqlite);
			dbcon.Open();
			
			//AddTable();
			
			/*
			IDbCommand dbcmd = dbcon.CreateCommand();
			dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS mirage"
				+ " (trackid INTEGER PRIMARY KEY, audioFeature BLOB, name TEXT, duration INTEGER)";
			dbcmd.ExecuteNonQuery();
			dbcmd.Dispose();
			 */
		}
		
		~Db()
		{
			dbcon.Close();
		}

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

		public Dictionary<string, int> GetTracks() {

			Dictionary<string, int> trackNames = new Dictionary<string, int>();
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}

			dbcmd.CommandText = "SELECT name, trackid FROM mirage";
			IDataReader reader = dbcmd.ExecuteReader();
			
			while(reader.Read())
			{
				string name = reader.GetString(0);
				int trackid = reader.GetInt32(1);
				trackNames.Add(name, trackid);
			}
			
			reader.Close();
			return trackNames;
		}
		
		public int AddTrack(int trackid, AudioFeature audioFeature)
		{
			IDbDataParameter dbAudioFeatureParam = new SQLiteParameter("@audioFeature", DbType.Binary);
			IDbDataParameter dbNameParam = new SQLiteParameter("@name", DbType.String);
			IDbDataParameter dbDurationParam = new SQLiteParameter("@duration", DbType.Int64);
			
			dbAudioFeatureParam.Value = audioFeature.ToBytes();
			dbNameParam.Value = audioFeature.Name;
			dbDurationParam.Value = audioFeature.Duration;
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "INSERT INTO mirage (trackid, audioFeature, name, duration) " +
				"VALUES (" + trackid + ", @audioFeature, @name, @duration)";
			dbcmd.Parameters.Add(dbAudioFeatureParam);
			dbcmd.Parameters.Add(dbNameParam);
			dbcmd.Parameters.Add(dbDurationParam);
			
			try {
				dbcmd.ExecuteNonQuery();
				//dbcmd.Dispose();
			} catch (SQLiteException) {
				return -1;
			}
			
			return trackid;
		}

		public bool RemoveTable()
		{
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "DROP TABLE IF EXISTS mirage";
			
			try {
				dbcmd.ExecuteNonQuery();
				//dbcmd.Dispose();
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
				+ " (trackid INTEGER PRIMARY KEY, audioFeature BLOB, name TEXT, duration INTEGER)";
			
			try {
				dbcmd.ExecuteNonQuery();
				//dbcmd.Dispose();
			} catch (SQLiteException){
				return false;
			}
			
			return true;
		}

		public int RemoveTrack(int trackid)
		{
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "DELETE FROM mirage WHERE trackid="+trackid;
			
			try {
				dbcmd.ExecuteNonQuery();
				//dbcmd.Dispose();
			} catch (SQLiteException){
				return -1;
			}
			
			return trackid;
		}

		public AudioFeature GetTrack(int trackid, Analyzer.AnalysisMethod analysisMethod)
		{
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "SELECT audioFeature, name, duration FROM mirage " +
				"WHERE trackid = " + trackid;
			IDataReader reader = dbcmd.ExecuteReader();
			if (!reader.Read()) {
				return null;
			}
			
			byte[] buf = (byte[]) reader.GetValue(0);
			string name = reader.GetString(1);
			long duration = (int) reader.GetInt64(2);
			
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
			
			return audioFeature;
		}
		
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
			
			dbcmd.CommandText = "SELECT audioFeature, trackid, name, duration FROM mirage " +
				"WHERE trackid NOT in (" + trackSql + ")";

			if (duration > 0) {
				dbcmd.CommandText += " AND duration > " + (int)(duration * (1.0 - percentage)) + " AND duration < " + (int)(duration * (1.0 + percentage));
			}
			//Console.Out.WriteLine(dbcmd.CommandText);
			
			return dbcmd.ExecuteReader();
		}
		
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
