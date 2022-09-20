using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static DifficultyManager;
using static MapsManager;
using static Save;
using static UnityEngine.RectTransform;

/// <summary>Displays result values at end of game</summary>
public class Graph : MonoBehaviour
{
	[Header("Settings")]
	public float fadeInDuration;
	public float animDelay;
	public float lastResultAnimDuration;

	[Header("Scene references")]
	public RectTransform dataZone;
	public CanvasGroup group;
	public RectTransform indicatorLine;
	public RectTransform lastResultLine;
	[Space]
	public GraphIndicator indicatorPrefab;
	public RectTransform linePrefab;

	List<GraphIndicator> spawnedIndicators;
	List<RectTransform> spawnedLines;

	public void Hide()
	{
		group.alpha = 0;
	}

	public IEnumerator AnimateScore(bool isVictory, MapSize mapSize, Difficulty difficulty, Save save)
	{
		// clear graph
		if (spawnedIndicators != null)
			spawnedIndicators.ForEach(item => Destroy(item.gameObject));

		if (spawnedLines != null)
			spawnedLines.ForEach(item => Destroy(item.gameObject));

		lastResultLine.gameObject.SetActive(false);

		// fade in
		float timer = 0;

		while (timer < fadeInDuration)
		{
			timer += Time.deltaTime;
			group.alpha = Mathf.Lerp(0, 1, timer / fadeInDuration);

			yield return null;
		}

		// delay
		yield return new WaitForSeconds(animDelay);

		// spawn in values
		spawnedIndicators = new List<GraphIndicator>();
		spawnedLines = new List<RectTransform>();
		List<LevelResult> results = save.GetResults(mapSize, difficulty, isVictory);

		if (results.Count > 1)
		{
			float min = results[0].stat;
			float max = min;

			foreach (LevelResult result in results)
			{
				if (result.stat < min)
					min = result.stat;

				if (result.stat > max)
					max = result.stat;
			}

			float horizontalStep = dataZone.rect.width / results.Count;
			float startHorizontalPoint = dataZone.position.x - dataZone.rect.width / 2 + horizontalStep / 2;
			float verticalStep = dataZone.rect.height / max - min;
			float[] heights = new float[results.Count];

			// specific case where all results are equal
			bool allResultsEqual = true;

			results.ForEach(item =>
			{
				if (item.stat != results[0].stat)
					allResultsEqual = false;
			});

			// spawn indicators
			for (int i = 0; i < results.Count; i++)
			{
				int currentResult = results[i].stat;

				GraphIndicator indicator = Instantiate(indicatorPrefab, dataZone);
				indicator.transform.position = new Vector2(
					startHorizontalPoint + horizontalStep * i,
					indicatorLine.position.y
				);

				float heightPercent = allResultsEqual ? 0.5f : Mathf.InverseLerp(min, max, currentResult);
				heights[i] = Mathf.Lerp(dataZone.position.y - dataZone.rect.height / 2, dataZone.position.y + dataZone.rect.height / 2, heightPercent);

				bool shouldAnimate = i != results.Count - 1;

				indicator.SetValue(shouldAnimate ? currentResult : 0, isVictory, i, heights[i], shouldAnimate);
				spawnedIndicators.Add(indicator);
			}

			GraphIndicator lastIndicator = spawnedIndicators[spawnedIndicators.Count - 1];

			// delay
			yield return new WaitForSeconds(lastIndicator.animationDuration + animDelay);

			// spawn lines
			for (int i = 0; i < spawnedIndicators.Count - 1; i++)
			{
				RectTransform currentIndicator = spawnedIndicators[i].scoreIndicator;
				RectTransform nextIndicator = spawnedIndicators[i + 1].scoreIndicator;

				RectTransform line = Instantiate(linePrefab, dataZone);
				line.SetAsFirstSibling();
				spawnedLines.Add(line);

				Vector2 startPos = new Vector2(spawnedIndicators[i].scoreIndicator.position.x, heights[i]);
				Vector2 endPos = new Vector2(spawnedIndicators[i + 1].scoreIndicator.position.x, heights[i + 1]);

				if (i != spawnedIndicators.Count - 2)
				{
					yield return AnimateLine(line, startPos, endPos, lastIndicator.animationDuration / spawnedIndicators.Count);
				}
			}

			// last score line
			Vector2 initialPos = new Vector2(dataZone.position.x - dataZone.rect.width / 2, heights[heights.Length - 2]);
			Vector2 targetPos = initialPos;
			targetPos.x = indicatorLine.position.x;

			lastResultLine.SetSizeWithCurrentAnchors(Axis.Horizontal, 0);
			lastResultLine.position = initialPos;
			lastResultLine.gameObject.SetActive(true);

			timer = 0;
			while (timer < lastResultAnimDuration)
			{
				timer += Time.deltaTime;
				float percent = timer / lastResultAnimDuration;
				Vector3 currentPos = Vector3.Lerp(initialPos, targetPos, percent);

				SetLine(
					lastResultLine,
					currentPos,
					Vector3.Distance(initialPos, currentPos) * 2,
					0
				);

				yield return null;
			}

			lastResultLine.position = targetPos;

			// animate last indicator and line
			Vector2 startPoint = spawnedIndicators[spawnedIndicators.Count - 2].scoreIndicator.position;
			Vector2 endPoint = new Vector2(lastIndicator.scoreIndicator.position.x, heights[heights.Length - 1]);

			StartCoroutine(lastIndicator.Animate(results[results.Count - 1].stat, isVictory, heights[heights.Length - 1]));
			yield return AnimateLine(spawnedLines[spawnedLines.Count - 1], startPoint, endPoint, lastIndicator.animationDuration);
		}
		else
		{
			GraphIndicator indicator = Instantiate(indicatorPrefab, dataZone);
			indicator.transform.position = indicatorLine.transform.position;

			lastResultLine.gameObject.SetActive(false);
			spawnedIndicators.Add(indicator);

			indicator.SetValue(results[0].stat, isVictory, 0, dataZone.position.y, true);
			yield return new WaitForSeconds(indicator.animationDuration);
		}

		yield return new WaitForSeconds(animDelay);
	}

	IEnumerator AnimateLine(RectTransform line, Vector2 startPoint, Vector2 endPoint, float duration)
	{
		float timer = 0;
		float angle = Vector3.SignedAngle(Vector3.right, endPoint - startPoint, Vector3.forward);
		Vector2 endLinePos = Vector2.Lerp(startPoint, endPoint, 0.5f);

		while (timer < duration)
		{
			timer += Time.deltaTime;
			float percent = timer / duration;

			Vector2 targetPos = Vector3.Lerp(startPoint, endLinePos, percent);
			float length = Vector3.Distance(startPoint, Vector3.Lerp(startPoint, endPoint, percent));

			SetLine(
				spawnedLines[spawnedLines.Count - 1],
				targetPos,
				length,
				angle
			);

			yield return null;
		}

		SetLine(
			spawnedLines[spawnedLines.Count - 1],
			endLinePos,
			Vector2.Distance(startPoint, endPoint),
			angle
		);
	}

	void SetLine(RectTransform line, Vector2 pos, float width, float angle)
	{
		line.position = pos;
		line.eulerAngles = new Vector3(0, 0, angle);
		line.SetSizeWithCurrentAnchors(Axis.Horizontal, width);
	}
}