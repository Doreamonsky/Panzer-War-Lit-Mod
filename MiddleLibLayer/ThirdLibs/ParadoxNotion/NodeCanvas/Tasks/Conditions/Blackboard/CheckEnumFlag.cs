using System;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Design;

[Category("✫ Blackboard")]
public class CheckEnumFlag : ConditionTask
{
    [BlackboardOnly]
    [RequiredField]
    public readonly BBObjectParameter Variable = new BBObjectParameter(typeof(Enum));

    public readonly BBObjectParameter Flag = new BBObjectParameter(typeof(Enum));

    protected override string info => $"{Variable} has {Flag} flag";

    protected override bool OnCheck() => ((Enum)Variable.value).HasFlag((Enum)Flag.value);

#if UNITY_EDITOR

    protected override void OnTaskInspectorGUI()
    {
        DrawDefaultInspector();

        if (Flag.varType != Variable.refType) Flag.SetType(Variable.refType);
    }

#endif
}