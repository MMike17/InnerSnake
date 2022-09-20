using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>Displays results values for game run</summary>
public class GraphIndicator : MonoBehaviour
{
	[Header("Settings")]
	public float animationDuration;

	[Header("Scene references")]
	public TMP_Text value;
	public TMP_Text valueBis;
	public RectTransform scoreIndicator;

	public void SetValue(int displayValue, bool isVictory, int index, float height, bool animate)
	{
		bool main = index % 2 != 0;
		value.enabled = main;
		valueBis.enabled = !main;

		if (animate)
			StartCoroutine(Animate(displayValue, isVictory, height));
		else
			SetText(displayValue, isVictory);
	}

	public IEnumerator Animate(int displayValue, bool isVictory, float height)
	{
		scoreIndicator.gameObject.SetActive(true);

		float timer = 0;

		while (timer < animationDuration)
		{
			timer += Time.deltaTime;
			float percent = timer / animationDuration;

			scoreIndicator.position = new Vector2(transform.position.x, Mathf.Lerp(transform.position.y, height, percent));
			SetText(Mathf.FloorToInt(Mathf.Lerp(0, displayValue, percent)), isVictory);

			yield return null;
		}

		scoreIndicator.position = new Vector2(transform.position.x, height);
	}

	void SetText(int displayValue, bool isVictory)
	{
		string result = new TimeSpan(0, 0, 0, 0, displayValue).ToNiceString();

		value.text = isVictory ? result : displayValue.ToString();
		valueBis.text = isVictory ? result : displayValue.ToString();
	}
}