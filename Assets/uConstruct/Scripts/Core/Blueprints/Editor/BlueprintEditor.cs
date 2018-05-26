using UnityEngine;
using UnityEditor;

using uConstruct;
using uConstruct.Core.Blueprints;

using System.Collections;
using System.Collections.Generic;

using System.IO;

public class BlueprintEditor : EditorWindow
{
    public const KeyCode exportBtn = KeyCode.LeftControl;

    Texture2D _dropdown_down_icon;
    Texture2D dropdown_down_icon
    {
        get
        {
            if (_dropdown_down_icon == null)
            {
                _dropdown_down_icon = Resources.Load<Texture2D>("Icons/Dropdown_Down_Dark");
            }

            return _dropdown_down_icon;
        }
    }

    Texture2D _dropdown_right_icon;
    Texture2D dropdown_right_icon
    {
        get
        {
            if (_dropdown_right_icon == null)
            {
                _dropdown_right_icon = Resources.Load<Texture2D>("Icons/Dropdown_Right_Dark");
            }

            return _dropdown_right_icon;
        }
    }

    List<Blueprint> loadedBlueprints = new List<Blueprint>();

    public Blueprint currentBlueprint;
    public BlueprintFieldData currentChild;
    public BlueprintField lastFocusedBlueprint;

    GUIStyle invisibleButtonStyle;
    GUIStyle boxStyle;

    [System.NonSerialized]
    GUIStyle _centeredStyle = null;
    GUIStyle centeredStyle
    {
        get
        {
            if (_centeredStyle == null)
            {
                _centeredStyle = GUI.skin.GetStyle("Label");
                _centeredStyle.contentOffset = new Vector2(0, 7.5f);
            }

            return _centeredStyle;
        }
    }

    Vector2 blueprintListScrollPos = new Vector2();
    Vector2 fieldsListScrollPos = new Vector2();

    [MenuItem("Window/UConstruct/Blueprints", priority = -2)]
    public static void OpenWindow()
    {
        var instance = GetWindow<BlueprintEditor>();
        instance.LoadBlueprints();
    }

    void LoadBlueprints()
    {
        loadedBlueprints.Clear();

        string[] files = Directory.GetFiles(Blueprint.BLUEPRINT_ASSET_PATH);
        Blueprint current;

        for (int i = 0; i < files.Length; i++)
        {
            current = AssetDatabase.LoadAssetAtPath<Blueprint>(files[i]);

            if (current != null)
            {
                loadedBlueprints.Add(current);
            }
        }
    }

    void Update()
    {
        this.Repaint();
    }

    void OnGUI()
    {
        UC_EditorUtility.OnGUI();

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

        EditorGUILayout.BeginHorizontal();
        DrawBlueprintsList();
        DrawBlueprintsEditor();
        EditorGUILayout.EndHorizontal();
    }

    void DrawBlueprintsList()
    {
        Blueprint tempBP;

        GUILayout.BeginVertical("Blueprints", boxStyle);

        GUILayout.Space(15);

        blueprintListScrollPos = GUILayout.BeginScrollView(blueprintListScrollPos);

        for (int i = 0; i < loadedBlueprints.Count; i++)
        {
            tempBP = loadedBlueprints[i];

            if (tempBP == currentBlueprint)
                GUI.backgroundColor = Color.cyan;

            GUILayout.BeginHorizontal(boxStyle);
            if (GUILayout.Button("Blueprint : " + tempBP.blueprintName, invisibleButtonStyle))
            {
                if (tempBP != currentBlueprint)
                {
                    currentBlueprint = tempBP;
                }
                else
                {
                    currentBlueprint = null;
                }
            }
            GUILayout.EndHorizontal();

            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.EndScrollView();


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+", invisibleButtonStyle, GUILayout.Height(20), GUILayout.Width(20)))
        {
            var bp = Blueprint.CreateBlueprint();

            if (bp != null)
            {
                LoadBlueprints(); // reset blueprints
            }
        }
        if (GUILayout.Button("-", invisibleButtonStyle, GUILayout.Height(20), GUILayout.Width(20)))
        {
            if (currentBlueprint != null)
            {
                currentBlueprint.Delete();
                LoadBlueprints();
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    void DrawBlueprintsEditor()
    {
        GUILayout.BeginVertical("Edit Blueprints", boxStyle, GUILayout.Width(300), GUILayout.ExpandHeight(true));

        GUILayout.Space(15);

        if (currentBlueprint != null)
        {
            BlueprintField tempField;

            currentBlueprint.blueprintName = EditorGUILayout.TextField("Blueprint name : ", currentBlueprint.blueprintName);

            GUILayout.Space(10);

            fieldsListScrollPos = EditorGUILayout.BeginScrollView(fieldsListScrollPos, boxStyle);

            for (int i = 0; i < currentBlueprint.fields.Count; i++)
            {
                tempField = currentBlueprint.fields[i];

                GUILayout.BeginVertical(boxStyle, GUILayout.Height(35));

                GUILayout.BeginHorizontal();

                GUI.enabled = tempField.data.Count > 0;
                if (GUILayout.Button(tempField.isOpen ? dropdown_down_icon : dropdown_right_icon, centeredStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    tempField.isOpen = !tempField.isOpen;
                }
                GUI.enabled = true;

                GUILayout.Label("Field : " + tempField.name, invisibleButtonStyle);

                if (GUILayout.Button("+", boxStyle, GUILayout.Width(25), GUILayout.Height(25)))
                {
                    BlueprintEditEditor.OpenWindow(this, false, tempField);
                }

                if (GUILayout.Button("-", boxStyle, GUILayout.Height(25), GUILayout.Width(25)))
                {
                    if (currentBlueprint != null)
                    {
                        if (EditorUtility.DisplayDialog("Delete", "Are you sure you want to delete this field? \nThis cannot be undone!", "Yes", "No"))
                        {
                            currentBlueprint.RemoveField(tempField);
                        }
                    }
                }

                GUILayout.EndHorizontal();

                if (tempField.isOpen)
                {
                    GUILayout.Space(5);

                    for (int b = 0; b < tempField.data.Count; b++)
                    {
                        currentChild = tempField.data[b];

                        GUILayout.BeginVertical(boxStyle, GUILayout.Height(35));
                        GUILayout.BeginHorizontal();

                        GUILayout.Label("Building : " + currentChild.name, invisibleButtonStyle);

                        if (GUILayout.Button("-", boxStyle, GUILayout.Height(20), GUILayout.Width(20)))
                        {
                            if (currentBlueprint != null)
                            {
                                if (EditorUtility.DisplayDialog("Delete", "Are you sure you want to delete this field? \nThis cannot be undone!", "Yes", "No"))
                                {
                                    tempField.data.Remove(currentChild);
                                }
                            }
                        }

                        GUI.enabled = currentChild.data != null && currentChild.data.Count != 0;

                        if (GUILayout.Button(new GUIContent("P", "Pack a gameObject data to this field"), boxStyle, GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            if ((currentChild.data == null || currentChild.data.Count == 0) ||
                                    (tempField.data.Count != 0 &&
                                        EditorUtility.DisplayDialog("Confirm Override", "Are u sure you want to override the existing data ? \nThis cannot be undone!", "Yes Continue", "No")))
                            {
                                lastFocusedBlueprint = tempField;

                                UC_EditorUtility.DisplayObjectField((GameObject go, int index) =>
                                {
                                    if (lastFocusedBlueprint != null)
                                    {
                                        lastFocusedBlueprint.data[index].Pack(go);
                                        currentBlueprint.Save();
                                    }
                                }, b, true);
                            }
                        }
                        if (GUILayout.Button(new GUIContent("Ex", "Export this child to a game object"), boxStyle, GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            if (EditorUtility.DisplayDialog("Prefab Save", "Do u want to save changes to the prefab ? \nPlease note this cannot be undone!.", "Yes Continue", "No"))
                            {
                                lastFocusedBlueprint = tempField;

                                UC_EditorUtility.DisplayObjectField((GameObject go, int index) =>
                                {
                                    if (lastFocusedBlueprint != null)
                                    {
                                        lastFocusedBlueprint.data[index].UnPack(go, true);
                                    }
                                }, b, true);
                            }
                            else
                            {
                                lastFocusedBlueprint = tempField;

                                UC_EditorUtility.DisplayObjectField((GameObject go, int index) =>
                                {
                                    if (lastFocusedBlueprint != null)
                                    {
                                        lastFocusedBlueprint.data[index].UnPack(go, false);
                                    }
                                }, b, true);
                            }
                        }

                        GUI.enabled = true;

                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                    }
                }

                GUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+", invisibleButtonStyle, GUILayout.Height(20), GUILayout.Width(20)))
            {
                BlueprintEditEditor.OpenWindow(this, true, null);
            }

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Apply Changes"))
            {
                if (currentBlueprint != null)
                {
                    currentBlueprint.Save();
                }
            }
        }
        else
        {
            GUILayout.Label("Please choose a blueprint in order to edit it.");
        }

        GUILayout.EndVertical();
    }
}

public class BlueprintEditEditor : EditorWindow
{
    GUIStyle invisibleButtonStyle;
    GUIStyle boxStyle;

    BlueprintEditor editor;
    GameObject sourceGO;
    BuildingType fieldType;

    bool creatingAField;
    BlueprintField currentField;

    public static void OpenWindow(BlueprintEditor editor, bool creatingAField, BlueprintField currentField)
    {
        var instance = GetWindow<BlueprintEditEditor>();

        instance.editor = editor;
        instance.creatingAField = creatingAField;
        instance.currentField = currentField;
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

        DrawCreate();
    }
    
    void DrawCreate()
    {
        if(creatingAField)
        {
            DrawCreateField();
        }
        else
        {
            DrawCreateFieldChild();
        }
    }

    void DrawCreateField()
    {
        GUILayout.BeginVertical("Create A Blueprint Field :", boxStyle);
        GUILayout.Space(15);

        if (editor != null && editor.currentBlueprint != null)
        {
            fieldType = (BuildingType)EditorGUILayout.EnumPopup("Field type :", fieldType);
            sourceGO = (GameObject)EditorGUILayout.ObjectField("Source GameObject :", sourceGO, typeof(GameObject), true);

            if (GUILayout.Button("Create"))
            {
                editor.currentBlueprint.AddField(new BlueprintField(fieldType, sourceGO));
                Close();
            }
        }
        else
        {
            if (editor == null)
                Close();
            else
                GUILayout.Label("Please choose a blueprint to continue.");
        }

        GUILayout.EndVertical();
    }

    void DrawCreateFieldChild()
    {
        GUILayout.BeginVertical("Create A Blueprint Field Child :", boxStyle);
        GUILayout.Space(15);

        if (editor != null && editor.currentBlueprint != null && currentField != null)
        {
            sourceGO = (GameObject)EditorGUILayout.ObjectField("Source GameObject :", sourceGO, typeof(GameObject), true);

            if (GUILayout.Button("Create"))
            {
                currentField.AddChild(sourceGO);
                Close();
            }
        }
        else
        {
            Close();
        }

        GUILayout.EndVertical();
    }

    void DrawEdit()
    {
        GUILayout.BeginVertical("Edit fields", "Box");
        GUILayout.Space(15);
        GUILayout.EndVertical();
    }

}