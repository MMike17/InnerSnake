using UnityEngine;

/// <summary>Class executing special commands to debug the game</summary>
public class DebugManager : MonoBehaviour
{
	[Header("Settings")]
	public KeyCode loseKey;
	public KeyCode timePlusKey;
	public KeyCode timeMinusKey;
	public KeyCode stopPlayerKey;

	bool stopSpeed;

	Player _player;
	Player player
	{
		get
		{
			if (_player == null)
				_player = FindObjectOfType<Player>();

			return _player;
		}
	}

#if UNITY_EDITOR
	void Update()
	{
		if (Input.GetKeyDown(loseKey))
			FindObjectOfType<Player>().GameOver();

		Time.timeScale = 1;

		if (Input.GetKey(timePlusKey))
			Time.timeScale = 2;

		if (Input.GetKey(timeMinusKey))
			Time.timeScale = 0.5f;

		if (Input.GetKeyDown(stopPlayerKey))
			stopSpeed = !stopSpeed;

		if (stopSpeed)
			player.BlockSpeed(stopSpeed);
	}
#endif
}