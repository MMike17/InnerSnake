using System.Collections.Generic;
using UnityEngine;

/// <summary>Base class to animate an object's color depending on time (can be synched with others)</summary>
public abstract class AnimateColor : MonoBehaviour
{
	static List<AnimateColor> instances;
	static float globalTimer;
	public static float cycleDuration;

	[Range(0, 1)]
	public float offset;

	float timer;

	protected float percent { get; private set; }

	void Awake()
	{
		if (instances == null)
			instances = new List<AnimateColor>();

		instances.Add(this);
		timer = globalTimer + offset * cycleDuration;
	}

	public void Reset(float offset)
	{
		this.offset = offset;
		timer = globalTimer + offset * cycleDuration;
	}

	public static void Update()
	{
		if (instances == null)
			return;

		instances.RemoveAll(item => item == null);
		instances.ForEach(item => item.UpdateTimer());

		AnimateColor reference = instances.Find(item => item.offset == 0);
		globalTimer = reference != null ? reference.timer : 0;

		instances.ForEach(item => item.ApplyColor());
	}

	void UpdateTimer()
	{
		timer += Time.deltaTime;

		if (timer >= cycleDuration)
			timer -= cycleDuration;

		percent = timer / cycleDuration;
	}

	protected abstract void ApplyColor();
}