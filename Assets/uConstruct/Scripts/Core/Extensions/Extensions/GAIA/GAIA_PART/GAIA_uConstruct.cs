#if GAIA_PRESENT

using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using System.IO;

using uConstruct;
using uConstruct.Core.AOI;
using uConstruct.Demo;
using uConstruct.Core.Manager;
using uConstruct.Sockets;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia.GX.EEProductions
{
    /// <summary>
    /// Simple camera and light FX for Gaia.
    /// 
    /// Many thanks to Josh Savage for help and coaching on the setup and settings for this.
    /// If you need an awesome level designer then look him up!
    /// </summary>
    public class uConstruct_GAIAExtension : MonoBehaviour
    {
        #region Generic informational methods

        /// <summary>
        /// Returns the publisher name if provided. 
        /// This will override the publisher name in the namespace ie Gaia.GX.PublisherName
        /// </summary>
        /// <returns>Publisher name</returns>
        public static string GetPublisherName()
        {
            return "EE Productions";
        }

        /// <summary>
        /// Returns the package name if provided
        /// This will override the package name in the class name ie public class PackageName.
        /// </summary>
        /// <returns>Package name</returns>
        public static string GetPackageName()
        {
            return "uConstruct";
        }

        #endregion

        #region Methods exposed by Gaia as buttons must be prefixed with GX_

        public static void GX_ReadMe()
        {
#if UNITY_EDITOR
            SerializedObject layersManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = layersManager.FindProperty("layers");

            //check if layers exists
            bool buildingLayerExists = checkIfLayerExists(layers, LayersData.DefaultBuildingLayerString);
            bool socketLayerExists = checkIfLayerExists(layers, LayersData.DefaultSocketLayerString);


            EditorUtility.DisplayDialog("ReadMe", string.Format("This is the uConstruct extension. \nBefore using uConstruct make sure to check the tutorials on youtube and read the documentation. \n \n Layers Created : \n Building Layer({0}) : {1} \n Socket Layer({2}) : {3}", LayersData.DefaultBuildingLayerString, buildingLayerExists, LayersData.DefaultSocketLayerString, socketLayerExists), "Close");
#endif
        }

#if UNITY_EDITOR
        public static void GX_CreateDefaultLayers()
        {
            SerializedObject layersManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = layersManager.FindProperty("layers");

            //check if layers exists
            bool buildingLayerExists = checkIfLayerExists(layers, LayersData.DefaultBuildingLayerString);
            bool socketLayerExists = checkIfLayerExists(layers, LayersData.DefaultSocketLayerString);

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
            int emptyIndex = getEmptyLayerIndex(layers);

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

        static int getEmptyLayerIndex(SerializedProperty layers)
        {
            for (int i = 8; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue.ToLower() == "")
                    return i;
            }

            return -1;
        }

        static bool checkIfLayerExists(SerializedProperty layers, string Layer)
        {
            bool found = false;

            for (int i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue.ToLower() == Layer.ToLower())
                    found = true;
            }

            return found;
        }
#endif

        /// <summary>
        /// Add uConstruct to the scene, it will ignore it if its already created.
        /// </summary>
        public static void GX_InitiateManagers()
        {
            //Set uConstruct
            try
            {
                UCCallbacksManager.CreateAndInitialize(); // This will create our uConstruct manager in the scene, and if there is already one, it would just skip the process.
            }
            catch (Exception)
            {
                Debug.LogError("Applying uConstruct Manager Failed.");
            }
        }

        /// <summary>
        /// Add uConstruct demo into the scene.
        /// </summary>
        public static void GX_ApplyDemoToSelectedPlayer()
        {
#if UNITY_EDITOR

            GameObject player = Selection.activeGameObject;

            if (player == null)
            {
                Debug.LogError("No player is chosen, ABORT.");
                return;
            }
            else
            {
                try
                {
                    GameObject demoPlayerPrefab = Resources.Load<GameObject>("Players/DemoPlayer");

                    if (demoPlayerPrefab != null)
                    {
                        BuildingPlacer placer = player.GetComponent<BuildingPlacer>();
                        BaseAOIFinder finder = player.GetComponent<BaseAOIFinder>();
                        BuildingPlacer demoPlacer = demoPlayerPrefab.GetComponent<BuildingPlacer>();

                        if (demoPlacer == null)
                        {
                            Debug.LogError("Cant find building placer on CubesDemo player, Buildings cannot be automatically assigned");
                        }

                        if (placer == null)
                        {
                            placer = player.AddComponent<BuildingPlacer>();
                        }

                        if (finder == null)
                        {
                            finder = player.AddComponent<BaseAOIFinder>();
                        }

                        if (demoPlacer != null)
                        {
                            placer.buildings = demoPlacer.buildings;
                            placer.playerCamera = player.GetComponentInChildren<Camera>();
                        }
                    }
                    else
                    {
                        Debug.LogError("Cant find demo player on CubesDemo, Buildings cannot be automatically assigned");
                    }

                }
                catch (Exception)
                {
                    Debug.LogError("Applying demo scripts failed.");
                }
            }

            Terrain[] terrains = Terrain.activeTerrains;
            Terrain terrain;

            BaseSocket socket;

            for (int i = 0; i < terrains.Length; i++)
            {
                terrain = terrains[i];

                if (terrain.GetComponent<BaseSocket>() == null)
                {
                    socket = terrain.gameObject.AddComponent<BaseSocket>();
                    socket.receiveType = (BuildingType)Enum.Parse(typeof(BuildingType), "Foundation", true);

                    socket.placingType = PlacingRestrictionType.FreelyBased;
                    socket.usePhysics = false;
                }
            }

            if (GameObject.FindObjectOfType<DemoUI>() == null)
            {
                GameObject canvas = Resources.Load<GameObject>("Canvas");
                var canvasGO = GameObject.Instantiate(canvas);
                canvasGO.name = canvas.name; // remove the "Clone" thingy from the name
            }

#endif
        }


        #endregion

#region Helper methods

        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns></returns>
        private static string GetAssetPath(string name)
        {
#if UNITY_EDITOR
            string[] assets = AssetDatabase.FindAssets(name, null);
            if (assets.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(assets[0]);
            }
#endif
            return null;
        }

        /// <summary>
        /// Get the asset prefab if we can find it in the project
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject GetAssetPrefab(string name)
        {
#if UNITY_EDITOR
            string[] assets = AssetDatabase.FindAssets(name, null);
            for (int idx = 0; idx < assets.Length; idx++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assets[idx]);
                if (path.Contains(".prefab"))
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
            }
#endif
            return null;
        }

        /// <summary>
        /// Get the range from the terrain or return a default range if no terrain
        /// </summary>
        /// <returns></returns>
        public static float GetRangeFromTerrain()
        {
            Terrain terrain = GetActiveTerrain();
            if (terrain != null)
            {
                return Mathf.Max(terrain.terrainData.size.x, terrain.terrainData.size.z) / 2f;
            }
            return 1024f;
        }

        /// <summary>
        /// Get the currently active terrain - or any terrain
        /// </summary>
        /// <returns>A terrain if there is one</returns>
        public static Terrain GetActiveTerrain()
        {
            //Grab active terrain if we can
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null && terrain.isActiveAndEnabled)
            {
                return terrain;
            }

            //Then check rest of terrains
            for (int idx = 0; idx < Terrain.activeTerrains.Length; idx++)
            {
                terrain = Terrain.activeTerrains[idx];
                if (terrain != null && terrain.isActiveAndEnabled)
                {
                    return terrain;
                }
            }
            return null;
        }

        #endregion
    }
}

#endif