using UnityEngine;
using UnityEditor;
using System.Collections;

namespace uConstruct
{
    [CustomEditor(typeof(PreviewBuilding))]
    public class PreviewBuildingEditor : Editor
    {
        PreviewBuilding script;

        public override void OnInspectorGUI()
        {
            if(script == null)
            {
                script = (PreviewBuilding)target;
            }

            if (UC_EditorUtility.DisplayScriptField(this))
            {
                return;
            }

            if(GUILayout.Button("Apply Changes To Prefab"))
            {
                if(EditorUtility.DisplayDialog("Saving changes", "Are u sure you want to save changes to prefab ?", "Yes", "No"))
                {
                    script.ApplyChangesToPrefab(script.previewPrefab);
                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                }
            }

            if(GUILayout.Button("Get Scale From Socket"))
            {
                if (EditorUtility.DisplayDialog("Changing Scale", "Are u sure you want to grab scale from socket ?", "Yes", "No"))
                {
                    script.FitToLocalSpace();
                }
            }
        }

    }
}