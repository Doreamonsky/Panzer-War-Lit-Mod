using System;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using LibGit2Sharp;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace GitForArtists
{
	
	class FilesTreeView : TreeView
	{
		static Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D> () {
			{ "", EditorGUIUtility.FindTexture ("TextAsset Icon") },
			{ ".prefab", EditorGUIUtility.FindTexture ("Prefab Icon") },
			{ ".unity", EditorGUIUtility.FindTexture ("UnityLogo") },
			{ ".png", EditorGUIUtility.FindTexture ("PreTextureRGB") },
			{ ".jpg", EditorGUIUtility.FindTexture ("PreTextureRGB") },
			{ ".bmp", EditorGUIUtility.FindTexture ("PreTextureRGB") },
			{ ".fbx", EditorGUIUtility.FindTexture ("Transform Icon") },
			{ ".obj", EditorGUIUtility.FindTexture ("Transform Icon") },
		};

		List<StatusEntry> files = new List<StatusEntry> ();

		public FilesTreeView (TreeViewState treeViewState, MultiColumnHeader multiColumnHeader)
			: base (treeViewState, multiColumnHeader)
		{
			Reload ();
		}

		public void SetData (List<StatusEntry> files)
		{
			this.files = files;
			Reload ();
		}

		protected override void RowGUI (RowGUIArgs args)
		{
			var row = (TreeEntry)args.item;
			var ext = Path.GetExtension (row.path);
			Texture2D icon = null;
			if (icons.ContainsKey (ext)) {
				icon = icons [ext];
			} else {
				icon = icons [""];
			}
			GUI.DrawTexture (args.GetCellRect (0), icon, ScaleMode.ScaleToFit);
			var style = new GUIStyle ();
			if (row.state == FileStatus.DeletedFromIndex || row.state == FileStatus.DeletedFromWorkdir) {
				style.normal.textColor = new Color (204 / 255f, 0, 0);
			} else if (row.state == FileStatus.NewInIndex || row.state == FileStatus.NewInWorkdir) {
				style.normal.textColor = new Color (0, 110 / 255f, 0);
			} else {
				style.normal.textColor = new Color (0, 0, 204 / 255f);
			}
			EditorGUI.LabelField (args.GetCellRect (1), row.path, style);
		}

		protected override TreeViewItem BuildRoot ()
		{
			var root = new TreeEntry{ id = 0, depth = -1, path = "Root" };
			var items = files.Select ((f, i) => new {f, i}).ToList ()
						.ConvertAll (e => new TreeEntry {
				id = e.i,
				depth = 0,
				path = e.f.FilePath,
				state = e.f.State
			})
					.ConvertAll (e => (TreeViewItem)e);
			//Debug.Log ("Updating merge tree " + items.Count);
			SetupParentsAndChildrenFromDepths (root, items);
		
			return root;
		}

		public static MultiColumnHeader CreateHeader ()
		{
			MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[] {
				new MultiColumnHeaderState.Column () {
					headerContent = new GUIContent ("Type"),
					width = 40,
					minWidth = 40,
					maxWidth = 40,
					canSort = false
				},
				new MultiColumnHeaderState.Column () {
					headerContent = new GUIContent ("Path"),
					width = 300,
					minWidth = 300,
					canSort = false
				}
			};
			var headerState = new MultiColumnHeaderState (columns);
			return new MultiColumnHeader (headerState);
		}
	}
}
