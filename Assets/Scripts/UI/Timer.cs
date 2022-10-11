using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>Start of game timer</summary>
public class Timer : MonoBehaviour
{
	[Header("Settings")]
	public int duration;
	public string startText;

	[Header("Scene references")]
	public Animator anim;
	public TMP_Text display;

	Action OnDone;

	public void StartTimer(Action callback)
	{
		OnDone = callback;
		anim.Play("Count");
		StartCoroutine(CountDown());
	}

	IEnumerator CountDown()
	{
		float timer = 0;
		string lastText = (Mathf.FloorToInt(duration) + 1).ToString();

		while (timer < duration)
		{
			timer += Time.deltaTime;
			display.text = (Mathf.FloorToInt(duration - timer) + 1).ToString();

			if (display.text != lastText && int.Parse(display.text) != 0)
				SoundsManager.PlaySound("UI");

			lastText = display.text;
			yield return null;
		}

		display.text = startText;
		anim.Play("Start");
		SoundsManager.PlaySound("UI", 1.5f);
		SoundsManager.FadeSound("Game", 1, true);

		OnDone?.Invoke();
	}
}