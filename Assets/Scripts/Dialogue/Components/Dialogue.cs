using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dialogue : ScriptableObject {
	
	//public string dialogueName = "";// {get {return dialogueAsset.dialogueName; } set {dialogueAsset.dialogueName = value; } }
	public List<string> openingLines = new List<string>();
	public List<Line> allLines = new List<Line>();
	
	public enum SpeakerIndex
	{
		TARGET = 0,
		PLAYER = 1,
		ADD_SPEAKER = 2
	}
	public List<string> speakers = new List<string>(System.Enum.GetNames(typeof(SpeakerIndex)));
	
	private Dictionary<string, Line> lineDict = new Dictionary<string, Line>();
	
	public Line CreateLine()
	{
		Line line = new Line();
		line.id = System.Guid.NewGuid().ToString();
		lineDict.Add(line.id, line);
		return line;
	}
	
	public Line GetLine(string id)
	{
		if(string.IsNullOrEmpty(id)) return null;
		
		if(lineDict.ContainsKey(id))
		{
			if(lineDict[id] != null)
				return lineDict[id];
			else
				lineDict.Remove(id);
		}
		
		for(int i = 0; i < allLines.Count; ++i)
		{
			if(!lineDict.ContainsKey(allLines[i].id))
				lineDict.Add(allLines[i].id, allLines[i]);
		}

		if(lineDict.ContainsKey(id) && lineDict[id] != null)
		{
			return lineDict[id];
		}
		else
		{
			Debug.LogWarning("Couldn't find id: " + id);
			return null;
		}
	}
	
	public bool RemoveUnusedLine(Line line)
	{
		if(line == null) return false;
		//Go through all lines to see if there are any remaining references to this one.
		foreach(Line l in allLines)
		{
			if(l.replies.Contains(line.id) || openingLines.Contains(line.id))
				return false;
		}
		
		//Ok, I'm not wanted anymore. Kill me and all my (unused) children!
		allLines.Remove(line);
		foreach(string reply in line.replies)
			RemoveUnusedLine(GetLine(reply));
		return true;
	}
}
