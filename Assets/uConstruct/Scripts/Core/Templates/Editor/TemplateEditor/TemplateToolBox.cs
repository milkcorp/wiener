using UnityEditor;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace uConstruct.Core.Templates
{
    public partial class TemplateCreaterEditor : EditorWindow
    {
        void DrawToolBox()
        {
            GUILayout.BeginHorizontal("Box", GUILayout.Height(20), GUILayout.ExpandHeight(true));
            EditorGUILayout.LabelField("Edit Templates");

            GUILayout.Space(10);
            
            if(GUILayout.Button(onCreationTemplate == null ? "Create A New Template" : "Delete In Creation Template"))
            {
                if (onCreationTemplate == null)
                    onCreationTemplate = new TemplateCreationData(buildingEditor.script.transform);
                else
                    onCreationTemplate = null;
            }
            if(GUILayout.Button("Add Existing Template"))
            {
                var selectionIndex = GetWindow<TemplateSelectionWindow>();
                selectionIndex.Init(building);
            }

            GUILayout.EndHorizontal();
        }
    }
}