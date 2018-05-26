using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using System.IO;

namespace uConstruct.Demo
{
    public class LevelEditorSaveSlot : MonoBehaviour
    {
        public static List<LevelEditorSaveSlot> slots = new List<LevelEditorSaveSlot>();

        public Text saveText;

        string savePath;

        public void Delete()
        {
            if(savePath != null && File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            slots.Remove(this);
            Destroy(this.gameObject);
        }

        public void Load()
        {
            if (saveText.name == "") return;

            LevelEditorUI.instance.LoadSave(this);
        }

        public static LevelEditorSaveSlot CreateInstance(Transform parent, GameObject slot, string name, string path)
        {
            GameObject instance = GameObject.Instantiate(slot);
            instance.transform.SetParent(parent);

            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;

            LevelEditorSaveSlot script = instance.GetComponent<LevelEditorSaveSlot>();

            if (script == null) return null;

            script.saveText.text = name;
            script.savePath = path;

            return script;
        }
    }
}
