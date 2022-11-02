using System;
using UnityEngine;

/// <summary>Manages the main flow of the game</summary>
public class GameManager : MonoBehaviour
{
	public static Player GamePlayerPrefab;
	public static event Action<GameState> OnStateChanged;
	public static GameState CurrentState => currentState;

	public enum GameState
	{
		First_Game,
		Main_Menu,
		Score_History,
		Level_Selection,
		Game,
		End_Menu
	}

	[Header("Settings")]
	public float colorAnimCycleDuration;

	[Header("Scene references")]
	public CameraManager cameraManager;
	public DifficultyManager difficultyManager;
	public MainMenu mainMenu;
	public ScoreHistoryMenu scoreHistoryMenu;
	public EndLevelMenu endLevelMenu;
	public MapsManager mapsManager;
	public SoundsManager soundsManager;
	[Space]
	public Player gamePlayerPrefab;

	static GameState currentState;

	public static void ChangeState(GameState target)
	{
		// prevent double calling
		if (currentState == target)
			return;

		currentState = target;
		OnStateChanged(target);
	}

	void Awake()
	{
		AnimateColor.cycleDuration = colorAnimCycleDuration;
		GamePlayerPrefab = gamePlayerPrefab;

		cameraManager.Init();
		difficultyManager.Init();
		mapsManager.Init();
		soundsManager.Init();

		DataSaver.LoadGameData(DifficultyManager.DifficultiesCount, MapsManager.MapsCount);
		currentState = string.IsNullOrEmpty(Save.Data.playerName) && ServerManager.HasConnection ? GameState.First_Game : GameState.Main_Menu;

		// pause until logged in
		Time.timeScale = 0;
		ServerManager.Login(() => Time.timeScale = 1);

		mainMenu.Init();
		scoreHistoryMenu.Init();
		endLevelMenu.Init(
			() => mainMenu.anim.Play("ShowEndButtons"),
			() => mainMenu.anim.Play("HideEndButtons"),
			() => StartCoroutine(mainMenu.StartLevel())
		);

		if (Save.Data.waitingResults.Count > 0 && ServerManager.HasConnection)
			ServerManager.SendWaitingScores();

		OnStateChanged.Invoke(currentState);
	}

	void Update() => AnimateColor.Update();

	void OnApplicationQuit() => DataSaver.SaveGameData();
}