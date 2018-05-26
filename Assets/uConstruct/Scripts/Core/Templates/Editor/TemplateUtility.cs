using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uConstruct.Sockets;
using uConstruct.Conditions;

using UnityEditor;
using System.IO;

namespace uConstruct.Core.Templates
{
    public class TemplateUtility : MonoBehaviour
    {
        /// <summary>
        /// Our path to the prefab folder. (Used with prefab utility)
        /// </summary>
        public static string PREFAB_PATH
        {
            get { return uConstruct.Core.Manager.UCCallbacksManager.ProjectPath + "Resources/Templates/";}
        }
        /// <summary>
        /// Our path to the resources folder. (Used with Resources class)
        /// </summary>
        public const string RESOURCES_PATH = "Templates/";

        /// <summary>
        /// Generate our templates for the building.
        /// </summary>
        /// <param name="name">The name of the template</param>
        /// <param name="building">what building is the template created for</param>
        /// <param name="templateTargets">the template objects you want to template</param>
        /// <param name="copy">Auto-Assign the template into the building</param>
        /// <returns>The generated template prefab</returns>
        public static GameObject GenerateTemplate(string name, BaseBuilding building, ITemplateObject[] templateTargets, bool copy)
        {
            Transform templateTransform;

            GameObject templateParent = new GameObject(name);
            templateParent.transform.position = building.transform.position;

            var templateScript = templateParent.AddComponent<Template>();
            templateScript.templateObjects = templateTargets;
            templateScript.name = name;

            for (int i = 0; i < templateTargets.Length; i++)
            {
                if (copy)
                    templateTransform = templateTargets[i].GetTransform();
                else
                {
                    templateTransform = ((GameObject)Instantiate(templateTargets[i].GetTransform().gameObject, templateTargets[i].GetTransform().position, templateTargets[i].GetTransform().rotation)).transform;
                    templateTransform.name = templateTargets[i].GetTransform().name;
                }

                templateTransform.SetParent(templateParent.transform);
            }

            var prefab = PrefabUtility.CreatePrefab(PREFAB_PATH + name + ".prefab", templateParent, ReplacePrefabOptions.Default);

            if (copy)
            {
                building.AddTemplate(prefab);
            }

            DestroyImmediate(templateParent);

            AssetDatabase.SaveAssets();

            return prefab;
        }
    }
}
