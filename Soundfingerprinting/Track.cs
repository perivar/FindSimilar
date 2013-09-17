using System;

namespace Soundfingerprinting.DbStorage.Entities
{
	public class Track
	{
		private string artist;

		private string title;

		private int trackLengthSec;

		public Track()
		{
		}

		public Track(int trackId, string artist, string title, int albumId)
		{
			Id = trackId;
			Artist = artist;
			Title = title;
			AlbumId = albumId;
		}

		public Track(int trackId, string artist, string title, int albumId, int trackLength)
			: this(trackId, artist, title, albumId)
		{
			TrackLengthSec = trackLength;
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

		public int TrackLengthSec
		{
			get
			{
				return trackLengthSec;
			}

			set
			{
				if (value < 0)
				{
					throw new Exception("Track's Length cannot be less than 0");
				}

				trackLengthSec = value;
			}
		}
		
		public override string ToString() {
			return String.Format("Id: {0}, artist: {1}, title: {2}, albumId: {3}, length: {4}", Id, Artist, Title, AlbumId, TrackLengthSec);
		}

	}
}