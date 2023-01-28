using System.Collections;
using System.Collections.Generic;
using System;
using LibGit2Sharp;
using System.Linq;
using LibGit2Sharp.Handlers;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;

namespace GitForArtists
{

	public class PushTask : AsyncTask
	{
	
		public PushTask (Action<Action> enqueueAction, Action<AsyncProgress> updateProgress, string repoPath, List<StatusEntry> alteredFiles, string userfullName, string userEmail, string origin, string userName, string password, string currentBranchName,string comment)
		{
			this.updateProgress = updateProgress;
			this.enqueueAction = enqueueAction;
			this.repoPath = repoPath;
			this.alteredFiles = alteredFiles;
			this.userfullName = userfullName;
			this.userEmail = userEmail;
			this.origin = origin;
			this.userName = userName;
			this.password = password;
			this.currentBranchName = currentBranchName;
			this.comment = comment;
		}



		Action<AsyncProgress> updateProgress;

		Action<Action> enqueueAction;
		string repoPath;

		List<StatusEntry> alteredFiles;

		string userfullName;

		string userEmail;

		string origin;

		string userName;

		string password;

		string currentBranchName;

		private string comment;

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
					if (alteredFiles.Count > 0) {
						Commands.Stage (repo, "*");
						string message = comment;
						//commit
						Signature author = new Signature (userfullName, userEmail, DateTime.Now);
						repo.Commit (message, author, author);
					}
					Remote remote = repo.Network.Remotes [origin];
					//set branch upstream
					var currentBranch = repo.Branches [currentBranchName];
					repo.Branches.Update (currentBranch, b => b.Remote = remote.Name, b => b.UpstreamBranch = currentBranch.CanonicalName);
					//push
					PushOptions options = new PushOptions ();
					options.CredentialsProvider = new CredentialsHandler (
						(url, usernameFromUrl, types) =>
					new UsernamePasswordCredentials () {
							Username = userName,
							Password = password
						});
					options.OnPushTransferProgress += (current, total, bytes) => {
						enqueueAction (() => {
							updateProgress (new AsyncProgress ("Working hard...", current / (float)total));
							Debug.Log (string.Format ("Progress: {0}/{1}, MB: {2}", current, total, bytes / (1024f * 1024f)));
						});
						return true;
					};
					// Force push the new commit
					//string pushRefSpec = string.Format ("+{0}:{0}", repo.Head.CanonicalName);
					enqueueAction (() => {
						Debug.Log ("Pushing changes..");
					});
					repo.Network.Push (currentBranch, options);
					enqueueAction (() => {
						Debug.Log ("Pushed changes");
					});
				}

			}
		}
	}
}