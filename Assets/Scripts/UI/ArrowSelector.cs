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
	public Animator completionAnim;
	public TMP_Text display;

	[HideInInspector] public int index;

	string[] choices;

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
		minusButton.onClick.RemoveAllListeners();
		minusButton.onClick.AddListener(() =>
		{
			if (autoSetValue)
			{
				if (index != 0)
					index--;

				DisplayText(choices[index]);
				SetInterractibility(index != 0, index < choices.Length - 1);
			}

			SoundsManager.PlaySound("Click");
			onClickLeft();
		});

		plusButton.onClick.RemoveAllListeners();
		plusButton.onClick.AddListener(() =>
		{
			if (autoSetValue)
			{
				if (index < choices.Length)
					index++;

				DisplayText(choices[index]);
				SetInterractibility(index != 0, index < choices.Length - 1);
			}

			SoundsManager.PlaySound("Click");
			onClickRight();
		});
	}

	public void DisplayText(string text) => display.text = text;

	public void SetInterractibility(bool left, bool right)
	{
		minusButton.gameObject.SetActive(left);
		plusButton.gameObject.SetActive(right);
	}

	public void SetCompletion(bool state)
	{
		if (state)
			completionAnim.Play("Show");
		else if (completionAnim.GetCurrentAnimatorStateInfo(0).IsName("Show"))
			completionAnim.Play("Hide");
	}
}