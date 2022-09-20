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

		while (timer < duration)
		{
			timer += Time.deltaTime;
			display.text = (Mathf.FloorToInt(duration - timer) + 1).ToString();

			yield return null;
		}

		display.text = startText;
		anim.Play("Start");

		OnDone?.Invoke();
	}
}