using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>Represents results displayed in a highscore leaderboard</summary>
public class HighscoreTicket : MonoBehaviour
{
	[Header("Settings")]
	public float resultAnimDuration;

	[Header("Scene references")]
	public TMP_Text rank;
	public TMP_Text playerName;
	public TMP_Text result;

	Coroutine routine;

	public void SetData(int rankNum, string name, int resultMil)
	{
		rank.text = "#" + rankNum;
		playerName.text = name;

		if (routine != null)
			StopCoroutine(routine);

		routine = StartCoroutine(AnimateResult(resultMil));
	}

	IEnumerator AnimateResult(int resultMil)
	{
		float timer = 0;

		while (timer < resultAnimDuration)
		{
			timer += Time.deltaTime;

			int targetResult = Mathf.FloorToInt(Mathf.Lerp(0, resultMil, timer / resultAnimDuration));
			result.text = new TimeSpan(0, 0, 0, 0, targetResult).ToNiceString();

			yield return null;
		}

		result.text = new TimeSpan(0, 0, 0, 0, resultMil).ToNiceString();

		routine = null;
	}
}