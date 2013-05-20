using UnityEngine;
using System.Collections;

public class Befriend : MonoBehaviour {
	
	public int friendship = 0;
	
	public void ImproveFriendship(int amount) {
		friendship += amount;
	}
}
