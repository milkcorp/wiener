using UnityEditor;
using UnityEngine;

using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace uConstruct.Core.Templates
{
    public partial class TemplateCreaterEditor : EditorWindow
    {
        BuildingEditor buildingEditor;
        BaseBuilding building;
        
        void Update()
        {
            this.Repaint();
        }

        public void Init(BuildingEditor editorWindow)
        {
            minSize = new Vector2(300, 600);
            building = editorWindow.script;

            this.buildingEditor = editorWindow;
        }

        void OnGUI()
        {
            DrawToolBox();

            GUILayout.BeginHorizontal();
            DrawTemplatesView();
            DrawCreateWindow();
            GUILayout.EndHorizontal();
        }
    }

    class TemplateMenuEditor : EditorWindow
    {

        [MenuItem("Window/UConstruct/Update Templates")]
        public static void Open()
        {
            var folders = Directory.GetFiles(@"Assets", "*.prefab", SearchOption.AllDirectories);
            BaseBuilding building;

            for (int folderIndex = 0; folderIndex < folders.Length; folderIndex++)
            {
                building = AssetDatabase.LoadAssetAtPath(folders[folderIndex], typeof(BaseBuilding)) as BaseBuilding;

                if (building != null)
                {
                    UpdateBuilding(building);
                }

                float progress = Mathf.Clamp01((float)folderIndex / (float)folders.Length);
                EditorUtility.DisplayProgressBar("Templates Manager", "Update Templates : " + (int)(progress * 100), progress);
            }

            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();
            Debug.Log("uConstruct succesfully updated templates");

        }

        static void UpdateBuilding(BaseBuilding building)
        {
            if (building.templates.Count > 0) // if we have templates
            {
                Template template;
                GameObject templatePrefab;

                GameObject buildingInstance = Instantiate<GameObject>(building.gameObject);
                BaseBuilding buildingComponent = buildingInstance.GetComponent<BaseBuilding>();

                for (int i = 0; i < buildingComponent.templates.Count; i++)
                {
                    template = buildingComponent.templates[i];
                    templatePrefab = Resources.Load<GameObject>(TemplateUtility.RESOURCES_PATH + template.name);

                    buildingComponent.RemoveTemplate(template);
                    buildingComponent.AddTemplate(templatePrefab);
                }

                PrefabUtility.ReplacePrefab(buildingInstance, building.gameObject, ReplacePrefabOptions.ReplaceNameBased);

                DestroyImmediate(buildingInstance);
            }
        }

    }

}