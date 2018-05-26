using UnityEngine;
using UnityEditor;

using System.Collections;
using uConstruct;

namespace uConstruct.Sockets
{
    [CustomEditor(typeof(BaseSnapPoint))]
    public class SnapPointEditor : Editor
    {
        BaseSnapPoint script;

        public override void OnInspectorGUI()
        {
            if (UC_EditorUtility.DisplayScriptField(this))
            {
                return;
            }

            if (script == null)
                script = (BaseSnapPoint)target;

            script.receiveType = (BuildingType)EditorGUILayout.EnumMaskField("Receive :", script.receiveType);

            if(GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }

}
