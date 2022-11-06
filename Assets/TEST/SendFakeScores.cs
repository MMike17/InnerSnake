using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

using static DifficultyManager;
using static MapsManager;

public class SendFakeScores : MonoBehaviour
{
	const string LEADERBOARD_NAME_FORMAT = "Score{0}_{1}";

	public MapSize size;
	public Difficulty difficulty;
	[Space]
	public ScoreSettings[] settings;

	bool completed;

	void Awake() => StartCoroutine(SendInfos());

	IEnumerator SendInfos()
	{
		completed = false;

		foreach (ScoreSettings setting in settings)
		{
			completed = false;
			LogOut(setting);

			yield return new WaitUntil(() => completed);
		}
	}

	void LogOut(ScoreSettings settings)
	{
		if (PlayFabClientAPI.IsClientLoggedIn())
		{
			Debug.Log("Logging out : " + settings.name);
			PlayFabClientAPI.ForgetAllCredentials();
		}

		Login(settings);
	}

	void Login(ScoreSettings settings)
	{
		LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
		{
			CustomId = settings.ID,
			CreateAccount = true
		};

		Debug.Log("Loging in : " + settings.name);

		PlayFabClientAPI.LoginWithCustomID(
			request,
			result => SetPlayerName(settings),
			error =>
			{
				Debug.LogError(error.GenerateErrorReport());
				completed = true;
			}
		);
	}

	void SetPlayerName(ScoreSettings settings)
	{
		UpdateUserTitleDisplayNameRequest request = new UpdateUserTitleDisplayNameRequest()
		{
			DisplayName = settings.name
		};

		Debug.Log("Setting player name : " + settings.name);

		PlayFabClientAPI.UpdateUserTitleDisplayName(
			request,
			result => SendScore(settings),
			error =>
			{
				Debug.Log(error.GenerateErrorReport());
				completed = true;
			}
		);
	}

	void SendScore(ScoreSettings settings)
	{
		string leaderboardName = string.Format(LEADERBOARD_NAME_FORMAT, size, difficulty);

		UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest()
		{
			Statistics = new List<StatisticUpdate>(){ new StatisticUpdate() {
				StatisticName = leaderboardName,
				Value = settings.GetScore()
			}}
		};

		Debug.Log("Sending score : " + settings.name);

		PlayFabClientAPI.UpdatePlayerStatistics(
			request,
			result => completed = true,
			error =>
			{
				Debug.Log(error.GenerateErrorReport());
				completed = true;
			}
		);
	}

	[Serializable]
	public class ScoreSettings
	{
		public string ID;
		public string name;
		[Space]
		public int minutes;
		public int seconds;

		public int GetScore() => (minutes * 60 + seconds) * 1000;
	}
}