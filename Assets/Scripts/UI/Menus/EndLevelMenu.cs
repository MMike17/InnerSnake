using System;
using System.Collections;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static DifficultyManager;
using static GameManager;
using static MapsManager;
using static Save;

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
			PlayerLeaderboardEntry playerResult = null;
			bool hasResult = false;

			ServerManager.GetLeaderboard(
				MapsManager.SpawnedMap.size,
				DifficultyManager.CurrentDifficulty,
				results =>
				{
					results.Sort((first, second) => { return second.Position - first.Position; });
					playerResult = results.Find(item => item.DisplayName == Save.Data.playerName);

					n1Ticket.SetData(1, results[0].DisplayName, results[0].StatValue);
					n2Ticket.SetData(2, results[1].DisplayName, results[1].StatValue);

					if (playerResult.Position > 1)
						currentTicket.SetData(playerResult.Position, playerResult.DisplayName, playerResult.StatValue);

					hasResult = true;
				},
				() => hasResult = true
			);

			yield return new WaitUntil(() => { return hasResult; });

			HighscoreTicket selectedTicket = playerResult.Position switch
			{
				0 => n1Ticket,
				1 => n2Ticket,
				_ => currentTicket
			};

			// pop before player result
			if (playerResult.Position < 2)
			{
				currentTicket.anim.Play("Show");
				yield return new WaitForSeconds(0.5f);

				if (playerResult.Position == 0)
				{
					n2Ticket.anim.Play("Show");
					yield return new WaitForSeconds(0.5f);
				}
			}

			selectedTicket.rank.text = "#";
			selectedTicket.anim.Play("Show");

			yield return new WaitForSeconds(0.5f);

			timer = 0;

			while (timer < eolRankAnimDuration)
			{
				timer += Time.deltaTime;
				selectedTicket.rank.text = "#" + Mathf.FloorToInt(Mathf.Lerp(1, playerResult.Position + 1, timer / eolRankAnimDuration));

				yield return null;
			}

			selectedTicket.rank.text = "#" + (playerResult.Position + 1);

			// pop after player
			if (playerResult.Position > 0)
			{
				if (playerResult.Position == 2)
				{
					n2Ticket.anim.Play("Show");
					yield return new WaitForSeconds(0.5f);
				}

				n1Ticket.anim.Play("Show");
				yield return new WaitForSeconds(0.5f);
			}
		}
		else
		{
			n1Ticket.anim.Play("Hide");
			n2Ticket.anim.Play("Hide");
			currentTicket.anim.Play("Hide");
		}

		if (hasHardUnlock)
			messagePopup.Pop(unlockHardModeMessage);

		if (hasFinishedGame != Save.Data.finishedGame)
		{
			messagePopup.Pop(finishGameMessage);
			Save.Data.finishedGame = true;
		}

		ShowEndButtons();
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