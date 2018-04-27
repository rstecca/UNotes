using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UNote {

    /// <summary>
    /// Reference to the game object
    /// Used at runtime only as not serializable
    /// </summary>
    [System.NonSerialized]
    public GameObject gameObject;

    public string text = "";

    public float placeIndicator = -1; // Value used for reordering in the UNotes Window

    // These two fields must be converted to SerializableColor class as in UNotesOptionsWindow
    float[] ser_bgColor; // serializable bgColor
    float[] ser_color;


    // formatting fields
    public int fontSize = 10;
    public bool bold = false;

    //public UNote()
    //{
        
    //}

    public UNote(string _text, GameObject _gameObject, UNotesOptions options = null)
    {
        gameObject = _gameObject;
        text = _text;
        if(options != null)
        {
            bold = options.bold;
            fontSize = options.fontSize;
            ser_color = options.textColor.GetValues();
            ser_bgColor = options.bgColor.GetValues();
        }
    }

    public bool isEmpty {
        get {
            if (string.IsNullOrEmpty(text))
            {
                return true;
            } else
            {
                return false;
            }
        }
    }

    public Color bgColor {
        get {
            if (ser_bgColor == null)
            {
                return new Color(1,1,1,1);
            } else
            {
                return new Color(ser_bgColor[0], ser_bgColor[1], ser_bgColor[2], ser_bgColor[3]);
            }
        }
        set
        {
            ser_bgColor = new float[4] { value.r, value.g, value.b, value.a };
        }
    }

    public Color color {
        get {
            if (ser_color == null)
            {
                return new Color(0,0,0,1);
            } else
            {
                return new Color(ser_color[0], ser_color[1], ser_color[2], ser_color[3]);
            }
        }
        set
        {
            ser_color = new float[4] { value.r, value.g, value.b, value.a };
        }
    }
}
