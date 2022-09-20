using UnityEditor;
using UnityEngine;

/// <summary>Manages game data</summary>
public static class DataSaver
{
	public const string SAVE_KEY = "GameSave";

	public static void SaveGameData()
	{
		PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(Save.Data, true));
		PlayerPrefs.Save();
	}

	public static void LoadGameData(int difficultiesCount, int mapsCount)
	{
		Save.Data = JsonUtility.FromJson<Save>(PlayerPrefs.GetString(SAVE_KEY));

		if (Save.Data == null)
			Save.Data = new Save(difficultiesCount, mapsCount);
	}

#if UNITY_EDITOR
	[MenuItem("InnerSnake/Delete save data")]
#endif
	public static void DeleteGameData()
	{
		PlayerPrefs.DeleteKey(SAVE_KEY);
		PlayerPrefs.Save();

#if UNITY_EDITOR
		Debug.Log("Deleted save data");
#endif

		Save.Data = null;
	}
}