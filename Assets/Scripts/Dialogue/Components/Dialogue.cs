using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dialogue : ScriptableObject {
	
	//public string dialogueName = "";// {get {return dialogueAsset.dialogueName; } set {dialogueAsset.dialogueName = value; } }
	public List<Line> openingLines = new List<Line>();
	public List<Line> allLines = new List<Line>();
	
	public enum SpeakerIndex
	{
		TARGET = 0,
		PLAYER = 1,
		ADD_SPEAKER = 2
	}
	public List<string> speakers = new List<string>(System.Enum.GetNames(typeof(SpeakerIndex)));
}
