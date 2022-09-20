using System;
using UnityEngine;

/// <summary>Helper class to convert From and To a min-max range</summary>
[Serializable]
public class MinMax
{
	public float min;
	public float max;

	public MinMax(float min, float max)
	{
		this.min = min;
		this.max = max;
	}

	public float GetValue(float percent) => Mathf.Lerp(min, max, percent);
	public float GetPercent(float value) => ((float)Mathf.Clamp(value, min, max) - min) / (max - min);
}