using UnityEngine;
using System.Collections;

public class Countdown : MonoBehaviour {
	
	private int currentValue;
	private bool isRunningTimer;
	private bool isBlinking;

	public float blinkValue;
	public Color alertColor;
	public float alertBlinkSpeed;
	
	public void SetTimer(int val)
	{
		guiText.text = (val < 10) ? "0" + val.ToString() : val.ToString();
		currentValue = val;
	}
	
	// Use this for initialization
	public void StartTimer (int startValue) {
		gameObject.SetActive(true);
		isBlinking = false;
		StopCoroutine("Blink");
		StopCoroutine("RunTimer");
		SetTimer(startValue);
		StartCoroutine("RunTimer");
	}
	
	public void StopTimer()
	{
		isBlinking = false;
		gameObject.SetActive(false);
	}
	
	public IEnumerator RunTimer() {
		isRunningTimer = true;
		while(isRunningTimer)
		{
			if(currentValue <= 0)
			{
				SendMessageUpwards("CountdownEnded", SendMessageOptions.DontRequireReceiver);
				isRunningTimer = false;
			}
			else if(currentValue <= blinkValue && !isBlinking)
			{
				StartCoroutine("Blink");
			}
			else
			{
				yield return new WaitForSeconds(1);
				SetTimer(currentValue - 1);
			}
		}
	}
	
	public IEnumerator Blink()
	{
		isBlinking = true;
		Color normalColor = guiText.material.color;
		while(isBlinking)
		{
			guiText.material.color = alertColor;
			yield return new WaitForSeconds(alertBlinkSpeed);
			guiText.material.color = normalColor;
			yield return new WaitForSeconds(alertBlinkSpeed);
		}
		guiText.material.color = normalColor;
	}
}
