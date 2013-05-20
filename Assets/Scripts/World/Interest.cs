using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Interest : MonoBehaviour {
	
	public enum InterestType
	{
		Person,
		Item
	}
	
	public List<Condition> conditions;
	public InterestType interestType;
	
	public void OnMouseDown()
	{
		gameObject.SendMessage("activate", SendMessageOptions.DontRequireReceiver);
	}
	
	public void OnMouseOver()
	{
	}
}
