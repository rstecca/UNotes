using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[System.Serializable]
public struct SerializableColor {
    
	public static SerializableColor white { get { return new SerializableColor (Color.white); } }
	public static SerializableColor black { get { return new SerializableColor (Color.black); } }

    public float R,G,B;

    public SerializableColor(Color c)
    {
        R = c.r;
        G = c.g;
        B = c.b;
    }

    public Color GetColor()
    {
        return new Color(R,G,B);
    }

    public float[] GetValues()
    {
        return new float[4] { R, G, B, 1f };
    }
}


/// <summary>
/// UNotesOptions wraps all options that need to be stored and loaded at request.
/// Because they can be requested at any time, UNotesOptions uses a singleton pattern
/// that loads from the file when instance requested for the first time.
/// </summary>
[System.Serializable]
public class UNotesOptions {

    public string fileName;
    public string filePath { get { return (Directory.GetParent(Application.dataPath).ToString() + "/" + fileName); } }
    public SerializableColor textColor;
    public SerializableColor bgColor;
    public bool bold;
    public int fontSize;
    public bool debug_mode;

    static string _settingsFile = "";
    static string settingsFile {
        get {
            if(string.IsNullOrEmpty(_settingsFile))
                _settingsFile = (Directory.GetParent(Application.dataPath)).ToString() + "/ProjectSettings/UNotesOptions.dat";
            return _settingsFile;
        }
    }

    private static UNotesOptions _instance;
    public static UNotesOptions instance
    {   get {
            if (_instance == null)
            {
                _instance = new UNotesOptions();
                // Load or create options
				Load();
//                if (File.Exists(settingsFile))
//                {
//                    Load();
//                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UNotesOptions"/> class.
    /// </summary>
    public UNotesOptions()
    {
        
    }


    public static void Save()
    {
        Debug.Log("Saving options to " + settingsFile);
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        System.IO.FileStream fs = new System.IO.FileStream(settingsFile, System.IO.FileMode.Create);
        bf.Serialize(fs, UNotesOptions.instance);
        fs.Close();
    }

    public static void Load()
    {
        Debug.Log("UNotes: Loading options from " + settingsFile);
		if (!System.IO.File.Exists(settingsFile) || string.IsNullOrEmpty(settingsFile))
        {
			// Default values
			//instance.filePath = (Directory.GetParent (Application.dataPath)).ToString () + "/UNotesDatabase.dat";
            instance.fileName = "UNotesDatabase.dat";
			instance.textColor = SerializableColor.white;
			instance.bgColor = SerializableColor.black;
			instance.fontSize = 12;
			instance.bold = false;

            Debug.LogWarning(settingsFile + " file does not found.");
            return;
        }
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        System.IO.FileStream fs = new System.IO.FileStream(settingsFile, System.IO.FileMode.OpenOrCreate);
        UNotesOptions options = (UNotesOptions)bf.Deserialize(fs); // https://msdn.microsoft.com/en-us/library/hh158566(v=vs.110).aspx
        //instance.filePath = options.filePath;
        instance.fileName = options.fileName;
        instance.textColor = options.textColor;
        instance.bgColor = options.bgColor;
        instance.bold = options.bold;
        instance.fontSize = options.fontSize;
        fs.Close();
    }
}

public class UNotesOptionsWindow : EditorWindow {
    
    // OPTION FIELDS
//    string filePath;
    Color textColor; // we need colors as we must convert to SerializableColor
    Color bgColor;
//    int fontSize;
//    bool bold;

    Vector2 scroll;

    UNotesOptions options;
    bool showAdvanced = false;

    public static bool DEBUG_MODE = false;

    void OnEnable()
    {
        
    }

    [MenuItem ("Window/UNotes/Options")]
    static void Init () {
        UNotesOptionsWindow window = (UNotesOptionsWindow)EditorWindow.GetWindow (typeof (UNotesOptionsWindow), true, "UNotes Options", true);
//        window.minSize = new Vector2(300f, 220f);
        //window.maxSize = new Vector2(300f, 220f);
        window.Show();
    }

    void OnGUI()
    {
        options = UNotesOptions.instance; // get the instance

        EditorStyles.label.wordWrap = true;

        EditorGUI.BeginChangeCheck();

        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Default Style", EditorStyles.boldLabel);

        textColor = EditorGUILayout.ColorField("Text Color", options.textColor.GetColor());
        bgColor = EditorGUILayout.ColorField("Backgr. Color", options.bgColor.GetColor());

        options.fontSize = EditorGUILayout.IntField("Font Size", options.fontSize);
        options.bold = EditorGUILayout.Toggle("Bold", options.bold);

        showAdvanced = EditorGUILayout.Toggle("Show Advanced", showAdvanced);

        if (showAdvanced)
        {
            EditorGUILayout.Space();
            Color guiCurrentColor = GUI.color;
            GUI.color = Color.red;
            EditorGUILayout.LabelField("ADVANCED OPTIONS", EditorStyles.boldLabel);
            GUI.color = new Color(1f,.7f,0f);
            EditorGUILayout.LabelField("Backup your files!\nPlease note the following options can make you lose your work if not properly set.");
            GUI.color = guiCurrentColor;

            EditorGUILayout.BeginHorizontal();
            //options.filePath = EditorGUILayout.TextField("File Location", options.filePath);
            options.fileName = EditorGUILayout.TextField("UNotes database file name", options.fileName);
            //        if (GUILayout.Button("Set...", GUILayout.MaxWidth(40f)))
            //        {
            //            options.filePath = EditorUtility.OpenFilePanel("UNotes database file", Application.dataPath, "dat");
            //            //options.filePath = EditorUtility.SaveFilePanel("UNotes database file", Application.dataPath, "UNotesDatabase", "dat");
            //        }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();
            EditorStyles.label.wordWrap = true;
            EditorGUILayout.LabelField("TIP: When changing file name, save your project immediately to store all notes in the new database file.", EditorStyles.helpBox);
            DEBUG_MODE = EditorGUILayout.Toggle("DEBUG MODE", DEBUG_MODE);
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();

        if (EditorGUI.EndChangeCheck())
        {
            options.textColor = new SerializableColor(textColor);
            options.bgColor = new SerializableColor(bgColor);
        }

        if (GUILayout.Button("Save and Close"))
        {
            UNotesOptions.Save();
            Close();
        }
    }

}
