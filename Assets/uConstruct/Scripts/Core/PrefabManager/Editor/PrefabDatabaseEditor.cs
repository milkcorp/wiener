using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace uConstruct.Core.PrefabDatabase
{
    public class PrefabDatabaseEditor : EditorWindow
    {

        [MenuItem("Window/UConstruct/Initiate Prefabs Update")]
        public static void OpenWindow()
        {
            UpdateDB();
        }

        /// <summary>
        /// Update the prefab database
        /// </summary>
        static void UpdateDB()
        {
            PrefabDB.instance.ResetDB();

            var folders = Directory.GetFiles(@"Assets", "*.prefab", SearchOption.AllDirectories);
            BaseBuilding building;
            int currentID = 0;
            int id;

            for (int folderIndex = 0; folderIndex < folders.Length; folderIndex++)
            {
                building = AssetDatabase.LoadAssetAtPath(folders[folderIndex], typeof(BaseBuilding)) as BaseBuilding;

                if (building != null)
                {
                    id = building.prefabID == -1 ? currentID : building.prefabID;

                    if (PrefabDB.instance.Contains(id))
                        id = PrefabDB.instance.ReturnUID();

                    PrefabDB.instance.AddToDB(building.gameObject, id);
                    building.prefabID = id;

                    EditorUtility.SetDirty(building);
                    EditorUtility.SetDirty(building.gameObject);
                    currentID++;
                }

                float progress = Mathf.Clamp01((float)folderIndex / (float)folders.Length);
                EditorUtility.DisplayProgressBar("Prefab Database", "Compiling prefab database : " + (int)(progress * 100), progress);
            }


            EditorUtility.SetDirty(PrefabDB.instance);
            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();
            Debug.Log("uConstruct succesfully created prefabs IDs.");
        }
    }
}