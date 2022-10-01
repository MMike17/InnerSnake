using System.Collections;
using UnityEngine;

using static DifficultyManager;
using static MapsManager;

/// <summary>Tests Graph script</summary>
public class TEST_Graph : MonoBehaviour
{
	[Header("Settings")]
	public bool isVictory;
	public MapSize mapSize;
	public Difficulty difficulty;
	[Space]
	public Save testData;

	[Header("Scene references")]
	public Graph graph;

	bool running;

	[ContextMenu("Start Graph")]
	void StartGraph()
	{
		if (!running)
		{
            testData.results.ForEach(item => item.completed = isVictory);
			StartCoroutine(Run());
		}
	}

	IEnumerator Run()
	{
		running = true;

		yield return graph.AnimateScore(
			isVictory,
			mapSize,
			difficulty,
			testData
		);

		running = false;
	}
}