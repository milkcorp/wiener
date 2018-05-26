using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;
using uConstruct.Core.PrefabDatabase;

namespace uConstruct.Core.Saving
{
    [ExecuteInEditMode]
    public class SaveDrawer : MonoBehaviour
    {
        [SerializeField]
        List<BuildingGroupSaveData> groupSavingData = new List<BuildingGroupSaveData>();

        #if UNITY_EDITOR
        [MenuItem("Window/UConstruct/Visualize Save")]
        public static void EnableDraw()
        {
            UCSavingManager.renderVisualSave = !UCSavingManager.renderVisualSave;
        }
        #endif

        public void DrawSave(List<BuildingGroupSaveData> stashedSavedData)
        {
            this.groupSavingData = stashedSavedData;

            if (!Application.isPlaying)
            {
                BuildingGroupSaveData saveData = null;
                BuildingSaveData data;
                GameObject dataPrefab;

                for (int i = 0; i < groupSavingData.Count; i++)
                {
                    saveData = groupSavingData[i];

                    for (int b = 0; b < saveData.buildingsData.Count; b++)
                    {
                        data = saveData.buildingsData[b];
                        dataPrefab = PrefabDB.instance.GetGO(data.prefabID);

                        if (dataPrefab != null)
                        {
                            GameObject prefabInstance = GameObject.Instantiate(dataPrefab, (Vector3)data.pos, (Quaternion)data.rot) as GameObject;
                            prefabInstance.transform.parent = this.transform;
                        }
                    }
                }
            }
        }

        public void Awake()
        { 
            if(Application.isPlaying)
            {
                DestroyImmediate(this.gameObject);
            }
        }
    }

}