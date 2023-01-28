using System;
using LibGit2Sharp;
using System.Threading.Tasks;
using UnityEngine;

namespace GitForArtists
{

	public class CreateBranchTask : AsyncTask
	{
		public CreateBranchTask (Action<string> onCreatedBranch, string newBrachName, string repoPath, string mainBranchNameRemote, Action<Action> enqueueAction)
		{
			this.onCreatedBranch = onCreatedBranch;
			this.newBrachName = newBrachName;
			this.repoPath = repoPath;
			this.mainBranchNameRemote = mainBranchNameRemote;
			this.enqueueAction = enqueueAction;
		}

		public Task Run ()
		{
			return Task.Run (() => {
				RunSync ();
			});
		}


		Action<string> onCreatedBranch;
		string newBrachName;

		string repoPath;

		string mainBranchNameRemote;

		Action<Action> enqueueAction;

		public void RunSync ()
		{
			var name = newBrachName;
			using (var repo = new Repository (repoPath)) {
				//get origin/master head . We rely on the fact our local master should always be up to date with remote master
				Branch masterBranch = repo.Branches [mainBranchNameRemote];
				Commit masterHead = masterBranch.Tip;
				enqueueAction (() => {
					Debug.Log ("master head" + masterHead.Sha);
				});
				//create new branch from master head
				var myBranch = repo.Branches [name];
				if (myBranch == null) {
					enqueueAction (() => {
						Debug.Log (string.Format ("Creating new branch {0} and checking out to it", name));
					});
					var branch = repo.CreateBranch (name, masterHead);
					enqueueAction (() => {
						Debug.Log ("branch head" + branch.Tip.Sha);
					});
					try {
						Commands.Checkout (repo, branch);
					} catch (CheckoutConflictException) {
						throw new Exception (string.Format ("Conflict prevents checkout to. Reset your working directory.", branch.FriendlyName));
					}
				} else {
					throw new Exception (string.Format ("Branch {0} already exists.. pick another name", name));
				}
				enqueueAction (() => {
					onCreatedBranch (newBrachName);
				});
			}
		}
	}

}