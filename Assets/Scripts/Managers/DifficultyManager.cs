using System;
using System.Collections.Generic;
using UnityEngine;

using static MapsManager;

/// <summary>Manages the difficulty of the game</summary>
public class DifficultyManager : MonoBehaviour
{
	public static int DifficultiesCount => instance.difficulties.Count;
	public static float MinSpeed => instance.difficulties[0].speedRange.min;
	public static float MaxSpeed => instance.difficulties[2].speedRange.max;
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

	MinMax currentPiecesRange;

	public static DifficultySetting GetCurrentDifficultySetting()
	{
		return instance.difficulties.Find(item => item.difficulty == CurrentDifficulty);
	}

	public static void SetCurrentPiecesTarget(int target) => instance.currentPiecesRange = new MinMax(0, target);

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
		public MinMax speedRange;

		public int GetTotalPieces(MapSize size)
		{
			return Mathf.RoundToInt(Mathf.Pow(int.Parse(size.ToString().TrimStart('_')), 2) * piecesPerCell);
		}

		public float GetPieceDistance(MapSize size, int collectedPieces)
		{
			return pieceDistancePercent.GetValue(new MinMax(0, GetTotalPieces(size)).GetPercent(collectedPieces));
		}

		public float GetSpeed(MapSize size, int collectedPieces)
		{
			return speedRange.GetValue(new MinMax(0, GetTotalPieces(size)).GetPercent(collectedPieces));
		}
	}
}