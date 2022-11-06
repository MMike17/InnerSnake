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
	// website link
	// https://developer.playfab.com/en-US/BDD4E/dashboard

	const string LEADERBOARD_NAME_FORMAT = "Score{0}_{1}";

	public static bool HasConnection => Application.internetReachability != NetworkReachability.NotReachable;

	public static void Login(Action OnResult)
	{
		if (!HasConnection)
			return;

		LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
		{
			CustomId = SystemInfo.deviceUniqueIdentifier,
			CreateAccount = true
		};

		PlayFabClientAPI.LoginWithCustomID(
			request,
			result =>
			{
				if (!Save.Data.userRegistered && !string.IsNullOrEmpty(Save.Data.playerName))
					SetPlayerName(Save.Data.playerName);

				OnResult();
			},
			error =>
			{
				Debug.LogError(error.GenerateErrorReport());
				OnResult();
			}
		);
	}

	public static void IsNameValid(string name, Action OnNameInvalid, Action OnNameValid, Action OnNoConnection)
	{
		if (!HasConnection)
		{
			OnNoConnection();
			return;
		}

		GetAccountInfoRequest request = new GetAccountInfoRequest() { TitleDisplayName = name };

		PlayFabClientAPI.GetAccountInfo(
			request,
			result => OnNameInvalid(),
			error =>
			{
				SetPlayerName(name);
				OnNameValid();
			}
		);
	}

	static void SetPlayerName(string playerName)
	{
		if (!HasConnection)
		{
			Save.Data.playerName = playerName;
			return;
		}

		UpdateUserTitleDisplayNameRequest request = new UpdateUserTitleDisplayNameRequest()
		{
			DisplayName = playerName
		};

		PlayFabClientAPI.UpdateUserTitleDisplayName(
			request,
			result =>
			{
				Save.Data.playerName = playerName;
				Save.Data.userRegistered = true;
			},
			error => Save.Data.playerName = playerName
		);
	}

	public static void SendScore(MapSize size, Difficulty difficulty, int completionTimeMil)
	{
		if (!HasConnection)
		{
			StoreScore(size, difficulty, completionTimeMil);
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

		PlayFabClientAPI.UpdatePlayerStatistics(request, result => { }, error => StoreScore(size, difficulty, completionTimeMil));
	}

	static void StoreScore(MapSize size, Difficulty difficulty, int completionTimeMil)
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

	public static void GetLeaderboard(MapSize size, Difficulty difficulty, Action<List<PlayerLeaderboardEntry>> OnSuccess, Action OnFailure)
	{
		if (!HasConnection)
		{
			OnFailure();
			return;
		}

		GetLeaderboardRequest request = new GetLeaderboardRequest()
		{
			StatisticName = string.Format(LEADERBOARD_NAME_FORMAT, size, difficulty),
			StartPosition = 0,
			MaxResultsCount = 3
		};

		PlayFabClientAPI.GetLeaderboard(request, results => OnSuccess(results.Leaderboard), error => OnFailure());
	}
}