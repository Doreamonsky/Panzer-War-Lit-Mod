using ParadoxNotion.Design;
using NodeCanvas.Framework;
using NodeCanvas.DialogueTrees;
using ParadoxNotion;

namespace NodeCanvas.Tasks.Actions
{

    [Category("Dialogue")]
    [Description("You can use a variable inline with the text by using brackets likeso: [myVarName] or [Global/myVarName].\nThe bracket will be replaced with the variable value ToString")]
    [ParadoxNotion.Design.Icon("Dialogue")]
    public class Say : ActionTask<IDialogueActor>
    {

        public Statement statement = new Statement("This is a dialogue text...");

        protected override string info {
            get { return string.Format("<i>' {0} '</i>", ( statement.text.CapLength(30) )); }
        }

        protected override void OnExecute() {
            var tempStatement = statement.BlackboardReplace(blackboard);
            DialogueTree.RequestSubtitles(new SubtitlesRequestInfo(agent, tempStatement, EndAction));
        }
    }
}