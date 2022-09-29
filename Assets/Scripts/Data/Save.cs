using System;
using System.Collections.Generic;
using UnityEngine;

using static DifficultyManager;
using static MapsManager;

/// <summary>Stores all the persistant data of the game</summary>
[Serializable]
public class Save
{
	const int MAX_RESULTS_COUNT = 10;

	public static Save Data;

	public List<LevelDifficulty> unlockedDifficulties;
	public List<LevelResult> results;

	public Save(int difficultiesCount, int mapsCount)
	{
		unlockedDifficulties = new List<LevelDifficulty>();

		for (int i = 0; i < mapsCount; i++)
			unlockedDifficulties.Add(new LevelDifficulty((MapSize)i, new bool[difficultiesCount]));

		unlockedDifficulties[0].difficulties[0] = true;

		results = new List<LevelResult>();
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

	public bool CompletedLevel(MapSize map, Difficulty difficulty)
	{
		return results.Find(item => item.size == map && item.difficulty == difficulty && item.completed) != null;
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