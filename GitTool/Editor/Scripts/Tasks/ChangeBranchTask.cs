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

	public class ChangeBranchTask : AsyncTask
	{
		public ChangeBranchTask (Action<Action> enqueueAction, Action<string> onChangedBranch, string repoPath, string name)
		{
			this.onChangedBranch = onChangedBranch;
			this.enqueueAction = enqueueAction;
			this.repoPath = repoPath;
			this.selectedBranchName = name;
		}

		public Task Run ()
		{
			return Task.Run (() => {
				RunSync ();
			});
		}


		Action<string> onChangedBranch;

		Action<Action> enqueueAction;
		string repoPath;

		string selectedBranchName;

		public void RunSync ()
		{
			using (var repo = new Repository (repoPath)) {
				var branch = repo.Branches [selectedBranchName];
				if (branch.IsRemote) {
					throw new Exception (string.Format ("{0} is a remote branch. Can't check out.", branch.FriendlyName));
				}
				try {
					Commands.Checkout (repo, branch);
				} catch (CheckoutConflictException) {
					throw new Exception (string.Format ("Conflict prevents checkout to {0}. Reset your working directory", branch.FriendlyName));
				}
				enqueueAction (() => {
					Debug.Log ("Changed to branch: " + branch.FriendlyName);
					onChangedBranch (branch.FriendlyName);
				});
			}
		}
	}
}