using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using static DifficultyManager;
using static MapsManager;

public class TEST_Login : MonoBehaviour
{
	public string testName;

	[ContextMenu("Test login")]
	void Login()
	{
		LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
		{
			CustomId = testName,
			CreateAccount = true
		};

		PlayFabClientAPI.LoginWithCustomID(
			request,
			result => Debug.Log(result.ToJson()),
			error => Debug.Log(error.GenerateErrorReport())
		);
	}

	[ContextMenu("test leaderboard")]
	void Leaderboard()
	{
		StartCoroutine(Test());
	}

	IEnumerator Test()
	{
		ServerManager.SendScore(MapSize._6, Difficulty.Easy, 10);

		yield return new WaitForSeconds(2);

		ServerManager.GetLeaderboard(
			MapSize._6,
			Difficulty.Easy,
			results =>
			{
				foreach (var result in results)
					Debug.Log(result.DisplayName + " / " + result.Position);
			},
			() => { }
		);
	}
}