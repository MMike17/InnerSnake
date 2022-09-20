using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>UI selector using arrows to display choices</summary>
public class ArrowSelector : MonoBehaviour
{
	[Header("Scene references")]
	public Button minusButton;
	public Button plusButton;
	public GameObject completionIndicator;
	public TMP_Text display;

	public int index;

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