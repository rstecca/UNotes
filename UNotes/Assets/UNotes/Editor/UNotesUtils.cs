using UnityEngine;
using System.Collections;

public static class UNotesUtils {

	public static int SimpleStringSearch(string text, string pattern, int maxLength = 64)
	{
		int t = text.Length;
		int p = pattern.Length;
		string cleanText = text.Replace ("\n", " ").Replace ("\r", " ").Trim ().ToLower();
		string cleanPattern = pattern.ToLower ();
		cleanPattern = cleanPattern.Substring (0, Mathf.Min (pattern.Length, maxLength)); // cut off pattern to maxLength
		for (int i = 0; i < t; i++)
		{
			if (i + p > t)
			{
				return - 1;
			}
			if (string.Compare (cleanText.Substring (i, p), cleanPattern) == 0)
			{
				return i;
			}
		}
		return -1;
	}

}
