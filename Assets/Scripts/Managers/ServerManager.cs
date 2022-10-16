using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

using static DifficultyManager;
using static MapsManager;

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

	// TODO : Call this from "first game" panel
	public static void IsNameValid(string name, Action OnSuccess, Action OnFail)
	{
		GetAccountInfoRequest request = new GetAccountInfoRequest() { Username = name };
		PlayFabClientAPI.GetAccountInfo(request, result => OnSuccess(), error => OnFail());
	}

	public static void SendScore(int completionTimeMil, MapSize size, Difficulty difficulty)
	{
		if (!HasConnection)
		{
			// TODO : store the statistics so that we can send them later ;)
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

		// TODO : Finish this
		// PlayFabClientAPI.UpdatePlayerStatistics(request, OnSendScoreSuccess, OnSendScoreFail);
	}

	// static void OnSendScoreSuccess(UpdatePlayerStatisticsResult result) => Save.Data.firstGame = false;

	// static void OnSendScoreFail(PlayFabError error)
	// {
	// Debug.LogError(error.GenerateErrorReport());
	// }
}