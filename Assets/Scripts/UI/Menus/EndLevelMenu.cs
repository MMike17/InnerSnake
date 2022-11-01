using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static DifficultyManager;
using static GameManager;
using static MapsManager;

/// <summary>Manages the end of level menu</summary>
public class EndLevelMenu : MonoBehaviour
{
	[Header("Settings")]
	public float eolScreenFadeInDuration;
	public float eolScreenFadeOutDuration;
	public float eolScoreAnimDuration;
	public float eolRankAnimDuration;
	[Space]
	[TextArea]
	public string unlockHardModeMessage;
	[TextArea]
	public string finishGameMessage;

	[Header("Scene references")]
	public CanvasGroup eolScreenGroup;
	public TMP_Text eolScoreLabel;
	public TMP_Text eolScore;
	public Graph eolScoreGraph;
	public HighscoreTicket n1Ticket;
	public HighscoreTicket n2Ticket;
	public HighscoreTicket currentTicket;
	public Button eolReplayButton;
	public Button eolMenuButton;
	[Space]
	public PopupMessage messagePopup;
	public PopupRating ratingPopup;

	Action ShowEndButtons;
	Action StartLevel;
	float completionTime;

	public void Init(Action ShowEndButtons, Action StartLevel)
	{
		this.ShowEndButtons = ShowEndButtons;
		this.StartLevel = StartLevel;

		eolReplayButton.onClick.AddListener(() => StartCoroutine(Replay()));
		eolMenuButton.onClick.AddListener(() => StartCoroutine(FromEndToMenu()));

		GameManager.OnStateChanged += OnGameStateChange;
	}

	void OnGameStateChange(GameState state)
	{
		switch (state)
		{
			case GameState.End_Menu:
				completionTime = (Time.time - completionTime) * 1000;
				StartCoroutine(EndLevelAnim());
				break;
		}
	}

	IEnumerator FadeEndScreen(float target)
	{
		eolScreenGroup.interactable = false;
		eolScreenGroup.blocksRaycasts = false;

		float initialAlpha = eolScreenGroup.alpha;

		float timer = 0;
		float fadeDuration = target == 1 ? eolScreenFadeInDuration : eolScreenFadeOutDuration;

		while (timer < fadeDuration)
		{
			timer += Time.deltaTime;

			eolScreenGroup.alpha = Mathf.Lerp(initialAlpha, target, timer / fadeDuration);
			yield return null;
		}

		eolScreenGroup.interactable = target == 1;
		eolScreenGroup.blocksRaycasts = target == 1;
	}

	IEnumerator EndLevelAnim()
	{
		// unlocks
		bool isVictory = Player.CollectedPieces == DifficultyManager.GetCurrentDifficultySetting().GetTotalPieces(MapsManager.SpawnedMap.size);
		bool hasFinishedGame = Save.Data.finishedGame;
		int currentResult = isVictory ? (int)completionTime : Player.CollectedPieces;

		bool hasHardUnlock = Save.Data.ProcessUnlocks(
			currentResult,
			isVictory,
			MapsManager.SpawnedMap.size,
			DifficultyManager.CurrentDifficulty
		);

		// save score online
		if (isVictory)
		{
			ServerManager.SendScore(
				MapsManager.SpawnedMap.size,
				DifficultyManager.CurrentDifficulty,
				(int)completionTime
			);
		}

		// show anim
		n1Ticket.anim.Play("Hide");
		n2Ticket.anim.Play("Hide");
		currentTicket.anim.Play("Hide");

		eolScoreGraph.Hide();
		eolScoreLabel.text = isVictory ? "Completion time" : "Collected pieces";
		eolScore.text = string.Empty;

		yield return FadeEndScreen(1);

		float timer = 0;

		if (isVictory)
		{
			// show time
			TimeSpan span = new TimeSpan(0, 0, 0, 0, (int)completionTime);

			while (timer < eolScoreAnimDuration)
			{
				timer += Time.deltaTime;
				int currentMiliseconds = Mathf.FloorToInt(Mathf.Lerp(0, completionTime, timer / eolScoreAnimDuration));

				eolScore.text = new TimeSpan(0, 0, 0, 0, currentMiliseconds).ToNiceString();
				yield return null;
			}

			eolScore.text = new TimeSpan(0, 0, 0, 0, (int)completionTime).ToNiceString();
		}
		else
		{
			while (timer < eolScoreAnimDuration)
			{
				timer += Time.deltaTime;
				float percent = timer / eolScoreAnimDuration;

				eolScore.text = Mathf.FloorToInt(Mathf.Lerp(0, Player.CollectedPieces, percent)).ToString();
				yield return null;
			}

			eolScore.text = Player.CollectedPieces.ToString();
		}

		yield return eolScoreGraph.AnimateScore(
			isVictory,
			MapsManager.SpawnedMap.size,
			DifficultyManager.CurrentDifficulty,
			Save.Data
		);

		if (isVictory)
		{
			List<PlayerLeaderboardEntry> allResults = null;
			bool hasResult = false;

			ServerManager.GetLeaderboard(
				MapsManager.SpawnedMap.size,
				DifficultyManager.CurrentDifficulty,
				results =>
				{
					allResults = results;
					hasResult = true;
				},
				() => hasResult = true
			);

			yield return new WaitUntil(() => { return hasResult; });

			PlayerLeaderboardEntry playerResult = allResults.Find(item => item.DisplayName == Save.Data.playerName);
			PlayerLeaderboardEntry localResult = null;

			for (int index = 2; index >= 0; index--)
			{
				if (index == 2)
				{
					localResult = allResults.Count > 2 ? allResults[2] : null;
					yield return AnimateTicket(currentTicket, playerResult != null && playerResult.Position > 1 ? playerResult : localResult);
				}
				else
				{
					HighscoreTicket ticket = index switch
					{
						1 => n2Ticket,
						0 => n1Ticket,
						_ => null
					};

					localResult = allResults.Count > index ? allResults[index] : null;
					yield return AnimateTicket(ticket, playerResult != null && playerResult.Position == index ? playerResult : localResult);
				}
			}
		}
		else
		{
			n1Ticket.anim.Play("Hide");
			n2Ticket.anim.Play("Hide");
			currentTicket.anim.Play("Hide");
		}

		if (hasHardUnlock)
			messagePopup.Pop(unlockHardModeMessage, false);
		else if (!Save.Data.askedRating)
		{
			bool hasMap6 = Save.Data.results.Find(item => item.completed && item.size == MapSize._6) != null;
			bool hasMap8 = Save.Data.results.Find(item => item.size == MapSize._8) != null;
			bool hasEasy = Save.Data.results.Find(item => item.completed && item.difficulty == Difficulty.Easy) != null;
			bool hasMid = Save.Data.results.Find(item => item.difficulty == Difficulty.Medium) != null;

			if (hasMap6 && hasMap8 && hasEasy && hasMid)
				ratingPopup.Pop();

			Save.Data.askedRating = true;
		}

		if (hasFinishedGame != Save.Data.finishedGame)
		{
			messagePopup.Pop(finishGameMessage, true);
			this.DelayAction(() => SoundsManager.PlaySound("Win"), 0.8f);

			Save.Data.finishedGame = true;
		}

		ShowEndButtons();
	}

	IEnumerator AnimateTicket(HighscoreTicket ticket, PlayerLeaderboardEntry result)
	{
		if (result != null)
		{
			if (result.DisplayName == Save.Data.playerName)
				ticket.SetEmpty(result.DisplayName);
			else
				ticket.SetData(result.Position, result.DisplayName, result.StatValue, false);
		}
		else
			ticket.SetNoData();

		ticket.anim.Play("Show");
		yield return new WaitForSeconds(0.5f);

		if (result != null && result.DisplayName == Save.Data.playerName)
		{
			ticket.SetData(result.Position, result.DisplayName, result.StatValue, true);
			yield return new WaitForSeconds(1);
		}
	}

	IEnumerator Replay()
	{
		SoundsManager.PlaySound("Click");

		Player.CleanPlayer();
		StartLevel();

		yield return FadeEndScreen(0);
	}

	IEnumerator FromEndToMenu()
	{
		SoundsManager.PlaySound("Click");

		Player.CleanPlayer();
		GameManager.ChangeState(GameState.Main_Menu);

		yield return FadeEndScreen(0);
	}
}