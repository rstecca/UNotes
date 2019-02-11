using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UNotesTestingWindow : EditorWindow {

    // Add menu item to show Unotes Window
    [MenuItem ("Window/UNotes/Debug Window")]
    static void Init () {
        UNotesTestingWindow window = (UNotesTestingWindow)EditorWindow.GetWindow (typeof (UNotesTestingWindow), false, "Testing Window");
        window.Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Consolidate"))
        {
            UNotesHInventory.SYNC_TEST();
        } else if (GUILayout.Button("Restore"))
        {
            UNotesHInventory.RESTORE_TEST();
        } else if (GUILayout.Button("Load"))
        {
            UNotesHInventory.LOAD_TEST();
        } else if (GUILayout.Button("Save"))
        {
            UNotesHInventory.SAVE_TEST();
        } else if (GUILayout.Button("Debug DB"))
        {
            UNotesHInventory.DEBUG_DB();
        } else if (GUILayout.Button("Debug Current"))
        {
            UNotesHInventory.DEBUG_CURRENT();
        } else if (GUILayout.Button("Debug Scene Hash"))
        {
            UNotesHInventory.DEBUG_SCENEHASH();
        } else if (GUILayout.Button("Debug Hash Functions"))
        {
            UNotesHInventory.DEBUG_HASHFUCNTIONS();
        }
        else if(GUILayout.Button("Repaint Hierarchy"))
        {
            EditorApplication.RepaintHierarchyWindow();
        }
        else if(GUILayout.Button("Print Note's Hash"))
        {
            UNotesHInventory.PrintInfo();
        }
        //else if (GUILayout.Button("Reattach to GameObjects"))
        //{

        //}
    }

}
