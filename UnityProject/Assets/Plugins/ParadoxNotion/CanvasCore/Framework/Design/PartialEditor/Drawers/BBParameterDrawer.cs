#if UNITY_EDITOR

using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using ParadoxNotion;

namespace NodeCanvas.Editor
{

    public class BBParameterDrawer : ObjectDrawer<BBParameter>
    {
        public override BBParameter OnGUI(GUIContent content, BBParameter instance) {
            var required = fieldInfo.RTIsDefined<RequiredFieldAttribute>(true);
            var bbOnly = fieldInfo.RTIsDefined<BlackboardOnlyAttribute>(true);
            instance = BBParameterEditor.ParameterField(content, instance, bbOnly, required, info);
            return instance;
        }
    }
}

#endif