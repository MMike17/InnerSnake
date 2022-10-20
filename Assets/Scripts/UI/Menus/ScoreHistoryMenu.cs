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
	public HighscoreTicket n4Ticket;
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
		returnButton.onClick.AddListener(() => GameManager.ChangeState(GameState.Score_History));

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

		DisplayResults();
	}

	void DisplayResults()
	{
		// n1Ticket.
		// n2Ticket.
		// n3Ticket.
		// n4Ticket.

		graph.ClearGraph();

		LevelResult targetResult = Save.Data.results.Find(item => item.size == (MapSize)levelSelector.index && item.difficulty == (Difficulty)difficultySelector.index);

		noDataMessage.SetActive(targetResult == null);

		if (targetResult != null)
		{
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
}