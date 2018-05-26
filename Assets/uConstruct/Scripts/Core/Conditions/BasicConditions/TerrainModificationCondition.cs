using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using uConstruct.Core.Blueprints;
using uConstruct.Core.Saving;

namespace uConstruct.Conditions
{
    /// <summary>
    /// This condition is a built-in condition that will clean details around you on place.
    /// Should be used for stuff like foundations.
    /// </summary>
    public class TerrainModificationCondition : BaseCondition
    {
        class RestoreData
        {
            public Dictionary<int, int[,]> details = new Dictionary<int, int[,]>();
            public float[,] heights;
            public float[, ,] maps;

            public bool hasHeights
            {
                get { return heights != null; }
            }
        }

        #region TerrainModificationDataVariables
        static Dictionary<Terrain, TerrainData> globalDefaultData = new Dictionary<Terrain, TerrainData>();

        List<TerrainModificationData> savedDetailData = new List<TerrainModificationData>();

        Terrain terrain;
        int detailIndex;
        int xBase;
        int zBase;
        TerrainData terrainData;
        int[,] details;
        float[,] heights;
        Vector3 HalfScale;
        Vector3 terrainPoint;
        Vector3 normalizedPos;

        public bool assignTextureOnFlattenArea;
        public int flattenAreaTextureIndex;
        public float flattenAreaTextureStrength = 1;
        #endregion
        #region Variables
        static Dictionary<Terrain, RestoreData> restoreData = new Dictionary<Terrain, RestoreData>();
        static bool restored;

        public bool revertOnDestroy = true;

        public TerrainModificationType modificationType;

        public int xScale = 15;
        public int zScale = 15;

        public Vector3 offset = new Vector3();

        public bool isDetails
        {
            get
            {
                return modificationType == TerrainModificationType.ClearDetails;
            }
        }
        public bool isHeight
        {
            get
            {
                return !isDetails;
            }
        }
        public Vector3 position
        {
            get { return rootBuilding.transform.position + offset; }
        }
        #endregion

        public override bool DisableOnPlace
        {
            get
            {
                return false;
            }
        }
        public override bool CheckCondition()
        {
            return true;
        }

        public override void Awake()
        {
            base.Awake();
            rootBuilding.OnPlacedEvent += HandleTerrainModifications;
            rootBuilding.OnDeattachEvent += RestoreTerrainModifications;

            flattenAreaTextureStrength = Mathf.Clamp(flattenAreaTextureStrength, 0, 1);
        }
        void HandleTerrainModifications()
        {
            RaycastHit[] hits = Physics.RaycastAll(new Ray(transform.position, Vector3.down), 100);
            RaycastHit hit;

            for (int hitsIndex = 0; hitsIndex < hits.Length; hitsIndex++)
            {
                hit = hits[hitsIndex];
                terrain = hit.transform.GetComponent<Terrain>();

                if (terrain != null)
                {
                    terrainData = terrain.terrainData;

                    if(!globalDefaultData.ContainsKey(terrain))
                    {
                        globalDefaultData.Add(terrain, Object.Instantiate<TerrainData>(terrainData));
                    }

                    HalfScale = rootBuilding.transform.lossyScale / 2;

                    int resCorrection;

                    if(isDetails)
                        resCorrection = terrainData.detailResolution > 1024 ? Mathf.FloorToInt(terrainData.detailResolution / 600) : 0;
                    else
                        resCorrection = 0;

                    int actualXScale = Mathf.CeilToInt((xScale * HalfScale.x) / terrainData.size.x) + resCorrection;
                    int actualZScale = Mathf.CeilToInt((zScale * HalfScale.z) / terrainData.size.z) + resCorrection;

                    int xScale_Alpha = (actualXScale * terrainData.heightmapWidth) / terrainData.alphamapWidth;
                    int zScale_Alpha = (actualZScale * terrainData.heightmapHeight) / terrainData.alphamapHeight;

                    terrainPoint = position - HalfScale;

                    var terrainLocalPos = terrainPoint - terrain.transform.position;

                    normalizedPos = new Vector3(Mathf.InverseLerp(0, terrainData.size.x, terrainLocalPos.x),
                                                Mathf.InverseLerp(0, terrainData.size.y, terrainLocalPos.y),
                                                Mathf.InverseLerp(0, terrainData.size.z, terrainLocalPos.z));

                    xBase = (int)(normalizedPos.x * (isDetails ? terrainData.detailResolution : terrainData.heightmapWidth));
                    zBase = (int)(normalizedPos.z * (isDetails ? terrainData.detailResolution : terrainData.heightmapHeight));

                    int xBase_Alpha = 0, zBase_Alpha = 0;

                    if (assignTextureOnFlattenArea)
                    {
                        xBase_Alpha = (int)(normalizedPos.x * terrainData.alphamapWidth);
                        zBase_Alpha = (int)(normalizedPos.z * terrainData.alphamapHeight);
                    }

                    if (isDetails)
                    {
                        for (detailIndex = 0; detailIndex < terrainData.detailPrototypes.Length; detailIndex++)
                        {
                            AddModificationData(terrain, terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, detailIndex), detailIndex);

                            savedDetailData.Add(new TerrainModificationData(xBase, zBase, detailIndex, terrainData.GetDetailLayer(xBase, zBase, actualXScale, actualZScale, detailIndex), terrain)); // Apply details
                        }
                    }
                    else
                    {
                        AddModificationData(terrain, terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight), assignTextureOnFlattenArea ? terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight) : null);

                        savedDetailData.Add(new TerrainModificationData(xBase, zBase, xBase_Alpha, zBase_Alpha, terrainData.GetHeights(xBase, zBase, actualXScale, actualZScale), assignTextureOnFlattenArea ? terrainData.GetAlphamaps(xBase_Alpha, zBase_Alpha, xScale_Alpha, zScale_Alpha) : null, actualXScale, actualZScale, xScale_Alpha, zScale_Alpha, terrain)); //Apply heights & alphamaps
                    }

                    uConstruct.Core.Manager.UCCallbacksManager.instance.AddApplicationQuitAction(RevertModifications);

                    int[,] _details;
                    float[,] _heights;
                    float[,,] _maps;

                    TerrainModificationData data;
                    for (int i = 0; i < savedDetailData.Count; i++)
                    {
                        data = savedDetailData[i];

                        if (isDetails)
                        {
                            _details = (int[,])data.details.Clone();

                            for (int x = 0; x < actualXScale; x++)
                            {
                                for (int z = 0; z < actualZScale; z++)
                                {
                                    _details[x, z] = 0;
                                }
                            }

                            terrainData.SetDetailLayer(data.xIndex_Height, data.yIndex_Height, data.layer, _details);
                        }
                        else
                        {
                            _heights = (float[,])data.heights.Clone();

                            for (int z = 0; z < actualZScale; z++)
                            {
                                for (int x = 0; x < actualXScale; x++)
                                {
                                    _heights[x, z] = normalizedPos.y;
                                }
                            }

                            terrainData.SetHeightsDelayLOD(data.xIndex_Height, data.yIndex_Height, _heights);
                            terrain.ApplyDelayedHeightmapModification();


                            if(data.maps != null)
                            {
                                _maps = (float[,,])data.maps.Clone();

                                for (int z = 0; z < zScale_Alpha; z++)
                                {
                                    for (int x = 0; x < xScale_Alpha; x++)
                                    {
                                        for (int index = 0; index < terrainData.splatPrototypes.Length; index++)
                                        {
                                            _maps[x, z, index] = index == flattenAreaTextureIndex ? flattenAreaTextureStrength : 0;
                                        }
                                    }
                                }

                                terrainData.SetAlphamaps(data.xIndex_Alpha, data.yIndex_Alpha, _maps);
                            }
                        }
                    }

                    break;
                }
            }
             
             
        }

        /// <summary>
        /// Restore the terrain modification caused by this building only.
        /// </summary>
        void RestoreTerrainModifications()
        {
            if (terrain == null || !revertOnDestroy) return;

            TerrainModificationData detailData;

            if (isDetails)
            {
                for (int detailIndex = 0; detailIndex < savedDetailData.Count; detailIndex++)
                {
                    detailData = savedDetailData[detailIndex];

                    terrainData.SetDetailLayer(detailData.xIndex_Height, detailData.yIndex_Height, detailData.layer, detailData.details);
                }
            }
            else
            {
                for (int i = 0; i < savedDetailData.Count; i++)
                {
                    detailData = savedDetailData[detailIndex];

                    TerrainData tData;

                    if (globalDefaultData.TryGetValue(detailData.terrain, out tData))
                    {
                        terrainData.SetHeights(detailData.xIndex_Height, detailData.yIndex_Height, tData.GetHeights(detailData.xIndex_Height, detailData.yIndex_Height, detailData.xScale_Height, detailData.zScale_Height)); // Assign heights from default data.

                        if (detailData.maps != null)
                        {
                            terrainData.SetAlphamaps(detailData.xIndex_Alpha, detailData.yIndex_Alpha, detailData.originalMaps);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Add Heights To A Terrain
        /// </summary>
        /// <param name="terrain">Specific Terrain</param>
        /// <param name="heights">The heights of the terrain</param>
        static void AddModificationData(Terrain terrain, float[,] heights, float[,,] maps)
        {
            RestoreData currentData;

            if (!restoreData.ContainsKey(terrain))
            {
                currentData = new RestoreData();
                restoreData.Add(terrain, currentData);
            }

            currentData = restoreData[terrain];

            if (currentData.hasHeights) return;

            currentData.heights = heights;
            currentData.maps = maps;
        }

        /// <summary>
        /// Add Details Data To Terrain
        /// </summary>
        /// <param name="terrain">A specific terrain</param>
        /// <param name="details">terrain's details</param>
        /// <param name="layersIndex">detail's layer</param>
        static void AddModificationData(Terrain terrain, int[,] details, int layersIndex)
        {
            RestoreData currentData;

            if (!restoreData.ContainsKey(terrain))
            {
                currentData = new RestoreData();
                restoreData.Add(terrain, currentData);
            }

            currentData = restoreData[terrain];

            if (currentData.details.ContainsKey(layersIndex)) return;

            currentData.details.Add(layersIndex, details);
        }

        /// <summary>
        /// Revert the terrain modifications globally
        /// </summary>
        static void RevertModifications()
        {
            if (restored) return;
            else restored = true;

            Terrain currentTerrain;

            foreach (var currentData in restoreData)
            {
                currentTerrain = currentData.Key;

                if (currentData.Value.hasHeights)
                {
                    currentTerrain.terrainData.SetHeights(0, 0, currentData.Value.heights);
                }

                if(currentData.Value.maps != null)
                {
                    currentTerrain.terrainData.SetAlphamaps(0, 0, currentData.Value.maps);
                }

                foreach (var currentLayerIndex in currentData.Value.details)
                {
                    currentTerrain.terrainData.SetDetailLayer(0, 0, currentLayerIndex.Key, currentLayerIndex.Value);
                }
            }
        }

        public override void OnDrawGizmos()
        {
            Gizmos.DrawRay(transform.position, Vector3.down * 2);
        }

        public override BlueprintData Pack()
        {
            return new TerrainModification_BlueprintData(this);
        }

    }

    public class HeightsData
    {
        public float x;
        public float z;

        public float value;

        public HeightsData(float _x, float _z, float _value)
        {
            this.x = _x;
            this.z = _z;

            this.value = _value;
        }

        public static float[,] returnArray(List<HeightsData> list, int count)
        {
            float[,] array = new float[count, count];

            for (int x = 0; x < count; x++)
            {
                for (int z = 0; z < count; z++)
                {
                    if (x + z >= list.Count) continue;
                    array[x, z] = list[x + z].value;
                }
            }

            return array;
        }

    }

    public class TerrainModificationData
    {
        public int xIndex_Height;
        public int yIndex_Height;

        public int xIndex_Alpha;
        public int yIndex_Alpha;

        public int xScale_Height;
        public int zScale_Height;

        public int xScale_Alpha;
        public int zScale_Alpha;

        public int[,] details;
        public float[,] heights;
        public float[, ,] maps;
        public float[, ,] originalMaps;

        public int layer;
        public Terrain terrain;

        public TerrainModificationData()
        {
            xIndex_Height = -1;
            yIndex_Height = -1;

            layer = -1;

            xScale_Height = -1;
            zScale_Height = -1;

            terrain = null;
        }

        public TerrainModificationData(int _xIndex, int _yIndex, int _detailLayer, int[,] _details, Terrain _terrain)
        {
            this.xIndex_Height = _xIndex;
            this.yIndex_Height = _yIndex;

            this.details = _details;

            this.layer = _detailLayer;
            this.terrain = _terrain;
        }

        public TerrainModificationData(int _xIndex, int _yIndex, int _xIndex_Alpha, int _yIndex_Alpha, float[,] _heights, float [,,] _maps, int xScale_Height, int zScale_Height, int xScale_Alpha, int zScale_Alpha, Terrain _terrain)
        {
            this.xIndex_Height = _xIndex;
            this.yIndex_Height = _yIndex;

            this.xIndex_Alpha = _xIndex_Alpha;
            this.yIndex_Alpha = _yIndex_Alpha;

            this.xScale_Height = xScale_Height;
            this.zScale_Height = zScale_Height;

            this.xScale_Alpha = xScale_Alpha;
            this.zScale_Alpha = zScale_Alpha;

            this.heights = _heights;
            this.maps = _maps;
            
            if(maps != null)
            {
                originalMaps = (float[, ,])maps.Clone();
            }

            this.terrain = _terrain;
        }

    }

    public enum TerrainModificationType
    {
        ClearDetails,
        FlattenHeight
    }

    [System.Serializable]
    public class TerrainModification_BlueprintData : BlueprintData
    {
        public bool revertOnDestroy;

        public TerrainModificationType modificationType;

        public int xScale;
        public int zScale;

        public SerializeableVector3 offset;

        public bool assingAlphaMaps;
        public int alphaMap;
        public float alphaMapStrength;

        public TerrainModification_BlueprintData(TerrainModificationCondition condition)
        {
            this.name = condition.transform.name;

            this.position = condition.transform.localPosition;
            this.rotation = condition.transform.localRotation;
            this.scale = condition.transform.localScale;

            this.revertOnDestroy = condition.revertOnDestroy;
            this.modificationType = condition.modificationType;
            this.xScale = condition.xScale;
            this.zScale = condition.zScale;

            this.offset = condition.offset;

            this.assingAlphaMaps = condition.assignTextureOnFlattenArea;
            this.alphaMap = condition.flattenAreaTextureIndex;
            this.alphaMapStrength = condition.flattenAreaTextureStrength;
        }

        public override void UnPack(GameObject target)
        {
            BaseBuilding building = target.GetComponentInParent<BaseBuilding>();

            if (building != null)
            {
                TerrainModificationCondition condition = (TerrainModificationCondition)building.CreateCondition(name, SocketPositionAnchor.Center, typeof(TerrainModificationCondition));
                condition.transform.localPosition = (Vector3)position;
                condition.transform.localScale = (Vector3)scale;
                condition.transform.localRotation = (Quaternion)rotation;

                condition.revertOnDestroy = this.revertOnDestroy;
                condition.modificationType = this.modificationType;
                condition.xScale = this.xScale;
                condition.zScale = this.zScale;
                condition.offset = (Vector3)this.offset;

                condition.assignTextureOnFlattenArea = this.assingAlphaMaps;
                condition.flattenAreaTextureIndex = this.alphaMap;
                condition.flattenAreaTextureStrength = this.alphaMapStrength;
            }
        }
    }

}
