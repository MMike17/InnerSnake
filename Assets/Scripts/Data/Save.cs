using System;
using System.Collections.Generic;
using UnityEngine;

using static DifficultyManager;
using static MapsManager;

/// <summary>Stores all the persistant data of the game</summary>
[Serializable]
public class Save
{
	const int MAX_RESULTS_COUNT = 8;

	public static Save Data;

	public List<LevelDifficulty> unlockedDifficulties;
	public List<LevelResult> results;
	public List<NetworkResult> waitingResults;
	public string playerName;
	public bool finishedGame;
	public bool showedTutorial;
	public bool userRegistered;
	public bool askedRating;

	public Save(int difficultiesCount, int mapsCount)
	{
		unlockedDifficulties = new List<LevelDifficulty>();

		for (int i = 0; i < mapsCount; i++)
			unlockedDifficulties.Add(new LevelDifficulty((MapSize)i, new bool[difficultiesCount]));

		unlockedDifficulties[0].difficulties[0] = true;

		results = new List<LevelResult>();
		waitingResults = new List<NetworkResult>();
		playerName = null;

		finishedGame = false;
		showedTutorial = false;
		userRegistered = false;
		askedRating = false;
	}

	public List<LevelResult> GetResults(MapSize map, Difficulty difficulty, bool completed)
	{
		return results.FindAll(item => item.size == map && item.difficulty == difficulty && item.completed == completed);
	}

	public bool UnlockedMap(MapSize size)
	{
		LevelDifficulty selected = unlockedDifficulties.Find(item => item.mapSize == size);

		if (selected == null)
		{
			Debug.LogWarning("Couldn't find LevelDifficulty for map size " + size);
			return false;
		}

		foreach (bool unlock in selected.difficulties)
		{
			if (unlock)
				return true;
		}

		return false;
	}

	public bool CompletedMap(MapSize map)
	{
		bool easy = results.Find(item => item.size == map && item.difficulty == Difficulty.Easy && item.completed) != null;
		bool medium = results.Find(item => item.size == map && item.difficulty == Difficulty.Medium && item.completed) != null;
		bool hard = results.Find(item => item.size == map && item.difficulty == Difficulty.Hard && item.completed) != null;
		return easy && medium && hard;
	}

	public bool CompletedLevel(MapSize map, Difficulty difficulty)
	{
		return results.Find(item => item.size == map && item.difficulty == difficulty && item.completed) != null;
	}

	public bool ProcessUnlocks(int result, bool isVictory, MapSize size, Difficulty currentDifficulty)
	{
		bool hasHardUnlock = false;

		if (isVictory)
		{
			switch (currentDifficulty)
			{
				case Difficulty.Easy:
					unlockedDifficulties[(int)size].difficulties[(int)Difficulty.Medium] = true;
					break;

				case Difficulty.Medium:
					switch (size)
					{
						case MapSize._6:
							if (!unlockedDifficulties[(int)size].difficulties[(int)Difficulty.Hard])
								hasHardUnlock = true;

							unlockedDifficulties[(int)size].difficulties[(int)Difficulty.Hard] = true;
							unlockedDifficulties[(int)size + 1].difficulties[(int)Difficulty.Easy] = true;
							break;

						case MapSize._8:
							unlockedDifficulties[(int)size + 1].difficulties[(int)Difficulty.Easy] = true;
							break;

						case MapSize._10:
							unlockedDifficulties[(int)size + 1].difficulties[(int)Difficulty.Easy] = true;
							break;

						case MapSize._12:
							unlockedDifficulties[(int)size].difficulties[(int)Difficulty.Hard] = true;
							break;
					}
					break;

				case Difficulty.Hard:
					if (size != MapSize._12)
						unlockedDifficulties[(int)size + 1].difficulties[(int)Difficulty.Hard] = true;
					else if (!finishedGame)
						finishedGame = true;
					break;
			}
		}

		// save data
		results.Add(new LevelResult(size, currentDifficulty, isVictory, result));

		return hasHardUnlock;
	}

	/// <summary>Represents a result for a level played on a certain difficulty</summary>
	[Serializable]
	public class LevelResult
	{
		public MapSize size;
		public Difficulty difficulty;

		public bool completed;
		public int collected;
		public int completionTimeMil;

		public int stat => completed ? completionTimeMil : collected;

		public LevelResult(MapSize size, Difficulty difficulty, bool completed, int stat)
		{
			this.size = size;
			this.difficulty = difficulty;

			this.completed = completed;

			if (completed)
				completionTimeMil = stat;
			else
				collected = stat;
		}
	}

	/// <summary>Represents a game's results that we couldn't send to the server</summary>
	[Serializable]
	public class NetworkResult
	{
		public MapSize size;
		public Difficulty difficulty;
		public int completionTimeMil;

		public NetworkResult(MapSize size, Difficulty difficulty, int completionTimeMil)
		{
			this.size = size;
			this.difficulty = difficulty;
			this.completionTimeMil = completionTimeMil;
		}
	}

	/// <summary>Unlocked difficulties by map size</summary>
	[Serializable]
	public class LevelDifficulty
	{
		public MapSize mapSize;
		public bool[] difficulties;

		public LevelDifficulty(MapSize mapSize, bool[] difficulties)
		{
			this.mapSize = mapSize;
			this.difficulties = difficulties;
		}
	}
}