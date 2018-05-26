using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using uConstruct.Sockets;
using uConstruct.Conditions;
using uConstruct.Core.PrefabDatabase;
using uConstruct.Core.Templates;

namespace uConstruct
{
    [CustomEditor(typeof(BaseBuilding), true)]
    [CanEditMultipleObjects]
    public class BuildingEditor : Editor
    {
        public BaseBuilding script;
        private bool showSockets;
        private bool showTagEditor;
        private bool showConditions;
        private GUIStyle boxStyle;

        BaseSocket socketData;
        BaseSocket[] sockets;

        BaseSnapPoint point;
        BaseSnapPoint[] snapPoints;

        BaseCondition conditionData;
        BaseCondition[] conditions;

        public override void OnInspectorGUI()
        {
            if (script == null)
            {
                script = (BaseBuilding)target;
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

            GUILayout.BeginVertical(script.transform.name + " : " + script.prefabID.ToString() + (script.uid == -1 ? "" : " ( " + script.uid + " )"), boxStyle);

            GUILayout.Space(15);

            if (script.prefabID == -1)
            {
                GUI.enabled = false;

            }

            script.buildingType = (BuildingType)EditorGUILayout.EnumPopup(new GUIContent("Building Type :", "What is the type of the building ?, used for socket mechanic"), script.buildingType);
            script.placingRestrictionType = (PlacingRestrictionType)EditorGUILayout.EnumMaskPopup(new GUIContent("Placing Type: ", "How will this object be placed ? (sockets - stuff like walls, ceilings, stuff that needs to be restricted and snapped OR freelyPlaced - stuff like foundation that dont have restrictions or needs snap)"), script.placingRestrictionType);
            script.batchBuilding = EditorGUILayout.Toggle(new GUIContent("Batch Building :", "Will the system batch this building ?"), script.batchBuilding);

            GUILayout.Space(10);

            script.canBeRotated = EditorGUILayout.BeginToggleGroup(new GUIContent("Can Be Rotated :", "Can this building be rotated ?, used in the building placer script"), script.canBeRotated);
            script.rotationAmount = EditorGUILayout.IntField(new GUIContent("Rotate Amount :", "The amount of rotation that will be applied"), script.rotationAmount);
            EditorGUILayout.EndToggleGroup();

            GUILayout.Space(10);

            script.placingOffset = EditorGUILayout.Vector3Field(new GUIContent("Placing Offset :", "The placing offset of the this building"), script.placingOffset);
            script.rotationOffset = EditorGUILayout.Vector3Field(new GUIContent("Rotation Offset :", "The rotation offset of the this building"), script.rotationOffset);

            if (FlagsHelper.IsBitSet<PlacingRestrictionType>(script.placingRestrictionType, PlacingRestrictionType.FreelyBased))
            {
                GUILayout.Space(8);
                script.rotateWithSlope = EditorGUILayout.Toggle(new GUIContent("Align To Slope: ", "Will this building be rotated with slopes ?"), script.rotateWithSlope);
            }
            if (FlagsHelper.IsBitSet<PlacingRestrictionType>(script.placingRestrictionType, PlacingRestrictionType.SocketBased))
            {
                GUILayout.Space(8);

                script.rotateToFit = EditorGUILayout.BeginToggleGroup(new GUIContent("Rotate To Fit : ", "On placing, rotate this building in order to fit it into the socket automatically."), script.rotateToFit);

                script.rotateAxis = (Axis)EditorGUILayout.EnumPopup(new GUIContent("Rotation Axis: ", "What axis will the building try to fit it self on?"), script.rotateAxis);
                script.rotateThreshold = EditorGUILayout.FloatField(new GUIContent("Rotation Amount :", "How much rotation will it try to adjust to in order to find the right snap ?"), script.rotateThreshold);
                script.rotationSteps = EditorGUILayout.IntField(new GUIContent("Rotation Steps :", "How many steps will it do to try to get the rotation to fit ? "), script.rotationSteps);

                EditorGUILayout.EndToggleGroup();
            }

            GUILayout.EndVertical();

            #region HandlePrefabManagement

            GUI.enabled = script.gameObject.activeInHierarchy;

            #endregion

            #region Sockets

            showSockets = EditorGUILayout.Foldout(showSockets, string.Format("{0}", "Sockets"));

            if (showSockets)
            {
                EditorGUILayout.BeginVertical(boxStyle);

                sockets = script.transform.GetComponentsInChildren<BaseSocket>(true);

                for (int i = 0; i < sockets.Length; i++)
                {
                    socketData = sockets[i];

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(socketData.name);

                    if (GUILayout.Button(">", GUILayout.Width(25)))
                    {
                        Selection.activeObject = socketData.transform;
                    }

                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        OpenPropertyCreatingWindow("Socket #" + (sockets.Length + 1), ModifierType.Socket);
                    }

                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        if (EditorUtility.DisplayDialog("Remove Property", string.Format("Are u sure you want to remove {0} permanently ?", socketData.name), "Yes", "No"))
                        {
                            DestroyImmediate(socketData.gameObject);
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    socketData.receiveType = (BuildingType)EditorGUILayout.EnumMaskPopup(new GUIContent("Receives Building Type :", "What building types will this socket receive ?"), socketData.receiveType);

                    EditorGUILayout.BeginHorizontal();

                    socketData.PreviewObject = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Preview Object :", "What Object will be used for preview on the socket, can be left null"), socketData.PreviewObject, typeof(GameObject), false);

                    if (socketData.PreviewObject != null)
                    {
                        if (GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            socketData.PreviewObject = null;
                        }
                        if (socketData.previewInstance != null)
                        {
                            if (GUILayout.Button("S", GUILayout.Width(20)))
                            {
                                if (EditorUtility.DisplayDialog("Saving changes", "Are u sure you want to save changes to prefab ?", "Yes", "No"))
                                {
                                    socketData.previewInstance.ApplyChangesToPrefab(socketData.PreviewObject);
                                    EditorUtility.SetDirty(target);
                                    AssetDatabase.SaveAssets();
                                }
                            }
                            if (GUILayout.Button("C", GUILayout.Width(20)))
                            {
                                if (EditorUtility.DisplayDialog("Changing Scale", "Are u sure you want to grab scale from socket ?", "Yes", "No"))
                                {
                                    socketData.previewInstance.FitToLocalSpace();
                                }
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    socketData.placingType = (PlacingRestrictionType)EditorGUILayout.EnumMaskPopup(new GUIContent("Placing Type :", "What the placing method will be"), socketData.placingType);

                    EditorGUILayout.BeginHorizontal(boxStyle);
                    EditorGUILayout.LabelField("", GUILayout.Height(0.5f));
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Create Socket"))
                {
                    OpenPropertyCreatingWindow("Socket #" + (sockets.Length + 1), ModifierType.Socket);
                }

                EditorGUILayout.EndVertical();

                #region SnapPoints
                GUILayout.Space(15);

                EditorGUILayout.BeginVertical(boxStyle);
                snapPoints = script.transform.GetComponentsInChildren<BaseSnapPoint>(true);

                for (int i = 0; i < snapPoints.Length; i++)
                {
                    point = snapPoints[i];

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(point.name);

                    if (GUILayout.Button(">", GUILayout.Width(25)))
                    {
                        Selection.activeObject = socketData.transform;
                    }

                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        if (EditorUtility.DisplayDialog("Remove Property", string.Format("Are u sure you want to remove {0} permanently ?", point.name), "Yes", "No"))
                        {
                            DestroyImmediate(point.gameObject);
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal(boxStyle);
                    EditorGUILayout.LabelField("", GUILayout.Height(0.5f));
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Create Snap Point"))
                {
                    OpenPropertyCreatingWindow("SnapPoint #" + (snapPoints.Length + 1), ModifierType.SnapPoint);
                }
                #endregion

                EditorGUILayout.EndVertical();
            }

            #endregion

            #region Conditions

            showConditions = EditorGUILayout.Foldout(showConditions, string.Format("{0}", "Conditions"));

            if (showConditions)
            {
                EditorGUILayout.BeginVertical(boxStyle);

                conditions = script.transform.GetComponentsInChildren<BaseCondition>(true);

                for (int i = 0; i < conditions.Length; i++)
                {
                    conditionData = conditions[i];

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(conditionData.name);

                    if (GUILayout.Button(">", GUILayout.Width(25)))
                    {
                        Selection.activeObject = conditionData.transform;
                    }

                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        OpenPropertyCreatingWindow("Condition #" + (conditions.Length + 1), ModifierType.Condition);
                    }

                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        if (EditorUtility.DisplayDialog("Remove Property", string.Format("Are u sure you want to remove {0} permanently ?", conditionData.name), "Yes", "No"))
                        {
                            DestroyImmediate(conditionData.gameObject);
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal(boxStyle);
                    EditorGUILayout.LabelField("", GUILayout.Height(0.5f));
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Create Condition"))
                {
                    OpenPropertyCreatingWindow("Condition #" + (conditions.Length + 1), ModifierType.Condition);
                }

                EditorGUILayout.EndVertical();
            }

            #endregion

            if (!script.gameObject.activeInHierarchy)
            {
                GUI.enabled = true;
                GUILayout.Space(12);

                EditorGUILayout.HelpBox("You must be enabled or placed in the scene in order to edit the Conditions and the Sockets.", MessageType.Info);
            }
            else
            {
                if (GUILayout.Button("Templates"))
                {
                    var window = EditorWindow.GetWindow<TemplateCreaterEditor>();
                    window.Init(this);
                }
            }

            bool isPrefabInstance = PrefabUtility.GetPrefabType(script.gameObject) == PrefabType.Prefab;
            bool isAttachedToPrefab = PrefabUtility.GetPrefabType(script.gameObject) == PrefabType.PrefabInstance || PrefabUtility.GetPrefabType(script.gameObject) == PrefabType.Prefab;

            if (isPrefabInstance)
            {
                GUI.enabled = true;

                if (script.prefabID != -1 && GUILayout.Button("Reset Prefab ID"))
                {
                    if (EditorUtility.DisplayDialog("Reset ID", "Are u sure you want to reset prefab id?, this might corrupt your saves.", "Yes", "No"))
                    {
                        script.prefabID = -1;
                        PrefabDB.instance.RemoveFromDB(script.gameObject);

                        EditorUtility.SetDirty(target);
                        EditorUtility.SetDirty(PrefabDB.instance);

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
            }

            if (script.prefabID == -1 || !isAttachedToPrefab)
            {
                GUI.enabled = true;

                if (script.prefabID == -1)
                    EditorGUILayout.HelpBox("This building does not have PrefabID initialized, Please compile prefabs or click on the compile button bellow.", MessageType.Info);
                if (!isAttachedToPrefab)
                    EditorGUILayout.HelpBox("This building isnt attached to a prefab, please attach it to a prefab and try again", MessageType.Info);

                if (isAttachedToPrefab)
                {
                    if (GUILayout.Button("Add Prefab to prefabDB"))
                    {
                        script.prefabID = uConstruct.Core.PrefabDatabase.PrefabDB.instance.AddToDB(script.gameObject);
                    }
                }
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }

        }

        void OpenPropertyCreatingWindow(string Name, ModifierType type)
        {
            var window = EditorWindow.GetWindow<PropertyCreaterEditor>();
            window.Init(this, Name, boxStyle, type);
        }

        void OnEnable()
        {
            script = (BaseBuilding)target;
        }

    }

    public class PropertyCreaterEditor : EditorWindow
    {
        BuildingEditor buildingEditor;

        string propertyName;
        SocketPositionAnchor positionAnchor = SocketPositionAnchor.Center;

        public ModifierType creatingType = ModifierType.Socket;

        #region Sockets
        GameObject previewGameObject;
        BuildingType receivesBuildings;
        PlacingRestrictionType socketPlacingType;
        #endregion

        #region Conditions
        MonoScript condition;
        #endregion

        #region SnapPoints
        BuildingType targetType;
        #endregion

        GUIStyle boxStyle;

        public void Init(BuildingEditor editorWindow, string startingName, GUIStyle style, ModifierType modifer)
        {
            this.propertyName = startingName;
            this.buildingEditor = editorWindow;

            this.boxStyle = style;
            this.creatingType = modifer;
        }

        void OnGUI()
        {
            GUILayout.Space(5);

            if (buildingEditor == null) // close the window if we dont have a reference to our building editor window, can happens due to unity compiling scripts.
            {
                Close();
            }

            GUILayout.BeginVertical("Property Creator Editor Window", boxStyle);

            GUILayout.Space(20);

            propertyName = EditorGUILayout.TextField(new GUIContent("Name :", "What is the name of this property ?, can be used for knowing what that property is being used for."), propertyName);
            positionAnchor = (SocketPositionAnchor)EditorGUILayout.EnumPopup(new GUIContent("Starting Position Anchor :", "Where will it spawn the object in the building local position ? use this to help you position your property faster, it doesnt influence any further features."), positionAnchor);

            if (creatingType == ModifierType.Socket)
            {
                previewGameObject = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Socket Gizmos Preview GameObject :", "This GameObject will be used for preview on gizmos, it will grab the parameters from it into the collider to hlep you easily get your scale right."), previewGameObject, typeof(GameObject), false);
                receivesBuildings = (BuildingType)EditorGUILayout.EnumMaskPopup(new GUIContent("Receive Buildings :", "What building will be snapped into this socket ?"), receivesBuildings);
                socketPlacingType = (PlacingRestrictionType)EditorGUILayout.EnumMaskPopup(new GUIContent("Placing Type :", "What the placing method will be on this socket ?"), socketPlacingType);
            }
            else if (creatingType == ModifierType.Condition)
            {
                condition = EditorGUILayout.ObjectField(new GUIContent("Condition Script :", "What condition script will this condition use ? you can always edit it later. if you leave this blank it will use the BaseCondition script. "), condition, typeof(MonoScript), false) as MonoScript;

                if (condition != null && !condition.GetClass().BaseType.Equals(typeof(BaseCondition))) // reset condition if its type isnt what we need ( a condition ).
                {
                    condition = null;
                }
            }
            else // snap points
            {
                targetType = (BuildingType)EditorGUILayout.EnumMaskField("Target type :", targetType);
            }

            if (GUILayout.Button("Create " + creatingType.ToString()))
            {
                if (creatingType == ModifierType.Socket) buildingEditor.script.CreateSocket(propertyName, positionAnchor, previewGameObject, receivesBuildings, socketPlacingType);
                else if (creatingType == ModifierType.Condition) buildingEditor.script.CreateCondition(propertyName, positionAnchor, condition == null ? typeof(BaseCondition) : condition.GetClass());
                else if (creatingType == ModifierType.SnapPoint) buildingEditor.script.CreateSnapPoint(propertyName, positionAnchor, targetType);

                Close();
            }
            else

                EditorGUILayout.EndVertical();
        }

    }

    public enum ModifierType
    {
        Condition,
        SnapPoint,
        Socket
    }
}