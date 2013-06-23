using UnityEngine;
using System.Collections;

public class Tracker : MonoBehaviour {
	
	public Environment currentEnvironment;
	
	public void Start()
	{
		if(currentEnvironment != null)
		{
			currentEnvironment.gameObject.SetActive(true);
			currentEnvironment.transform.Translate(0, 1, 0);
		}
	}
	
	public void ChangeEnvironment(Environment destination)
	{
		if(destination != currentEnvironment && destination != null)
		{
			currentEnvironment.transform.Translate(0, -1, 0);
			currentEnvironment.gameObject.SetActive(false);
			//todo: play transition effects?
			currentEnvironment = destination;
			currentEnvironment.gameObject.SetActive(true);
			currentEnvironment.transform.Translate(0, 1, 0);
		}
	}
	
}
