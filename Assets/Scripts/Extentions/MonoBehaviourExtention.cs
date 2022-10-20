using System;
using System.Collections;
using UnityEngine;

/// <summary>Extends the MonoBehaviour class</summary>
public static class MonoBehaviourExtention
{
	public static void DelayAction(this MonoBehaviour runner, Action callback, float delay)
	{
		runner.StartCoroutine(DelayRoutine(callback, delay));
	}

	static IEnumerator DelayRoutine(Action callback, float delay)
	{
		yield return new WaitForSecondsRealtime(delay);
		callback();
	}
}