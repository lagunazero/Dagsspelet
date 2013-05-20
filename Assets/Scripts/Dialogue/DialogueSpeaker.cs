using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueSpeaker : MonoBehaviour {

	public string nameInDialogues;
	public Color editorWindowColor = new Color(1,1,1,1);
	public List<Dialogue> dialogues;
	
	public void Awake()
	{
		if(string.IsNullOrEmpty(nameInDialogues))
			nameInDialogues = name;
	}
}
