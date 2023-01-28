using System;
using UnityEngine;
using UnityEditor;

 //prevents from unintended Config usage outside Editor

namespace GitForArtists
{

	public class Config
	{
		static string USERNAME_KEY = string.Format ("GIT_FOR_ARTISTS/userName");
		static string PASSWORD_KEY = string.Format ("GIT_FOR_ARTISTS/pw");
		static string EMAIL_KEY = string.Format ("GIT_FOR_ARTISTS/email");
		static string MAIN_BRANCH_KEY = string.Format ("GIT_FOR_ARTISTS/mainBranch");
		static string ORIGIN_KEY = string.Format ("GIT_FOR_ARTISTS/origin");

		string userName;
		string origin;
		string mainBranch;

		public string MainBranch {
			get {
				return mainBranch;
			}
		}

		public string Origin {
			get {
				return origin;
			}
		}

		public string UserName {
			get {
				return userName;
			}
		}

		string password;

		public string Password {
			get {
				return password;
			}
		}

		string userEmail;

		public string UserEmail {
			get {
				return userEmail;
			}
		}


		private Config ()
		{
		
		}

		public static void Save (string userName, string password, string userEmail, string origin, string mainBranch)
		{
			PlayerPrefs.SetString (USERNAME_KEY, userName);
			PlayerPrefs.SetString (PASSWORD_KEY, password);
			PlayerPrefs.SetString (EMAIL_KEY, userEmail);
			PlayerPrefs.SetString (ORIGIN_KEY, origin);
			PlayerPrefs.SetString (MAIN_BRANCH_KEY, mainBranch);
			PlayerPrefs.Save ();
		}

		public static Config Get ()
		{
			var cfg = new Config ();
			cfg.userName = PlayerPrefs.GetString (USERNAME_KEY);
			cfg.password = PlayerPrefs.GetString (PASSWORD_KEY);
			cfg.userEmail = PlayerPrefs.GetString (EMAIL_KEY);
			cfg.origin = PlayerPrefs.GetString (ORIGIN_KEY);
			cfg.mainBranch = PlayerPrefs.GetString (MAIN_BRANCH_KEY);
			return cfg;
		}

		public static void Clear ()
		{
			PlayerPrefs.DeleteKey (USERNAME_KEY);
			PlayerPrefs.DeleteKey (PASSWORD_KEY);
			PlayerPrefs.DeleteKey (EMAIL_KEY);
			PlayerPrefs.DeleteKey (ORIGIN_KEY);
			PlayerPrefs.DeleteKey (MAIN_BRANCH_KEY);
		}
	}

}