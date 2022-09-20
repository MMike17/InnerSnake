using System;
using System.Collections.Generic;
using UnityEngine;

using static MapsManager;

/// <summary>Manages the difficulty of the game</summary>
public class DifficultyManager : MonoBehaviour
{
	public static int DifficultiesCount => instance.difficulties.Count;
	public static Difficulty CurrentDifficulty;

	static DifficultyManager instance;

	public enum Difficulty
	{
		Easy = 0,
		Medium = 1,
		Hard = 2
	}

	[Header("Settings")]
	public List<DifficultySetting> difficulties;
	public MinMax speedRange;

	MinMax currentPiecesRange;

	public static DifficultySetting GetCurrentDifficultySetting()
	{
		return instance.difficulties.Find(item => item.difficulty == CurrentDifficulty);
	}

	public static void SetCurrentPiecesTarget(int target) => instance.currentPiecesRange = new MinMax(0, target);

	public static float UpdateSpeed(int currentCount)
	{
		float piecesPercent = instance.currentPiecesRange.GetPercent(currentCount);
		return instance.speedRange.GetValue(piecesPercent);
	}

	public void Init()
	{
		instance = this;
	}

	/// <summary>Game settings for a difficulty level</summary>
	[Serializable]
	public class DifficultySetting
	{
		public Difficulty difficulty;
		[Range(0, 1)]
		[SerializeField] float piecesPerCell;
		[Space]
		[SerializeField] MinMax pieceDistancePercent;

		public int GetTotalPieces(MapSize size)
		{
			return Mathf.RoundToInt(Mathf.Pow(int.Parse(size.ToString().TrimStart('_')), 2) * piecesPerCell);
		}

		public float GetPieceDistance(MapSize size, int collectedPieces)
		{
			return pieceDistancePercent.GetValue(new MinMax(0, GetTotalPieces(size)).GetPercent(collectedPieces));
		}
	}
}