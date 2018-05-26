using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uConstruct.Core.Manager;

using System.Linq;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace uConstruct
{
    public class UCSettings : ScriptableObject
    {
        public const string fileName = "uConstructSettings";

        static UCSettings _instance;
        public static UCSettings instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Resources.Load<UCSettings>(fileName);

                    if(_instance == null)
                    {
                        _instance = CreateInstance<UCSettings>();

                        #if UNITY_EDITOR
                        AssetDatabase.CreateAsset(_instance, UCCallbacksManager.ProjectPath + "Resources/" + fileName + ".asset");
                        AssetDatabase.SaveAssets();
                        #endif
                    }
                }

                return _instance;
            }
        }

        [UCSetting(UCSettingCategories.Saving, "Enable Saving :", "Will uConstruct save and load your buildings ?")]
        public bool UCSavingEnabled = true;

        [UCSetting(UCSettingCategories.Saving, "Unique Saves For Each Scenes :", "Will the system generate a unique save for each scene ? (So you wont have different scenes with the same save.")]
        public bool UCSavingUniqueSceneSave = true;

        [UCSetting(UCSettingCategories.Saving, "UCSaving Path :", "In what path type will uConstruct stash the saved buildings?")]
        public SavingPathType UCSavingPathType;

        [UCSetting(UCSettingCategories.Saving, "Saving File Name :", "The name which will be used for the saving file.")]
        public string UCSavingFileName = "UConstructData";

        [UCSetting(UCSettingCategories.Threading, "Enable MultiThreading :", "Will uConstruct try to decrease overload on the main thread when possible?")]
        public bool UCThreadingEnabled = true;

        [UCSetting(UCSettingCategories.Batching, "Enable Batching :", "Will uConstruct try to batch your buildings and reduce draw calls ?")]
        public bool UCBatchingEnabled = true;

        [UCSetting(UCSettingCategories.Batching, "Batch-LOD Levels :", "How many LOD levels will be generated ? leave 0 for none.")]
        public int UCBatchingLODLevels = 3;

        [UCSetting(UCSettingCategories.AOI, "Area Of Interest Calculation Method :", "What method will be used to calculate AOI ?")]
        public UCAOIMethod UCAOICalculationMethod = UCAOIMethod.PerGroup;

    }

    public enum SavingPathType
    {
        Persistent,
        Data
    }

    public enum UCSettingCategories
    {
        Saving,
        General,
        Conditions,
        Sockets,
        Blueprints,
        Utilities,
        Templates,
        Prefabs,
        Physics,
        Threading,
        Batching,
        AOI
    }

    public enum UCAOIMethod
    {
        PerGroup,
        PerBuilding
    }

    public class UCSettingCategory
    {
        static List<UCSettingCategory> _categories;
        public static List<UCSettingCategory> categories
        {
            get
            {
                if(_categories == null)
                {
                    _categories = new List<UCSettingCategory>();

                    List<FieldInfo> fitFields = new List<FieldInfo>();

                    FieldInfo[] fields = typeof(UCSettings).GetFields();

                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (fields[i].GetCustomAttributes(true).Select(x => x as UCSettingAttribute).Count() > 0)
                        {
                            fitFields.Add(fields[i]);
                        }
                    }

                    FieldInfo field;
                    UCSettingAttribute attribute;
                    UCSettingCategory category;
                    for(int i = 0; i < fitFields.Count; i++)
                    {
                        field = fitFields[i];

                        attribute = (UCSettingAttribute)field.GetCustomAttributes(true).FirstOrDefault(x => (x as UCSettingAttribute) != null);
                        category = GetCategory(attribute.category);

                        if(category == null)
                        {
                            category = new UCSettingCategory(attribute.category);
                            categories.Add(category);
                        }

                        category.attributes.Add(attribute);
                        category.fields.Add(field);
                    }
                }

                return _categories;
            }
        }

        public bool show;

        public UCSettingCategories type;
        public List<UCSettingAttribute> attributes = new List<UCSettingAttribute>();
        public List<FieldInfo> fields = new List<FieldInfo>();

        public UCSettingCategory(UCSettingCategories category)
        {
            this.type = category;
        }

        public static UCSettingCategory GetCategory(UCSettingCategories category)
        {
            for (int i = 0; i < categories.Count; i++)
            {
                if (categories[i].type == category)
                    return categories[i];
            }

            return null;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple=false)]
    public class UCSettingAttribute : System.Attribute
    {
        public UCSettingCategories category;
        public string name;
        public string desc;

        GUIContent _content;
        GUIContent content
        {
            get
            {
                if(_content == null)
                {
                    _content = desc == "" ? new GUIContent(name) : new GUIContent(name, desc);
                }

                return _content;
            }
        }

        public UCSettingAttribute(UCSettingCategories category, string name)
        {
            this.category = category;
            this.name     = name;
        }

        public UCSettingAttribute(UCSettingCategories category, string name, string desc)
        {
            this.category = category;
            this.name = name;
            this.desc = desc;
        }

        public object Draw(object instance)
        {
            #if UNITY_EDITOR
            System.Type type = instance.GetType();

            if (CheckType(type, typeof(string)))
                return EditorGUILayout.TextField(content, (string)instance);
            else if (CheckType(type, typeof(int)))
                return EditorGUILayout.IntField(content, (int)instance);
            else if (CheckType(type, typeof(float)))
                return EditorGUILayout.FloatField(content, (float)instance);
            else if (CheckType(type, typeof(bool)))
                return EditorGUILayout.Toggle(content, (bool)instance);
            else if (CheckType(type, typeof(System.Enum)))
                return (object)EditorGUILayout.EnumPopup(content, (System.Enum)instance);
            #endif

            return null;
        }

        bool CheckType(System.Type type, System.Type target)
        {
                return type == target || type.BaseType == target;
        }

    }

}
