using UnityEngine;
using System.Collections;

public class Exit : BaseAction {
	
	public Environment destination;
	
	public override void Activate ()
	{
		if(destination != null)
		{
			GameObject.Find("#Player").GetComponent<Tracker>().ChangeEnvironment(destination);
		}
	}
}
