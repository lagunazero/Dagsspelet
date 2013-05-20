using UnityEngine;
using System.Collections;

[System.Serializable]
public class Condition {
	public string flag = "";
	public bool hasMin, hasMax;
	public int minValue, maxValue;
}
