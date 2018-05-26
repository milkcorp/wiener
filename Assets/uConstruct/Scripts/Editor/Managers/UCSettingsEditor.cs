using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using System.Reflection;
using System.Linq;

namespace uConstruct
{
    public class UCSettingsEditor : EditorWindow
    {
        public UCSettings _settings;
        public UCSettings settings
        {
            get
            {
                if(_settings == null)
                {
                    _settings = UCSettings.instance;
                }

                return _settings;
            }
        }

        GUIStyle invisibleButtonStyle;
        GUIStyle boxStyle;

        Vector2 scrollPos;

        [MenuItem("Window/UConstruct/Settings")]
        public static void Open()
        {
            var instance = GetWindow<UCSettingsEditor>("UCSettings");
            instance._settings = UCSettings.instance;
        }

        void OnGUI()
        {
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

            EditorGUILayout.BeginVertical("Box");

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            UCSettingCategory category;
            object fieldValue;

            for (int i = 0; i < UCSettingCategory.categories.Count; i++)
            {
                EditorGUILayout.BeginVertical(boxStyle);

                category = UCSettingCategory.categories[i];

                category.show = EditorGUILayout.Foldout(category.show, "Show " + category.type.ToString() + " Settings");

                if (category.show)
                {
                    GUILayout.Space(15);

                    for (int b = 0; b < category.attributes.Count; b++)
                    {
                        fieldValue = category.fields[b].GetValue(settings);

                        if (fieldValue == null) continue;

                        category.fields[b].SetValue(settings, category.attributes[b].Draw(fieldValue));
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            if(GUILayout.Button("Save"))
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.EndVertical();
        }

    }
}