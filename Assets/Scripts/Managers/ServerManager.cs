using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

using static DifficultyManager;
using static MapsManager;
using static Save;

/// <summary>Manages connexion to Playfab servers</summary>
public static class ServerManager
{
	// video tutorial link
	// https://www.youtube.com/watch?v=DQWYMfZyMNU&list=PL1aAeF6bPTB4oP-Tejys3n8P8iXlj7uj-

	// website link
	// https://developer.playfab.com/en-US/BDD4E/dashboard

	const string LEADERBOARD_NAME_FORMAT = "Score{0}_{1}";

	public static bool HasConnection => Application.internetReachability != NetworkReachability.NotReachable;

	public static void Login()
	{
		if (!HasConnection)
		{
			Save.Data.firstGame = false;
			return;
		}

		LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
		{
			CustomId = Save.Data.playerName,
			CreateAccount = true
		};

		PlayFabClientAPI.LoginWithCustomID(
			request,
			result => Save.Data.firstGame = false,
			error => Debug.LogError(error.GenerateErrorReport())
		);
	}

	public static void IsNameValid(string name, Action OnNameInvalid, Action OnNameValid)
	{
		GetAccountInfoRequest request = new GetAccountInfoRequest() { Username = name };
		PlayFabClientAPI.GetAccountInfo(request, result => OnNameInvalid(), error => OnNameValid());
	}

	// TODO : Call this at the end of a game
	public static void SendScore(int completionTimeMil, MapSize size, Difficulty difficulty)
	{
		if (!HasConnection)
		{
			StoreScore(completionTimeMil, size, difficulty);
			return;
		}

		string leaderboardName = string.Format(LEADERBOARD_NAME_FORMAT, size, difficulty);

		UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest()
		{
			Statistics = new List<StatisticUpdate>(){ new StatisticUpdate() {
				StatisticName = leaderboardName,
				Value = completionTimeMil
			}}
		};

		PlayFabClientAPI.UpdatePlayerStatistics(request, result => { }, error => StoreScore(completionTimeMil, size, difficulty));
	}

	static void StoreScore(int completionTimeMil, MapSize size, Difficulty difficulty)
	{
		NetworkResult result = Save.Data.waitingResults.Find(item => item.size == size && item.difficulty == difficulty);

		if (result != null)
			result.completionTimeMil = completionTimeMil;
		else
			Save.Data.waitingResults.Add(new NetworkResult(size, difficulty, completionTimeMil));
	}

	public static void SendWaitingScores()
	{
		List<StatisticUpdate> scores = new List<StatisticUpdate>();
		string leaderboardName;

		Save.Data.waitingResults.ForEach(item =>
		{
			leaderboardName = string.Format(LEADERBOARD_NAME_FORMAT, item.size, item.difficulty);

			scores.Add(new StatisticUpdate()
			{
				StatisticName = leaderboardName,
				Value = item.completionTimeMil
			});
		});

		UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest() { Statistics = scores };
		PlayFabClientAPI.UpdatePlayerStatistics(request, result => Save.Data.waitingResults.Clear(), error => { });
	}

	// TODO : Call this from score history panel
	public static void GetLeaderboard(MapSize size, Difficulty difficulty, Action<GetLeaderboardResult> OnSuccess, Action OnFailure)
	{
		GetLeaderboardRequest request = new GetLeaderboardRequest()
		{
			StatisticName = string.Format(LEADERBOARD_NAME_FORMAT, size, difficulty),
			StartPosition = 0,
			MaxResultsCount = 3
		};

		PlayFabClientAPI.GetLeaderboard(request, OnSuccess, error => OnFailure());
	}

	// TODO : Get player rank for leaderboard
	public static void GetPlayerRank()
	{

	}
}