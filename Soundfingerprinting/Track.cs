using System;
using System.Collections.Generic;

namespace Soundfingerprinting.DbStorage.Entities
{
	public class Track
	{
		private string artist;

		private string title;

		private int trackLengthMs;
		
		private string filePath;
		
		private Dictionary<string, string> tags = new Dictionary<string, string>();

		public Track()
		{
		}

		public Track(int trackId, string artist, string title, int albumId, string filePath)
		{
			Id = trackId;
			Artist = artist;
			Title = title;
			AlbumId = albumId;
			FilePath = filePath;
		}

		public Track(int trackId, string artist, string title, int albumId, string filePath, int trackLength)
			: this(trackId, artist, title, albumId, filePath)
		{
			TrackLengthMs = trackLength;
		}

		public int Id { get; set; }

		public string Artist
		{
			get
			{
				return artist;
			}

			set
			{
				if (value.Length > 255)
				{
					throw new Exception(
						"Artist's length cannot exceed a predefined value. Check the documentation");
				}

				artist = value;
			}
		}

		public string Title
		{
			get
			{
				return title;
			}

			set
			{
				if (value.Length > 255)
				{
					throw new Exception(
						"Title's length cannot exceed a predefined value. Check the documentation");
				}

				title = value;
			}
		}

		public int AlbumId { get; set; }

		public int TrackLengthMs
		{
			get
			{
				return trackLengthMs;
			}

			set
			{
				if (value < 0)
				{
					throw new Exception("Track's Length cannot be less than 0");
				}

				trackLengthMs = value;
			}
		}
		
		public string FilePath {
			get {
				return filePath;
			}
			set {
				filePath = value;
			}
		}
		
		public Dictionary<string, string> Tags {
			get {
				return tags;
			}
			set {
				tags = value;
			}
		}
		
		public override string ToString() {
			return String.Format("Id: {0}, artist: {1}, title: {2}, albumId: {3}, length: {4} ms", Id, Artist, Title, AlbumId, TrackLengthMs);
		}

	}
}