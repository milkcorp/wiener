
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace uConstruct.CodeGenerator
{

    public class BuildingTypesCodeGeneratorEditor : EditorWindow
    {
        public static List<string> data;
        public static BuildingTypesCodeGeneratorEditor instance;

        GUIStyle boxStyle;
        Vector2 scrollPos;

        public static bool removeButtonClicked;

        [MenuItem("Window/UConstruct/CodeGenerator", priority = 1)]
        public static void OpenWindow()
        {
            var instance = GetWindow<BuildingTypesCodeGeneratorEditor>();
            instance.autoRepaintOnSceneChange = true;
            instance.maxSize = new Vector2(500, 300);
            instance.Show();

            BuildingTypesCodeGeneratorEditor.instance = instance;

#if !UC_Free
            data = BuildingTypesCodeGenerator.LoadEnumData();
#endif
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnUnityCompiledScripts()
        {
            if (instance != null)
                instance.Repaint();
        }

        void Update()
        {
            this.Repaint();
        }

        void OnGUI()
        {
#if !UC_Free
            removeButtonClicked = Event.current.control && string.IsNullOrEmpty(GUI.GetNameOfFocusedControl());

            if (data == null)
            {
                data = BuildingTypesCodeGenerator.LoadEnumData();
            }

            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.alignment = TextAnchor.UpperLeft;
                boxStyle.fontStyle = FontStyle.Bold;
                boxStyle.normal.textColor = Color.white;
            }

            EditorGUILayout.BeginVertical(boxStyle);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            for (int i = 0; i < data.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                data[i] = EditorGUILayout.TextField("Enum Field Name :", data[i]);

                if (removeButtonClicked)
                {
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        if (EditorUtility.DisplayDialog("Remove Enum Property", string.Format("Are u sure you want to remove {0} permanently ?", data[i]), "Yes", "No"))
                        {
                            data.Remove(data[i]);
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(boxStyle);
                EditorGUILayout.LabelField("", GUILayout.Height(0.5f));
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Create Enum Field"))
            {
                data.Add("Field : " + (data.Count + 1));
            }

            if (GUILayout.Button("Compile Assembly"))
            {
                BuildingTypesCodeGenerator.CompileAssembly(data);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

#else
            EditorGUILayout.LabelField("uConstruct's Code Generator is not available in uConstruct Free.");
#endif
        }

    }

}