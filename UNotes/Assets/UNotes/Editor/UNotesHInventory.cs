using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text;

/// <summary>
/// Hinventory: Hierarchy + Inventory
/// Hierarchy based in-Editor inventory or collection
/// </summary>
[InitializeOnLoad]
public static class UNotesHInventory {


    /*
     * CONSTANTS AND VARIABLES
     */
    #region COMMON_CONSTANT_AND_VARIABLES

    //static string datFile;
//    static string datFilePath {
//        get {
//            return datPath +  "HIerarchy.dat";
//        }
//    }
    //static string mainDatFilename = "HIerarchy.dat";

    #endregion

    // _-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_

    #region COMMON_METHODS


    static void log(string message, bool __printStack = false)
    {
        if(UNotesOptionsWindow.DEBUG_MODE)
            Debug.Log("Hinventory: " + message); // Commented out for production
        if (__printStack)
            Debug.Log(System.Environment.StackTrace);
    }


    #endregion

    // _-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_

    #region EVENT DELEGATES
    public delegate void OnNoteAdded_delegate(int _instanceID, UNote _note);//, GameObject _gameObject);
    public static OnNoteAdded_delegate OnNoteAdded;
    public delegate void OnNoteRemoved_delegate(int _instanceID, UNote _note);//, GameObject _gameObject);
    public static OnNoteAdded_delegate OnNoteRemoved;
    public delegate void OnObjectWithNoteDeleted_delegate(GameObject _go);
    public static OnObjectWithNoteDeleted_delegate OnObjectWithNoteDeleted;
    #endregion

    // _-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_


    /*
     * CONSTRUCTOR
     */
    /// <summary>
    /// Initializes the <see cref="Hinventory"/> class.
    /// </summary>
    static UNotesHInventory()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyDraw;
        EditorApplication.hierarchyWindowChanged += HierarchyChanged;
        UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += SceneOpenedCallback;
        UnityEditor.SceneManagement.EditorSceneManager.sceneSaved += SceneSavedCallback;

        //System.IO.DirectoryInfo di = System.IO.Directory.GetParent(Application.dataPath);
        //datPath = di.ToString() + "/";
        //datFile = UNotesOptions.instance.filePath;
        LoadOrCreate(); // PRELOAD DATA
        guiIconStyle = new GUIStyle();

    }

	// _-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_

	#region SCENE EDITOR EVENTS DELEGATES
    static void SceneOpenedCallback(Scene _scene, UnityEditor.SceneManagement.OpenSceneMode _mode)
    {
        log("SCENE LOADED");
        LoadOrCreate();
    }

    static void SceneSavedCallback(Scene _scene)
    {
        log("SCENE SAVED");
        Consolidate();
        Save();
    }
	#endregion

    // _-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_

    /*
     * HIERARCHY RELATED CALLBACKS 
     */

    static GUIStyle guiIconStyle;

    static void HierarchyDraw(int _instanceID, Rect _selectionRect)
    {
        if(!collection.ContainsKey(_instanceID))
            return;

        Rect r = _selectionRect;
        r.x += _selectionRect.width - 10f;
        r.width = 30f;
        Color guiCurrentColor = GUI.color;
        //GUI.color = collection [_instanceID].bgColor;
        guiIconStyle.normal.textColor = collection[_instanceID].bgColor;
        guiIconStyle.fontStyle = FontStyle.Bold;
        guiIconStyle.fontSize = 12;
        GUI.Label(r, "•", guiIconStyle);
        guiIconStyle.normal.textColor = collection[_instanceID].color;
        guiIconStyle.fontStyle = FontStyle.Normal;
        guiIconStyle.fontSize = 10;
        r.x += 1f;
        r.y += 1f;
        GUI.Label(r, "•", guiIconStyle);
        GUI.color = guiCurrentColor;
    }

    static bool firstLoad = true;
    static void HierarchyChanged()
    {
		if (EditorApplication.isPlaying)
			return;
		
        //if (firstLoad)
        //{
        //    LoadOrCreate();
        //    firstLoad = false;
        //    Debug.Log("UNotes loaded");
        //}
        Consolidate();
//        Restore();
    }


    //_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_

    /*
     * CURRENT, VOLATILE COLLECTION OF NOTES BASED ON GameObject.instanceID FIELD.
     * instanceID IS NOT PERSISTENT BETWEEN SCENE LOADINGS.
     */

    #region CURRENT_VOLATILE_COLLECTION

    /// <summary>
    /// The collection is the volatile side of the database which stores notes
    /// by indexing them with the object's instanceID they are attached to.
    /// </summary>
    static Dictionary<int, UNote> collection; // current collection instanceID/Note

    /// <summary>
    /// Returns all notes as a list.
    /// </summary>
    public static List<UNote> notes
    {
        get
        {
            List<UNote> notesList = new List<UNote>();
            foreach(int K in collection.Keys)
            {
                notesList.Add(collection [K]);
            }
            return notesList;
        }
    }

    /// <summary>
    /// Adds the note menu callback.
    /// </summary>
    [MenuItem("GameObject/UNotes/Add\\Edit Note", false, 0)]
    static void AddNoteMenuCallback()
    {
        // AddNote(Selection.activeInstanceID, new UNote("TEST NOTE " + Selection.activeInstanceID.ToString(), Selection.activeGameObject, UNotesOptions.instance));
        AddNote(Selection.activeInstanceID, new UNote("New Note", Selection.activeGameObject, UNotesOptions.instance));
        UNoteEditWindow.InitWithSelectedNote();
    }

    /// <summary>
    /// Adds the note menu callback.
    /// </summary>
    [MenuItem("GameObject/UNotes/Remove Note", false, 0)]
    public static void RemoveNoteMenuCallback()
    {
        RemoveNote(Selection.activeInstanceID);
        UNoteEditWindow.instance.Clear();
    }

    // [MenuItem("GameObject/UNotes/Print Hash", false, 0)]
    public static void PrintInfo()
    {
        log("" + GetObjectHash( Selection.activeTransform.gameObject.GetInstanceID()) );
    }

//    [MenuItem("Window/UNotes/Export to TXT file/All notes")]
//    public static void ExportAllToTXT()
//    {
//        //throw new System.NotImplementedException();
//        foreach(string sh in persistentDatabase.Keys)
//        {
//            Dictionary<string, UNote> nList = persistentDatabase [sh]; // scenehash
//            foreach (string k in nList.Keys)
//            {
//                if (nList [k].gameObject == null)
//                {
//                    Debug.LogError("null game object for note " + k);
//                    return;
//                }
//                
//                Transform hierarchyExplore = nList [k].gameObject.transform;
//                
//                StringBuilder sb = new StringBuilder();
//                while (hierarchyExplore != null)
//                {
//                    sb.Append(hierarchyExplore.name);
//                    //SHDAISUDHIAUHDIAUHSDIUd
//                    hierarchyExplore = hierarchyExplore.transform.parent;
//                }
//                Debug.Log(k + "  |  " + sb);
//            }
//        }
//        // TO DO
//    }

    /// <summary>
    /// Adds the note.
    /// </summary>
    /// <param name="_instanceID">Instance I.</param>
    /// <param name="_note">Note.</param>
    static void AddNote(int _instanceID, UNote _note)
    {
        if (collection.ContainsKey(_instanceID))
        {
            Debug.LogWarning("Only one note per GameObject is allowed.");
        }
        else
        {
            collection.Add(_instanceID, _note);
            if (OnNoteAdded != null)
                OnNoteAdded(_instanceID, _note);
        }
    }


    /// <summary>
    /// Removes the note.
    /// </summary>
    /// <param name="_instanceID">Instance I.</param>
    static void RemoveNote(int _instanceID)
    {
        UNote tmpNote = collection[_instanceID];
        collection.Remove(_instanceID);
        if (OnNoteRemoved != null)
            OnNoteRemoved(_instanceID, tmpNote);
    }

    /// <summary>
    /// Gets the note from the object's instance ID
    /// </summary>
    /// <returns>The note.</returns>
    /// <param name="_instanceID">Instance ID</param>
    public static UNote GetNote(int _instanceID)
    {
        if (collection.ContainsKey(_instanceID))
        {
            return collection [_instanceID];
        }
        else
        {
            return null;
        }
            
    }


    public static bool HasNote(int _instanceID)
    {
        return collection.ContainsKey(_instanceID);
    }
    #endregion


    // _-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_


    #region PERSISTENT_DATABASE


    static Dictionary<string, Dictionary<string, UNote>> persistentDatabase;

    /// <summary>
    /// Gets the scene hash.
    /// </summary>
    /// <value>The scene hash.</value>
    static string sceneHash {
        get {
            //eturn SceneManager.GetActiveScene().path.Replace("/", "").Replace(".unity", "");
            return AssetDatabase.AssetPathToGUID(SceneManager.GetActiveScene().path);
        }
    }

    static string[] separators = new string[] { "//" }; // for object hashing

    /// <summary>
    /// Gets the index of the object child.
    /// </summary>
    /// <returns>The object child index.</returns>
    /// <param name="_go">Go.</param>
    static int GetObjectChildIndex(GameObject _go)
    {
        //Assert.IsNotNull(_go);
        if (_go == null)
            throw new GameObjectNotFoundException();

        if (_go.transform.parent == null)
        {
            GameObject[] _roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < _roots.Length; i++)
            {
                if (_roots [i] == _go)
                {
                    return i;
                }
            }
            throw new GameObjectNotFoundException();
        }
        else
        {
            int i = 0;
            foreach (Transform T in _go.transform.parent)
            {
                if (T.gameObject == _go);
                    return i;
                i++;
            }
            throw new GameObjectNotFoundException();
        }
    }

    /// <summary>
    /// Object's the hash from instanceID.
    /// </summary>
    /// <returns>The hash.</returns>
    /// <param name="_instanceID">Instance ID</param>
    static string GetObjectHash(int _instanceID)
    {
        GameObject go = EditorUtility.InstanceIDToObject(_instanceID) as GameObject;
        if (go == null)
            return null;

        string hash = GetObjectChildIndex(go).ToString();

        if (go.transform.parent == null)
            return hash;
        else
            go = go.transform.parent.gameObject;
            
        while (true)
        {
            try {
                int idx = GetObjectChildIndex(go);
                hash = idx.ToString() + "//" + hash;
                if (go.transform.parent == null)
                {
                    break;
                }
                else
                {
                    go = go.transform.parent.gameObject;
                }
            } catch (GameObjectNotFoundException E)
            {
                log(E.ToString());
            }
        }

        return hash;
    }

    static int? GetInstanceID(string _objectHash)
    {
        // String tokens that translate to child indexes relative to the parent
        string[] indexes = _objectHash.Split(separators, System.StringSplitOptions.None);
        GameObject[] _roots = SceneManager.GetActiveScene().GetRootGameObjects();

        GameObject currentGO = _roots[int.Parse(indexes[0])];
        try {
            for (int i = 1; i < indexes.Length; i++)
            {
                if (currentGO == null)
                    return null;
                int index = int.Parse(indexes [i]);
                Transform child = currentGO.transform.GetChild(index);
                currentGO = child.gameObject;
            }
        } catch (UnityException e)
        {
            return null;
        }

        return currentGO.GetInstanceID();
    }


    /// <summary>
    /// Serializes the persistentDatabase to disk
    /// </summary>
    /// <param name="filename">the dat Filename</param>
    static void DataDump(string filename)
    {
        Debug.Log("UNotes: Saving to " + filename);
        Assert.IsNotNull(persistentDatabase);
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.OpenOrCreate);
        bf.Serialize(fs, persistentDatabase);
        fs.Close();
    }
    /// <summary>
    /// Deserializes the datafile into the persistentDatabase
    /// </summary>
    /// <param name="filename">the dat filename</param>
    static void DataUndump(string filename)
    {
        if (!System.IO.File.Exists(filename))
        {
            Debug.LogError(filename + " file does not exist.");
            return;
        }
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open);
        persistentDatabase = (Dictionary<string, Dictionary<string, UNote>>)bf.Deserialize(fs); // https://msdn.microsoft.com/en-us/library/hh158566(v=vs.110).aspx
        fs.Close();
    }

    #endregion


    // _-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_


    #region CONSISTENCY_AND_SYNCH
    // where the database and the collection are kept consistent

    static void LoadOrCreate()
    {
        log("LoadOrCreate");
        if (System.IO.File.Exists(UNotesOptions.instance.filePath))
        {
            Load();
            Restore();

        } else
        {
            InitializeBrandNewDatabases();
        }
    }


    /// <summary>
    /// 
    /// </summary>
    static void InitializeBrandNewDatabases()
    {
        //Dictionary<string, string> current = new Dictionary<string, string>();

        collection = new Dictionary<int, UNote>();

        persistentDatabase = new Dictionary<string, Dictionary<string, UNote>>();
        persistentDatabase.Add(sceneHash, new Dictionary<string, UNote>());
    }

    /// <summary>
    /// Just a public API for consolidation
    /// </summary>
    public static void ConsolidateRequest()
    {
        Consolidate();
    }
    /// <summary>
    /// Updates the persistentDatabase with the current collection content.
    /// </summary>
    static void Consolidate()
    {
        log("Consolidate");
        Assert.IsNotNull(sceneHash);
        string _sceneHash = sceneHash;

        // Make sure the entry exists or create it
        if (!persistentDatabase.ContainsKey(sceneHash))
        {
            persistentDatabase.Add(sceneHash, new Dictionary<string, UNote>());
        }
        else
        {
            persistentDatabase [sceneHash].Clear(); // = new Dictionary<string, string>();
        }
        Dictionary<string, UNote> scenePersistentCollection = persistentDatabase [sceneHash]; // persistentDatabase [sceneHash];
        //

        List<int> markForDeletion = new List<int>();

        foreach (int K in collection.Keys)
        {
            string _oHash = GetObjectHash(K);

            if (_oHash == null) //Object with note DELETED");
            {
                markForDeletion.Add(K);
            }
            else
            {
                if (!scenePersistentCollection.ContainsKey(_oHash))
                {
                    scenePersistentCollection.Add(_oHash, collection [K]);
                } else
                {
                    scenePersistentCollection [_oHash] = collection [K];
                }
            }
        }

        for (int i = 0; i < markForDeletion.Count; i++)
        {
            RemoveNote(markForDeletion [i]);
        }
    }

    static void Restore()
    {
        log("Restore");
        if (collection != null)
            collection.Clear();
        else
            collection = new Dictionary<int, UNote>();
        
        if (persistentDatabase.ContainsKey(sceneHash))
        {
            foreach (string objectHash in persistentDatabase[sceneHash].Keys)
            {
                int? _ID = GetInstanceID(objectHash);
                if (_ID.HasValue)
                {
                    UNote n = persistentDatabase[sceneHash][objectHash];
                    if (n.gameObject == null)
                    {
                        // in case the database item has an inconsistency on the game object (may happen when using an old DB).
                        // Debug.LogWarning("Null game object repaired."); // Commented out for production
                        n.gameObject = (GameObject) EditorUtility.InstanceIDToObject(_ID.Value);
                    }
                    collection.Add(_ID.Value, n);
                }
                else
                {
                    log("One object was removed. Object hash: " + objectHash);
                }
            }
        }
        EditorApplication.RepaintHierarchyWindow();
    }

    static void Save()
    {
        DataDump(UNotesOptions.instance.filePath);
    }

    static void Load()
    {
        DataUndump(UNotesOptions.instance.filePath);
    }

    #endregion


    // _-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_

    #region Editor Utilities

    public static void SelectObjectWithNote(UNote _note)
    {
        if (collection.ContainsValue(_note))
        {
            int __instanceID = collection.FirstOrDefault(x => x.Value == _note).Key;
            Selection.activeInstanceID = __instanceID;
        }
    }

    #endregion

    // _-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_

    #region testing_API

    public static void SYNC_TEST()
    {
        Consolidate();
    }

    public static void RESTORE_TEST()
    {
        Restore();
    }

    public static void LOAD_TEST()
    {
        Load();
    }

    public static void SAVE_TEST()
    {
        Save();
    }

    public static void DEBUG_DB()
    {
        Assert.IsNotNull(persistentDatabase);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (string K in persistentDatabase.Keys)
        {
            sb.Append("<~").Append(K).Append("~>").AppendLine();
            Dictionary<string, UNote> _collection = persistentDatabase [K];
            foreach (string KK in _collection.Keys)
            {
                sb.Append("<").Append(KK).Append(", ").Append(_collection[KK]).Append(">").AppendLine();
            }
        }
        Debug.Log(sb.ToString());
    }

    public static void DEBUG_CURRENT()
    {
        Assert.IsNotNull(collection);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (int K in collection.Keys)
        {
            sb.Append("<").Append(K).Append(",").Append(collection[K]).Append(">").AppendLine();
        }
        Debug.Log(sb.ToString());
    }

    public static void DEBUG_SCENEHASH()
    {
        Assert.IsNotNull(sceneHash);
        Assert.IsFalse(string.IsNullOrEmpty(sceneHash), "Scene is not saved.");
        Debug.Log(sceneHash);
    }

    public static void DEBUG_HASHFUCNTIONS()
    {
        log("" + Selection.activeObject.GetInstanceID() + " = = " + GetInstanceID( GetObjectHash( Selection.activeObject.GetInstanceID() ) ));
    }

    //public static void DEBUG_REATTACH_GAMEOBJECTS()
    //{
    //    foreach (var item in collection)
    //    {
    //        Selection.activeInstanceID()
    //    }
    //}

    #endregion

}



public class GameObjectNotFoundException : System.Exception {

    public GameObjectNotFoundException()
    {
        
    }

}