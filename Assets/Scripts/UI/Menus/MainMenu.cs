using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using static DifficultyManager;
using static GameManager;
using static MapsManager;

/// <summary>Manages the main menu of the game</summary>
public class MainMenu : MonoBehaviour
{
	[Header("Settings")]
	[TextArea]
	public string invalidNameErrorFormat;
	[Space]
	public float levelAnimDuration;
	public float mapRotationSpeed;
	[Space]
	public float spawnAnimSpeed;

	[Header("Scene references")]
	public TMP_InputField nameInput;
	public TMP_Text errorName;
	public Button validateNameButton;
	[Space]
	public Transform animScreenCenter;
	public MenuFakePlayer fakePlayer;
	public Animator anim;
	[Space]
	public Button newGameButton;
	public Button scoreHistoryButton;
	// public Button highscoreButton;
	public Button quitButton;
	[Space]
	public ArrowSelector levelSelector;
	public ArrowSelector difficultySelector;
	public Button playButton;
	[Space]
	public PopupTutorial tutorialPopup;
	[Space]
	public Timer startGameTimer;

	MenuFakePlayer player;
	Vector3 animCenter;
	float completionTime;
	float animSphereSize;

	public void Init()
	{
		validateNameButton.onClick.AddListener(() =>
		{
			SoundsManager.PlaySound("Click");
			errorName.text = null;

			ServerManager.IsNameValid(
				nameInput.text,
				() => errorName.text = string.Format(invalidNameErrorFormat, nameInput.text),
				() =>
				{
					Save.Data.playerName = nameInput.text;

					anim.Play("HideIntro", 0);
					this.DelayAction(() => GameManager.ChangeState(GameState.Main_Menu), 2);
				}
			);
		});
		newGameButton.onClick.AddListener(() =>
		{
			SoundsManager.PlaySound("Click");

			anim.Play("HideMain", 0);
			player.Stop(GameState.Level_Selection);
		});
		scoreHistoryButton.onClick.AddListener(() =>
		{
			SoundsManager.PlaySound("Click");

			anim.Play("HideMain", 0);
			player.Stop(GameState.Score_History);
		});
		quitButton.onClick.AddListener(() =>
		{
			SoundsManager.PlaySound("Click");
			Application.Quit();
		});
		playButton.onClick.AddListener(() =>
		{
			SoundsManager.PlaySound("Click");

			anim.Play("HideLevel", 1);
			levelSelector.SetCompletion(false);
			difficultySelector.SetCompletion(false);

			StartCoroutine(StartLevel());
		});

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
		if (MapsManager.SpawnedMap != null && GameManager.CurrentState == GameState.Level_Selection)
			MapsManager.SpawnedMap.transform.Rotate(0, mapRotationSpeed * Time.deltaTime, 0);
	}

	void OnGameStateChange(GameState state)
	{
		switch (state)
		{
			case GameState.Main_Menu:
				if (!anim.GetCurrentAnimatorStateInfo(2).IsName("HideHistory"))
					anim.Play("HideHistory", 2);

				anim.Play("ShowMain", 0);

				player = Instantiate(fakePlayer, animCenter, Quaternion.identity);
				player.Init(animCenter, animSphereSize);
				break;

			case GameState.Score_History:
				anim.Play("HideMain", 0);
				anim.Play("ShowHistory", 2);
				break;

			case GameState.Level_Selection:
				anim.Play("ShowLevel", 1);
				levelSelector.index = 0;

				StartCoroutine(ChangeLevel());
				break;
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
		bool playedSound = false;

		while (timer < levelAnimDuration)
		{
			float percent = timer / levelAnimDuration;
			MapsManager.SpawnedMap.transform.localScale = Vector3.Lerp(initialSize, targetSize, percent);

			if (percent > 0.7f && !playedSound)
			{
				SoundsManager.PlaySound("UI");
				playedSound = true;
			}

			timer += Time.deltaTime;
			yield return null;
		}

		MapsManager.SpawnedMap.transform.localScale = targetSize;

		// selector completions
		levelSelector.SetCompletion(Save.Data.CompletedMap(size));
		difficultySelector.SetCompletion(Save.Data.CompletedLevel(size, DifficultyManager.CurrentDifficulty));
	}

	public IEnumerator StartLevel()
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

		// pop tutorial
		if (Save.Data.firstGame)
		{
			tutorialPopup.Pop(() =>
			{
				this.DelayAction(() => Time.timeScale = 1, 1);
				Save.Data.firstGame = false;
			});
		}

		startGameTimer.StartTimer(() =>
		{
			completionTime = Time.time;
			player.StartGame();
			MapsManager.SpawnPickUp();
		});
	}
}