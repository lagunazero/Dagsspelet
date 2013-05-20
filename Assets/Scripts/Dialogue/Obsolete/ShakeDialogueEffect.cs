using UnityEngine;
using System.Collections;

public class ShakeDialogueEffect : MonoBehaviour {
	
	public float shakeDuration = 0.25f;
	
	public IEnumerator Shake(float amount)
	{
		float timer = 0;
		float x = 0, y = 0;
		do{
			timer += Time.deltaTime;
			x = Random.Range(-amount, amount);
			y = Random.Range(-amount, amount);
			transform.Translate(x, y, 0);
			yield return new WaitForEndOfFrame();
			transform.Translate(-x, -y, 0);
		} while(timer < shakeDuration);
	}
}
