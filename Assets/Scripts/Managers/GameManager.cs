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
		Main_Menu,
		Level_selection,
		Game,
		End_Menu
	}

	[Header("Settings")]
	public float colorAnimCycleDuration;

	[Header("Scene references")]
	public CameraManager cameraManager;
	public DifficultyManager difficultyManager;
	public MainMenu mainMenu;
	public MapsManager mapsManager;
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

		currentState = GameState.Main_Menu;
		GamePlayerPrefab = gamePlayerPrefab;

		cameraManager.Init();
		difficultyManager.Init();
		mapsManager.Init();

		DataSaver.LoadGameData(DifficultyManager.DifficultiesCount, MapsManager.MapsCount);

		mainMenu.Init();

		OnStateChanged.Invoke(currentState);
	}

	void Update() => AnimateColor.Update();

	void OnApplicationQuit() => DataSaver.SaveGameData();
}