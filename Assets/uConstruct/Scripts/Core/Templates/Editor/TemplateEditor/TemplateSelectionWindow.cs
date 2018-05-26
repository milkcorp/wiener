using UnityEditor;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace uConstruct.Core.Templates
{
    public class TemplateSelectionWindow : EditorWindow
    {
        GameObject _selectedTemplate;
        GameObject selectedTemplate
        {
            get { return _selectedTemplate; }
            set
            {
                if (value != _selectedTemplate)
                {
                    _selectedTemplate = value;
                    if (value.GetComponent<Template>() == null)
                    {
                        _selectedTemplate = null;
                    }
                }
            }
        }

        BaseBuilding building;

        public void Init(BaseBuilding building)
        {
            this.building = building;

            minSize = new Vector2(500, 100);
            maxSize = new Vector2(500, 100);
            maximized = false;
        }

        void OnGUI()
        {
            selectedTemplate = (GameObject)EditorGUILayout.ObjectField("Template :", selectedTemplate, typeof(GameObject), true);

            if (GUILayout.Button("Choose"))
            {
                if(selectedTemplate != null)
                {
                    building.AddTemplate(selectedTemplate);
                }
                Close();
                return;
            }
        }

    }
}