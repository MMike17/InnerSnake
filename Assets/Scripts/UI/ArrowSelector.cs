using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>UI selector using arrows to display choices</summary>
public class ArrowSelector : MonoBehaviour
{
	[Header("Settings")]
	public bool autoSetValue;

	[Header("Scene references")]
	public Button minusButton;
	public Button plusButton;
	public GameObject completionIndicator;
	public TMP_Text display;

	[HideInInspector] public int index;

	string[] choices;

	void Awake()
	{
		if (!autoSetValue)
			return;

		SubscribeEvents(
			() =>
			{
				if (index != 0)
					index--;

				DisplayText(choices[index]);
				SetInterractibility(index != 0, index < choices.Length - 1);
			},
			() =>
			{
				if (index < choices.Length)
					index++;

				DisplayText(choices[index]);
				SetInterractibility(index != 0, index < choices.Length - 1);
			}
		);
	}

	public void SetChoices(string[] choices)
	{
		this.choices = choices;

		if (autoSetValue)
		{
			index = 0;
			DisplayText(choices[index]);
		}

		SetInterractibility(index != 0, index < choices.Length - 1);
	}

	public void SubscribeEvents(Action onClickLeft, Action onClickRight)
	{
		minusButton.onClick.AddListener(() => onClickLeft());
		plusButton.onClick.AddListener(() => onClickRight());
	}

	public void DisplayText(string text) => display.text = text;

	public void SetInterractibility(bool left, bool right)
	{
		minusButton.gameObject.SetActive(left);
		plusButton.gameObject.SetActive(right);
	}

	public void SetCompletion(bool state) => completionIndicator.SetActive(state);
}