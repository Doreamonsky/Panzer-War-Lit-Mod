using System;
using LibGit2Sharp;
using System.Linq;
using LibGit2Sharp.Handlers;
using System.Threading.Tasks;
using UnityEngine;

namespace GitForArtists
{

	public class UpdateOriginBranchesTask : AsyncTask
	{
		public UpdateOriginBranchesTask (Action<Action> enqueueAction, Action<AsyncProgress> updateProgress, string repoPath, string origin, string userName, string password)
		{
			this.updateProgress = updateProgress;
			this.enqueueAction = enqueueAction;
			this.repoPath = repoPath;
			this.origin = origin;
			this.userName = userName;
			this.password = password;
		}

		Action<AsyncProgress> updateProgress;

		string repoPath;

		string origin;

		string userName;

		string password;

		Action<Action> enqueueAction;

		public Task Run ()
		{
			return Task.Run (() => {
				RunSync ();
			});
		}

		public void RunSync ()
		{
			enqueueAction (() => {
				Debug.Log ("Updating origin...");
			});
			using (var repo = new Repository (repoPath)) {
				var remote = repo.Network.Remotes.First (e => e.Name == origin);
				string logMessage = "";
				FetchOptions options = new FetchOptions ();
				options.OnTransferProgress += (TransferProgress p) => {
					enqueueAction (() => {
						updateProgress (new AsyncProgress ("Working hard...", p.ReceivedObjects / (float)p.TotalObjects));
					});
					return true;
				};
				options.CredentialsProvider = new CredentialsHandler ((url, usernameFromUrl, types) => 
			new UsernamePasswordCredentials () {
					Username = userName,
					Password = password
				});
				//fetch --all
				Commands.Fetch (repo, remote.Name, remote.FetchRefSpecs.Select (x => x.Specification), options, logMessage);
				
				// 暂时不需要在本地显示远端的分支
				// create local branches for all remote bvranches
				// var remoteBranchesToAdd = repo.Branches.Where (b => b.IsRemote && !BranchHasLocalEquivalent (repo, b)).ToList ();
				// foreach (var remoteBranch in remoteBranchesToAdd) {
				// 	enqueueAction (() => {
				// 		Debug.Log ("Adding " + remoteBranch.FriendlyName);
				// 	});
				// 	var localBranch = repo.Branches.Add (remoteBranch.FriendlyName.Replace (string.Format ("{0}/", origin), ""), remoteBranch.Tip);
				// 	repo.Branches.Update (localBranch, b => b.TrackedBranch = remoteBranch.CanonicalName);
				// }
				enqueueAction (() => {
					Debug.Log ("Updated origin branches");
				});
			}
		}

		bool BranchHasLocalEquivalent (Repository repo, Branch remoteBranch)
		{
			return repo.Branches
			.Where (b => !b.IsRemote)
			.Select (b => b.FriendlyName)
			.Contains (remoteBranch.FriendlyName.Replace (string.Format ("{0}/", origin), ""));
		}
	}

}