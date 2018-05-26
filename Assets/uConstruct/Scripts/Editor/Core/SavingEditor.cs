using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using uConstruct.Core.Saving;

namespace uConstruct
{
    public class SavingEditor : Editor
    {

        [MenuItem("Window/UConstruct/Saving/ResetSave")]
        public static void ResetSave()
        {
            if (EditorUtility.DisplayDialog("Remove Save", "Are you sure you want to remove your save ?", "Yes", "No"))
            {
                if (File.Exists(UCSavingManager.dataPath))
                {
                    try
                    {
                        File.Delete(UCSavingManager.dataPath);

                        Debug.Log("Saving file was deleted sucesfully.");
                    }
                    catch (IOException ex)
                    {
                        Debug.Log("Deleting save error!! : " + ex.ToString());
                    }
                }
                else
                {
                    Debug.Log("Saving file does not exist.");
                }
            }
        }

    }
}
