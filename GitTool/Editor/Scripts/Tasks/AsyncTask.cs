using System;
using System.Threading.Tasks;

namespace GitForArtists
{

	public interface AsyncTask
	{
		Task Run ();

		void RunSync ();
	}

}