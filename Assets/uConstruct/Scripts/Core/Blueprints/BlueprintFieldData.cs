using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace uConstruct.Core.Blueprints
{
    /// <summary>
    /// A sub-child of the blueprint fields.
    /// Using this, the system can have many fields for each one of the actual blueprint field.
    /// So a wall could have many types of walls for instance.
    /// </summary>
    [System.Serializable]
    public class BlueprintFieldData
    {
        public string name;

        /// <summary>
        /// Unserialized data that is being serialized by a custom method as unity's scriptable object doesnt work well with
        /// inherited classes.
        /// </summary>
        public List<BlueprintData> data = new List<BlueprintData>();

        /// <summary>
        /// Loads data from a certain GO.
        /// </summary>
        /// <param name="target">our targeted GO</param>
        public void Pack(GameObject target)
        {
            if (target == null) return;

            data.Clear();

            name = target.name;

            var items = target.GetComponentsInChildren<IBlueprintItem>(true);
            items = items.OrderBy(x => x.priority).ToArray();

            BlueprintData currentData;

            for (int i = 0; i < items.Length; i++)
            {
                currentData = items[i].Pack();

                if (currentData == null) continue;

                data.Add(currentData);
            }
        }

        /// <summary>
        /// Unpack data into a gameobject.
        /// </summary>
        /// <param name="target">Our targeted gameobject</param>
        /// <param name="saveToPrefab">Save the changes into the prefab, if available.</param>
        public void UnPack(GameObject target, bool saveToPrefab)
        {
            if (data.Count == 0 || target == null) return;

            GameObject instantiatedTarget;

            #if UNITY_EDITOR
            var type = PrefabUtility.GetPrefabType(target);

            instantiatedTarget = type == PrefabType.ModelPrefab || type == PrefabType.Prefab ? GameObject.Instantiate<GameObject>(target) : target;
            #else
            instantiatedTarget = target;
            #endif

            instantiatedTarget = HandlePivot(instantiatedTarget);

            for (int i = 0; i < data.Count; i++)
            {
                data[i].UnPack(instantiatedTarget);
            }

            #if UNITY_EDITOR
            if (!Application.isPlaying && saveToPrefab)
            {
                PrefabType prefabType = PrefabUtility.GetPrefabType(target);

                if (prefabType != PrefabType.Prefab && prefabType != PrefabType.PrefabInstance)
                {
                    Debug.LogWarning("uConstruct Blueprints : Could not save changes to prefab because the target isnt a prefab.");
                    return;
                }
                else
                {
                    PrefabUtility.ReplacePrefab(instantiatedTarget, target, ReplacePrefabOptions.ConnectToPrefab | ReplacePrefabOptions.ReplaceNameBased);
                    AssetDatabase.SaveAssets();

                    GameObject.DestroyImmediate(instantiatedTarget);

                    BaseBuilding buildingInstance = target.GetComponent<BaseBuilding>();

                    if (buildingInstance != null && buildingInstance.prefabID == -1)
                    {
                        buildingInstance.prefabID = uConstruct.Core.PrefabDatabase.PrefabDB.instance.AddToDB(buildingInstance.gameObject);
                    }
                }
            }
            #endif
        }

        /// <summary>
        /// Handle wrongly placed pivots.
        /// </summary>
        /// <param name="go">what object to fix?</param>
        /// <returns>the fixed result</returns>
        GameObject HandlePivot(GameObject go)
        {
            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>(true);

            if (renderers.Length == 0) return go;

            MeshRenderer renderer = renderers[0];

            if (Vector3.Distance(go.transform.position, renderer.bounds.center) < 0.1f) return go;

            float distanceX = renderer.bounds.center.x - go.transform.position.x;
            float distanceY = renderer.bounds.center.y - go.transform.position.y;
            float distanceZ = renderer.bounds.center.z - go.transform.position.z;

            Vector3 resultPosition = go.transform.position - new Vector3(distanceX, distanceY, distanceZ);

            GameObject parent = new GameObject(go.name);
            parent.transform.position = go.transform.position;

            go.transform.parent = parent.transform;
            go.transform.position = resultPosition;

            return parent;
        }
    }
}
