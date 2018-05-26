using UnityEngine;
using UnityEditor;

namespace uConstruct
{
    public class UConstructManager : EditorWindow
    {
        private Vector2 _scrollPos = Vector3.zero;

        private const string UConstructDefine = "UCONSTRUCT_PRESET";

        [MenuItem("Window/UConstruct/uConstruct Manager", false, -1)]
        public static void OpenWindow()
        {
            var instance = GetWindow<UConstructManager>();
            instance.autoRepaintOnSceneChange = true;
            instance.maxSize = new Vector2(500, 300);
            instance.titleContent = new GUIContent("uConstruct Manager");
            instance.Show();
        }

        [UnityEditor.Callbacks.DidReloadScripts()]
        public static void UpdateCompilingDefines()
        {
            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            if (!currentDefines.Contains(UConstructDefine))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currentDefines + ";" + UConstructDefine);
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical("uConstruct Manager: ", "Box");
            GUILayout.Space(20);
            EditorGUILayout.LabelField("uConstruct Manager provides you with tools to create basic mechanics.");
            GUILayout.EndVertical();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Helpers :");

            GUILayout.Space(10);

            if(GUILayout.Button(new GUIContent("1. ReadMe")))
            {
                ReadMeBtn();
            }
            if(GUILayout.Button(new GUIContent("2. Create Missing Layers")))
            {
                CreateMissingLayersBtn();
            }
            if(GUILayout.Button(new GUIContent("3. Open LayersManager")))
            {
                LayersEditor.OpenWindow();
            }
            if (GUILayout.Button(new GUIContent("4. Initiate Prefabs Update")))
            {
                Core.PrefabDatabase.PrefabDatabaseEditor.OpenWindow();
            }
            if(GUILayout.Button(new GUIContent("5. Open extensions manager")))
            {
                #if UCONSTRUCT_PRESET
                Extensions.ExtensionsEditor.Open();
                #endif
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        void ReadMeBtn()
        {
            SerializedObject layersManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = layersManager.FindProperty("layers");

            //check if layers exists
            bool buildingLayerExists = CheckIfLayerExists(layers, LayersData.DefaultBuildingLayerString);
            bool socketLayerExists = CheckIfLayerExists(layers, LayersData.DefaultSocketLayerString);


            EditorUtility.DisplayDialog("ReadMe", string.Format("Before using uConstruct make sure to check the tutorials on youtube and read the documentation. \n \n Layers Created : \n Building Layer({0}) : {1} \n Socket Layer({2}) : {3}", LayersData.DefaultBuildingLayerString, buildingLayerExists, LayersData.DefaultSocketLayerString, socketLayerExists), "Close");
        }

        void CreateMissingLayersBtn()
        {
            SerializedObject layersManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = layersManager.FindProperty("layers");

            //check if layers exists
            bool buildingLayerExists = CheckIfLayerExists(layers, LayersData.DefaultBuildingLayerString);
            bool socketLayerExists = CheckIfLayerExists(layers, LayersData.DefaultSocketLayerString);

            if (buildingLayerExists && socketLayerExists)
            {
                Debug.Log("Default layers are already created. aborting");
                return;
            }

            if (!buildingLayerExists)
            {
                bool success = CreateLayer(layersManager, layers, LayersData.DefaultBuildingLayerString);

                if (!success)
                {
                    Debug.LogError(string.Format("Creating layer {0} failed. Make sure you have enough space for new layers in your LayersManager", LayersData.DefaultBuildingLayerString));
                    return;
                }
            }
            if (!socketLayerExists)
            {
                bool success = CreateLayer(layersManager, layers, LayersData.DefaultSocketLayerString);

                if (!success)
                {
                    Debug.LogError(string.Format("Creating layer {0} failed. Make sure you have enough space for new layers in your LayersManager", LayersData.DefaultSocketLayerString));
                    return;
                }
            }

            Debug.Log("Layers created succesfully.");
        }

        static bool CreateLayer(SerializedObject layersManager, SerializedProperty layers, string layerName)
        {
            int emptyIndex = GetEmptyLayerIndex(layers);

            if (emptyIndex == -1)
                return false;

            SerializedProperty layer = layers.GetArrayElementAtIndex(emptyIndex);

            if (layer != null)
            {
                layer.stringValue = layerName;
                layersManager.ApplyModifiedProperties();
            }

            return true;
        }

        private static int GetEmptyLayerIndex(SerializedProperty layers)
        {
            for (int i = 8; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue.ToLower() == "")
                    return i;
            }

            return -1;
        }

        private static bool CheckIfLayerExists(SerializedProperty layers, string layer)
        {
            var found = false;

            for (var i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue.ToLower() == layer.ToLower())
                    found = true;
            }

            return found;
        }

    }
}
