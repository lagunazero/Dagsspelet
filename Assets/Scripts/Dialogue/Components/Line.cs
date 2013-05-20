using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Line {
	
	public bool isPlayer;
	public string speaker = "";
	public string text = "";
	public int priority = 0;
	public List<Condition> conditions = new List<Condition>();
	public List<Line> replies = new List<Line>();
	
	//Action with parameters. Need to divide them up since we can't pass them as object properly.
	public string action;
	public string actionType; //Need to store as string for serialization.
	public Object actionParam;
	public string actionParamString;
	public int actionParamInt;
	public float actionParamFloat;
	public Vector3 actionParamVector3;
	
	public string flagActionName;
	public string flagActionParam;
	
	//UI effects. Don't use these for replies, as there may be side effects.
	public string uiActionName;
	public string uiActionType; //Need to store as string for serialization.
	public Object uiActionParam;
	public string uiActionParamString;
	public int uiActionParamInt;
	public float uiActionParamFloat;
	public Vector3 uiActionParamVector3;
	
	public AnimationClip uiAnimation;
	
	public string comment;

	public int countdown = 0;
	
	public bool HasDetailSettings()
	{
		return //speaker != null
			 priority != 0
			|| conditions.Count != 0
			|| countdown != 0
			|| !string.IsNullOrEmpty(action)
			|| !string.IsNullOrEmpty(flagActionName)
			|| !string.IsNullOrEmpty(uiActionName)
			|| !string.IsNullOrEmpty(comment);
	}
	
	//Sort lines with highest priority first.
	public static int LineComparison(Line x, Line y)
	{
		if(x.priority > y.priority) return -1;
		else if(x.priority < y.priority) return 1;
		else return 0;
	}
	
	/*
	public Dialogue FindDialogue()
	{
		return FindDialogue(transform);
	}
	private Dialogue FindDialogue(Transform t)
	{
		Dialogue dia = t.GetComponent<Dialogue>();
		if(dia == null)
			return FindDialogue(t.parent);
		else return dia;
	}
	*/
	/*
	public DialogueSpeaker FindSpeaker()
	{
		return FindSpeaker(transform);
	}
	private DialogueSpeaker FindSpeaker(Transform t)
	{
		DialogueSpeaker dia = t.GetComponent<DialogueSpeaker>();
		if(dia == null)
			return FindSpeaker(t.parent);
		else return dia;
	}
	*/
}
