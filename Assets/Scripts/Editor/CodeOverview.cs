using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>Code analyser displaying classes that are overloaded with code</summary>
class CodeOverview : EditorWindow
{
	const string CLASS_FLAG = "class ";
	const string COMMENT_FLAG = "//";
	const string EDITOR_FLAG = "using UnityEditor;";
	const string MONOBEHAVIOUR_FLAG = "MonoBehaviour";

	GUIStyle boldCenteredTitleStyle;
	GUIStyle buttonStyle;
	Vector2 scrollPos;
	Vector2 excludeScroll;
	Vector2 mediumScroll;
	Vector2 badScroll;
	Color goodColor;
	Color mediumColor;
	Color badColor;

	int scriptsCount = 0;
	int classCount = 0;
	int editorScriptsCount = 0;
	int monoBehavioursCount = 0;
	int nonMonoBehavioursCount = 0;
	int lineCount = 0;
	int selectedScriptIndex = 0;
	float averageLineCount = 0;

	int goodThreshold = 150;
	int mediumThreshold = 300;

	List<Script> mediumScripts;
	List<Script> badScripts;
	List<TextAsset> allScripts;
	ExcludedList excludedScripts;

	static CodeOverview window;

	[MenuItem("Tools/CodeOverview")]
	static void ShowWindow()
	{
		window = GetWindow<CodeOverview>();
		window.titleContent = new GUIContent("CodeOverview");
		window.Show();
	}

	void OnGUI()
	{
		GetValues();
		GenerateRequesites();

		EditorGUILayout.LabelField("Code Overview", boldCenteredTitleStyle);

		EditorGUILayout.Space();

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar);

		GUIStyle mainFrame = new GUIStyle(GUI.skin.textArea) { stretchWidth = true };
		EditorGUILayout.BeginVertical(mainFrame);

		EditorGUILayout.LabelField("Scripts count : " + scriptsCount);

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Total classes count : " + classCount);
		EditorGUILayout.LabelField("Editor classes count : " + editorScriptsCount);
		EditorGUILayout.LabelField("MonoBehaviours count : " + monoBehavioursCount);
		EditorGUILayout.LabelField("Non-MonoBehaviours count : " + nonMonoBehavioursCount);

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Total lines count : " + lineCount);

		string averageLineString = averageLineCount.ToString();

		if (averageLineString.Contains(","))
		{
			if (averageLineString[averageLineString.Length - 1] != '0')
				averageLineString = Mathf.FloorToInt(averageLineCount).ToString();
			else
			{
				int totalCount = 0;
				string[] fragments = averageLineCount.ToString().Split(',');
				totalCount = fragments[0].Length + 2;
				averageLineString = averageLineCount.ToString().Substring(0, totalCount);
			}
		}
		else
			averageLineString = averageLineCount.ToString();

		Color averageColor = badColor;

		if (averageLineCount <= goodThreshold)
			averageColor = goodColor;
		else if (averageLineCount <= mediumThreshold)
			averageColor = mediumColor;

		averageColor -= Color.grey / 2;

		EditorGUILayout.LabelField("Average line count: <color=#" + ColorUtility.ToHtmlStringRGB(averageColor) + ">" + averageLineString + "</color>", new GUIStyle(GUI.skin.label) { richText = true });

		EditorGUILayout.EndVertical();
		EditorGUILayout.EndScrollView();

		EditorGUILayout.Space();

		EditorGUILayout.BeginVertical(mainFrame);

		goodThreshold = EditorGUILayout.IntField("Good threshold", goodThreshold);
		mediumThreshold = EditorGUILayout.IntField("Medium threshold", mediumThreshold);

		PlayerPrefs.SetInt("goodThreshold", goodThreshold);
		PlayerPrefs.SetInt("mediumThreshold", mediumThreshold);

		EditorGUILayout.Space();

		excludeScroll = EditorGUILayout.BeginScrollView(excludeScroll);

		List<string> toRemove = new List<string>();

		foreach (string script in excludedScripts.scriptsNames)
		{
			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField(script);

			if (GUILayout.Button("Remove"))
				toRemove.Add(script);

			EditorGUILayout.EndHorizontal();
		}

		toRemove.ForEach(script => excludedScripts.scriptsNames.Remove(script));

		EditorGUILayout.EndScrollView();

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();

		List<TextAsset> nonExcluded = new List<TextAsset>(allScripts);

		foreach (string scriptName in excludedScripts.scriptsNames)
			nonExcluded.Remove(nonExcluded.Find(script => script.name == scriptName));

		List<string> fileNames = new List<string>();
		nonExcluded.ForEach(script => fileNames.Add(script.name));

		selectedScriptIndex = EditorGUILayout.Popup(selectedScriptIndex, fileNames.ToArray());

		if (GUILayout.Button("Add exception"))
		{
			excludedScripts.scriptsNames.Add(nonExcluded[selectedScriptIndex].name);
			selectedScriptIndex = 0;

			PlayerPrefs.SetString("excludedScripts", JsonUtility.ToJson(excludedScripts));
		}

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		EditorGUILayout.Space();

		EditorGUILayout.BeginVertical(mainFrame);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.BeginVertical();
		mediumScroll = EditorGUILayout.BeginScrollView(mediumScroll, GUIStyle.none, GUI.skin.verticalScrollbar);

		ShowScripts(mediumScripts, mediumColor);

		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical();
		badScroll = EditorGUILayout.BeginScrollView(badScroll, GUIStyle.none, GUI.skin.verticalScrollbar);

		ShowScripts(badScripts, badColor);

		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		EditorGUILayout.Space();

		if (GUILayout.Button("Refresh"))
		{
			LoadAllProjectScripts();
			ScanProjectScripts();
		}
	}

	void GenerateRequesites()
	{
		boldCenteredTitleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
		buttonStyle = new GUIStyle(GUI.skin.box) { stretchWidth = true, alignment = TextAnchor.MiddleCenter, richText = true };

		goodColor = Color.green;
		mediumColor = new Color(0.75f, 0.5f, 0);
		badColor = Color.red;

		if (window == null)
			window = GetWindow<CodeOverview>();

		if (mediumScripts == null)
			mediumScripts = new List<Script>();

		if (badScripts == null)
			badScripts = new List<Script>();

		if (allScripts == null)
			allScripts = new List<TextAsset>();

		if (excludedScripts == null)
			excludedScripts = new ExcludedList();
	}

	void GetValues()
	{
		goodThreshold = PlayerPrefs.GetInt("goodThreshold", goodThreshold);
		mediumThreshold = PlayerPrefs.GetInt("mediumThreshold", mediumThreshold);

		excludedScripts = JsonUtility.FromJson<ExcludedList>(PlayerPrefs.GetString("excludedScripts"));
	}

	void ScanProjectScripts()
	{
		scriptsCount = 0;
		classCount = 0;
		editorScriptsCount = 0;
		monoBehavioursCount = 0;
		nonMonoBehavioursCount = 0;
		lineCount = 0;
		averageLineCount = 0;

		mediumScripts = new List<Script>();
		badScripts = new List<Script>();

		if (allScripts.Count == 0)
			return;

		scriptsCount = allScripts.Count;

		// scan scripts
		foreach (TextAsset script in allScripts)
		{
			// skip this script
			if (excludedScripts.scriptsNames.Contains(script.name))
				continue;

			string[] fileLines = script.text.Split('\n');
			string classType = "Non-Mono";
			int classLineCount = 0;

			foreach (string line in fileLines)
			{
				// count classes
				if (!line.Contains(COMMENT_FLAG) && line.Contains(CLASS_FLAG) && !line.Contains("\""))
				{
					string[] fragments = line.Split(new string[] { CLASS_FLAG }, StringSplitOptions.RemoveEmptyEntries);
					string className = fragments.Length > 1 ? fragments[1] : fragments[0];
					className = className.TrimStart(' ').Split(' ')[0].Replace(",", "").Replace("\r", "");

					classCount++;

					// count MonoBehaviours
					if (DoesInheritFromMonobehaviour(className))
					{
						monoBehavioursCount++;
						classType = "MonoBhvr";
					}
				}

				int maxLineLength = 10;

				if (line.Length < maxLineLength)
					maxLineLength = line.Length;

				// count lines
				if (!string.IsNullOrWhiteSpace(line) && !line.Substring(0, maxLineLength).Contains(COMMENT_FLAG))
					classLineCount++;
			}

			lineCount += classLineCount;

			// count editor script
			if (script.text.Contains(EDITOR_FLAG))
			{
				editorScriptsCount++;
				classType = "Editor";
			}

			// gets bad and medium files
			if (classLineCount >= mediumThreshold)
				badScripts.Add(new Script(script, classType, classLineCount));
			else if (classLineCount >= goodThreshold)
				mediumScripts.Add(new Script(script, classType, classLineCount));
		}

		// count non MonoBehaviours
		nonMonoBehavioursCount = classCount - (monoBehavioursCount + editorScriptsCount);

		// count average lines
		averageLineCount = (float)lineCount / scriptsCount;
	}

	bool DoesInheritFromMonobehaviour(string className)
	{
		// get script with this class name
		TextAsset selectedScript = null;

		foreach (TextAsset script in allScripts)
		{
			if (script.text.Contains(CLASS_FLAG + className))
			{
				string[] scriptClassLines = new List<string>(script.text.Split('\n')).FindAll(item => item.Contains(CLASS_FLAG) && !item.Contains(COMMENT_FLAG) && item.Contains(className)).ToArray();

				// does one of the classes of the file match the searched one
				foreach (string scriptClassLine in scriptClassLines)
				{
					string[] scriptFragments = scriptClassLine.Split(new string[] { CLASS_FLAG }, StringSplitOptions.None);

					string scriptClassName = scriptFragments.Length > 1 ? scriptFragments[1] : scriptFragments[0];
					scriptClassName = scriptClassName.TrimStart(' ').Split(' ')[0].Replace(",", "").Replace("\r", "");

					if (className == scriptClassName)
					{
						selectedScript = script;
						break;
					}
				}
			}
		}

		if (selectedScript == null)
		{
			if (className != "EditorWindow" && className != "Editor")
				Debug.LogWarning("Couldn't find script for class " + className);

			return false;
		}

		// get class it inherits from
		string classLine = new List<string>(selectedScript.text.Split('\n')).Find(item => item.Contains(CLASS_FLAG) && !item.Contains(COMMENT_FLAG) && item.Contains(className));
		string[] fragments = classLine.Split(new string[] { CLASS_FLAG }, StringSplitOptions.None);
		fragments = fragments[1].TrimStart(' ').Split(' ');
		string inheritedClass = null;

		if (fragments.Length > 2)
			inheritedClass = fragments[2].Replace(",", "").Replace("\r", "");
		else
			return false;

		if (inheritedClass == MONOBEHAVIOUR_FLAG)
			return true;
		else
			return DoesInheritFromMonobehaviour(inheritedClass);
	}

	void ShowScripts(List<Script> listToShow, Color color)
	{
		listToShow.Sort();
		foreach (Script script in listToShow)
		{
			if (GUILayout.Button(script.ToString(color), buttonStyle))
				script.OpenInIDE();
		}
	}

	void LoadAllProjectScripts()
	{
		string[] assetsPaths = AssetDatabase.GetAllAssetPaths();
		allScripts = new List<TextAsset>();

		foreach (string assetPath in assetsPaths)
		{
			if (!assetPath.Contains("Package") && !assetPath.Contains("Plugins") && assetPath.EndsWith(".cs"))
				allScripts.Add(AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath));
		}
	}

	/// <summary>Lightweight class to keep infos of classes</summary>
	struct Script : IComparable
	{
		public int lineCount;
		public string fileName, classType;
		public Action OpenInIDE;

		public Script(TextAsset script, string classType, int lineCount)
		{
			this.lineCount = lineCount;
			fileName = script.name;
			OpenInIDE = () => AssetDatabase.OpenAsset(script);

			this.classType = classType;
		}

		public string ToString(Color color)
		{
			return "[" + classType + "] <b>" + fileName + "</b> (<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + lineCount + "</color>)";
		}

		int IComparable.CompareTo(object obj)
		{
			if (obj is Script)
			{
				Script other = (Script)obj;
				return lineCount.CompareTo(other.lineCount);
			}
			else
				return 0;
		}
	}


	[Serializable]
	class ExcludedList
	{
		public List<string> scriptsNames;

		public ExcludedList() => scriptsNames = new List<string>();
	}
}