using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Reply : MonoBehaviour {
	
	public string text = "";
	public List<Condition> conditions = new List<Condition>();
	public List<Line> leadsTo = new List<Line>();
	
	//Goes through the prerequisites, checking if
	//this reply is available based on the flags.
	public bool IsAvailable()
	{
		FlagManager fm = GameObject.Find("FlagManager").GetComponent<FlagManager>();
		foreach(Condition p in conditions)
		{
			var v = fm.GetValue(p.flag);
			if((p.hasMin && v < p.minValue) || (p.hasMax && v > p.maxValue))
				return false;
		}
		return true;
	}

}
