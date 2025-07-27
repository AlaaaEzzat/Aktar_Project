using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArabicSupport; // 

public class ArabicTextFixer : MonoBehaviour
{
	[TextArea]
	public string text;

	public bool ShowTashkeel = false;
	public bool UseHinduNumbers = true;

	public Text questionText; // Add this line to declare the 'questionText' variable

	void Start()
	{
		questionText.text = ArabicFixer.Fix(text, ShowTashkeel, UseHinduNumbers); // Replace 'currentQuestion.question' with 'text'
	}
}
