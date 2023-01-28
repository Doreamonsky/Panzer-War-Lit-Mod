"Git for artists" by RepoGames

This is simple git client for letting artists to change the project without any git knowledge. 
No more time-consuming meetings with your artists typing git commands in terminal. 
Its standard "Git flow" approach throwing away all merge conflicts. 

-Initial setup: 
1) Ensure you got .gitignore file so you dont push files you should not push
e.g. /Temp. This may end up in Unity locking files errors. Sample can be copied from https://www.gitignore.io/api/unity .

2) After you download the "Git for artists" you need to merge this directory to your main remote branch.
	a) Open Menu Tools/Git-For-Artists, fill in config and textfield with 'gfa-init' and press 'Create new'.
	b) Press Push
	c) Merge your pushed branch to server main branch(e.g. via merge request).


Usages:
I. Adding a change:
	1) Create new branch
	2) Do project changes and Push

II. I commited too much files and I want to undo them.
	1) Select Project view
	2) Right click on asset -> Git-for-artists -> Undo. It will get origin/master version. 

III. I want to update the branches.
	1) Press Refresh
	2) Switch the branch
	3) Press Update

Prerequisities: 
-Unity 2017.2 +
-Set unity runtime to Experimental(.NET 4.6 Equivalent) : Edit->Project settings -> Player -> Configuration -> Scripting Runtime Version
-Project needs to be downloaded by HTTPS
-Local branches names must match remote names.
-Branch of the user should not be accessed by other people so he does not run into conflicts. Its only his branch.
-No merging at all. if you want to get changes from main branch just create new one.

License allows for one copy per computer.


FAQ:
-Error: 'Multiple plugins with the same name...' :
Enter Git-for-artists/Libs/LibGit2Sharp/runtimes and just remove the directories that your machine cannot run.
E.g. If you are using windows x64 leave only win7-x64 directory. 

-Error: "Too many redirect or authentication replays" or "401" or "403" means your credentials are invalid.

-How to merge my change to master?.
This is standard git flow. Merge your pushed branch to server main branch(e.g. via merge request).

It uses https://github.com/libgit2/libgit2sharp
__
Contact: tomasz.szepczynski@gmail.com