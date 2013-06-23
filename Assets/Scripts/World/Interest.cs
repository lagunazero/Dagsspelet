using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Interest : MonoBehaviour {
	
	public enum InterestType
	{
		Person,
		Item,
		Exit
	}
	
	public List<Condition> conditions;
	public InterestType interestType;
	
	public void OnMouseDown()
	{
		gameObject.SendMessage("Activate", SendMessageOptions.DontRequireReceiver);
	}
	
	public void OnMouseOver()
	{
	}
}
