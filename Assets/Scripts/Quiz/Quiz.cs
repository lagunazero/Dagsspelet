using UnityEngine;
using System.Collections;

public class Quiz : MonoBehaviour {
	
	//Object refs
	public Countdown countdown;
	public GameObject[] alternativesGUI;
	public Question[] questions;
	private Color normalColor;
	private FontStyle normalFont;

	//Values
	public int pauseTimeAfterSelection = 2;
	public int countdownTime = 10;
	public FontStyle selectedFont;
	public Color selectedColor;
	public FontStyle correctFont;
	public Color correctColor;
	
	//States
	private bool isUsable = true;
	private int selected;
	private Question activeQuestion;
	
	// Use this for initialization
	void Start () {
		normalFont = alternativesGUI[0].guiText.fontStyle;
		normalColor = alternativesGUI[0].guiText.material.color;
		PrepareQuestion(FindRandomQuestion());
		gameObject.SetActive(true);
	}
	
	private void ChangeSelection(int newVal)
	{
		alternativesGUI[selected].guiText.fontStyle = normalFont;
		alternativesGUI[selected].guiText.material.color = normalColor;
		selected = newVal;
		alternativesGUI[selected].guiText.fontStyle = selectedFont;
		alternativesGUI[selected].guiText.material.color = selectedColor;
	}
	
	// Update is called once per frame
	void Update () {
		
		if(!isUsable) return;
		
		//Choose with space + arrows
		if(selected > 0 && Input.GetKeyDown(KeyCode.UpArrow))
		{
			ChangeSelection(selected-1);
		}
		else if(selected < activeQuestion.alternatives.Length - 1 && Input.GetKeyDown(KeyCode.DownArrow))
		{
			ChangeSelection(selected+1);
		}
		else if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			if(activeQuestion.correctAnswers.Contains(selected))
				StartCoroutine(CorrectAnswer(selected));
			else
				StartCoroutine(FalseAnswer(selected));
		}
		
		//Choosec directly with letter or number
		if(activeQuestion.alternatives.Length >= 1 &&
			(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Alpha1)))
		{
			if(activeQuestion.correctAnswers.Contains(0))
				StartCoroutine(CorrectAnswer(0));
			else
				StartCoroutine(FalseAnswer(0));
		}
		else if(activeQuestion.alternatives.Length >= 2 &&
			(Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Alpha2)))
		{
			if(activeQuestion.correctAnswers.Contains(1))
				StartCoroutine(CorrectAnswer(1));
			else
				StartCoroutine(FalseAnswer(1));
		}
		else if(activeQuestion.alternatives.Length >= 3 &&
			(Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Alpha3)))
		{
			if(activeQuestion.correctAnswers.Contains(2))
				StartCoroutine(CorrectAnswer(2));
			else
				StartCoroutine(FalseAnswer(2));
		}
		else if(activeQuestion.alternatives.Length >= 4 &&
			(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Alpha4)))
		{
			if(activeQuestion.correctAnswers.Contains(3))
				StartCoroutine(CorrectAnswer(3));
			else
				StartCoroutine(FalseAnswer(3));
		}
		else if(activeQuestion.alternatives.Length >= 5 &&
			(Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Alpha5)))
		{
			if(activeQuestion.correctAnswers.Contains(4))
				StartCoroutine(CorrectAnswer(4));
			else
				StartCoroutine(FalseAnswer(4));
		}
		else if(activeQuestion.alternatives.Length >= 6 &&
			(Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Alpha6)))
		{
			if(activeQuestion.correctAnswers.Contains(5))
				StartCoroutine(CorrectAnswer(5));
			else
				StartCoroutine(FalseAnswer(5));
		}
		else if(activeQuestion.alternatives.Length >= 7 &&
			(Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.Alpha7)))
		{
			if(activeQuestion.correctAnswers.Contains(6))
				StartCoroutine(CorrectAnswer(6));
			else
				StartCoroutine(FalseAnswer(6));
		}
		else if(activeQuestion.alternatives.Length >= 8 &&
			(Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.Alpha8)))
		{
			if(activeQuestion.correctAnswers.Contains(7))
				StartCoroutine(CorrectAnswer(7));
			else
				StartCoroutine(FalseAnswer(7));
		}
		else if(activeQuestion.alternatives.Length >= 9 &&
			(Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Alpha9)))
		{
			if(activeQuestion.correctAnswers.Contains(8))
				StartCoroutine(CorrectAnswer(8));
			else
				StartCoroutine(FalseAnswer(8));
		}
	}
	
	public void PrepareQuestion(Question q)
	{
		activeQuestion = q;
		
		//Setup content and form of the gui
		guiText.text = activeQuestion.question;
		for(int i = 0; i < activeQuestion.alternatives.Length; i++)
		{
			alternativesGUI[i].SetActive(true);
			alternativesGUI[i].guiText.text = char.ConvertFromUtf32(i+97) + ") " + activeQuestion.alternatives[i];
			alternativesGUI[i].guiText.fontStyle = normalFont;
			alternativesGUI[i].guiText.material.color = normalColor;
		}
		for(int i = activeQuestion.alternatives.Length; i < alternativesGUI.Length; i++)
		{
			alternativesGUI[i].SetActive(false);
		}
		
		//Reset the selection and timer.
		ChangeSelection(0);
		countdown.StartTimer(countdownTime);
		isUsable = true;
	}
	
	private Question FindRandomQuestion()
	{
		Question q;
		do
		{
			q = questions[Random.Range(0, questions.Length)];
		} while (q == activeQuestion);
		return q;
	}
	
	private IEnumerator CorrectAnswer(int selection)
	{
		isUsable = false;
		countdown.StopTimer();
		
		if(selected != selection)
		{
			alternativesGUI[selected].guiText.fontStyle = normalFont;
			alternativesGUI[selected].guiText.material.color = normalColor;
		}
		foreach(int a in activeQuestion.correctAnswers)
		{
			alternativesGUI[a].guiText.fontStyle = correctFont;
			alternativesGUI[a].guiText.material.color = correctColor;
		}
		yield return new WaitForSeconds(pauseTimeAfterSelection);
		
		if(activeQuestion.followUpQuestion != null)
		{
			PrepareQuestion(activeQuestion.followUpQuestion);
		}
		else
		{
			gameObject.SetActive(false);
		}
	}
	
	private IEnumerator FalseAnswer(int selection)
	{
		isUsable = false;
		countdown.StopTimer();
		
		if(selected != selection)
		{
			alternativesGUI[selected].guiText.fontStyle = normalFont;
			alternativesGUI[selected].guiText.material.color = normalColor;
			alternativesGUI[selection].guiText.fontStyle = selectedFont;
			alternativesGUI[selection].guiText.material.color = selectedColor;
		}
		foreach(int a in activeQuestion.correctAnswers)
		{
			alternativesGUI[a].guiText.fontStyle = correctFont;
			alternativesGUI[a].guiText.material.color = correctColor;
		}
		yield return new WaitForSeconds(pauseTimeAfterSelection);

		PrepareQuestion(FindRandomQuestion());
	}
	
	public void CountdownEnded()
	{
		if(activeQuestion.correctAnswers.Contains(selected))
			StartCoroutine(CorrectAnswer(selected));
		else
			StartCoroutine(FalseAnswer(selected));
	}
}
