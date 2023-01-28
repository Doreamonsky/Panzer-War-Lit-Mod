using System;
using UnityEditor.IMGUI.Controls;
using LibGit2Sharp;

namespace GitForArtists
{
	[Serializable]
	public class TreeEntry : TreeViewItem
	{
		public TreeEntry ()
		{
		}

		public TreeEntry (int id, int depth, string path, FileStatus state) : base (id, depth, path)
		{
			this.path = path;
			this.state = state;
		}

		public FileStatus state;
		public string path;
	}
}