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

	// TODO : When do we send the stored scores ?
	// TODO : Call this at the end of a game
	public static void SendScore(int completionTimeMil, MapSize size, Difficulty difficulty)
	{
		if (!HasConnection)
		{
			StoreScore(completionTimeMil, size, difficulty);
			return;
		}

		string leaderboardName = "Score" + size.ToString() + "_" + difficulty;

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
}