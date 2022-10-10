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

			if (display.text != lastText)
				SoundsManager.PlaySound("UI");

			lastText = display.text;
			yield return null;
		}

		// TODO : Fix double UI sound on start

		display.text = startText;
		anim.Play("Start");
		SoundsManager.PlaySound("UI", 1.5f);

		OnDone?.Invoke();
	}
}