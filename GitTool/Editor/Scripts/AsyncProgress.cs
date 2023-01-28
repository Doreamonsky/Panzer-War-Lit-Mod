using System;

namespace GitForArtists
{

	public class AsyncProgress
	{
		public AsyncProgress (string info, float progress)
		{
			this.info = info;
			this.value = progress;
		}

		public string info;
		public float value;
	}
}