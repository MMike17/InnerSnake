using UnityEngine;
using UnityEditor;

using static DifficultyManager;
using static MapsManager;
using static Save;

/// <summary>Editor script to edit saved data</summary>
class SaveEditor : EditorWindow
{
	static Save loadedSave;

	[MenuItem("InnerSnake/SaveEditor", false, 0)]
	static void ShowWindow()
	{
		var window = GetWindow<SaveEditor>();
		window.titleContent = new GUIContent("Save");
		window.minSize = new Vector2(1120, 480);

		loadedSave = JsonUtility.FromJson<Save>(PlayerPrefs.GetString(DataSaver.SAVE_KEY));

		window.Show();
	}

	GUIStyle normalCenter;
	GUIStyle boldCenter;
	GUIStyle fadedCenter;

	GUIStyle tableStyle;
	GUIStyle inTableLabelStyle;
	GUIStyle inTableToggleStyle;

	Vector2 mainScroll;

	void GenerateIfNeeded()
	{
		if (normalCenter == null)
			normalCenter = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

		if (boldCenter == null)
			boldCenter = new GUIStyle(normalCenter) { fontStyle = FontStyle.Bold };

		if (fadedCenter == null)
		{
			fadedCenter = new GUIStyle(normalCenter) { fontStyle = FontStyle.Italic, normal = new GUIStyleState() { textColor = Color.grey } };
		}

		if (tableStyle == null)
		{
			tableStyle = new GUIStyle(GUI.skin.box)
			{
				padding = new RectOffset(10, 10, 10, 10),
				margin = new RectOffset(20, 20, 0, 0)
			};
		}

		if (inTableLabelStyle == null)
			inTableLabelStyle = new GUIStyle(GUI.skin.label) { fixedWidth = 150, alignment = TextAnchor.MiddleCenter };

		if (inTableToggleStyle == null)
			inTableToggleStyle = new GUIStyle(GUI.skin.toggle) { fixedWidth = 25, alignment = TextAnchor.MiddleCenter, imagePosition = ImagePosition.ImageOnly };
	}

	void OnGUI()
	{
		GenerateIfNeeded();

		EditorGUILayout.LabelField($"Save key : \"{DataSaver.SAVE_KEY}\"", fadedCenter);

		EditorGUILayout.Space();
		EditorGUILayout.Space();

		if (loadedSave == null)
		{
			EditorGUILayout.BeginVertical();

			EditorGUILayout.LabelField("No save data found", boldCenter);
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Do you want to generate new save data ?", normalCenter);

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();

			if (GUILayout.Button("Generate save data"))
			{
				loadedSave = new Save(
					FindObjectOfType<DifficultyManager>(true).difficulties.Count,
					FindObjectOfType<MapsManager>(true).gameMaps.Count
				);
			}

			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
		}
		else
		{
			EditorGUILayout.BeginVertical();
			{
				mainScroll = EditorGUILayout.BeginScrollView(mainScroll);
				{
					EditorGUILayout.LabelField("Unlocked difficulties", boldCenter);
					EditorGUILayout.Space();

					EditorGUILayout.BeginVertical(tableStyle);
					{
						for (int y = 0; y < loadedSave.unlockedDifficulties[0].difficulties.Length + 1; y++)
						{
							EditorGUILayout.BeginHorizontal();
							{
								for (int x = 0; x < loadedSave.unlockedMaps.Length + 1; x++)
								{
									if (y == 0)
									{
										if (x == 0)
											EditorGUILayout.LabelField("[Map size / Difficulty]", inTableLabelStyle);
										else
										{
											string size = ((MapSize)x - 1).ToString().Replace("_", "");
											EditorGUILayout.LabelField(size, inTableLabelStyle);
										}
									}
									else
									{
										if (x == 0)
											EditorGUILayout.LabelField(((Difficulty)y - 1).ToString(), inTableLabelStyle);
										else
										{
											LevelDifficulty selected = loadedSave.unlockedDifficulties.Find(item => item.mapSize == (MapSize)x - 1);

											if (selected == null)
												Debug.LogWarning("This should not happen");
											else
												selected.difficulties[y - 1] = EditorGUILayout.Toggle(selected.difficulties[y - 1], inTableToggleStyle);
										}
									}
								}
							}
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.Space();
						}

						loadedSave.unlockedDifficulties.ForEach(item =>
						{
							bool mapActive = false;

							foreach (bool diff in item.difficulties)
							{
								if (diff)
									mapActive = true;
							}

							loadedSave.unlockedMaps[(int)item.mapSize] = mapActive;
						});
					}
					EditorGUILayout.EndVertical();

					EditorGUILayout.Space();
					EditorGUILayout.BeginVertical();
					{
						EditorGUILayout.LabelField("Game results", boldCenter);

						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.Space();

						int newCount = EditorGUILayout.IntField("Results count :", loadedSave.results.Count);

						if (newCount < loadedSave.results.Count)
						{
							while (loadedSave.results.Count != newCount)
								loadedSave.results.RemoveAt(loadedSave.results.Count - 1);
						}

						if (newCount > loadedSave.results.Count)
						{
							while (loadedSave.results.Count != newCount)
								loadedSave.results.Add(new LevelResult(0, 0, false, 0));
						}

						EditorGUILayout.Space();
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.Space();

						for (int i = 0; i < loadedSave.results.Count; i++)
						{
							EditorGUILayout.Space();
							EditorGUILayout.LabelField($"Element {i}", fadedCenter);

							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.Space();

							loadedSave.results[i].size = (MapSize)EditorGUILayout.EnumPopup("Map size :", loadedSave.results[i].size);
							EditorGUILayout.Space();
							loadedSave.results[i].completed = EditorGUILayout.Toggle("Completed :", loadedSave.results[i].completed);

							EditorGUILayout.Space();
							EditorGUILayout.EndHorizontal();

							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.Space();

							loadedSave.results[i].difficulty = (Difficulty)EditorGUILayout.EnumPopup("Difficulty :", loadedSave.results[i].difficulty);
							EditorGUILayout.Space();

							if (loadedSave.results[i].completed)
							{
								loadedSave.results[i].completionTimeMil = EditorGUILayout.IntField("Completion miliseconds :", loadedSave.results[i].completionTimeMil);
							}
							else
							{
								loadedSave.results[i].collected = EditorGUILayout.IntField("Collected pieces :", loadedSave.results[i].collected);
							}

							EditorGUILayout.Space();
							EditorGUILayout.EndHorizontal();
						}

						EditorGUILayout.Space();
						EditorGUILayout.EndVertical();
					}
				}
				EditorGUILayout.EndScrollView();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();

				if (GUILayout.Button("Save data"))
				{
					PlayerPrefs.SetString(DataSaver.SAVE_KEY, JsonUtility.ToJson(loadedSave, true));
					PlayerPrefs.Save();
				}

				EditorGUILayout.Space();
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Space();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();

				if (GUILayout.Button("Delete data"))
				{
					loadedSave = null;

					PlayerPrefs.DeleteKey(DataSaver.SAVE_KEY);
					PlayerPrefs.Save();
				}

				EditorGUILayout.Space();
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();
			EditorGUILayout.Space();
		}
	}
}