using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using uConstruct;
using uConstruct.Core.Saving;

using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

namespace uConstruct.Core.Blueprints
{
    [System.Serializable]
    /// <summary>
    /// Blueprints are a set of data that allows you to quickly create a set of data that can be applied on any kind of a building with not efforts. 
    /// </summary>
    public class Blueprint : ScriptableObject
    {
        /// <summary>
        /// A static path to the Assets blueprints folder.
        /// </summary>
        public static string BLUEPRINT_ASSET_PATH
        {
            get { return uConstruct.Core.Manager.UCCallbacksManager.ProjectPath + "Resources/Blueprints/"; }
        }
        /// <summary>
        /// The first blueprint name.
        /// </summary>
        public const string BLUEPRINT_ASSET_FIRST = "uBlueprint_";

        /// <summary>
        /// The blueprint name.
        /// </summary>
        [HideInInspector]
        public string blueprintName = "New Blueprint";
        /// <summary>
        /// The blueprint's fields.
        /// </summary>
        [HideInInspector]
        public List<BlueprintField> fields = new List<BlueprintField>();

        /// <summary>
        /// Our path.
        /// </summary>
        string selfPath
        {
            get
            {
                #if UNITY_EDITOR
                return AssetDatabase.GetAssetPath(this);
                #else
                return "";
                #endif
            }
        }

        /// <summary>
        /// Save the blueprint, works only on editor.
        /// </summary>
        public void Save()
        {
            #if UNITY_EDITOR
            if(AssetDatabase.LoadAssetAtPath<Blueprint>(GetPath(blueprintName)) == null)
            {
                AssetDatabase.MoveAsset(selfPath, GetPath(blueprintName));
            }

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            #endif
        }
        /// <summary>
        /// Delete the blueprint, works only on editor.
        /// </summary>
        public void Delete()
        {
            #if UNITY_EDITOR
            AssetDatabase.DeleteAsset(selfPath);
            AssetDatabase.SaveAssets();
            #endif
        }

        /// <summary>
        /// Add a field to the blueprint, works on both runtime and editor.
        /// </summary>
        /// <param name="field"></param>
        public void AddField(BlueprintField field)
        {
            if (field != null && !BlueprintField.Contains(field, fields))
            {
                fields.Add(field);
                Save();
            }
        }
        /// <summary>
        /// Remove a field from the blueprint, works on both runtime and editor.
        /// </summary>
        /// <param name="field"></param>
        public void RemoveField(BlueprintField field)
        {
            fields.Remove(field);
            Save();
        }

        /// <summary>
        /// Get path to a certain name
        /// </summary>
        /// <param name="name">the name</param>
        /// <returns>the path of that name.</returns>
        public static string GetPath(string name)
        {
            return BLUEPRINT_ASSET_PATH + BLUEPRINT_ASSET_FIRST + name + ".asset";
        }

        /// <summary>
        /// Create a new blueprint, works on both runtime and editor.
        /// </summary>
        /// <returns>Our newely created blueprint.</returns>
        public static Blueprint CreateBlueprint()
        {
            Blueprint instance = CreateInstance<Blueprint>();

            #if UNITY_EDITOR
            if (AssetDatabase.LoadAssetAtPath<Blueprint>(instance.selfPath) == null)
            {
                AssetDatabase.CreateAsset(instance, GetPath(instance.blueprintName));
                AssetDatabase.SaveAssets();
            }
            #endif

            return instance;
        }
    }

    [System.Serializable]
    /// <summary>
    /// Blueprint field holds data about the blueprint.
    /// </summary>
    public class BlueprintField : ISerializationCallbackReceiver
    {
        public List<BlueprintFieldData> data = new List<BlueprintFieldData>();

        /// <summary>
        /// Target for packaging, used in the editors.
        /// </summary>
        //[System.NonSerialized]
        //public GameObject target;

        /// <summary>
        /// Serialized bytes
        /// </summary>
        [SerializeField]
        byte[] dataBytes;

        /// <summary>
        /// our field type.
        /// </summary>
        [SerializeField]
        BuildingType type;

        /// <summary>
        /// Is this blueprint field open in the blueprints editor.
        /// </summary>
        [System.NonSerialized]
        public bool _isOpen;
        public bool isOpen
        {
            get
            {
                return _isOpen && data != null && data.Count > 0;
            }
            set
            {
                _isOpen = value;
            }
        }

        /// <summary>
        /// The name of the field.
        /// </summary>
        public string name
        {
            get { return type.ToString(); }
        }

        /// <summary>
        /// Create a new field instance
        /// </summary>
        /// <param name="type">the field type</param>
        public BlueprintField(BuildingType type)
        {
            this.type = type;
        }
        /// <summary>
        /// Create a new blueprint field.
        /// </summary>
        /// <param name="type">the type of the field</param>
        /// <param name="source">the source that the building will get data from.</param>
        public BlueprintField(BuildingType type, GameObject source)
        {
            this.type = type;
            
            if(source != null) // add a new instance if the source game object isnt null.
            {
                BlueprintFieldData fieldData = new BlueprintFieldData();
                fieldData.Pack(source);

                data.Add(fieldData);
            }
        }

        /// <summary>
        /// Customly serialize the inheritable data.
        /// </summary>
        public void OnBeforeSerialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(stream, data);

                dataBytes = stream.ToArray();
            }
        }
        /// <summary>
        /// Customly serialize the inheritable data.
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (dataBytes == null || dataBytes.Length == 0) return;

            using(MemoryStream stream = new MemoryStream(dataBytes))
            {
                BinaryFormatter bf = new BinaryFormatter();
                try
                {
                    data = (List<BlueprintFieldData>)bf.Deserialize(stream);
                }
                catch
                {
                    dataBytes = null;
                }
            }
        }

        /// <summary>
        /// Create a child which contains blueprint data.
        /// </summary>
        public void AddChild(GameObject go)
        {
            if (go == null)
                return;

            var child = new BlueprintFieldData();
            child.Pack(go);

            data.Add(child);
        }

        /// <summary>
        /// Remove a child which contains data.
        /// </summary>
        public void RemoveChild(BlueprintFieldData child)
        {
            data.Remove(child);
        }

        /// <summary>
        /// Check if a field is contained.
        /// </summary>
        /// <param name="field">our field</param>
        /// <param name="fields">the list of fields</param>
        /// <returns>is this field already contained?</returns>
        public static bool Contains(BlueprintField field, List<BlueprintField> fields)
        {
            for(int i = 0; i < fields.Count; i++)
            {
                if (fields[i].type == field.type)
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// An interface that each one of the blueprinted items should have.
    /// </summary>
    public interface IBlueprintItem
    {
        /// <summary>
        /// Pack our data
        /// </summary>
        /// <returns>our data</returns>
        BlueprintData Pack();
        /// <summary>
        /// Our priority, this is used when ordering the "Pack" methods.
        /// </summary>
        int priority { get; }
    }

    /// <summary>
    /// A serializeable data class that needs to be inherited from on any data that can be serialized into the blueprint system.
    /// </summary>
    [System.Serializable]
    public class BlueprintData
    {
        public string name;

        public SerializeableVector3 position;
        public SerializeableQuaternion rotation;
        public SerializeableVector3 scale;

        public virtual void UnPack(GameObject target)
        {

        }
    }

}
