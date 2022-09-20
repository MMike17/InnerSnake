using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

class CustomPlacer : EditorWindow
{
	[MenuItem("InnerSnake/CustomPlacer")]
	static void ShowWindow()
	{
		var window = GetWindow<CustomPlacer>();
		window.titleContent = new GUIContent("CustomPlacer");
		window.Show();
	}

	enum Placement
	{
		Middle,
		Rotate_Around
	};

	GUIStyle titleStyle;
	Placement placement;
	Vector2 scroll;

	// Middle
	Transform first;
	Transform second;
	Transform placed;

	// Rotate around
	Transform pivotPoint;
	Transform pointsHolder;
	List<Transform> points;
	int faceCount;

	void OnGUI()
	{
		GenerateIfNeeded();

		EditorGUILayout.LabelField("Custom placer", titleStyle);
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		placement = (Placement)EditorGUILayout.EnumPopup("Placement type", placement);
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		scroll = EditorGUILayout.BeginScrollView(scroll);
		switch (placement)
		{
			case Placement.Middle:
				DisplayMiddle();
				break;

			case Placement.Rotate_Around:
				DisplayRotateAround();
				break;
		}
		EditorGUILayout.EndScrollView();

		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		if (IsInputValid())
		{
			if (GUILayout.Button("Apply"))
			{
				switch (placement)
				{
					case Placement.Middle:
						PlaceMiddle();
						break;

					case Placement.Rotate_Around:
						PlaceRotateAround();
						break;
				}
			}
		}
	}

	void GenerateIfNeeded()
	{
		if (titleStyle == null)
			titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperCenter, fontStyle = FontStyle.Bold };

		if (points == null)
			points = new List<Transform>();
	}

	void DisplayMiddle()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("First object");
		first = (Transform)EditorGUILayout.ObjectField(first, typeof(Transform), true);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Second object");
		second = (Transform)EditorGUILayout.ObjectField(second, typeof(Transform), true);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Placed object");
		placed = (Transform)EditorGUILayout.ObjectField(placed, typeof(Transform), true);
		EditorGUILayout.EndHorizontal();
	}

	void DisplayRotateAround()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Pivot point");
		pivotPoint = (Transform)EditorGUILayout.ObjectField(pivotPoint, typeof(Transform), true);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();

		pointsHolder = (Transform)EditorGUILayout.ObjectField(pointsHolder, typeof(Transform), true);

		if (GUILayout.Button("Extract"))
		{
			points.Clear();

			foreach (Transform child in pointsHolder)
				points.Add(child);
		}

		EditorGUILayout.EndHorizontal();

		int newCount = Mathf.Max(0, EditorGUILayout.IntField("size", points.Count));
		EditorGUILayout.Space();

		while (newCount < points.Count)
			points.RemoveAt(points.Count - 1);

		while (newCount > points.Count)
			points.Add(null);

		for (int i = 0; i < points.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			points[i] = (Transform)EditorGUILayout.ObjectField(points[i], typeof(Transform), true);

			if (GUILayout.Button("Remove"))
				points.RemoveAt(i);
			EditorGUILayout.EndHorizontal();
		}

		if (GUILayout.Button("Add"))
			points.Add(null);

		EditorGUILayout.Space();

		faceCount = EditorGUILayout.IntField("Face count", faceCount);
	}

	bool IsInputValid()
	{
		switch (placement)
		{
			case Placement.Middle:
				return first != null && second != null && placed != null;

			case Placement.Rotate_Around:
				return pivotPoint != null && points != null && points.Count > 0 && points.Find(item => item == null) == null;
		}

		return false;
	}

	void PlaceMiddle()
	{
		placed.transform.position = Vector3.Lerp(first.position, second.position, 0.5f);

		EditorUtility.SetDirty(first.parent.parent);
	}

	void PlaceRotateAround()
	{
		float angle = 360 / faceCount;

		for (int loop = 1; loop < faceCount; loop++)
		{
			Transform[] newPoints = new Transform[points.Count];

			for (int i = 0; i < newPoints.Length; i++)
				newPoints[i] = Instantiate(points[i], points[i].parent);

			foreach (Transform point in newPoints)
			{
				point.RotateAround(pivotPoint.position, Vector3.up, angle * loop);
				point.rotation = Quaternion.identity;
			}
		}

		EditorUtility.SetDirty(pivotPoint.parent);
	}
}