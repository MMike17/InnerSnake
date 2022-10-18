using System;
using System.Collections;
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
	[Space]
	[TextArea]
	public string unlockHardModeMessage;
	[TextArea]
	public string finishGameMessage;

	[Header("Scene references")]
	public CanvasGroup eolScreenGroup;
	public TMP_Text eolScoreLabel;
	public TMP_Text eolScore;
	public Button eolReplayButton;
	public Button eolMenuButton;
	public Graph eolScoreGraph;
	[Space]
	public Popup popup;

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

	// TODO : Add online rank

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
		int totalPieces = DifficultyManager.GetCurrentDifficultySetting().GetTotalPieces(MapsManager.SpawnedMap.size);
		bool isVictory = Player.CollectedPieces == totalPieces;

		bool hasHardUnlock = false;
		bool hasFinishedGame = false;

		if (isVictory)
		{
			switch (DifficultyManager.CurrentDifficulty)
			{
				case Difficulty.Easy:
					Save.Data.unlockedDifficulties[(int)MapsManager.SpawnedMap.size].difficulties[(int)Difficulty.Medium] = true;
					break;

				case Difficulty.Medium:
					switch (MapsManager.SpawnedMap.size)
					{
						case MapSize._6:
							if (!Save.Data.unlockedDifficulties[(int)MapsManager.SpawnedMap.size].difficulties[(int)Difficulty.Hard])
								hasHardUnlock = true;

							Save.Data.unlockedDifficulties[(int)MapsManager.SpawnedMap.size].difficulties[(int)Difficulty.Hard] = true;
							Save.Data.unlockedDifficulties[(int)MapsManager.SpawnedMap.size + 1].difficulties[(int)Difficulty.Easy] = true;
							break;

						case MapSize._8:
							Save.Data.unlockedDifficulties[(int)MapsManager.SpawnedMap.size + 1].difficulties[(int)Difficulty.Easy] = true;
							break;

						case MapSize._10:
							Save.Data.unlockedDifficulties[(int)MapsManager.SpawnedMap.size + 1].difficulties[(int)Difficulty.Easy] = true;
							break;

						case MapSize._12:
							Save.Data.unlockedDifficulties[(int)MapsManager.SpawnedMap.size].difficulties[(int)Difficulty.Hard] = true;
							break;
					}
					break;

				case Difficulty.Hard:
					if (MapsManager.SpawnedMap.size != MapSize._12)
					{
						Save.Data.unlockedDifficulties[(int)MapsManager.SpawnedMap.size + 1].difficulties[(int)Difficulty.Hard] = true;
					}
					else if (!Save.Data.finishedGame)
						hasFinishedGame = true;
					break;
			}
		}

		// save data
		Save.Data.results.Add(new LevelResult(
			MapsManager.SpawnedMap.size,
			DifficultyManager.CurrentDifficulty,
			isVictory,
			isVictory ? (int)completionTime : Player.CollectedPieces
		));

		// show anim
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

		if (hasHardUnlock)
			popup.Pop(unlockHardModeMessage);

		if (hasFinishedGame)
		{
			popup.Pop(finishGameMessage);
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