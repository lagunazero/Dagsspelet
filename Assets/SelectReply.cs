using UnityEngine;
using System.Collections;

public class SelectReply : MonoBehaviour {
	
	public int index;
	
	void OnMouseOver() {
		transform.parent.GetComponent<DialogueManager>().MouseOver(index);
	}

	void OnMouseDown() {
		transform.parent.GetComponent<DialogueManager>().MouseDown(index);
	}
}
