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
	public Animator anim;
	public TMP_Text rank;
	public TMP_Text playerName;
	public TMP_Text result;

	Coroutine routine;

	public void SetData(int rankNum, string name, int resultMil, bool animate)
	{
		rank.text = "#" + (rankNum + 1);
		playerName.text = name;

		if (animate)
		{
			if (routine != null)
				StopCoroutine(routine);

			routine = StartCoroutine(AnimateResult(resultMil));
		}
		else
			result.text = new TimeSpan(0, 0, 0, 0, resultMil).ToNiceString();
	}

	public void SetEmpty(string playerName)
	{
		rank.text = "#";
		this.playerName.text = playerName;
		result.text = "";
	}

	public void SetNoData()
	{
		rank.text = null;
		playerName.text = "No data";
		result.text = null;
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