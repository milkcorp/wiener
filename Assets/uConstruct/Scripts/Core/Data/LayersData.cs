using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace uConstruct
{

    /// <summary>
    /// A class that containes information about custom layers data of the asset.
    /// this data is used when initiating layers assigning and so on.
    /// </summary>
    public class LayersData : ScriptableObject
    {
        [HideInInspector]
        public List<string> _socketLayers = new List<string>() { "BuildingSocket" };

        [HideInInspector]
        public List<string> _buildingLayers = new List<string>() { "Building" };

        [HideInInspector]
        public int defaultSocketLayer;

        [HideInInspector]
        public int defaultBuildingLayer;

        public const string FILE_PATH = "Data/" + "LayersData";

        static LayersData _instance;
        public static LayersData instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Resources.Load<LayersData>(FILE_PATH);
                }

                return _instance;
            }
        }

        static int _socketMask = -999;
        /// <summary>
        /// A mask that aims for all of the allowed socket layers. ( used for raycasts ).
        /// </summary>
        public static int SocketMask
        {
            get
            {
                if (_socketMask == -999)
                {
                    string currentLayer;
                    _socketMask = new LayerMask();

                    for (int Layerindex = 0; Layerindex < instance._socketLayers.Count; Layerindex++)
                    {
                        currentLayer = instance._socketLayers[Layerindex];

                        _socketMask |= 1 << LayerMask.NameToLayer(currentLayer);
                    }
                }

                return _socketMask;
            }
        }

        static int _buildingMask = -999; 
        /// <summary>
        /// A mask that aims for all of the allowed building layers. ( used for raycasts ).
        /// </summary>
        public static int BuildingMask
        {
            get
            {
                if (_buildingMask == -999)
                {
                    _buildingMask = new LayerMask();

                    string currentLayer;

                    for (int Layerindex = 0; Layerindex < instance._buildingLayers.Count; Layerindex++)
                    {
                        currentLayer = instance._buildingLayers[Layerindex];

                        _buildingMask |= 1 << LayerMask.NameToLayer(currentLayer);
                    }
                }

                return _buildingMask;
            }
        }
        /// <summary>
        /// All the layers that can be used for Sockets
        /// </summary>
        public static List<string> SocketLayers
        {
            get { return instance._socketLayers; }
        }
        /// <summary>
        /// All the layers that can be used for buildings
        /// </summary>
        public static List<string> BuildingLayers
        {
            get { return instance._buildingLayers; }
        }
        /// <summary>
        /// The default building layer that will be assigned to a building that doesnt have a layer that is contained inside the building layers list
        /// </summary>
        public static int DefaultBuildingLayer
        {
            get { return LayerMask.NameToLayer(BuildingLayers[instance.defaultBuildingLayer]); }
        }
        /// <summary>
        /// The default socket layer that will be assigned to a building that doesnt have a layher that is contained inside the socket layers list
        /// </summary>
        public static int DefaultSocketLayer
        {
            get { return LayerMask.NameToLayer(SocketLayers[instance.defaultSocketLayer]); }
        }
        /// <summary>
        /// The default building layer that will be assigned to a building that doesnt have a layer that is contained inside the building layers list
        /// </summary>
        public static string DefaultBuildingLayerString
        {
            get { return BuildingLayers[instance.defaultBuildingLayer]; }
        }
        /// <summary>
        /// The default socket layer that will be assigned to a building that doesnt have a layher that is contained inside the socket layers list
        /// </summary>
        public static string DefaultSocketLayerString
        {
            get { return SocketLayers[instance.defaultSocketLayer]; }
        }
    }

}