using UnityEngine;
using UnityEditor;
using System.Collections;

namespace uConstruct.Core.PrefabDatabase
{
    [CustomEditor(typeof(PrefabDB))]
    public class PrefabDBCustomEditor : Editor
    {
        PrefabDB instance;

        public override void OnInspectorGUI()
        {
            if(instance == null)
            {
                instance = (PrefabDB)target;
            }

            EditorGUILayout.LabelField("Available Prefabs :");
            GUILayout.Space(12);

            for (int i = 0; i < instance.prefabs.Count; i++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(instance.prefabs[i].go.name + " : " + instance.prefabs[i].ID))
                {
                    EditorGUIUtility.PingObject(instance.prefabs[i].go);
                }
                if(GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    PrefabDB.instance.RemoveFromDB(instance.prefabs[i].go);
                }

                GUILayout.EndHorizontal();
            }
        }

    }
}