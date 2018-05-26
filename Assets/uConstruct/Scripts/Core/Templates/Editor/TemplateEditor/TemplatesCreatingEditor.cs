using UnityEditor;
using UnityEngine;

using System.Linq;

using System.Collections;
using System.Collections.Generic;

namespace uConstruct.Core.Templates
{
    public partial class TemplateCreaterEditor : EditorWindow
    {
        TemplateCreationData onCreationTemplate = null;
        Vector2 creationScroll = new Vector2();

        void DrawCreateWindow()
        {
            GUILayout.BeginVertical("Box", GUILayout.ExpandHeight(true));

            if(onCreationTemplate == null)
            {
                EditorGUILayout.LabelField("No template is being created");
            }
            else
            {
                onCreationTemplate.name = EditorGUILayout.TextField("Template Name :", onCreationTemplate.name);
                DrawSelectionList();

                if(GUILayout.Button(onCreationTemplate.editedTemplate == null ? "Create":"Save Changes"))
                {
                    if (EditorUtility.DisplayDialog("Add To Templates", "Do u wish to add this template to the building templates ?", "Yes", "No"))
                    {
                        TemplateUtility.GenerateTemplate(onCreationTemplate.name, building, onCreationTemplate.ReturnTemplates(), true);
                    }
                    else
                    {
                        TemplateUtility.GenerateTemplate(onCreationTemplate.name, building, onCreationTemplate.ReturnTemplates(), false);
                    }

                    if (onCreationTemplate.editedTemplate != null) building.RemoveTemplate(onCreationTemplate.editedTemplate);
                    onCreationTemplate = null;
                    return;
                }
            }

            GUILayout.EndVertical();
        }

        void DrawSelectionList()
        {
            GUILayout.Space(12);

            creationScroll = GUILayout.BeginScrollView(creationScroll, GUILayout.ExpandHeight(true));
            TemplateObjectSelection templateSelection;

            onCreationTemplate.markAll = EditorGUILayout.Toggle("Mark All : ", onCreationTemplate.markAll);

            for(int i = 0; i < onCreationTemplate.templateObjects.Count; i++)
            {
                templateSelection = onCreationTemplate.templateObjects[i];

                templateSelection.chosen = EditorGUILayout.ToggleLeft(templateSelection.templateObject.GetTransform().name, templateSelection.chosen);
            }

            GUILayout.EndScrollView();
        }

    }

    public class TemplateCreationData
    {
        public string name;
        public List<TemplateObjectSelection> templateObjects = new List<TemplateObjectSelection>();
        public Template editedTemplate;

        bool _markAll;
        public bool markAll
        {
            get
            {
                return _markAll;
            }
            set
            {
                if(value != _markAll)
                {
                    _markAll = value;

                    for(int i = 0; i < templateObjects.Count; i++)
                    {
                        templateObjects[i].chosen = value;
                    }
                }
            }
        }

        public ITemplateObject[] ReturnTemplates()
        {
            List<ITemplateObject> templates = new List<ITemplateObject>();

            for(int i = 0; i < templateObjects.Count; i++)
            {
                if(templateObjects[i].chosen)
                    templates.Add(templateObjects[i].templateObject);
            }

            return templates.ToArray();
        }

        public TemplateCreationData(Transform transform)
        {
            name = "New Template";

            ITemplateObject template;
            var templateComponents = transform.GetComponentsInChildren<ITemplateObject>(true);

            for (int i = 0; i < templateComponents.Length; i++)
            {
                template = templateComponents[i];

                templateObjects.Add(new TemplateObjectSelection() { templateObject = template });
            }
        }
        public TemplateCreationData(Template copy, Transform root, string Name)
        {
            name = Name;
            editedTemplate = copy;

            ITemplateObject template;
            var templateComponents = root.GetComponentsInChildren<ITemplateObject>(true);
            var copyComponents = copy.GetComponentsInChildren<ITemplateObject>(true);

            for(int i = 0; i < templateComponents.Length; i++)
            {
                template = templateComponents[i];

                templateObjects.Add(new TemplateObjectSelection() { templateObject = template, chosen = copyComponents.Contains(template)});
            }
        }
    }

    public class TemplateObjectSelection
    {
        public ITemplateObject templateObject;
        public bool chosen;
    }

}