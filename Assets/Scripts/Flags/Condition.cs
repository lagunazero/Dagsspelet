using UnityEngine;
using System.Collections;

[System.Serializable]
public class Condition {
	public string flag = "";
	[HideInInspector]
	public bool hasMin, hasMax;
	public int minValue, maxValue;
}
