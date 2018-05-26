using UnityEngine;
using UnityEditor;
using System.Collections;

namespace uConstruct
{

    public class LayersEditor : EditorWindow
    {
        public static LayersData layersData;
        public const string DATA_PATH = "Data/" + "LayersData";

        GUIStyle boxStyle;
        bool removeButtonClicked;

        bool showBuildingLayers;
        bool showSocketLayers;

        [MenuItem("Window/UConstruct/LayersManager")]
        public static void OpenWindow()
        {
            var instance = EditorWindow.CreateInstance<LayersEditor>();
            instance.Show();
            instance.Init();
        }

        void Init()
        {
            LoadResources();
        }

        void Update()
        {
            this.Repaint();
        }

        void LoadResources()
        {
            layersData = Resources.Load<LayersData>(DATA_PATH);

            if(layersData == null)
            {
                layersData = CreateInstance<LayersData>();
                AssetDatabase.CreateAsset(layersData, uConstruct.Core.Manager.UCCallbacksManager.ProjectPath + "Resources/" + DATA_PATH + ".asset");

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        void OnGUI()
        {
            removeButtonClicked = Event.current.control && string.IsNullOrEmpty(GUI.GetNameOfFocusedControl());

            if(layersData == null)
            {
                LoadResources();
            }

            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.alignment = TextAnchor.UpperLeft;
                boxStyle.fontStyle = FontStyle.Bold;
                boxStyle.normal.textColor = Color.white;
            }

            #region BuildingLayers

            showBuildingLayers = EditorGUILayout.Foldout(showBuildingLayers, "Show BuildingLayers");

            if (showBuildingLayers)
            {
                EditorGUILayout.BeginVertical(boxStyle);

                for (int layerIndex = 0; layerIndex < layersData._buildingLayers.Count; layerIndex++)
                {
                    EditorGUILayout.BeginHorizontal();

                    layersData._buildingLayers[layerIndex] = EditorGUILayout.TextField(layersData._buildingLayers[layerIndex]);

                    if (removeButtonClicked)
                    {
                        if (GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            if (EditorUtility.DisplayDialog("Remove Property", string.Format("Are u sure you want to remove {0} permanently ?", layersData._buildingLayers[layerIndex]), "Yes", "No"))
                            {
                                layersData._buildingLayers.Remove(layersData._buildingLayers[layerIndex]);
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                layersData.defaultBuildingLayer = EditorGUILayout.Popup("Default Building Layer :", layersData.defaultBuildingLayer, layersData._buildingLayers.ToArray());

                if(GUILayout.Button("Add"))
                {
                    layersData._buildingLayers.Add("New Layer");
                }

                EditorGUILayout.EndVertical();
            }

            #endregion

            #region SocketLayers

            showSocketLayers = EditorGUILayout.Foldout(showSocketLayers, "Show SocketLayers");

            if (showSocketLayers)
            {
                EditorGUILayout.BeginVertical(boxStyle);

                for (int layerIndex = 0; layerIndex < layersData._socketLayers.Count; layerIndex++)
                {
                    EditorGUILayout.BeginHorizontal();

                    layersData._socketLayers[layerIndex] = EditorGUILayout.TextField(layersData._socketLayers[layerIndex]);

                    if (removeButtonClicked)
                    {
                        if (GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            if (EditorUtility.DisplayDialog("Remove Property", string.Format("Are u sure you want to remove {0} permanently ?", layersData._socketLayers[layerIndex]), "Yes", "No"))
                            {
                                layersData._socketLayers.Remove(layersData._socketLayers[layerIndex]);
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                layersData.defaultSocketLayer = EditorGUILayout.Popup("Default Socket Layer :", layersData.defaultSocketLayer, layersData._socketLayers.ToArray());

                if (GUILayout.Button("Add"))
                {
                    layersData._socketLayers.Add("New Layer");
                }

                EditorGUILayout.EndVertical();
            }

            #endregion

            if(GUILayout.Button("Save Changes"))
            {
                EditorUtility.SetDirty(layersData);
                AssetDatabase.SaveAssets();
            }
        }
    }

}
