using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;
using System.IO;
using System.Linq;

namespace uConstruct.Core.Saving
{
    public delegate void SavingProcessComplete();
    public delegate void LoadingProcessComplete();

    /// <summary>
    /// This class handles all the saving management of the asset.
    /// </summary>
    public static class UCSavingManager
    {
        /// <summary>
        /// Full path to the current save
        /// </summary>
        public static string dataPath
        {
            get { return folderPath + "/" + fileName + "." + fileFormat; }
        }

        /// <summary>
        /// The folder path to the saves.
        /// </summary>
        public static string folderPath
        {
            get { return UCSettings.instance.UCSavingPathType == SavingPathType.Persistent ? Application.persistentDataPath : Application.dataPath; }
        }

        /// <summary>
        /// Saving file name (for example : saveData)
        /// 
        /// NOTE : when getting the fileName it will include the unique scene name (if enabled) so if you want to get the pure save name just access it through UCSettings.
        /// 
        /// </summary>
        public static string fileName
        {
            get
            {
                return UCSettings.instance.UCSavingFileName + (UCSettings.instance.UCSavingUniqueSceneSave ? " " + Application.loadedLevelName : "");
            }
            set
            {
                UCSettings.instance.UCSavingFileName = value;
            }
        }
        /// <summary>
        /// Saving file format (for example : bin)
        /// </summary>
        public static string fileFormat = "bin";

        /// <summary>
        /// Will uConstruct save your buildings
        /// </summary>
        public static bool enabled
        {
            get { return UCSettings.instance.UCSavingEnabled; }
            set { UCSettings.instance.UCSavingEnabled = value; }
        }

        static bool _isLoading;
        public static bool IsLoading
        {
            get { return _isLoading; }
        }

        static bool _renderVisualSave;
        public static bool renderVisualSave
        {
            get { return _renderVisualSave; }
            set
            {
                _renderVisualSave = value;

                SaveDrawer drawer = GameObject.FindObjectOfType<SaveDrawer>();

                if (drawer != null)
                    GameObject.DestroyImmediate(drawer.gameObject);

                if (value)
                {
                    List<BuildingGroupSaveData> saveData = new List<BuildingGroupSaveData>();
                    GameObject instance = new GameObject("Save Drawer");

                    drawer = instance.AddComponent<SaveDrawer>();

                    FileStream file;
                    BuildingGroupSaveData groupData;

                    using (file = File.Open(dataPath, FileMode.Open))
                    {
                        var data = DeserializeStream(file);

                        for (int i = 0; i < data.Length; i++)
                        {
                            groupData = data[i] as BuildingGroupSaveData;

                            if (groupData != null)
                            {
                                saveData.Add(groupData);
                            }
                        }
                    }

                    drawer.DrawSave(saveData);
                }
            }
        }

        public static event SavingProcessComplete OnSavingProcessComplete;
        public static event LoadingProcessComplete OnLoadingProcessComplete;

        /// <summary>
        /// Deserialize a given stream
        /// </summary>
        /// <param name="stream">our stream</param>
        /// <returns>the deserialized Stream</returns>
        public static BaseUCSaveData[] DeserializeStream(Stream stream)
        {
            BinaryFormatter bf = new BinaryFormatter();

            return bf.Deserialize(stream) as BaseUCSaveData[];
        }

        /// <summary>
        /// Load all data from files
        /// </summary>
        public static void Load()
        {
            if (!enabled) return;

            try
            {
                if (File.Exists(dataPath))
                {
                    _isLoading = true;

                    FileStream file;

                    using (file = File.Open(dataPath, FileMode.Open))
                    {
                        LoadExternalData(file);
                    }

                    Debug.Log("--- uConstruct Succesfully Loaded data ---");

                    _isLoading = false;
                }
                else
                {
                    Debug.LogWarning("--- uConstruct : File path does not exist. Loading FAILED. ---");
                }

                if (OnLoadingProcessComplete != null)
                    OnLoadingProcessComplete();
            }
            catch (FileLoadException ex)
            {
                Debug.Log("--- uConstruct : Saving file is corrupted, Removing saving file ---" + "/n" + ex.ToString());
                if (File.Exists(dataPath))
                {
                    File.Delete(dataPath);
                }

                _isLoading = false;
            }
        }

        /// <summary>
        /// Load an external data
        /// </summary>
        /// <param name="stream">our data.</param>
        public static void LoadExternalData(Stream stream)
        {
            BaseUCSaveData[] data;
            BaseUCSaveData currentData;

            using (stream)
            {
                data = DeserializeStream(stream);

                for (int i = 0; i < data.Length; i++)
                {
                    currentData = data[i];

                    currentData.Load(currentData);
                }
            }
        }

        /// <summary>
        /// Return all save data from the objects in the scene
        /// </summary>
        /// <returns>array of the saving data</returns>
        static List<BaseUCSaveData> ReturnSaveData()
        {
            MonoBehaviour[] worldObjects = GameObject.FindObjectsOfType<MonoBehaviour>();
            List<BaseUCSaveData> data = new List<BaseUCSaveData>();
            MonoBehaviour component;
            UCSavedItem[] saveInterfaces;
            BaseUCSaveData ucData;

            List<UCSavedItem> checkForDuplication = new List<UCSavedItem>();

            for (int worldIndex = 0; worldIndex < worldObjects.Length; worldIndex++)
            {
                component = worldObjects[worldIndex];
                saveInterfaces = component.GetComponents<UCSavedItem>();

                for (int i = 0; i < saveInterfaces.Length; i++)
                {
                    if (checkForDuplication.Contains(saveInterfaces[i])) continue;
                    checkForDuplication.Add(saveInterfaces[i]);

                    if (saveInterfaces[i] != null)
                    {
                        ucData = saveInterfaces[i].Save();

                        if (!data.Contains(ucData))
                            data.Add(saveInterfaces[i].Save());
                    }
                }
            }

            return data.OrderBy(x => x.priority).ToList();
        }

        /// <summary>
        /// Save all data into a file
        /// </summary>
        public static void Save()
        {
            if (!enabled) return;

            FileStream file;

            if (File.Exists(dataPath))
                File.Delete(dataPath);

            using (file = File.Open(dataPath, FileMode.OpenOrCreate))
            {
                BaseUCSaveData[] data;

                Serialize(file, out data);
                Debug.Log("--- uConstruct Succesfully Saved data! ---");
            }

            if (OnSavingProcessComplete != null)
                OnSavingProcessComplete();
        }

        /// <summary>
        /// Serialize our save data.
        /// </summary>
        /// <param name="stream">our stream</param>
        /// <param name="data">our data</param>
        /// <returns>result data</returns>
        public static Stream Serialize(Stream stream, out BaseUCSaveData[] data)
        {
            data = ReturnSaveData().ToArray();
            BinaryFormatter bf = new BinaryFormatter();

            bf.Serialize(stream, data);

            return stream;
        }

        /// <summary>
        /// Serialize our save data.
        /// </summary>
        /// <param name="stream">our stream</param>
        /// <returns>result data</returns>
        public static Stream Serialize(Stream stream)
        {
            BaseUCSaveData[] data;

            return Serialize(stream, out data);
        }

        /// <summary>
        /// Destroy all of the data objects on the current scene.
        /// </summary>
        public static void DestoryDataOnCurrentScene()
        {
            MonoBehaviour[] worldObjects = GameObject.FindObjectsOfType<MonoBehaviour>();
            MonoBehaviour component;
            UCSavedItem saveInterface;

            for (int worldIndex = 0; worldIndex < worldObjects.Length; worldIndex++)
            {
                component = worldObjects[worldIndex];
                saveInterface = component.GetComponent<UCSavedItem>();

                if (saveInterface != null)
                {
                    GameObject.Destroy(component.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// A base class for saving data, inherite from this class when ever you want to create a custom save data
    /// </summary>
    [System.Serializable]
    public class BaseUCSaveData
    {
        public string GUID;

        /// <summary>
        /// Initiate loading of the data
        /// </summary>
        /// <param name="data">the data</param>
        public virtual void Load(BaseUCSaveData data)
        {
        }

        public virtual int priority
        {
            get { return 100; }
        }

    }

    /// <summary>
    /// A serializeable version of the vector3
    /// </summary>
    [System.Serializable]
    public class SerializeableVector3
    {
        public float x, y, z;

        public SerializeableVector3()
        {
            this.x = 0;
            this.y = 0;
            this.z = 0;
        }

        public SerializeableVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator SerializeableVector3(Vector3 data)
        {
            return new SerializeableVector3(data.x, data.y, data.z);
        }

        public static explicit operator Vector3(SerializeableVector3 data)
        {
            return new Vector3(data.x, data.y, data.z);
        }

        public static SerializeableVector3 operator -(SerializeableVector3 a, Vector3 b)
        {
            a.x -= b.x;
            a.y -= b.y;
            a.z -= b.z;

            return a;
        }
    }

    /// <summary>
    /// A serializeable version of quaternion
    /// </summary>
    [System.Serializable]
    public class SerializeableQuaternion
    {
        public float x, y, z, w;

        public SerializeableQuaternion()
        {
            this.x = 0;
            this.y = 0;
            this.z = 0;
            this.w = 0;
        }

        public SerializeableQuaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static implicit operator SerializeableQuaternion(Quaternion data)
        {
            return new SerializeableQuaternion(data.x, data.y, data.z, data.w);
        }

        public static explicit operator Quaternion(SerializeableQuaternion data)
        {
            return new Quaternion(data.x, data.y, data.z, data.w);
        }
    }

    /// <summary>
    /// An interface that each saveable object in the scene needs to have.
    /// </summary>
    public interface UCSavedItem
    {
        /// <summary>
        /// Save data
        /// </summary>
        /// <returns>our save result</returns>
        BaseUCSaveData Save();
    }

}