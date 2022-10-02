using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static DifficultyManager;
using static GameManager;
using static MapsManager;
using static Save;
using Random = UnityEngine.Random;

/// <summary>Manages the main menu of the game</summary>
public class MainMenu : MonoBehaviour
{
	[Header("Settings")]
	[Range(0.1f, 0.5f)]
	public float minAnimDistance;
	public float mainAnimSpeed;
	[Space]
	public float levelAnimDuration;
	public float mapRotationSpeed;
	[Space]
	public float spawnAnimSpeed;
	[Space]
	public float eolScreenFadeDuration;
	public float eolScoreAnimDuration;

	// TODO : Add highscore panel

	[Header("Scene references")]
	public Transform animScreenCenter;
	public GameObject playerPrefab;
	public Animator anim;
	[Space]
	public Button newGameButton;
	// public Button highscoreButton;
	public Button quitButton;
	[Space]
	public ArrowSelector levelSelector;
	public ArrowSelector difficultySelector;
	public Button playButton;
	[Space]
	public Timer startGameTimer;
	[Space]
	public CanvasGroup eolScreenGroup;
	public TMP_Text eolScoreLabel;
	public TMP_Text eolScore;
	public Button eolReplayButton;
	public Button eolMenuButton;
	public Graph eolScoreGraph;

	Transform player;
	Vector3 animCenter;
	Vector3 animTarget;
	float showMenuDistance;
	float animSphereSize;
	float completionTime;
	bool showLevels;

	void OnDrawGizmos()
	{
		Gizmos.color = new Color(1, 0, 0, 0.5f);
		Gizmos.DrawSphere(animCenter, animSphereSize);
	}

	public void Init()
	{
		newGameButton.onClick.AddListener(() =>
		{
			showLevels = true;
			PickNewAnimTarget();
		});
		quitButton.onClick.AddListener(() => Application.Quit());
		playButton.onClick.AddListener(() =>
		{
			anim.Play("HideLevel", 1);
			levelSelector.SetCompletion(false);
			difficultySelector.SetCompletion(false);

			StartCoroutine(StartLevel());
		});

		eolReplayButton.onClick.AddListener(() => StartCoroutine(Replay()));
		eolMenuButton.onClick.AddListener(() => StartCoroutine(FromEndToMenu()));

		Camera mainCamera = CameraManager.MainCamera;
		Vector3 forwardOffset = mainCamera.transform.forward * 10;

		animCenter = mainCamera.ScreenToWorldPoint(animScreenCenter.position + forwardOffset);
		animSphereSize = Vector3.Distance(animCenter, mainCamera.ViewportToWorldPoint(new Vector3(0.8f, 0.5f, 0) + forwardOffset));

		GameManager.OnStateChanged += OnGameStateChange;
		levelSelector.SubscribeEvents(() => SelectLevel(false), () => SelectLevel(true));
	}

	void SelectLevel(bool right)
	{
		levelSelector.index = Mathf.Clamp(levelSelector.index + (right ? 1 : -1), 0, MapsCount);
		StartCoroutine(ChangeLevel());
	}

	void UpdateDifficulty()
	{
		DifficultyManager.CurrentDifficulty = (Difficulty)Enum.Parse(typeof(Difficulty), difficultySelector.display.text);

		difficultySelector.SetCompletion(Save.Data.CompletedLevel(MapsManager.SpawnedMap.size, DifficultyManager.CurrentDifficulty));
	}

	void Update()
	{
		if (player != null)
		{
			float targetDistance = Vector3.Distance(player.position, animTarget);

			if (showLevels)
				player.localScale = Vector3.one * Mathf.Lerp(0, 0.4f, targetDistance / showMenuDistance);

			if (targetDistance <= 0.1f)
			{
				if (showLevels)
				{
					GameManager.ChangeState(GameState.Level_selection);

					showLevels = false;
					Destroy(player.gameObject);
				}
				else
					PickNewAnimTarget();
			}

			player.position = Vector3.MoveTowards(player.position, animTarget, mainAnimSpeed * Time.deltaTime);
		}

		if (MapsManager.SpawnedMap != null && GameManager.CurrentState == GameState.Level_selection)
			MapsManager.SpawnedMap.transform.Rotate(0, mapRotationSpeed * Time.deltaTime, 0);
	}

	void OnGameStateChange(GameState state)
	{
		switch (state)
		{
			case GameState.Main_Menu:
				anim.Play("ShowMain", 0);

				player = Instantiate(playerPrefab, animCenter, Quaternion.Euler(0, 0, 0)).transform;
				PickNewAnimTarget();
				break;

			case GameState.Level_selection:
				anim.Play("ShowLevel", 1);
				levelSelector.index = 0;

				StartCoroutine(ChangeLevel());
				break;

			case GameState.End_Menu:
				completionTime = (Time.time - completionTime) * 1000;
				StartCoroutine(EndLevelAnim());
				break;
		}
	}

	void PickNewAnimTarget()
	{
		if (showLevels)
		{
			animTarget = animCenter;
			showMenuDistance = Vector3.Distance(animCenter, player.position);

			player.LookAt(animTarget, animTarget - player.position - player.forward);
			anim.Play("HideMain", 0);
		}
		else
		{
			int loopCount = 0;

		pickRandom:
			animTarget = animCenter + Random.onUnitSphere * animSphereSize;
			loopCount++;

			if (loopCount < 3 && Vector3.Distance(animTarget, player.position) < animSphereSize * minAnimDistance)
				goto pickRandom;

			player.LookAt(animTarget, animTarget - player.position - player.forward);
		}
	}

	IEnumerator ChangeLevel()
	{
		// disable interractibility
		levelSelector.SetCompletion(false);
		levelSelector.SetInterractibility(false, false);
		levelSelector.DisplayText("-");

		difficultySelector.SetCompletion(false);
		difficultySelector.SetInterractibility(false, false);
		difficultySelector.DisplayText("-");

		// map delete animation
		float timer = 0;
		Vector3 initialSize;

		if (MapsManager.SpawnedMap != null)
		{
			initialSize = MapsManager.SpawnedMap.transform.localScale;

			while (timer < levelAnimDuration)
			{
				float percent = timer / levelAnimDuration;
				MapsManager.SpawnedMap.transform.localScale = Vector3.Lerp(initialSize, Vector3.zero, percent);

				timer += Time.deltaTime;
				yield return null;
			}

			Destroy(MapsManager.SpawnedMap);
		}

		// spawn map
		MapSize size = MapsManager.GetMapPerIndex(levelSelector.index).size;
		MapsManager.SpawnMap(size, true, animCenter);

		// level selector
		int intSize = int.Parse(size.ToString().Replace("_", string.Empty));
		levelSelector.DisplayText(string.Format("{0} x {0}", intSize));
		bool canGoPlus = levelSelector.index < MapsCount - 1 ? Save.Data.UnlockedMap((MapSize)levelSelector.index + 1) : false;
		levelSelector.SetInterractibility(levelSelector.index != 0, canGoPlus);

		// difficulty selector
		difficultySelector.index = 0;
		List<string> choices = new List<string>();

		for (int i = 0; i < DifficultiesCount; i++)
		{
			if (Save.Data.unlockedDifficulties[levelSelector.index].difficulties[i])
				choices.Add(((Difficulty)i).ToString());
		}

		difficultySelector.SetChoices(choices.ToArray());
		difficultySelector.DisplayText(choices[0]);

		difficultySelector.SubscribeEvents(UpdateDifficulty, UpdateDifficulty);
		DifficultyManager.CurrentDifficulty = (Difficulty)Enum.Parse(typeof(Difficulty), difficultySelector.display.text);

		// map grow anim
		Vector3 targetSize = Vector3.one * 5;
		timer = 0;
		initialSize = Vector3.zero;

		while (timer < levelAnimDuration)
		{
			float percent = timer / levelAnimDuration;
			MapsManager.SpawnedMap.transform.localScale = Vector3.Lerp(initialSize, targetSize, percent);

			timer += Time.deltaTime;
			yield return null;
		}

		MapsManager.SpawnedMap.transform.localScale = targetSize;

		// selector completions
		levelSelector.SetCompletion(Save.Data.CompletedMap(size));
		difficultySelector.SetCompletion(Save.Data.CompletedLevel(size, DifficultyManager.CurrentDifficulty));
	}

	IEnumerator StartLevel()
	{
		DifficultyManager.SetCurrentPiecesTarget(DifficultyManager.GetCurrentDifficultySetting().GetTotalPieces(MapsManager.SpawnedMap.size));
		float timer = 0;

		if (GameManager.CurrentState == GameState.Main_Menu)
		{
			// fade preview map
			float initialSize = MapsManager.SpawnedMap.transform.localScale.x;

			while (timer < 0.5f)
			{
				timer += Time.deltaTime;

				MapsManager.SpawnedMap.transform.localScale = Vector3.one * Mathf.Lerp(initialSize, 0, timer / 0.5f);
				yield return null;
			}
		}

		// spawn map
		MapsManager.SpawnMap((MapSize)levelSelector.index, false, Vector3.zero);
		float targetSize = MapsManager.SpawnedMap.transform.localScale.x;
		MapsManager.SpawnedMap.transform.localScale = Vector3.zero;

		// spawn player
		Vector3 initialPos = MapsManager.SpawnedMap.transform.position - Vector3.up * (targetSize - GameManager.GamePlayerPrefab.height);
		Player player = Instantiate(GameManager.GamePlayerPrefab, initialPos, Quaternion.identity);
		player.Init();

		// start camera anim
		GameManager.ChangeState(GameState.Game);

		// animate map
		timer = 0;
		while (timer < spawnAnimSpeed)
		{
			timer += Time.deltaTime;

			MapsManager.SpawnedMap.transform.localScale = Vector3.one * Mathf.Lerp(0, targetSize, timer / spawnAnimSpeed);

			yield return null;
		}

		startGameTimer.StartTimer(() =>
		{
			completionTime = Time.time;
			player.StartGame();
			MapsManager.SpawnPickUp();
		});
	}

	IEnumerator FadeEndScreen(float target)
	{
		eolScreenGroup.interactable = false;
		eolScreenGroup.blocksRaycasts = false;

		float initialAlpha = eolScreenGroup.alpha;

		float timer = 0;
		while (timer < eolScreenFadeDuration)
		{
			timer += Time.deltaTime;

			eolScreenGroup.alpha = Mathf.Lerp(initialAlpha, target, timer / eolScreenFadeDuration);
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

		if (isVictory)
		{
			switch (DifficultyManager.CurrentDifficulty)
			{
				case Difficulty.Easy:
					Save.Data.unlockedDifficulties[(int)MapsManager.SpawnedMap.size].difficulties[1] = true;
					break;

				case Difficulty.Medium:
					switch (MapsManager.SpawnedMap.size)
					{
						case MapSize._6:
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

		anim.Play("ShowEndButtons");
	}

	IEnumerator Replay()
	{
		Player.CleanPlayer();
		StartCoroutine(StartLevel());

		yield return FadeEndScreen(0);
	}

	IEnumerator FromEndToMenu()
	{
		Player.CleanPlayer();
		GameManager.ChangeState(GameState.Main_Menu);

		yield return FadeEndScreen(0);
	}
}