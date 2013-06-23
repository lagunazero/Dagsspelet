using UnityEngine;
using System.Collections;

public class PickupItem : BaseAction {
	
	public Item itemObject;
	public string pickupDescription;
	
	public override void Activate ()
	{
		if(itemObject == null)
		{
			itemObject = gameObject.GetComponent<Item>();
		}
		if(itemObject != null)
		{
			GameObject.Find("#Player").GetComponent<Inventory>().AddItem(itemObject);
			gameObject.SetActive(false);
			return;
		}
	}
}
