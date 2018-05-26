using UnityEngine;
using System.Collections;
using System.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

namespace uConstruct.Extensions
{
    public class ExtensionsEditor : EditorWindow
    {
#if !UC_Free
        [SerializeField]
        Dictionary<string, List<Extension>> extensions = new Dictionary<string, List<Extension>>();
#endif


        GUIStyle invisibleButtonStyle, boxStyle;

        Vector2 scrollPos;

        static bool isOpen
        {
            get { return Resources.FindObjectsOfTypeAll<ExtensionsEditor>().Length != 0; }
        }

        public static void Open()
        {
            var instance = GetWindow<ExtensionsEditor>("uConstruct Extensions Manager");
            instance.Init();
        }

        [UnityEditor.Callbacks.DidReloadScripts()]
        public static void HandleCompile()
        {
            if (isOpen)
            {
                Open();
            }
        }

        void Init()
        {
#if !UC_Free
            extensions = new Dictionary<string, List<Extension>>();
            System.Type[] types = Assembly.GetAssembly(typeof(Extension)).GetTypes();

            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].BaseType != typeof(Extension) || types[i].IsAbstract) continue;

                Extension extension = (Extension)System.Activator.CreateInstance(types[i]);
                Extension.LoadMethods(extension, types[i]);

                if (!extensions.ContainsKey(extension.PublisherName))
                {
                    extensions.Add(extension.PublisherName, new List<Extension>());
                }

                extensions[extension.PublisherName].Add(extension);
            }
#endif
        }

        void OnGUI()
        {
#if !UC_Free
            if (invisibleButtonStyle == null)
            {
                invisibleButtonStyle = new GUIStyle("Button");

                invisibleButtonStyle.normal.background = null;
                invisibleButtonStyle.focused.background = null;
                invisibleButtonStyle.hover.background = null;
                invisibleButtonStyle.active.background = null;
            }
            if (boxStyle == null)
            {
                boxStyle = new GUIStyle("Box");

                boxStyle.normal.textColor = invisibleButtonStyle.normal.textColor;
                boxStyle.focused.textColor = invisibleButtonStyle.focused.textColor;
                boxStyle.hover.textColor = invisibleButtonStyle.hover.textColor;
                boxStyle.active.textColor = invisibleButtonStyle.active.textColor;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (var extensionGroup in extensions)
            {
                GUILayout.BeginVertical(extensionGroup.Key, boxStyle);

                GUILayout.Space(15);

                foreach (var extension in extensionGroup.Value)
                {
                    extension.isViewed = EditorGUILayout.Foldout(extension.isViewed, extension.AssetName);

                    if (extension.isViewed)
                    {
                        GUILayout.BeginHorizontal();

                        GUILayout.Space(15);

                        GUILayout.BeginVertical();

                        if (!extension.IsDefault)
                            extension.isActivated = EditorGUILayout.Toggle("Activated :", extension.isActivated);

                        GUILayout.Space(10);

                        GUILayout.BeginHorizontal();

                        Texture logo = Extension.GetLogo(extension);

                        if (logo != null)
                        {
                            GUILayout.Label(logo, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
                            EditorGUILayout.LabelField(extension.AssetDescription, GUILayout.Height(logo == null ? 200 : logo.height)); // draw description
                        }

                        GUILayout.EndHorizontal();

                        if (extension.isActivated && extension.HelperMethods.Count > 0)
                        {
                            GUILayout.Space(15);

                            GUILayout.BeginVertical("Tools : ", boxStyle);

                            GUILayout.Space(25);

                            for (int i = 0; i < extension.HelperMethods.Count; i++)
                            {
                                GUILayout.Space(2);

                                if (GUILayout.Button(extension.HelperMethods[i].Name))
                                {
                                    extension.HelperMethods[i].Invoke(extension, null);
                                }
                            }

                            GUILayout.EndVertical();

                        }

                        GUILayout.Space(15);

                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button("Open Documentation", boxStyle))
                        {
                            Extension.OpenDocs(extension);
                        }
                        if (GUILayout.Button("Asset Store", boxStyle))
                        {
                            Extension.OpenAssetStore(extension);
                        }

                        GUILayout.EndHorizontal();

                        GUILayout.EndVertical();

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.Space(5);
                }

                GUILayout.EndVertical();

                GUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();

#else
            EditorGUILayout.LabelField("uConstruct Extensions is not available on uConstruct Free.");
#endif
        }

    }
}