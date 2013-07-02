using UnityEngine;
using System.Collections;

public class StartDialogue : BaseAction {
	
	public Dialogue dialogue;
	public DialogueSpeaker speaker;
	
	public override void Activate ()
	{
		GameObject go = GameObject.Find("#DialogueManager");
		if(go == null)
		{
			Debug.LogError("Couldn't find a game object named DialogueManager, from StartDialogue: " + name);
			return;
		}
		DialogueManager dm = go.GetComponent<DialogueManager>();
		if(dm == null)
		{
			Debug.LogError("Couldn't find a DialogueManager component, from StartDialogue: " + name);
			return;
		}
		
		dm.StartDialogue(dialogue, speaker, gameObject);
		if(animation.isPlaying)
			animation.Stop();
	}
	
	public void OnEndDialogue()
	{
		if(animation.clip != null)
			animation.Play();
	}
}
