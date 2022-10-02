using System;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

/// <summary>Manages the playable maps of the game</summary>
public class MapsManager : MonoBehaviour
{
	public static int MapsCount => instance.gameMaps.Count;

	public static Pickup CurrentPickup { get; private set; }
	public static Map SpawnedMap { get; private set; }

	static MapsManager instance;

	public enum MapSize
	{
		_6 = 0,
		_8 = 1,
		_10 = 2,
		_12 = 3
	}

	[Header("Settings")]
	public List<GameMap> gameMaps;
	public Pickup pickupPrefab;

	public void Init()
	{
		instance = this;

		Map testMap = gameMaps.Find(item => item.size == MapSize._6).inPrefab;
	}

	public static void SpawnMap(MapSize size, bool isPreview, Vector3 position)
	{
		// clean map
		if (SpawnedMap != null)
			Destroy(SpawnedMap.gameObject);

		// clean pickup
		if (CurrentPickup != null)
			Destroy(CurrentPickup.gameObject);

		GameMap selectedMap = instance.gameMaps.Find(item => item.size == size);
		SpawnedMap = Instantiate(isPreview ? selectedMap.outPrefab : selectedMap.inPrefab, position, Quaternion.identity);
	}

	public static void SpawnPickUp()
	{
		List<Transform> validPoints = new List<Transform>();
		float currentDistance;
		float currentThreshold = SpawnedMap.Radius * DifficultyManager.GetCurrentDifficultySetting().GetPieceDistance(SpawnedMap.size, Player.CollectedPieces);

		foreach (Transform point in SpawnedMap.gridPoints)
		{
			currentDistance = Vector3.Distance(Player.Transform.position, point.position);

			if (currentDistance > currentThreshold)
				validPoints.Add(point);
		}

		if (validPoints.Count == 0)
		{
			Debug.LogError("This should never happen");
			return;
		}

		Vector3 selectedPosition = validPoints[Random.Range(0, validPoints.Count)].position;
		Vector3 normal = (SpawnedMap.transform.position - selectedPosition).normalized;

		CurrentPickup = Instantiate(instance.pickupPrefab, selectedPosition, Quaternion.identity);
		CurrentPickup.Init(SpawnedMap.transform.position, SpawnedMap.Radius, Player.PlayerHeight);
		CurrentPickup.transform.up = normal;
	}

	public static GameMap GetMapPerIndex(int index) => instance.gameMaps[index];

	/// <summary>Map set where the main gameplay takes place</summary>
	[Serializable]
	public class GameMap
	{
		public MapSize size;
		public Map inPrefab;
		public Map outPrefab;
	}
}