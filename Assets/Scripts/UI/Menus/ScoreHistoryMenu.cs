using System;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

using static DifficultyManager;
using static GameManager;
using static MapsManager;
using static Save;

/// <summary>Displays scores for every levels</summary>
public class ScoreHistoryMenu : MonoBehaviour
{
	const string LEVEL_FORMAT = "{0} x {0}";

	[Header("Scene references")]
	public HighscoreTicket n1Ticket;
	public HighscoreTicket n2Ticket;
	public HighscoreTicket n3Ticket;
	public HighscoreTicket currentTicket;
	[Space]
	public Graph graph;
	public GameObject noDataMessage;
	public GameObject completion;
	public ArrowSelector levelSelector;
	public ArrowSelector difficultySelector;
	[Space]
	public Button returnButton;

	Coroutine animationRoutine;

	public void Init()
	{
		returnButton.onClick.AddListener(() => GameManager.ChangeState(GameState.Main_Menu));

		levelSelector.SubscribeEvents(DisplayResults, DisplayResults);
		difficultySelector.SubscribeEvents(DisplayResults, DisplayResults);

		string[] levels = new string[MapsManager.MapsCount];
		string[] difficulties = new string[DifficultyManager.DifficultiesCount];

		for (int i = 0; i < levels.Length; i++)
			levels[i] = string.Format(LEVEL_FORMAT, ((MapSize)i).ToString().Replace("_", ""));

		for (int i = 0; i < difficulties.Length; i++)
			difficulties[i] = ((Difficulty)i).ToString();

		levelSelector.SetChoices(levels);
		difficultySelector.SetChoices(difficulties);

		GameManager.OnStateChanged += OnGameStateChange;
	}

	void OnGameStateChange(GameState state)
	{
		switch (state)
		{
			case GameState.Score_History:
				n1Ticket.SetNoData();
				n2Ticket.SetNoData();
				n3Ticket.SetNoData();
				currentTicket.SetNoData();
				break;
		}
	}

	void DisplayResults()
	{
		ServerManager.GetLeaderboard(
			(MapSize)Enum.Parse(typeof(MapSize), "_" + levelSelector.display.text.Split(' ')[0]),
			(Difficulty)Enum.Parse(typeof(Difficulty), difficultySelector.display.text),
			results =>
			{
				results.Sort((first, second) => { return second.Position - first.Position; });

				if (results.Count > 0)
					DisplayHighscoreData(results[0], n1Ticket);
				else
					n1Ticket.SetNoData();

				if (results.Count > 1)
					DisplayHighscoreData(results[1], n2Ticket);
				else
					n2Ticket.SetNoData();

				if (results.Count > 2)
					DisplayHighscoreData(results[2], n3Ticket);
				else
					n3Ticket.SetNoData();

				if (results.Count > 3)
				{
					DisplayHighscoreData(
						results.Find(item => item.DisplayName == Save.Data.playerName),
						currentTicket
					);
				}
				else
					currentTicket.SetNoData();
			},
			() =>
			{
				n1Ticket.SetNoData();
				n2Ticket.SetNoData();
				n3Ticket.SetNoData();
				currentTicket.SetNoData();
			}
		);

		graph.ClearGraph();

		LevelResult targetResult = Save.Data.results.Find(item => item.size == (MapSize)levelSelector.index && item.difficulty == (Difficulty)difficultySelector.index);

		noDataMessage.SetActive(targetResult == null);

		if (targetResult != null)
		{
			if (animationRoutine != null)
				StopCoroutine(animationRoutine);

			bool isVictory = Save.Data.CompletedLevel(
				(MapSize)levelSelector.index,
				(Difficulty)difficultySelector.index
			);
			completion.SetActive(isVictory);

			animationRoutine = StartCoroutine(
				graph.AnimateScore(
					isVictory,
					(MapSize)levelSelector.index,
					(Difficulty)difficultySelector.index,
					Save.Data
				)
			);
		}
	}

	void DisplayHighscoreData(PlayerLeaderboardEntry result, HighscoreTicket ticket)
	{
		if (result == null)
			ticket.SetNoData();
		else
			ticket.SetData(result.Position, result.DisplayName, result.StatValue);
	}
}