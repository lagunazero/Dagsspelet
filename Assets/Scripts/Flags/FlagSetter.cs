using UnityEngine;
using System.Collections;

public class FlagSetter : MonoBehaviour {

	public string flag;
	public int val = 1;
	public bool set;
	
	public void Activate () {
		FlagManager fm = GameObject.Find("#Player").GetComponent<FlagManager>();
		if(fm == null)
		{
			Debug.LogError("Couldn't find a FlagManager component and/or #Player gameobject.");
			return;
		}
		
		if(set)
			fm.SetValue(flag, val);
		else
			fm.IncrementValue(flag, val);
	}
}
