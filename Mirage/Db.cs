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
using System.Data.SQLite;
using System.Data;
using System.IO;
using System;
using System.Collections;

namespace Mirage
{
	public class Db
	{
		IDbConnection dbcon;

		public Db()
		{
			Console.WriteLine("Start");
			string homedir = Environment.GetFolderPath(
				Environment.SpecialFolder.Personal);
			string dbdir = Path.Combine(homedir,".mirage");
			string dbfile = Path.Combine(dbdir, "mirage.db");
			string sqlite = string.Format("Data Source={0};Version=3", dbfile);
			Console.WriteLine(sqlite);
			
			if (!Directory.Exists(dbdir)) {
				Directory.CreateDirectory(dbdir);
			}
			
			dbcon = (IDbConnection) new SQLiteConnection(sqlite);
			dbcon.Open();
			
			IDbCommand dbcmd = dbcon.CreateCommand();
			dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS mirage"
				+ " (trackid INTEGER PRIMARY KEY, scms BLOB, name TEXT)";
			dbcmd.ExecuteNonQuery();
			dbcmd.Dispose();
		}
		
		~Db()
		{
			dbcon.Close();
		}
		
		public int AddTrack(int trackid, Scms scms, string name)
		{
			IDbDataParameter dbparam = new SQLiteParameter("@scms", DbType.Binary);
			IDbDataParameter dbparamName = new SQLiteParameter("@name", DbType.String);
			
			dbparam.Value = scms.ToBytes();
			dbparamName.Value = name;
			
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "INSERT INTO mirage (trackid, scms, name) " +
				"VALUES (" + trackid + ", @scms, @name)";
			dbcmd.Parameters.Add(dbparam);
			dbcmd.Parameters.Add(dbparamName);
			
			try {
				dbcmd.ExecuteNonQuery();
			} catch (SQLiteException) {
				return -1;
			}
			
			return trackid;
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
			} catch (SQLiteException){
				return -1;
			}
			
			return trackid;
		}

		public Scms GetTrack(int trackid)
		{
			IDbCommand dbcmd;
			lock (dbcon) {
				dbcmd = dbcon.CreateCommand();
			}
			dbcmd.CommandText = "SELECT scms FROM mirage " +
				"WHERE trackid = " + trackid;
			IDataReader reader = dbcmd.ExecuteReader();
			if (!reader.Read()) {
				return null;
			}
			
			byte[] buf = (byte[]) reader.GetValue(0);
			reader.Close();
			
			return Scms.FromBytes(buf);
		}
		
		public IDataReader GetTracks(int[] trackId)
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
			
			dbcmd.CommandText = "SELECT scms, trackid FROM mirage " +
				"WHERE trackid NOT in (" + trackSql + ")";
			Console.WriteLine(dbcmd.CommandText);

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
		
		public int GetNextTracks(ref IDataReader tracksIterator, ref Scms[] tracks,
		                         ref int[] mapping, int len)
		{
			int i = 0;

			while ((i < len) && tracksIterator.Read()) {
				tracks[i] = Scms.FromBytes((byte[]) tracksIterator.GetValue(0));
				mapping[i] = tracksIterator.GetInt32(1);
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
