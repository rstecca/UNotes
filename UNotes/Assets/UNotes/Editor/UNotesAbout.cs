using UnityEngine;
using UnityEditor;
using System.Collections;

public class UNotesAbout : EditorWindow
{

    [MenuItem("Window/UNotes/About")]
    public static void About()
    {
        UNotesAbout window = (UNotesAbout)EditorWindow.GetWindow(typeof(UNotesAbout), true, "About UNotes", true);
        window.minSize = new Vector2(300f, 220f);
        window.maxSize = new Vector2(300f, 220f);
        window.Show();
        window.wantsMouseMove = false;
    }

    void OnGUI()
    {
        EditorStyles.label.wordWrap = true;
        EditorGUILayout.LabelField("UNotes 2.0\n" +
            "For any suggestions or complaints please write to unitysupport@riccardostecca.net.");
    }
}
