using UnityEditor;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace uConstruct.Core.Templates
{
    public partial class TemplateCreaterEditor : EditorWindow
    {
        Vector2 scrollView = new Vector2();

        void DrawTemplatesView()
        {
            scrollView = GUILayout.BeginScrollView(scrollView, "Box", GUILayout.ExpandHeight(true), GUILayout.Width(300));
            Template template;

            EditorGUILayout.LabelField("Templates:");

            for (int i = 0; i < building.templates.Count; i++)
            {
                template = building.templates[i];

                if(template == null)
                {
                    building.templates.Remove(template);
                    return;
                }

                GUILayout.BeginHorizontal();
                if(GUILayout.Button(template.transform.name))
                {
                    onCreationTemplate = new TemplateCreationData(template, building.transform, template.name);
                }

                if(Event.current.control && string.IsNullOrEmpty(GUI.GetNameOfFocusedControl()))
                {
                    if(GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        if(EditorUtility.DisplayDialog("Removing Template", "Are u sure you wish to remove " + template.name + " ?", "Yes", "No"))
                        {
                            building.RemoveTemplate(template);
                            onCreationTemplate = null;
                        }
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}