using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class UNotesWindow : EditorWindow {

	UNote[] notes;

//	GUIStyle[] styles;

    GUIStyle dynamicNoteStyle; // dynamic because it changes throughout the rendering of all notes
    GUIStyle draggingBarStyle;

	Texture2D whiteBgTex;

	Rect scale;

	const int maxCharacters = 40;

	public static UNotesWindow instance;

	Vector2 scrollPos = Vector2.zero;
    bool scrollBarsVisible = false;

	float noteHeight = 30f;
	float noteSpacing = 5f;

	bool isDragging = false;
	int draggingFromNote = -1;
	int dragginCurrentNote = -1;

	// HEADER MENU
	string searchString = "";
	int[] searchResultIndexes;
	float headerMenuGUITopOffset = 0f; //to compensate the space taken by search and other tools - VERY IMPORTANT FOR DRAGGING

	bool isSearchMode { get { return (searchString.Length > 1); } }

    int justLoadedCount = 10; // To prevent a Unity internal error (related to GetLastRect during WindowLayout loading) from showing up. This unfortunately makes the UNotes Window empty until it's clicked, but only when loading the project with the window open.

	// Add menu item to show Unotes Window
	[MenuItem ("Window/UNotes/Show Block")]
	static void Init () {
		UNotesWindow window = (UNotesWindow)EditorWindow.GetWindow (typeof (UNotesWindow), false, "UNotes Block");
		window.Show();
    }
    
    [MenuItem("Window/UNotes/Export to TXT file/Current Scene")]
	public static void ExportToTXT()
	{
		if(instance == null)
			Init();
		string filename = "UNotes_" + System.DateTime.Now.ToString("ddMMMyyyy_hhmmss") + ".txt";
		string path = EditorUtility.SaveFilePanel ("Unotes CSV Export", Application.dataPath, filename, "txt"); //Application.dataPath + "/" + filename;
        if (string.IsNullOrEmpty(path))
            return;
		System.IO.StreamWriter fsw = System.IO.File.CreateText (path);
		for (int i = 0; i < instance.notes.Length; i++)
		{
			fsw.WriteLine ("\""+instance.notes[i].text+"\"");
		}
		Debug.Log ("UNotes exported to "+path);
		fsw.Close ();
		AssetDatabase.Refresh ();
		EditorApplication.RepaintProjectWindow ();
	}

	//--------------------------------------------------------------------------------------------

	void OnEnable()
	{
        //Debug.Log("ONENABLE");
		//instance = this;
		whiteBgTex = EditorGUIUtility.whiteTexture;
        draggingBarStyle = CreateDraggingBarStyle();
		instance = this;
		//
		//getAndSortNotes (); // done as late initialization
                            //		createStyles (); // done as late initialization
        Repaint (); // done as late initialization

        UNotesHInventory.OnNoteAdded -= OnNoteAdded;
        UNotesHInventory.OnNoteRemoved -= OnNoteRemoved;
        UNotesHInventory.OnNoteAdded += OnNoteAdded;
        UNotesHInventory.OnNoteRemoved += OnNoteRemoved;
    }

    //--------------------------------------------------------------------------------------------

    #region Hinventory CALLBACKS
    static void OnNoteAdded(int _instanceID, UNote _note)
    {
        Debug.Log("OnNoteAdded");
        instance.getAndSortNotes(); // done as late initialization
        instance.Repaint();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
    static void OnNoteRemoved(int _instanceID, UNote _note)
    {
        Debug.Log("OnNoteRemoved");
        instance.getAndSortNotes(); // done as late initialization
        instance.Repaint();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
    #endregion

    //--------------------------------------------------------------------------------------------

    void OnHierarchyChange()
	{
		getAndSortNotes (); // done as late initialization
		Repaint ();
	}

	//--------------------------------------------------------------------------------------------

    void OnGUI () {
		
//        if (justLoadedCount > 0)
//        {
//            justLoadedCount--;
//            return;
//        }

		if (whiteBgTex == null)
		{
			whiteBgTex = EditorGUIUtility.whiteTexture;
			//createStyles();
		}

		Color cGUI = GUI.color;

		if (notes == null)
		{
			getAndSortNotes ();
		}
		else
		{
			if(notes.Length == 0) // Here we suspect the notes weren't found yet. Issue at start due to script execution order
			{
				getAndSortNotes ();
			}
		}

		float w = EditorGUIUtility.currentViewWidth;

		EditorGUILayout.BeginHorizontal ();
		if (isSearchMode)
			GUI.color = Color.cyan;
		EditorGUILayout.LabelField ("Search UNotes", GUILayout.MaxWidth(85f));
		if (isSearchMode)
			GUI.color = cGUI;
		searchString = EditorGUILayout.TextField (searchString);
		EditorGUILayout.EndHorizontal ();

		headerMenuGUITopOffset = 20f; // Should be computed somehow

		EditorGUILayout.BeginHorizontal();
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width (w));

		if(isDragging && dragginCurrentNote == -1)
		{
			Repaint();
			GUI.color = Color.white;
			GUILayout.Space (1f);
            GUILayout.Label("", draggingBarStyle, GUILayout.Width(w), GUILayout.Height(3f));
			GUILayout.Space (1f);
		}

		if (isSearchMode)
		{
			if (searchResultIndexes != null)
			{
				for (int i = 0; i < searchResultIndexes.Length; i++)
				{
					renderNote (searchResultIndexes [i], i, w);
					GUILayout.Space (noteSpacing);
				}
			}

		}
		else
		{
			for(int i=0; i<notes.Length;i++)
			{
				if(notes[i].placeIndicator == -1)
					notes[i].placeIndicator = i;

				renderNote (i, i, w);

				//			GUI.color = notes[i].bgColor;
				//			string str = notes[i].text;
				//
				//			if(str.Length > 0)
				//			{
				//				if(str.Length < maxCharacters)
				//				{
				//					GUILayout.Label(str, styles[0], GUILayout.Height(noteHeight), GUILayout.Width(w));
				//				}
				//				else
				//				{
				//					GUILayout.Label(str.Replace("\n"," ").Replace("\r"," ").Trim().Substring(0, Mathf.Min(maxCharacters,str.Length-1)) + "...", styles[0], GUILayout.Height(noteHeight), GUILayout.Width(w));
				//				}
				//				if(GUI.Button(new Rect(w - 55f,9f + i * (noteHeight + noteSpacing), 50f,22f), "Select"))
				//					Selection.activeGameObject = notes[i].gameObject;
				//            }
				if(isDragging && dragginCurrentNote == i)
				{
					Repaint();
					GUI.color = Color.white;
					GUILayout.Space (1f);
                    GUILayout.Label("", draggingBarStyle, GUILayout.Width(w), GUILayout.Height(3f));
					GUILayout.Space (1f);
				}
				else
				{
					GUILayout.Space (noteSpacing);
				}

			} // finished drawing notes
		}


        // SCROLL BARS VISIBILITY DETECTION PHASE 1
        float scrollBar__y = 0f;
        // To catch whether scrollbars are visible we get the y now and the height later
        //GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)

        if(notes.Length > 0) // Solves issue: "You cannot call GetLast immediately after beginning a group."
        if (Event.current.type == EventType.Repaint)
        {
            scrollBar__y = GUILayoutUtility.GetLastRect().y;
        }
        //Debug.Log(GUILayoutUtility.GetLastRect());


		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndHorizontal();


        // SCROLL BARS VISIBILITY DETECTION PHASE 2
        float scrollBar__height = 0f;
        // We now get the height and then
        //GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)
        if (Event.current.type == EventType.Repaint)
        {
            scrollBar__height = GUILayoutUtility.GetLastRect().height;
        }
        //Debug.Log(GUILayoutUtility.GetLastRect());

        // SCROLL BARS VISIBILITY DETECTION PHASE 3
        // Determine whether scrollbars are visible
        if (Event.current.type == EventType.Repaint)
        {
            if (scrollBar__y > scrollBar__height) // weird condition needed as Unity messes up last rect with scrollbars sometimes.
                scrollBarsVisible = true;
            else
                scrollBarsVisible = false;
        }

		GUI.color = cGUI;

		// MANAGE SORTING WITH CONTROLS

		Event e = Event.current;
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
        float mY = e.mousePosition.y + scrollPos.y;
		switch (e.GetTypeForControl(controlID))
		{
		case EventType.MouseDown:
			if (!isSearchMode) {
				for (int i = 0; i < notes.Length; i++)
				{
					// find note that was picked up
					float nTop = headerMenuGUITopOffset + (noteSpacing + noteHeight) * i;
					if (mY > nTop && mY < nTop + noteSpacing + noteHeight)
					{
						draggingFromNote = i;
						dragginCurrentNote = i;
						isDragging = true;
						break;
					}
				}
			}
			break;
		case EventType.MouseUp:
			if(isDragging)
			{

				// here i is the destination note
				int destinationNoteIndex = dragginCurrentNote;
				if(destinationNoteIndex < 0) // catch the -1 case for top of the list
				{
					destinationNoteIndex = 0;
					notes[draggingFromNote].placeIndicator = notes[destinationNoteIndex].placeIndicator - 0.5f;
				}
				else
				{
					notes[draggingFromNote].placeIndicator = notes[destinationNoteIndex].placeIndicator + 0.5f;  // set a small offset for sorting later. We know here that all placeIndicators are always integers (even if float) because they are reassigned when sorted
				}

				sortNotes();
				Repaint();
				isDragging = false;

                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
			}
			break;
		case EventType.MouseDrag:
			if(isDragging)
			{
                    dragginCurrentNote = yToNoteIndex(mY - noteHeight/2f);

			}
			break;
		}

		if (GUI.changed)
		{
			if (!string.IsNullOrEmpty (searchString))
			{
				if (isSearchMode)
				{
					searchResultIndexes = searchNotes(searchString);
				}
			}
		}

    } // end OnGUI
    
	//--------------------------------------------------------------------------------------------

	void renderNote(int noteIndex, int displayIndex, float _currentWidth)
	{
        Color GUICurrent_backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = notes[noteIndex].bgColor;
		string str = notes[noteIndex].text;

		//if(str.Length > 0) // Empty notes are discarded in getAndSortNotes

        GUIStyle tmpStyle = CreateStyleForNote(notes[noteIndex]);

		if(str.Length < maxCharacters)
		{
            //GUILayout.Label(str, styles[0], GUILayout.Height(noteHeight), GUILayout.Width(_currentWidth));
            GUILayout.Label(str.Replace("\n"," ").Replace("\r"," ").Trim(), tmpStyle, GUILayout.Height(noteHeight), GUILayout.Width(_currentWidth));
		}
		else
		{
            GUILayout.Label(str.Replace("\n"," ").Replace("\r"," ").Trim().Substring(0, Mathf.Min(maxCharacters,str.Length-1)) + "...", tmpStyle, GUILayout.Height(noteHeight), GUILayout.Width(_currentWidth));
		}

        float selectbuttonRightMargin = (scrollBarsVisible) ? 70f : 55f; // To adjust the button's position when scrollbars are active (and would cover it)

        if (!EditorGUIUtility.isProSkin)
            GUI.backgroundColor = Color.white;
        if (GUI.Button(new Rect(_currentWidth - selectbuttonRightMargin, 4f + displayIndex * (noteHeight + noteSpacing), 50f, 22f), "Select"))
        {
            //Hinventory.SelectObjectWithNote(notes [noteIndex]);
            Selection.activeGameObject = notes [noteIndex].gameObject;
        }
        GUI.backgroundColor = GUICurrent_backgroundColor;
	}

	//--------------------------------------------------------------------------------------------

	int yToNoteIndex (float y)
	{
		return Mathf.Min(notes.Length-1, Mathf.FloorToInt (y / (noteHeight + noteSpacing)));
	}

	//--------------------------------------------------------------------------------------------

	void getAndSortNotes()
	{
		//System.Collections.Generic.List<UNote> notesList = new List<UNote> ((UNote[]) FindObjectsOfType (typeof(UNote))); // very slow. Better method to be investigated (static variable / singleton)
        System.Collections.Generic.List<UNote> notesList = UNotesHInventory.notes;

		// ISSUE #1:
		// if empty notes are given, the indexing in the UNotesWindow is wrong and Select buttons are displaced.
		// Sanitize notes list from empty notes
		System.Collections.Generic.List<UNote> cleanNotesList = new List<UNote> ();
		for (int i = 0; i < notesList.Count; i++)
		{
            if (!notesList [i].isEmpty)
				cleanNotesList.Add (notesList [i]);
		}

		cleanNotesList.Sort (delegate (UNote n1, UNote n2) {
			return n1.placeIndicator.CompareTo(n2.placeIndicator);
		});

//		notesList.Sort (delegate (UNote n1, UNote n2) {
//			return n1.placeIndicator.CompareTo(n2.placeIndicator);
//		});
		
        notes = cleanNotesList.ToArray ();
		reassignNotesPlaceIndexes ();
	}

    //--------------------------------------------------------------------------------------------

    public void forceRefresh()
    {
        getAndSortNotes();
    }

	//--------------------------------------------------------------------------------------------

	void sortNotes()
	{
		System.Collections.Generic.List<UNote> notesList = new List<UNote> (notes);
		notesList.Sort (delegate (UNote n1, UNote n2) {
			return n1.placeIndicator.CompareTo(n2.placeIndicator);
		});
		
		notes = notesList.ToArray ();
        reassignNotesPlaceIndexes ();
	}

	//--------------------------------------------------------------------------------------------

	int[] searchNotes(string pattern)
	{
		if (notes == null)
			return null;

		List<int> indices = new List<int> ();
		for (int i = 0; i < notes.Length; i++)
		{
			if (UNotesUtils.SimpleStringSearch (notes [i].text, pattern) > -1)
			{
				indices.Add (i);
			}
		}
		return indices.ToArray ();

	}

	//--------------------------------------------------------------------------------------------

	void reassignNotesPlaceIndexes()
	{
		for(int i=0; i<notes.Length; i++)
		{
			notes[i].placeIndicator = i;
		}
	}

	//--------------------------------------------------------------------------------------------

//    void createStyles()
//	{
//		styles = new GUIStyle[2];
//
////		// DEFAULT
//		styles [0] = new GUIStyle ();
//		styles [0].normal.background = whiteBgTex;
//		styles [0].fontSize = 12;
//		styles [0].padding = new RectOffset(8,8,7,8);
//
//		// ALTERNATIVE
//		styles [1] = new GUIStyle ();
//		styles [1].normal.background = whiteBgTex;
//        styles [1].normal.textColor = Color.black;
//		styles [1].fontSize = 11;
//		styles [1].padding = new RectOffset(8,8,7,8);
//		styles [1].fontStyle = FontStyle.Bold;
//	}

    GUIStyle CreateStyleForNote(UNote _note)
    {
        GUIStyle _style = new GUIStyle();
        _style.normal.background = whiteBgTex;
        _style.normal.textColor = _note.color;
        _style.fontSize = Mathf.Max(Mathf.Min(_note.fontSize, 15), 8);
        _style.padding = new RectOffset(8,8,7,8);
        _style.fontStyle = FontStyle.Bold;
        return _style;
    }

    GUIStyle CreateDraggingBarStyle()
    {
        GUIStyle _style = new GUIStyle ();
        _style.normal.background = whiteBgTex;
        _style.fontSize = 12;
        _style.padding = new RectOffset(8,8,7,8);
        return _style;
    }

}
