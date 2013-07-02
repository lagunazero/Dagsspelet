using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour {
	
	//Object refs
	private FlagManager flagManager;
	public DialogueUI ui;
	public Countdown countdown;
	public GameObject speakerGUI;
	public GameObject textGUI;
	public GameObject[] repliesGUI;
//	[HideInInspector]
	public Dialogue previewDialogue;
	
	public DialogueSpeaker previewCharacter;
	public DialogueSpeaker[] constantSpeakers;

	//States
	private bool isUsable = false;
	/// <summary>Change this only using ChangeSelection(...)</summary>
	[HideInInspector]
	public int selectedReply;
	[HideInInspector]
	public Line activeLine;
	[HideInInspector]
	public List<string> availableReplies;
	[HideInInspector]
	public Dialogue activeDialogue;
	[HideInInspector]
	public DialogueSpeaker activeCharacter;
	[HideInInspector]
	public GameObject activeGameObject;
	private bool nextLineIsPlayers;
	
	void Awake () {
		flagManager = GameObject.Find("#Player").GetComponent<FlagManager>();
		if(flagManager == null) Debug.LogWarning("DialogueManager: Couldn't find a FlagManager at the #Player GameObject.");
	}
	
	void Start() {
		OnScreenSizeChanged();
		if(previewDialogue != null)
			StartDialogue(previewDialogue, previewCharacter, null);
	}
	
	void OnScreenSizeChanged()
	{
		int size = Screen.width / ui.fontRatio;
		speakerGUI.guiText.fontSize = size;
		textGUI.guiText.fontSize = size;
		foreach(GameObject r in repliesGUI)
		{
			r.guiText.fontSize = size;
			r.transform.GetChild(0).GetComponent<GUIText>().guiText.fontSize = size;
		}
	}
	
	private void ChangeSelection(int newVal)
	{
		SendMessage("OnBeforeReplyChange", selectedReply, SendMessageOptions.DontRequireReceiver);
		selectedReply = newVal;
		SendMessage("OnAfterReplyChange", selectedReply, SendMessageOptions.DontRequireReceiver);
	}
	
	public void StartDialogue(Dialogue d, DialogueSpeaker c, GameObject dialogueObject)
	{
//		if(c == null) { Debug.LogError("DialogueManager: Got a null Character."); return;}
		activeCharacter = c;
		if(d == null) { Debug.LogError("DialogueManager: Got a null Dialogue."); return;}
		activeDialogue = d;
		Line line = FindLine(d.openingLines);
		if(line == null) { Debug.LogError("DialogueManager: Got a null Line."); return;}
		activeGameObject = dialogueObject;
		
		PrepareLine(line);
		InvokeActions(line);
		speakerGUI.SetActive(true);
		textGUI.SetActive(true);
		isUsable = true;
		SendMessage("OnStartDialogue", activeDialogue, SendMessageOptions.DontRequireReceiver);
	}
	
	public void EndDialogue()
	{
		if(activeGameObject != null)
		{
			activeGameObject.SendMessage("OnEndDialogue", activeDialogue, SendMessageOptions.DontRequireReceiver);
			activeGameObject = null;
		}
		SendMessage("OnEndDialogue", activeDialogue, SendMessageOptions.DontRequireReceiver);
		activeCharacter = null;
		activeDialogue = null;
		activeLine = null;
		isUsable = false;
		countdown.StopTimer();
		speakerGUI.SetActive(false);
		textGUI.SetActive(false);
		foreach(GameObject r in repliesGUI)
			r.SetActive(false);
	}
	
	public void PrepareLine(Line q)
	{
		activeLine = q;
		
		//Text
		speakerGUI.guiText.text = (activeLine.speaker != null && activeLine.speaker.Length > 0)
			? activeLine.speaker : (activeCharacter != null ? activeCharacter.nameInDialogues : string.Empty);
		textGUI.guiText.text = activeLine.text;
		Rect textRect = FormatGuiTextArea(textGUI.guiText, ui.lineWidth);
		
		//Replies
		availableReplies = new List<string>();
		nextLineIsPlayers = false;
		for(int i = 0; i < activeLine.replies.Count; i++)
		{
			Line reply = activeDialogue.GetLine(activeLine.replies[i]);
			if(IsAvailable(reply))
			{
				if(reply.isPlayer)
				{
					nextLineIsPlayers = true;
					int a = availableReplies.Count;
					repliesGUI[a].SetActive(true);
					//repliesGUI[a].guiText.text = char.ConvertFromUtf32(a+97) + ") " + activeLine.replies[i].text;
					repliesGUI[a].guiText.text = reply.text;
					if(!HasLines(reply.replies))
						repliesGUI[a].guiText.text += " " + ui.endDialogueText;
					repliesGUI[a].guiText.fontStyle = ui.normalFont;
					repliesGUI[a].guiText.material.color = ui.normalColor;
					//An attempt to set the replies at reasonable intervals below the text
					repliesGUI[a].transform.position = new Vector3(textRect.xMin / Screen.width + ui.replyLeftPadding,
						textRect.yMin / Screen.height - ui.replyTopPadding, 0);
					//Increase the textRect by the reply's height.
					textRect.y -= FormatGuiTextArea(repliesGUI[a].guiText, ui.lineWidth).height + ui.replyBetweenPadding * Screen.height;
				}
				availableReplies.Add(reply.id);
				//System.Array.Resize(ref availableReplies, availableReplies.Length + 1);
				//availableReplies[availableReplies.Count - 1] = activeLine.replies[i];
			}
		}
		//Hide the unused reply gui:s.
		for(int i = availableReplies.Count; i < repliesGUI.Length; i++)
		{
			repliesGUI[i].SetActive(false);
		}

		//Will no-one say anything more? If so, add the End Dialogue reply.
		if(availableReplies.Count == 0)
		{
			repliesGUI[0].SetActive(true);
			repliesGUI[0].guiText.text = ui.endDialogueText;
			repliesGUI[0].guiText.fontStyle = ui.normalFont;
			repliesGUI[0].guiText.material.color = ui.normalColor;
		}
		//If we're not the next to talk, add the Continue reply.
		else if(!nextLineIsPlayers)
		{
			repliesGUI[0].SetActive(true);
			repliesGUI[0].guiText.text = ui.continueDialogueText;
			repliesGUI[0].guiText.fontStyle = ui.normalFont;
			repliesGUI[0].guiText.material.color = ui.normalColor;
		}

		//Reset the selection and timer.
		ChangeSelection(0);
		countdown.StopTimer();
		if(activeLine.countdown > 0 && nextLineIsPlayers)
			countdown.StartTimer(activeLine.countdown);
		isUsable = true;
	}

	//Gets the next line, based on the prerequisites of the possible lines.
	//This means that there may be multiple possible outcomes,
	//depending on the states of the prerequisite flags.
	//If multiple prerequisites are valid, the one with the highest
	//priority value gets selected (or the first, in the case of a tie).
	//Returns null if there is no line that fit.
	public Line FindLine(List<string> lines)
	{
		Line result = null;
		bool found;
		foreach(string id in lines)
		{
			Line l = activeDialogue.GetLine(id);
			if(l == null) continue;
			//Best prio so far?
			if(result == null || result.priority < l.priority)
			{
				found = true;
				//If any prereqs fail, skip this line
				foreach(Condition p in l.conditions)
				{
					var v = flagManager.GetValue(p.flag);
					if((p.hasMin && v < p.minValue) || (p.hasMax && v > p.maxValue))
					{
						found = false;
						break;
					}
				}
				if(found)
					result = l;
			}
		}
		return result;
	}
	
	public bool HasLines(List<string> lines)
	{
		foreach(string id in lines)
		{
			List<Condition> conditions = activeDialogue.GetLine(id).conditions;
			if(conditions.Count == 0) return true;
			foreach(Condition p in conditions)
			{
				var v = flagManager.GetValue(p.flag);
				if((!p.hasMin || v >= p.minValue) && (!p.hasMax || v <= p.maxValue))
					return true;
			}
		}
		return false;
	}
	
	//Goes through the prerequisites, checking if
	//this reply is available based on the flags.
	public bool IsAvailable(Line l)
	{
		foreach(Condition p in l.conditions)
		{
			var v = flagManager.GetValue(p.flag);
			if((p.hasMin && v < p.minValue) || (p.hasMax && v > p.maxValue))
				return false;
		}
		return true;
	}

	void Update () {

		if(!isUsable) return;
		
		//Choose with space + arrows
		if(selectedReply > 0 && Input.GetKeyDown(KeyCode.UpArrow))
		{
			ChangeSelection(selectedReply - 1);
		}
		else if(selectedReply < availableReplies.Count - 1 && Input.GetKeyDown(KeyCode.DownArrow))
		{
			ChangeSelection(selectedReply + 1);
		}
		else if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			ReplySelect(selectedReply);
		}
		
		//Choose directly with letter or number
		else if(activeLine.replies.Count >= 1 &&
			(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Alpha1)))
		{
			ReplySelect(0);
		}
		else if(activeLine.replies.Count >= 2 &&
			(Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Alpha2)))
		{
			ReplySelect(1);
		}
		else if(activeLine.replies.Count >= 3 &&
			(Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Alpha3)))
		{
			ReplySelect(2);
		}
		else if(activeLine.replies.Count >= 4 &&
			(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Alpha4)))
		{
			ReplySelect(3);
		}
		else if(activeLine.replies.Count >= 5 &&
			(Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Alpha5)))
		{
			ReplySelect(4);
		}
		else if(activeLine.replies.Count >= 6 &&
			(Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Alpha6)))
		{
			ReplySelect(5);
		}
		else if(activeLine.replies.Count >= 7 &&
			(Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.Alpha7)))
		{
			ReplySelect(6);
		}
		else if(activeLine.replies.Count >= 8 &&
			(Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.Alpha8)))
		{
			ReplySelect(7);
		}
		else if(activeLine.replies.Count >= 9 &&
			(Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Alpha9)))
		{
			ReplySelect(8);
		}
	}
	
	public void ReplySelect(int selection)
	{
		SendMessage("OnBeforeReplySelect", activeLine, SendMessageOptions.DontRequireReceiver);
		if(nextLineIsPlayers)
		{
			Line selected = activeDialogue.GetLine(availableReplies[selection]);
			InvokeActions(selected);
			//If the selected reply has no further lines, end the dialogue.
			if(!HasLines(selected.replies))
				EndDialogue();
			else
			{
				PrepareLine(FindLine(selected.replies));
				InvokeActions(activeLine);
				SendMessage("OnAfterReplySelect", activeLine, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			//If the AI's reply has no further lines, end the dialogue.
			if(availableReplies.Count == 0)
				EndDialogue();
			else
			{
				PrepareLine(FindLine(availableReplies));
				InvokeActions(activeLine);
				SendMessage("OnAfterReplySelect", activeLine, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	//Call a function if we've got one.
	private void InvokeActions(Line line)
	{
		//Flag actions
		if(!string.IsNullOrEmpty(line.flagActionName) && !string.IsNullOrEmpty(line.flagActionParam))
		{
			//If the effect starts with + or - we use IncrementValue, otherwise SetValue.
			if(line.flagActionParam.StartsWith("+"))
				flagManager.IncrementValue(line.flagActionName, int.Parse(line.flagActionParam.TrimStart('+')));
			else if(line.flagActionParam.StartsWith("-"))
				flagManager.IncrementValue(line.flagActionName, -int.Parse(line.flagActionParam.TrimStart('-')));
			else
				flagManager.SetValue(line.flagActionName, int.Parse(line.flagActionParam));
		}
		
		//Function calls
		if(!string.IsNullOrEmpty(line.action))
		{
			if(activeCharacter == null)
				Debug.LogWarning("Tried to use a Line action (" + line.action + ") but the dialogue has no active character");
			else
				switch(line.actionType)
				{
				case "system.string":
					activeCharacter.gameObject.BroadcastMessage(line.action,
						 line.actionParamString, SendMessageOptions.DontRequireReceiver);
					break;
				case "system.int32":
					activeCharacter.gameObject.BroadcastMessage(line.action,
						 line.actionParamInt, SendMessageOptions.DontRequireReceiver);
					break;
				case "system.single":
					activeCharacter.gameObject.BroadcastMessage(line.action,
						 line.actionParamFloat, SendMessageOptions.DontRequireReceiver);
					break;
				case "unityengine.vector3":
					activeCharacter.gameObject.BroadcastMessage(line.action,
						 line.actionParamVector3, SendMessageOptions.DontRequireReceiver);
					break;
				default:
					activeCharacter.gameObject.BroadcastMessage(line.action,
						 line.actionParam, SendMessageOptions.DontRequireReceiver);
					break;
				}
		}
		
		//UI Effect
		//Stop and rewind any ongoing animations.
		if(animation.clip != null)
		{
			animation.Rewind();
			//If the AnimationClip has WrapMode Once it will not be reset by calling this.
			//Force it to start playing, if you want to rewind even such animations.
			//Could be useful not to, though, to apply "permanent" effects.
//			if(!animation.isPlaying)
//				animation.Play();
			animation.Sample();
			animation.Stop();
		}
		//We got a new animation? Play it!
		if(line.uiAnimation != null)
		{
			animation.clip = line.uiAnimation;
			animation.Play(AnimationPlayMode.Queue);
		}
		/*
		if(!string.IsNullOrEmpty(line.uiActionName))
		{
			switch(line.uiActionType)
			{
			case "system.string":
				effectsGO.SendMessage(line.uiActionName, line.uiActionParamString);
				break;
			case "system.int32":
				effectsGO.SendMessage(line.uiActionName, line.uiActionParamInt);
				break;
			case "system.single":
				effectsGO.SendMessage(line.uiActionName, line.uiActionParamFloat);
				break;
			case "unityengine.vector3":
				effectsGO.SendMessage(line.uiActionName, line.uiActionParamVector3);
				break;
			default:
				effectsGO.SendMessage(line.uiActionName, line.uiActionParam);
				break;
			}
		}
		*/
	}
	
	public void MouseOver(int index)
	{
		ChangeSelection(index);
	}

	public void MouseDown(int index)
	{
		ReplySelect(index);
	}

	public void CountdownEnded()
	{
		ReplySelect(selectedReply);
	}
	
	//Quite horrendous way of word wrapping a guitext
    public static Rect FormatGuiTextArea(GUIText guiText, float maxAreaWidthFraction)
    {
        string[] words = guiText.text.Split(' '); 
        string result = "";
        Rect textArea = new Rect();
		float maxWidth = maxAreaWidthFraction * Screen.width;
		
		for(int i = 0; i < words.Length; i++)
        {
            // set the gui text to the current string including new word
            guiText.text = (result + words[i] + " ");

            // measure it
            textArea = guiText.GetScreenRect();

            // if it didn't fit, put word onto next line, otherwise keep it
		    if(textArea.width > maxWidth)
                result += ("\n" + words[i] + " ");
            else
                result = guiText.text;
        }
        return textArea;
    }
}
