using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using LibGit2Sharp;
using System.Linq;
using LibGit2Sharp.Handlers;
using System.ComponentModel;
using System.Threading.Tasks;

namespace GitForArtists
{

	class TaskRunner
	{
		public Task Run (AsyncTask[] tasks)
		{
			var enumerator = tasks.OfType<AsyncTask> ().GetEnumerator ();
			return Task.Run (() => {
				while (enumerator.MoveNext ()) {
					enumerator.Current.RunSync ();	
				}
			});
		}
	}
}