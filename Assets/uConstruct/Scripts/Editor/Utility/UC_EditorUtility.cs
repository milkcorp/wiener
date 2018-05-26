using UnityEngine;
using UnityEditor;
using System.Collections;

namespace uConstruct
{
    public static class UC_EditorUtility
    {
        static GameObject target;
        static System.Action<GameObject, int> onClose;
        static int index;

        /// <summary>
        /// Display a script field property, like the one unity draws.
        /// </summary>
        /// <param name="editor">The referenced editor</param>
        /// <returns>did we change our script type?</returns>
        public static bool DisplayScriptField(Editor editor)
        {
            try
            {
                editor.serializedObject.Update();
                SerializedProperty prop = editor.serializedObject.FindProperty("m_Script");

                EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
                editor.serializedObject.ApplyModifiedProperties();

                prop.Next(true); // detect any dispose errors
            }
            catch { return true; }

            return false;
        }

        public static void DisplayObjectField(System.Action<GameObject, int> onClose, int index, bool allowSceneObjects)
        {
            target = null;
            UC_EditorUtility.onClose = onClose;
            UC_EditorUtility.index = index;

            EditorGUIUtility.ShowObjectPicker<GameObject>(target, allowSceneObjects, "", GUIUtility.GetControlID(FocusType.Passive));
        }

        public static void OnGUI()
        {
            if (onClose != null && index != -1)
            {
                if (Event.current != null && Event.current.commandName == "ObjectSelectorClosed")
                {
                    if (target == null)
                    {
                        target = EditorGUIUtility.GetObjectPickerObject() as GameObject;
                        onClose(target, index);

                        onClose = null;
                        target = null;
                        index = -1;
                    }
                }
            }
        }

    }
}