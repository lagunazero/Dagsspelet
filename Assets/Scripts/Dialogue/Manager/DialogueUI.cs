using UnityEngine;
using System.Collections;

public class DialogueUI : MonoBehaviour {

	public enum ReplyPrefixStyle{
		Number, Letter, None
	};

	private DialogueManager dm;
	
	public int lineWidth = 500;
	public float replyLeftPadding = 0.025f;
	public float replyTopPadding = 0.03f;
	public float replyBetweenPadding = 0.03f;
	
	public ReplyPrefixStyle replyPrefixStyle = ReplyPrefixStyle.Number;
	public string replyPrefixDelimiter = ")";
	public float replyPrefixPadding = 0.025f;
	
	public string continueDialogueText = "<continue>";
	public string endDialogueText = "<end conversation>";

	//GUI
	public FontStyle normalFont;
	public Color normalColor = new Color(0.95f,0.95f,0.95f,1);
	public FontStyle selectedFont;
	public Color selectedColor = new Color(0.7f,0.65f,0,1);
	
	public GameObject background;
	public float backgroundPaddingTop = 5;
	public float backgroundPaddingBottom = 5;
	public float backgroundPaddingLeft = 25;
	public float backgroundPaddingRight = 0;
	
	void Awake()
	{
		dm = gameObject.GetComponent<DialogueManager>();
		if(dm == null) Debug.LogError("DialogueUI: Couldn't find a DialogueManager component on this GameObject.\n"
			+ "Some settings were not applied and the dialogue UI is likely to break if used.");
		else
		{
			for(int i = 0; i < dm.repliesGUI.Length; i++)
					dm.repliesGUI[i].transform.GetChild(0).localPosition = new Vector3(-replyPrefixPadding, 0, 0);
			switch(replyPrefixStyle)
			{
			case ReplyPrefixStyle.Letter:
				for(int i = 0; i < dm.repliesGUI.Length; i++)
					dm.repliesGUI[i].transform.GetChild(0).GetComponent<GUIText>().text = char.ConvertFromUtf32(i+97) + replyPrefixDelimiter;
				break;
			case ReplyPrefixStyle.Number:
				for(int i = 0; i < dm.repliesGUI.Length; i++)
					dm.repliesGUI[i].transform.GetChild(0).GetComponent<GUIText>().text = (i+1) + replyPrefixDelimiter;
				break;
			case ReplyPrefixStyle.None:
			default:
				for(int i = 0; i < dm.repliesGUI.Length; i++)
					dm.repliesGUI[i].transform.GetChild(0).GetComponent<GUIText>().text = "";
				break;
			}
		}
	}
	
	private void FitBackground()
	{
		for(int i = 1; i < dm.repliesGUI.Length; i++)
			if(!dm.repliesGUI[i].activeSelf)
			{
				Rect textRect = dm.repliesGUI[i-1].guiText.GetScreenRect();
				background.guiTexture.pixelInset = new Rect(
					textRect.xMin - backgroundPaddingLeft,
					textRect.yMin - backgroundPaddingBottom,
					-lineWidth + backgroundPaddingLeft + backgroundPaddingRight,
					-textRect.yMax + backgroundPaddingTop + backgroundPaddingBottom);
				break;
			}
	}
	
	public void OnStartDialogue(Dialogue dia)
	{
		if(background != null)
		{
			FitBackground();
			background.SetActive(true);
		}
	}

	public void OnEndDialogue(Dialogue dia)
	{
		if(background != null)
			background.SetActive(false);
	}

	public void OnBeforeReplyChange(int newVal)
	{
		dm.repliesGUI[dm.selectedReply].guiText.fontStyle = normalFont;
		dm.repliesGUI[dm.selectedReply].guiText.material.color = normalColor;
		
		GUIText prefix = dm.repliesGUI[dm.selectedReply].transform.GetChild(0).GetComponent<GUIText>();
		prefix.fontStyle = normalFont;
		prefix.material.color = normalColor;
	}
	
	public void OnAfterReplyChange(int newVal)
	{
		dm.repliesGUI[dm.selectedReply].guiText.fontStyle = selectedFont;
		dm.repliesGUI[dm.selectedReply].guiText.material.color = selectedColor;

		GUIText prefix = dm.repliesGUI[dm.selectedReply].transform.GetChild(0).GetComponent<GUIText>();
		prefix.fontStyle = selectedFont;
		prefix.material.color = selectedColor;
	}

	public void OnBeforeReplySelect(Line activeLine)
	{
	}

	public void OnAfterReplySelect(Line activeLine)
	{
		FitBackground();
	}
}
