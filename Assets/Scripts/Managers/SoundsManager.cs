using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static GameManager;

/// <summary>Manages all the sounds of the game</summary>
public class SoundsManager : MonoBehaviour
{
	const string DEBUG_FLAG = "<b>[SoundsManager]</b> : ";

	static SoundsManager instance;

	[Header("Settings")]
	public AnimationCurve fadeInCurve;
	public AnimationCurve fadeOutCurve;
	public List<SFX> sounds;

	[Header("Scene references")]
	public AudioSource sourcePrefab;

	List<AudioSource> pool;

	public void Init()
	{
		instance = this;
		pool = new List<AudioSource>();

		GameManager.OnStateChanged += OnStateChanged;
	}

	void OnStateChanged(GameState state)
	{
		switch (state)
		{
			case GameState.Main_Menu:
				FadeSound("Menu", 5, true);
				break;

			case GameState.Game:
				FadeSound("Menu", 2, false);
				break;

			case GameState.End_Menu:
				FadeSound("GameLoop", 2, false);
				FadeSound("Menu", 3, true);
				break;
		}
	}

	public static AudioSource PlaySound(string name, float customPitch = 1, float overrideVolume = 1)
	{
		SFX selectedSound = instance.FindSound(name);

		if (selectedSound == null)
			return null;

		AudioSource selectedSource = instance.GetAvailableSource();
		selectedSource.pitch = customPitch;
		selectedSound.Apply(selectedSource, overrideVolume);

		instance.pool.Add(selectedSource);
		return selectedSource;
	}

	public static void StopSound(string name)
	{
		AudioSource selectedSource = instance.FindSource(name);

		if (selectedSource == null)
			return;

		selectedSource.Stop();
	}

	public static void FadeSound(string name, float duration, bool fadeInOut)
	{
		AudioSource selectedSource = instance.FindSource(name, false);

		if (selectedSource == null)
			selectedSource = PlaySound(name);

		SFX selectedSound = instance.FindSound(name);

		if (selectedSound == null)
			return;

		instance.StartCoroutine(FadeSoundRoutine(selectedSource, duration, fadeInOut ? selectedSound.volume : 0));
	}

	static IEnumerator FadeSoundRoutine(AudioSource source, float duration, float targetVolume)
	{
		float timer = 0;
		float initialSound = source.volume;
		AnimationCurve selectedCurve = targetVolume == 0 ? instance.fadeOutCurve : instance.fadeInCurve;

		while (timer < duration)
		{
			timer += Time.deltaTime;

			source.volume = Mathf.Lerp(initialSound, targetVolume, selectedCurve.Evaluate(timer / duration));
			yield return null;
		}

		source.volume = targetVolume;
	}

	AudioSource FindSource(string name, bool LogError = true)
	{
		AudioSource selected = pool.Find(item => item.name == name && item.isPlaying);

		if (selected == null && LogError)
			Debug.LogError(DEBUG_FLAG + "Couldn't find playing source for sound \"" + name + "\"");

		return selected;
	}

	SFX FindSound(string name)
	{
		SFX selected = sounds.Find(item => item.name == name);

		if (selected == null)
			Debug.LogError(DEBUG_FLAG + "Couldn't find sound with name \"" + name + "\"");

		return selected;
	}

	AudioSource GetAvailableSource()
	{
		AudioSource selected = pool.Find(item => !item.isPlaying);

		if (selected == null)
			selected = Instantiate(sourcePrefab, transform);

		return selected;
	}

	/// <summary>Represents a sound we can play in the game</summary>
	[Serializable]
	public class SFX
	{
		public string name;
		public AudioClip clip;
		public float volume;
		public bool loop;

		public void Apply(AudioSource source, float overrideVolume)
		{
			source.clip = clip;
			source.volume = volume * overrideVolume;
			source.loop = loop;
			source.name = name;

			source.Play();
		}
	}
}