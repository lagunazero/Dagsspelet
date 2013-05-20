using UnityEngine;
using UnityEditor;
using System.Collections;

[ExecuteInEditMode]
public class DialogueWindowPreferences : MonoBehaviour {

	public enum ConditionUsage
	{
		None,
		FlagManager,
		Components
	}
	
	public bool confirmDelete = true;
	public bool autoAlternateSpeaker = true;
	public bool resetAfterPreview = true;
	public bool isDisplayingMultipleDialogues = false;
	public ConditionUsage conditionUsage = ConditionUsage.FlagManager;
	
	public Color activeObjectBackgroundColor = new Color(0.6f, 0.6f, 0.6f, 1f);
	public Color activeChildrenBackgroundColor = new Color(0.77f, 0.77f, 0.77f, 1f);

	public GUIStyle styleDetailsButtonNormal;
	public GUIStyle styleDetailsButtonConfigured;
	public GUIStyle styleDialogueNormal, styleDialogueActive, styleDialogueLinkNormal, styleDialogueLinkActive;
	public GUIStyle styleLineNormal, styleLineActive, styleLineLinkNormal, styleLineLinkActive;
	public GUIStyle styleReplyNormal, styleReplyActive, styleReplyLinkNormal, styleReplyLinkActive;
	public GUIStyle styleFoldoutButton;
	public GUIStyle styleSelectionLabel;
	
#if UNITY_EDITOR
	public void OnGUI()
	{
		//Just run once (or at least as few times as possible).
		enabled = false;
		
		if(UnityEditor.EditorUtility.DisplayDialog("Set defaults?",
			"Would you like to set all Dialogue Window Settings to their defaults?\n" +
			"This will override any settings you have made.\n" +
			"If you have just installed the Dialogue asset it is recommended to say yes, since this will let you more easily customize the settings afterwards.\n" +
			"If you have already been using the Dialogue asset and made your own changes, say no.\n\n" +
			"This message may appear multiple times in a row due to technical limitations. Sorry!",
			"Yes", "No"))
		{
			GUISkin skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			
			styleDetailsButtonNormal = new GUIStyle(skin.button);
			
			styleDetailsButtonConfigured = new GUIStyle(skin.button);
			styleDetailsButtonConfigured.fontStyle = FontStyle.Bold;

			//DIALOGUE
			styleDialogueNormal = new GUIStyle(skin.textField);
			styleDialogueNormal.focused.textColor = styleDialogueNormal.normal.textColor
				= new Color(220f / 255f, 220f / 255f, 80f / 255f);
			//Active
			styleDialogueActive = new GUIStyle(skin.textField);
			styleDialogueActive.focused.textColor = styleDialogueActive.normal.textColor
				= new Color(220f / 255f, 220f / 255f, 80f / 255f);
			//StyleActiveDialogue.fontStyle = FontStyle.BoldAndItalic;
			//Linked
			styleDialogueLinkNormal = new GUIStyle(skin.label);
			styleDialogueLinkNormal.focused.textColor = styleDialogueLinkNormal.normal.textColor
				= new Color(160f / 255f, 160f / 255f, 63f / 255f);
			styleDialogueLinkNormal.padding.left = 17;
			//Active & Linked
			styleDialogueLinkActive = new GUIStyle(skin.label);
			styleDialogueLinkActive.focused.textColor = styleDialogueLinkActive.normal.textColor
				= new Color(160f / 255f, 160f / 255f, 63f / 255f);
			//StyleActiveLinkedDialogue.fontStyle = FontStyle.BoldAndItalic;
			styleDialogueLinkActive.padding.left = 17;
			
			//LINE
			styleLineNormal = new GUIStyle(skin.textField);
			styleLineNormal.focused.textColor = styleLineNormal.normal.textColor
				= new Color(255f / 255f, 195f / 255f, 80f / 255f);
			styleLineNormal.wordWrap = true;
			//Active
			styleLineActive = new GUIStyle(skin.textField);
			styleLineActive.focused.textColor = styleLineActive.normal.textColor
				= new Color(255f / 255f, 195f / 255f, 80f / 255f);
			styleLineActive.wordWrap = true;
			//StyleActiveLine.fontStyle = FontStyle.BoldAndItalic;
			//Linked
			styleLineLinkNormal = new GUIStyle(skin.label);
			styleLineLinkNormal.focused.textColor = styleLineLinkNormal.normal.textColor
				= new Color(182f / 255f, 138f / 255f, 63f / 255f);
			styleLineLinkNormal.padding.left = 17;
			styleLineLinkNormal.wordWrap = true;
			//Active & Linked
			styleLineLinkActive = new GUIStyle(skin.label);
			styleLineLinkActive.focused.textColor = styleLineLinkActive.normal.textColor
				= new Color(182f / 255f, 138f / 255f, 63f / 255f);
			styleLineLinkActive.padding.left = 17;
			styleLineLinkActive.wordWrap = true;
			//StyleActiveLinkedLine.fontStyle = FontStyle.BoldAndItalic;
			
			//REPLY (player lines)
			styleReplyNormal = new GUIStyle(skin.textField);
			styleReplyNormal.focused.textColor = styleReplyNormal.normal.textColor
				= new Color(130f / 255f, 170f / 255f, 255f / 255f);
			styleReplyNormal.wordWrap = true;
			//Active
			styleReplyActive = new GUIStyle(skin.textField);
			styleReplyActive.focused.textColor = styleReplyActive.normal.textColor
				= new Color(130f / 255f, 170f / 255f, 255f / 255f);
			styleReplyActive.wordWrap = true;
			//StyleActiveReply.fontStyle = FontStyle.BoldAndItalic;
			//Linked
			styleReplyLinkNormal = new GUIStyle(skin.label);
			styleReplyLinkNormal.focused.textColor = styleReplyLinkNormal.normal.textColor
				= new Color(84f / 255f, 110f / 255f, 180f / 255f);
			styleReplyLinkNormal.padding.left = 17;
			styleReplyLinkNormal.wordWrap = true;
			//Active & Linked
			styleReplyLinkActive = new GUIStyle(skin.label);
			styleReplyLinkActive.focused.textColor = styleReplyLinkActive.normal.textColor
				= new Color(84f / 255f, 110f / 255f, 180f / 255f);
			//StyleActiveLinkedReply.fontStyle = FontStyle.BoldAndItalic;
			styleReplyLinkActive.padding.left = 17;
			styleReplyLinkActive.wordWrap = true;
			
			//FOLDOUT
			styleFoldoutButton = new GUIStyle(skin.button);
			styleFoldoutButton.padding.left = 1;
			
			//TEXT
			styleSelectionLabel = new GUIStyle(skin.label);
			styleSelectionLabel.fontStyle = FontStyle.Bold;
		}
	}
#endif
}
