using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using uConstruct.Sockets;
using uConstruct.Conditions;
using uConstruct.Core.Physics;

#if UNITY_5_3
using UnityEditor.SceneManagement;
#endif

namespace uConstruct
{

    [CustomEditor(typeof(UCPhysicsObject), true)]
    public class PhysicsObjectEditor : Editor
    {
        GUIStyle _boxStyle;
        UCPhysicsObject _script;

        SerializedProperty m_usePhysics;
        SerializedProperty m_center;
        SerializedProperty m_size;

        public virtual void OnEnable()
        {
            m_usePhysics = serializedObject.FindProperty("_usePhysics");
            m_center = serializedObject.FindProperty("_center");
            m_size = serializedObject.FindProperty("_size");
        }

        public override void OnInspectorGUI()
        {
            if (_script == null)
            {
                _script = (UCPhysicsObject)target;
            }

            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box);
                _boxStyle.alignment = TextAnchor.UpperLeft;
                _boxStyle.fontStyle = FontStyle.Bold;
                _boxStyle.normal.textColor = Color.white;
            }

            GUILayout.BeginVertical("uConstruct Physics Object", _boxStyle);
            GUILayout.Space(14);

            EditorGUILayout.PropertyField(m_usePhysics, new GUIContent("Use Physics", "Use the uConstruct Physics engine on this, for complex sockets like terrains you want to use the unity physics instead of this."));

            EditorGUILayout.PropertyField(m_center, new GUIContent("Center :", "The center of the bounds"));
            EditorGUILayout.PropertyField(m_size, new GUIContent("Size :", "The size of the bounds"));

            UCPhysicsObject.GizmosColor = EditorGUILayout.ColorField("Physics Gizmos Color :", UCPhysicsObject.GizmosColor);

            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                #if UNITY_5_3
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                #else
                EditorApplication.MarkSceneDirty();
                #endif

                for (int i = 0; i < targets.Length; i++)
                {
                    EditorUtility.SetDirty(targets[i]);
                }
            }

        }
    }

}
