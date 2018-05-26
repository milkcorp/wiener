using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using uConstruct.Core.Saving;
using System.IO;

namespace uConstruct.Demo
{
    public class LevelEditorUI : MonoBehaviour
    {
        public static LevelEditorUI instance;

        public GameObject saveSlotPrefab;
        public Transform saveSlotsParent;

        public Text slotCreationText;

        public CanvasGroup mainCanvasGroup;
        public CanvasGroup CreateSaveGroup;

        void Awake()
        {
            instance = this;
            UCSavingManager.enabled = false;
            FetchLevels();
        }

        public virtual string[] GetLevels()
        {
            string[] files;

            #if UNITY_WEBPLAYER
            files = Directory.GetFiles(UCSavingManager.folderPath, "*." + UCSavingManager.fileFormat);
            #else
            files = Directory.GetFiles(UCSavingManager.folderPath, "*." + UCSavingManager.fileFormat, SearchOption.AllDirectories);
            #endif

            return files;
        }

        public virtual void FetchLevels()
        {
            for(int i = 0 ; i < LevelEditorSaveSlot.slots.Count; i++)
            {
                Destroy(LevelEditorSaveSlot.slots[i].gameObject);
            }
            LevelEditorSaveSlot.slots.Clear();

            var levels = GetLevels();

            for(int i = 0; i < levels.Length; i++)
            {
                LevelEditorSaveSlot.CreateInstance(saveSlotsParent, saveSlotPrefab, Path.GetFileNameWithoutExtension(levels[i]), levels[i]);
            }
        }

        public virtual void CreateSave()
        {
            if (slotCreationText.text == "") return;

            UCSavingManager.enabled = true;

            UCSavingManager.fileName = slotCreationText.text;
            UCSavingManager.Save();

            LevelEditorController.canMove = true;

            EnableCreate(false);
            EnablePanel(false);
        }

        public virtual void LoadSave(LevelEditorSaveSlot slot)
        {
            if (slot == null) return;

            UCSavingManager.enabled = true;

            UCSavingManager.fileName = slot.saveText.text;
            UCSavingManager.Load();

            LevelEditorController.canMove = true;

            EnableCreate(false);
            EnablePanel(false);
        }

        public void EnablePanel(bool value)
        {
            ManageGroup(mainCanvasGroup, value);
        }
        
        public void EnableCreate(bool value)
        {
            ManageGroup(CreateSaveGroup, value);
        }

        public void ManageGroup(CanvasGroup group, bool value)
        {
            group.alpha = value == true ? 1 : 0;
            group.interactable = value;
            group.blocksRaycasts = value;
        }

    }
}
