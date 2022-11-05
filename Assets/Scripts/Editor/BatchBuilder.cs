using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

/// <summary>Tool to plan consécutive builds for multiple platforms</summary>
class BatchBuilder : EditorWindow
{
	string BuildPath => Path.Combine(Application.dataPath, "Builds");

	List<BuildTarget> platforms;
	string[] scenePaths;
	Vector2 scroll;
	BuildTarget selectedPlatform;
	BuildTarget toDelete;
	GUIStyle titleStyle;

	[MenuItem("Tools/Batch Builder")]
	static void ShowWindow()
	{
		var window = GetWindow<BatchBuilder>();
		window.titleContent = new GUIContent("Batch Builder");
		window.Show();
	}

	void GenerateIfNeeded()
	{
		if (titleStyle == null)
		{
			titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperCenter, fontStyle = FontStyle.Bold };
		}

		if (platforms == null)
			platforms = new List<BuildTarget>();
	}

	void OnGUI()
	{
		GenerateIfNeeded();

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Batch Builder", titleStyle);

		EditorGUILayout.Space();

		scroll = EditorGUILayout.BeginScrollView(scroll);

		foreach (BuildTarget platform in platforms)
			DisplayPlatform(platform);

		// this is hillarious
		if ((int)toDelete != 45)
		{
			platforms.Remove(toDelete);
			toDelete = (BuildTarget)45;
		}

		EditorGUILayout.EndScrollView();

		SavePlatforms();

		EditorGUILayout.Space();

		CenterDisplay(() =>
		{
			selectedPlatform = (BuildTarget)EditorGUILayout.EnumPopup(selectedPlatform);

			EditorGUILayout.Space();

			if (GUILayout.Button("Add platform"))
				platforms.Add(selectedPlatform);
		});

		EditorGUILayout.Space();

		CenterDisplay(() =>
		{
			if (GUILayout.Button("Start builds"))
			{
				scenePaths = new string[EditorSceneManager.sceneCountInBuildSettings];

				for (int i = 0; i < scenePaths.Length; i++)
					scenePaths[i] = EditorSceneManager.GetSceneByBuildIndex(i).path;

				platforms.ForEach(item => BuildForPlatform(item));
			}
		});

		EditorGUILayout.Space();
		EditorGUILayout.Space();
	}

	void CenterDisplay(Action GUIClips)
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.Space();

		GUIClips();

		EditorGUILayout.Space();
		EditorGUILayout.EndHorizontal();
	}

	void DisplayPlatform(BuildTarget platform)
	{
		CenterDisplay(() =>
		{
			EditorGUILayout.LabelField(platform.ToString());

			EditorGUILayout.Space();

			if (GUILayout.Button("Remove"))
				toDelete = platform;
		});
	}

	void SavePlatforms()
	{
		string json = JsonUtility.ToJson(platforms.ToArray());
		PlayerPrefs.SetString("BatchBuilder_platforms", json);
		PlayerPrefs.Save();
	}

	void BuildForPlatform(BuildTarget platform)
	{
		Debug.Log("Started build for <b>" + platform + "</b>");

		// generate folder
		string platformBuildPath = Path.Combine(BuildPath, platform.ToString());

		if (!Directory.Exists(platformBuildPath))
			Directory.CreateDirectory(platformBuildPath);

		BuildPlayerOptions buildOptions = new BuildPlayerOptions()
		{
			locationPathName = platformBuildPath,
			target = platform,
			scenes = scenePaths,
			options = BuildOptions.ShowBuiltPlayer
			// targetGroup =, // do we need this ?
		};

		BuildReport report = BuildPipeline.BuildPlayer(buildOptions);

		PrintReport(report);
	}

	void PrintReport(BuildReport report)
	{
		string result = report.summary + "\n\nPassed steps :\n";

		foreach (BuildStep step in report.steps)
		{
			string spaces = "";

			for (int i = 0; i < step.depth; i++)
				spaces += " ";

			result += spaces + "<b>" + step.name + "</b> | duration : " + step.duration.ToString(@"\hh:\mm:\ss") + "\n";
		}

		Debug.Log(result);
	}
}