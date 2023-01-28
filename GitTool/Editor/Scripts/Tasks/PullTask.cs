using System;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System.Threading.Tasks;
using UnityEngine;

namespace GitForArtists
{

	public class PullTask : AsyncTask
	{
		public PullTask (Action<Action> enqueueAction, Action<AsyncProgress> updateProgress, string repoPath, string origin, string currentBranch, string userName, string password, string userfullName, string userEmail)
		{
			this.enqueueAction = enqueueAction;
			this.updateProgress = updateProgress;
			this.repoPath = repoPath;
			this.origin = origin;
			this.currentBranch = currentBranch;
			this.userName = userName;
			this.password = password;
			this.userfullName = userfullName;
			this.userEmail = userEmail;
		}

		Action<Action> enqueueAction;
		Action<AsyncProgress> updateProgress;
		string repoPath;

		string origin;

		string currentBranch;

		string userName;

		string password;

		string userfullName;

		string userEmail;

		public Task Run ()
		{
			return Task.Run (() => {
				RunSync ();
			});
		}

		public void RunSync ()
		{
			{
				using (var repo = new Repository (repoPath)) {
					//update branches tracking
					Remote remote = repo.Network.Remotes [origin];
					var branch = repo.Branches [currentBranch];
					repo.Branches.Update (branch, b => b.Remote = remote.Name, b => b.UpstreamBranch = branch.CanonicalName);
					PullOptions options = new PullOptions ();
					options.FetchOptions = new FetchOptions ();
					options.FetchOptions.CredentialsProvider = new CredentialsHandler (
						(url, usernameFromUrl, types) =>
					new UsernamePasswordCredentials () {
							Username = userName,
							Password = password
						});
					options.FetchOptions.OnTransferProgress += (TransferProgress p) => {
						enqueueAction (() => {
							updateProgress (new AsyncProgress ("Working hard...", p.ReceivedObjects / (float)p.TotalObjects));
						});
						return true;
					};
					options.MergeOptions = new MergeOptions ();
					options.MergeOptions.FileConflictStrategy = CheckoutFileConflictStrategy.Theirs;
					try {
						Commands.Pull (repo, new Signature (userfullName, userEmail, new DateTimeOffset (DateTime.Now)), options);
					} catch (MergeFetchHeadNotFoundException e) {
						Debug.LogError (e);
						throw new Exception ("This branch is not a remote branch. Nothing to pull.");
					}
				}
				enqueueAction (() => {
					Debug.Log ("Pulled changes");
				});
			}
		}
	}

}