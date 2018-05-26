using UnityEngine;
using UnityEditor;
using System.Collections;
using uConstruct.Conditions;
using uConstruct;

[CanEditMultipleObjects]
[CustomEditor(typeof(CheckForBuildingCondition), true, isFallback=false)]
public class CheckForBuildingsEditor : Editor 
{
    CheckForBuildingCondition script;

    public override void OnInspectorGUI()
    {
        if(script == null)
        {
            script = (CheckForBuildingCondition)target;
        }

        if (UC_EditorUtility.DisplayScriptField(this))
        {
            return;
        }

        script.buildings = (BuildingType)EditorGUILayout.EnumMaskField("Target Buildings : ", script.buildings);
        script.distance = EditorGUILayout.FloatField("Distance :", script.distance);
        script.detectionMethod = (DetectionType)EditorGUILayout.EnumPopup("Detection Method :", script.detectionMethod);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
