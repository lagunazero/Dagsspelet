using UnityEngine;
using System.Collections;

public class TestFunctions : MonoBehaviour {

	public void LineFunc(Line b)
	{Debug.Log("Line: "+ b.ToString());}
	public void StringFunc(string b)
	{Debug.Log("String: "+ b.ToString());}
	public void IntFunc(int b)
	{Debug.Log("Int: "+ b.ToString());}
	public void FloatFunc(float b)
	{Debug.Log("Float: " + b.ToString());}
	public void Vector3Func(Vector3 b)
	{Debug.Log("Vector3: " + b.ToString());}
	public void NoParams()
	{Debug.Log("No params");}
}
