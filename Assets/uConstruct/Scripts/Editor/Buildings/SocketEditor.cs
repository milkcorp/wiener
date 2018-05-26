using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using uConstruct.Sockets;
using uConstruct.Conditions;

namespace uConstruct
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(BaseSocket), true)]
    public class SocketEditor : PhysicsObjectEditor
    {
        public SerializedProperty receiveType;
        public SerializedProperty placingType;
        public SerializedProperty isHoverTarget;
        public SerializedProperty drawIndividual;
        public SerializedProperty previewObject;

        GUIStyle boxStyle;
        BaseSocket script;

        public override void OnEnable()
        {
            base.OnEnable();

            receiveType = serializedObject.FindProperty("receiveType");
            placingType = serializedObject.FindProperty("placingType");
            isHoverTarget = serializedObject.FindProperty("isHoverTarget");
            drawIndividual = serializedObject.FindProperty("drawIndividual");
            previewObject = serializedObject.FindProperty("_previewObject");

            BaseSocket.OnPreviewObjectChangedEvent += OnPreviewChanged;
        }

        void OnDisable()
        {
            BaseSocket.OnPreviewObjectChangedEvent -= OnPreviewChanged;
        }

        void OnPreviewChanged(GameObject _target)
        {
            BaseSocket socket;

            for (int i = 0; i < targets.Length; i++)
            {
                socket = targets[i] as BaseSocket;

                if (socket != null)
                    socket.PreviewObject = _target;
            }
        }

        public override void OnInspectorGUI()
        {
            if (script == null)
            {
                script = (BaseSocket)target;

                receiveType = serializedObject.FindProperty("receiveType");
                placingType = serializedObject.FindProperty("placingType");
                isHoverTarget = serializedObject.FindProperty("isHoverTarget");
                drawIndividual = serializedObject.FindProperty("drawIndividual");
                previewObject = serializedObject.FindProperty("_previewObject");
            }

            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.alignment = TextAnchor.UpperLeft;
                boxStyle.fontStyle = FontStyle.Bold;
                boxStyle.normal.textColor = Color.white;
            }

            if (UC_EditorUtility.DisplayScriptField(this))
            {
                return;
            }

            GUILayout.BeginVertical("Building Variables :", boxStyle);
            GUILayout.Space(14);

            script.receiveType = (BuildingType)EditorGUILayout.EnumMaskPopup(new GUIContent("Receive Buildings :", "What buildings will this socket receive"), script.receiveType);

            if(GUI.changed)
            {
                BaseSocket socket;
                for (int i = 0; i < targets.Length; i++)
                {
                    socket = targets[i] as BaseSocket;

                    if(socket != null)
                    {
                        socket.receiveType = script.receiveType;
                    }

                    EditorUtility.SetDirty(targets[i]);
                }
            }

            script.placingType = (PlacingRestrictionType)EditorGUILayout.EnumMaskPopup(new GUIContent("Buildings Placing Type :", "How will buildings be placed on this socket"), script.placingType);

            if (GUI.changed)
            {
                BaseSocket socket;
                for (int i = 0; i < targets.Length; i++)
                {
                    socket = targets[i] as BaseSocket;

                    if (socket != null)
                    {
                        socket.placingType = script.placingType;
                    }

                    EditorUtility.SetDirty(targets[i]);
                }
            }

            GUILayout.Space(5);

            /*
            script.isHoverTarget = EditorGUILayout.Toggle(new GUIContent("Is Hover Target ?", "Will buildings be free placed on this socket if no match found ?"), script.isHoverTarget);
            script.drawIndividual = EditorGUILayout.Toggle(new GUIContent("Default draw socket :", "Draw this individual socket on runtime."), script.drawIndividual);
             */

            EditorGUILayout.PropertyField(isHoverTarget, new GUIContent("Is Hover Target ?", "Will buildings be free placed on this socket if no match found ?"));
            EditorGUILayout.PropertyField(drawIndividual, new GUIContent("Default draw socket :", "Draw this individual socket on runtime."));

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            //EditorGUILayout.PropertyField(previewObject, new GUIContent("Preview Object :", "Assigning an object would scale the socket to the mesh and also show it as a preview so you can test it."));
            script.PreviewObject = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Preview Object :", "Assigning an object would scale the socket to the mesh and also show it as a preview so you can test it."), script.PreviewObject, typeof(GameObject), false);

            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                BaseSocket socket;
                for (int i = 0; i < targets.Length; i++)
                {
                    socket = targets[i] as BaseSocket;

                    if (socket != null)
                    {
                        socket.PreviewObject = null;
                    }
                }
            }
            if (script.previewInstance != null)
            {
                if (GUILayout.Button("S", GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog("Saving changes", "Are u sure you want to save changes to prefab ?", "Yes", "No"))
                    {
                        BaseSocket socket;
                        for (int i = 0; i < targets.Length; i++)
                        {
                            socket = targets[i] as BaseSocket;

                            if (socket != null)
                            {
                                socket.previewInstance.ApplyChangesToPrefab(socket.PreviewObject);
                                EditorUtility.SetDirty(targets[i]);
                            }
                        }

                        AssetDatabase.SaveAssets();
                    }
                }
                if (GUILayout.Button("C", GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog("Changing Scale", "Are u sure you want to grab scale from socket ?", "Yes", "No"))
                    {
                        BaseSocket socket;
                        for (int i = 0; i < targets.Length; i++)
                        {
                            socket = targets[i] as BaseSocket;

                            if (socket != null)
                            {
                                socket.previewInstance.FitToLocalSpace();
                            }
                        }
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.EndVertical();

            if (GUI.changed)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    EditorUtility.SetDirty(targets[i]);
                }
            }

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }

}
