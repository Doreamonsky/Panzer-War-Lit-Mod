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
using UnityEngine.Assertions;
using UnityEditor.IMGUI.Controls;

namespace GitForArtists
{
    public class GitWindow : EditorWindow
    {
        string LOGO_PATH = "Assets/Git-for-artists/Editor/Icons/git-logo.png";
        string ICON_PATH = "Assets/Git-for-artists/Editor/Icons/git-icon.png";
        string repoPath = "../";
        static string origin = "origin";
        const string mainBranchName = "master";
        string mainBranchNameRemote = "";
        string userEmail = "";
        string userName = "";
        string password = "";
        List<StatusEntry> alteredFiles = new List<StatusEntry>();
        string currentBranch;
        Texture2D logo;
        List<string> allBranches = new List<string>();
        string newBrachName = "";
        private string comment = "";
        bool showConfig = true;
        bool blockUI = false;
        AsyncProgress progress = null;
        object _lock = new object();
        private Queue<Action> actionsQueue = new Queue<Action>();

        [SerializeField] TreeViewState treeViewState;
        FilesTreeView filesTreeView;

        [MenuItem("Tools/Git for artists")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(GitWindow));
        }

        void Update()
        {
            if (allBranches.Count == 0)
            {
                Debug.LogError("No branches present.");
                return;
            }

            lock (_lock)
            {
                while (actionsQueue.Count > 0)
                {
                    Action action = actionsQueue.Dequeue();
                    action();
                }
            }
        }

        void OnEnable()
        {
            logo = AssetDatabase.LoadMainAssetAtPath(LOGO_PATH) as Texture2D;
            var icon = AssetDatabase.LoadMainAssetAtPath(ICON_PATH) as Texture2D;
            titleContent = new GUIContent("Git for artists", icon);
            using (var repo = new Repository(repoPath))
            {
                UpdateBranches(repoPath);
                currentBranch = repo.Head.FriendlyName;
            }

            if (treeViewState == null)
                treeViewState = new TreeViewState();

            var multiHeader = FilesTreeView.CreateHeader();
            filesTreeView = new FilesTreeView(treeViewState, multiHeader);
        }

        void OnGUI()
        {
            DrawLogo();
            bool invalidConfig = String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(password) || String.IsNullOrEmpty(origin)
                                 || String.IsNullOrEmpty(mainBranchName);
            bool isOnMainBranch = currentBranch == mainBranchName;
            EditorGUI.BeginDisabledGroup(blockUI && allBranches.Count > 0 || invalidConfig);
            DrawBranchesActions();
            if (isOnMainBranch && !invalidConfig)
            {
                var style = new GUIStyle();
                style.normal.textColor = Color.red;
                style.fontSize = 12;
                EditorGUILayout.LabelField("无法编辑主分支，请切换分支或创建新分支 - Can't edit main branch. Change or create new one", style);
            }

            DrawMainActions();

            EditorGUI.BeginDisabledGroup(isOnMainBranch);
            EditorGUILayout.Separator();
            DrawPushActions();

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();
            DrawConfig(invalidConfig);
            EditorGUILayout.Separator();
            EditorGUI.BeginDisabledGroup(blockUI && allBranches.Count > 0 || invalidConfig);
            DrawChangedFiles();
            EditorGUI.EndDisabledGroup();
            DrawProgressBar();
            mainBranchNameRemote = string.Format("{0}/{1}", origin, mainBranchName);
        }

        void DrawBranchesActions()
        {
            Row(() => { Col(() => { EditorGUILayout.LabelField("当前分支 - Branch: "); }); });
            Row(() =>
            {
                Col(() =>
                {
                    var index = EditorGUILayout.Popup(allBranches.FindIndex(b => b == currentBranch), allBranches.ToArray());
                    var selectedBranch = allBranches.ElementAt(index);
                    if (currentBranch != selectedBranch)
                    {
                        Debug.Log("Changing branch to " + selectedBranch);
                        OnSelectBranch(selectedBranch);
                    }
                });
                Col(() => { newBrachName = EditorGUILayout.TextField(newBrachName); });
                Col(() =>
                {
                    var content = new GUIContent("创建分支 - Create new",
                        "This is equivalent of 'git checkout -b [branch]' from main branch. Creates new branch from main.");
                    EditorGUI.BeginDisabledGroup(String.IsNullOrEmpty(newBrachName));
                    RenderButton(content, OnCreateBranch);
                    EditorGUI.EndDisabledGroup();
                });
            });
        }

        void DrawMainActions()
        {
            Row(() =>
            {
                var tooltip = "This is equivalent of `git pull -s theirs`. Downloads from server and merges all changes done to this branch.";
                Col(() =>
                {
                    var content = new GUIContent($"下载本分支({currentBranch})最新内容 - Update this branch({currentBranch})", tooltip);
                    EditorGUILayout.LabelField(content);
                });
                Col(() => { RenderButton(new GUIContent("下载并覆盖 - Update", tooltip), OnPullChanges); });
            });
        }

        void DrawPushActions()
        {
            Row(() =>
            {
                Col(() =>
                {
                    var content = new GUIContent("修改内容注释", "阐述本次修改了哪些内容");
                    EditorGUILayout.LabelField(content);
                });
                Col(() => { comment = EditorGUILayout.TextField(comment); });
            });

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(comment));

            Row(() =>
            {
                var tooltip = "This is equivalent of 'git add . | git commit  | git push origin/[branch]'";
                Col(() =>
                {
                    var content = new GUIContent("上传修改内容到服务器 - Push your changes to server ", tooltip);
                    EditorGUILayout.LabelField(content);
                });
                Col(() => { RenderButton(new GUIContent("上传 - Push", tooltip), OnPushChanges); });
            });

            EditorGUI.EndDisabledGroup();
        }

        void DrawLogo()
        {
            if (logo != null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.background = logo;
                EditorGUI.LabelField(new Rect(5, 5, 50, 20), GUIContent.none, style);
                //TODO - may be changed by some const height
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
        }

        void DrawChangedFiles()
        {
            Row(() =>
            {
                Col(() => { EditorGUILayout.LabelField("本地修改文件列表 - Changed files"); });
                Col(() =>
                {
                    var content = new GUIContent("刷新 - Refresh", "Refreshes changed files.");
                    RenderButton(content, OnUpdateChangedFiles);
                });
            });
            EditorGUILayout.Separator();
            var currControlRect = EditorGUILayout.GetControlRect();
            var treeHeight = 400;
            filesTreeView.OnGUI(new Rect(0, currControlRect.y, position.width, treeHeight));
        }

        void DrawProgressBar()
        {
            if (progress != null)
            {
                var height = 30;
                EditorGUI.ProgressBar(new Rect(5, position.height - height, EditorGUIUtility.currentViewWidth, height), progress.value, progress.info);
            }
        }

        void DrawConfig(bool credentialsEmpty)
        {
            if (credentialsEmpty)
            {
                var style = new GUIStyle();
                style.normal.textColor = Color.red;
                style.fontSize = 12;
                EditorGUILayout.LabelField("Invalid config", style);
            }

            showConfig = EditorGUILayout.Foldout(showConfig, "账号配置 - Config");
            if (showConfig)
            {
                Row(() =>
                {
                    EditorGUILayout.Space();
                    Col(() => { EditorGUILayout.LabelField("用户名 - Username:"); });
                    Col(() => { userName = EditorGUILayout.TextField(userName); });
                });
                Row(() =>
                {
                    EditorGUILayout.Space();
                    Col(() => { EditorGUILayout.LabelField("密码 - Password:"); });
                    Col(() => { password = EditorGUILayout.PasswordField(password); });
                });
                Row(() =>
                {
                    EditorGUILayout.Space();
                    Col(() => { EditorGUILayout.LabelField("E-mail:"); });
                    Col(() => { userEmail = EditorGUILayout.TextField(userEmail); });
                });
                EditorGUILayout.Separator();
                // Row(() =>
                // {
                //     EditorGUILayout.Space();
                //     Col(() => { EditorGUILayout.LabelField("Main branch: "); });
                //     Col(() => { mainBranchName = EditorGUILayout.TextField(mainBranchName); });
                // });
                Row(() =>
                {
                    EditorGUILayout.Space();
                    Col(() => { EditorGUILayout.LabelField("源 - Origin: "); });
                    Col(() => { origin = EditorGUILayout.TextField(origin); });
                });
                Row(() =>
                {
                    EditorGUILayout.Space();
                    Col(() => { EditorGUILayout.LabelField("主分支 - Main server  branch: "); });
                    Col(() => { EditorGUILayout.SelectableLabel(mainBranchNameRemote); });
                });
                Row(() =>
                {
                    RenderButton("加载配置 - Load", OnConfigLoad);
                    RenderButton("保存配置 - Save", OnConfigSave);
                });
            }
        }

        void Row(Action fn)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(15));
            fn();
            EditorGUILayout.EndHorizontal();
        }

        void Col(Action fn)
        {
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            fn();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }

        void RenderButton(string label, Action onClick, params GUILayoutOption[] options)
        {
            RenderButton(new GUIContent(label), onClick, options);
        }

        void RenderButton(GUIContent content, Action onClick, params GUILayoutOption[] options)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fixedWidth = 250;
            style.fixedHeight = 20;
            if (GUILayout.Button(content, style, options))
            {
                onClick();
            }
        }

        public void AddAction(Action action)
        {
            lock (_lock)
            {
                actionsQueue.Enqueue(action);
            }
        }

        void OnConfigLoad()
        {
            var cfg = Config.Get();
            userName = cfg.UserName;
            password = cfg.Password;
            userEmail = cfg.UserEmail;
            origin = cfg.Origin;
            // mainBranchName = cfg.MainBranch;
        }

        void OnConfigSave()
        {
            Config.Save(userName, password, userEmail, origin, mainBranchName);
            Debug.Log("Config saved");
        }

        void OnRefreshBranches()
        {
            RunAsyncFlow(new AsyncTask[]
                {
                    new UpdateOriginBranchesTask(AddAction, UpdateProgress, repoPath, origin, userName, password)
                })
                .ContinueWith(t =>
                {
                    AddAction(() =>
                    {
                        Debug.Log("Repository updated");
                        using (var repo = new Repository(repoPath))
                        {
                            UpdateBranches(repoPath);
                            currentBranch = repo.Head.FriendlyName;
                        }
                    });
                });
        }

        void UpdateBranches(string repoPath)
        {
            //Debug.Log ("Updating UI branches");
            using (var repo = new Repository(repoPath))
            {
                allBranches = repo.Branches.Where(b => !b.IsRemote).Select(b => b.FriendlyName).ToList();
            }
        }

        //doesnt need locking cause i run it only from this process (IT CANT BE RUN DIRECTLY)
        private void UpdateProgress(AsyncProgress p)
        {
            progress = p;
        }

        void OnSelectBranch(string selectedBranch)
        {
            RunAsyncFlow(new AsyncTask[]
            {
                new ChangeBranchTask(AddAction, OnChangedBranch, repoPath, selectedBranch)
            });
        }

        void OnPullChanges()
        {
            RunAsyncFlow(new AsyncTask[]
            {
                new PullTask(AddAction, UpdateProgress, repoPath, origin, currentBranch, userName, password, userName, userEmail)
            });
        }

        void OnPushChanges()
        {
            RunAsyncFlow(new AsyncTask[]
            {
                new PushTask(AddAction, UpdateProgress, repoPath, alteredFiles, userName, userEmail, origin, userName, password, currentBranch, comment)
            });
        }

        void OnCreateBranch()
        {
            RunAsyncFlow(new AsyncTask[]
            {
                new UpdateOriginBranchesTask(AddAction, UpdateProgress, repoPath, origin, userName, password),
                new CreateBranchTask(OnCreatedBranch, $"pr/{newBrachName}", repoPath, mainBranchNameRemote, AddAction)
            });
        }

        void OnCreatedBranch(string branchCreatedName)
        {
            UpdateBranches(repoPath);
            OnChangedBranch(branchCreatedName);
        }

        void OnChangedBranch(string branchCreatedName)
        {
            using (var repo = new Repository(repoPath))
            {
                Assert.IsNotNull(repo.Branches[branchCreatedName], branchCreatedName + " is not a repository branch.");
                currentBranch = branchCreatedName;
                newBrachName = "";
            }
        }

        void OnUpdateChangedFiles()
        {
            if (!blockUI)
            {
                using (var repo = new Repository(repoPath))
                {
                    var files = repo.RetrieveStatus(new StatusOptions()).Where(file => !repo.Ignore.IsPathIgnored(file.FilePath));
                    var altered = files.Except(files.Where(file => file.State == FileStatus.Unaltered));
                    filesTreeView.SetData(altered.ToList());
                    alteredFiles = altered.ToList();
                    var currBranch = repo.Head;
                    var serverBranch = currBranch.TrackedBranch;
                    if (serverBranch != null)
                    {
                        if (serverBranch.Tip == null)
                        {
                            Debug.LogError("This branch has no remote equivalent. Push your branch.");
                        }
                        else
                        {
                            var diff = repo.ObjectDatabase.CalculateHistoryDivergence(repo.Head.Tip, serverBranch.Tip);
                            if (diff.AheadBy != null && diff.AheadBy > 0)
                            {
                                Debug.LogWarning("This branch has unpushed changes.");
                            }
                        }
                    }
                }
            }
        }

        Task RunAsyncFlow(AsyncTask[] tasks)
        {
            blockUI = true;
            OnUpdateChangedFiles();
            UpdateProgress(new AsyncProgress("Working hard...", 0));
            TaskRunner runner = new TaskRunner();
            return runner.Run(tasks)
                .ContinueWith(OnTaskError, TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(OnTaskFinished);
        }

        void OnTaskError(Task t)
        {
            AddAction(() =>
            {
                var exc = t.Exception != null ? t.Exception.InnerException : t.Exception;
                Debug.LogError("Task failed: " + exc);
            });
        }

        void OnTaskFinished(Task t)
        {
            AddAction(() =>
            {
                //Debug.Log ("Task finished, unblocking ui");
                blockUI = false;
                progress = null;
                OnUpdateChangedFiles();
            });
        }

        public void UndoFile(string path)
        {
            //Undo to origin/main branch

            using (var repo = new Repository(repoPath))
            {
                var ish = repo.Branches[mainBranchNameRemote].Tip.Sha;
                Debug.Log("Undoing file " + path + " to main branch, SHA :" + ish);
                CheckoutOptions options = new CheckoutOptions();
                options.CheckoutModifiers = CheckoutModifiers.Force;
                List<string> paths = new List<string>();
                paths.Add(path);
                repo.CheckoutPaths(ish, paths, options);
            }
        }

        [MenuItem("Assets/Git for artists/Undo")]
        private static void UndoFile()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var window = EditorWindow.GetWindow<GitWindow>(typeof(GitWindow));
            window.UndoFile(path);
        }
    }
}