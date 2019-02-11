using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Undo redo item. We cheat (maybe not) Unity by creating a ScriptableObject to hold some custom data to undo/redo.
/// </summary>
//[System.Serializable]
//public class UndoRedoItem : ScriptableObject {
//    public string S;
//}

public class UNoteEditWindow : EditorWindow {

    private static UNote currentNote;
    
    static GUIStyle style; // selected style
    static GUIStyle emptyWindowStyle;

    static bool showOptions = false;

    Vector2 scrollPosition = Vector2.zero;
    Rect scale;

    public static UNoteEditWindow instance;

    private int currSelectedInstanceID = 0;

//    UndoRedoItem undoredo;

    // Add menu item to show Unotes Window
    //[MenuItem("GameObject/UNotes/Open Note", false, 0)]
    public static void InitWithSelectedNote () {
        UNote note = UNotesHInventory.GetNote(Selection.activeInstanceID);
        currentNote = note;
        if (currentNote == null)
            return;
        style = CreateStyleForNote(currentNote);
        emptyWindowStyle = CreateEmptyWindowStyle();
        UNoteEditWindow window = (UNoteEditWindow)EditorWindow.GetWindow (typeof (UNoteEditWindow), false, "UNote");
        window.Show();
        instance = window;
    }

   
    void OnSelectionChange()
    {
        //if (Selection.activeInstanceID != 0)
        if(UNotesHInventory.HasNote(currSelectedInstanceID)) // if we deselect a (previously selected) note, consolidate
            UNotesHInventory.ConsolidateRequest();
        InitWithSelectedNote();
        Refresh();
        Refresh();
        currSelectedInstanceID = Selection.activeInstanceID; // update currently selected instance id
    }

    public void Clear()
    {
        currentNote = null;
        Refresh();
    }

    static void Refresh()
    {
        if (UNotesWindow.instance != null)
        {
            UNotesWindow.instance.Repaint();
        }
        if(UNoteEditWindow.instance != null)
        {
            UNoteEditWindow.instance.Repaint();
        }
    }

    static GUIStyle CreateStyleForNote(UNote _note)
    {
        GUIStyle _style = new GUIStyle();
        _style = new GUIStyle();
        _style.normal.background = EditorGUIUtility.whiteTexture;
        _style.normal.textColor = _note.color;
        _style.fontSize = _note.fontSize;
        _style.fontStyle = (_note.bold) ? FontStyle.Bold : FontStyle.Normal;
        _style.padding = new RectOffset(4, 4, 3, 4);
        _style.richText = true;
        _style.wordWrap = true; //////////////// WRAP ENABLED
        _style.stretchHeight = true;
        return _style;
    }

    static GUIStyle CreateEmptyWindowStyle()
    {
        GUIStyle _style = new GUIStyle();
        _style.normal.textColor = Color.gray;
        _style.alignment = TextAnchor.MiddleCenter;
        _style.fixedHeight = 100;
        _style.stretchHeight = true;
        _style.stretchWidth = true;
        return _style;
    }

    void OnGUI()
    {
        if (currentNote == null)
        {
            GUIRenderEmpty();
            return;
        }

        // TITLE: an excerpt of the note text
        //EditorGUILayout.BeginHorizontal ();
        //EditorGUILayout.LabelField(currentNote.text.Substring(0,(int)Mathf.Min(30,currentNote.text.Length)));
        //EditorGUILayout.EndHorizontal ();

        // RENDER OPTIONS
        GUIRenderOptions();

        EditorGUILayout.BeginVertical();
        scrollPosition = GUILayout.BeginScrollView (scrollPosition);
        
        EditorGUI.BeginChangeCheck();

        GUI.backgroundColor = currentNote.bgColor;
        string newText = EditorGUILayout.TextArea(currentNote.text, style);

        // Get the TextEditor's reference to later get cursor's position
        // Not working. Question posted: http://answers.unity3d.com/questions/1286039/editorguiutilitygetstateobject-does-not-catch-info.html
        //        TextEditor editor = (TextEditor)EditorGUIUtility.GetStateObject(typeof(TextEditor), EditorGUIUtility.keyboardControl); // http://answers.unity3d.com/questions/275973/find-cursor-position-in-a-textarea.html

        if (EditorGUI.EndChangeCheck())
        {
            if (newText.Length > 8)
            {
                // UGLY but necessary workaround for Unity Editor TextArea
                if (newText.StartsWith(currentNote.text)) // NEEDED FOR WRAP WORDS WORKAROUND http://answers.unity3d.com/questions/1285184/editor-guilayoutscrollview-not-scrolling-when-typi.html#comment-1286027
                    scrollPosition.y = Mathf.Infinity;
            }

            //UNotesWindow.instance.Repaint();
//            Undo.RecordObject(undoredo, "UNote changed");
//            undoredo.S = newText;
//
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()); // to allow the sceneSaved event to be triggered when editing notes.
            currentNote.text = newText;
        }

        GUILayout.EndScrollView ();
        EditorGUILayout.EndVertical ();
    }

    void GUIRenderOptions()
    {
        if(showOptions = EditorGUILayout.Foldout(showOptions, "Options"))
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();

            // Font Size
            EditorGUILayout.LabelField("Font Size", GUILayout.MaxWidth(70f));
            EditorGUI.BeginChangeCheck();
            currentNote.fontSize = EditorGUILayout.IntField(currentNote.fontSize, GUILayout.MaxWidth(35f));
            currentNote.fontSize = Mathf.Max(Mathf.Min(currentNote.fontSize, 256), 1);
            if (EditorGUI.EndChangeCheck())
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()); // to allow the sceneSaved event to be triggered when editing notes.
                style.fontSize = currentNote.fontSize;
                Refresh();
            }

            // Font Bold
            EditorGUILayout.LabelField("B", EditorStyles.boldLabel, GUILayout.MaxWidth(12f));
            EditorGUI.BeginChangeCheck();
            currentNote.bold = EditorGUILayout.Toggle(currentNote.bold, GUILayout.MaxWidth(10f));
            if (EditorGUI.EndChangeCheck())
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()); // to allow the sceneSaved event to be triggered when editing notes.
                style.fontStyle = (currentNote.bold) ? FontStyle.Bold : FontStyle.Normal;
                Refresh();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Foreground Color
            EditorGUILayout.LabelField("Colors  TXT/BG", GUILayout.MaxWidth(100f));
            EditorGUI.BeginChangeCheck();
            currentNote.color = EditorGUILayout.ColorField(currentNote.color, GUILayout.MaxWidth(60f));
            if (EditorGUI.EndChangeCheck())
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()); // to allow the sceneSaved event to be triggered when editing notes.
                style.normal.textColor = currentNote.color;
                Refresh();
            }

            //EditorGUILayout.Space();
            //EditorGUILayout.Space();

            // Background Color
            //EditorGUILayout.LabelField("BG", GUILayout.MaxWidth(20f));
            EditorGUI.BeginChangeCheck();
            currentNote.bgColor = EditorGUILayout.ColorField(currentNote.bgColor, GUILayout.MaxWidth(60f));
            if(EditorGUI.EndChangeCheck())
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()); // to allow the sceneSaved event to be triggered when editing notes.
                Refresh();
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
        }

    }

    void GUIRenderEmpty()
    {
        if(emptyWindowStyle == null)
        {
            emptyWindowStyle = CreateEmptyWindowStyle();
        }

        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Select a GameObject\nwith a note\nin the Hierarchy\nor Add Note to a GameObject in the Hierarchy", emptyWindowStyle);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

    }

//    void OnEnable()
//    {
//        undoredo = new UndoRedoItem();
//        Undo.undoRedoPerformed -= OnUndoRedo;
//        Undo.undoRedoPerformed += OnUndoRedo;
//    }
//
//    static void OnUndoRedo()
//    {
//        currentNote.text = instance.undoredo.S;
//    }

}
