using System.Collections;
using UnityEngine;

using static GameManager;

/// <summary>Manages the camera</summary>
public class CameraManager : MonoBehaviour
{
	public static Camera MainCamera;

	[Header("Settings")]
	public float camTransitionDuration;
	public AnimationCurve transitionCurve;

	[Header("Scene references")]
	public Camera mainCamera;
	[Space]
	public Transform mainMenuTarget;
	public Transform levelSelectionTarget;

	public void Init()
	{
		MainCamera = mainCamera;
		GameManager.OnStateChanged += OnGameStateChange;
	}

	void OnDrawGizmos()
	{
		if (mainMenuTarget != null)
		{
			Gizmos.color = new Color(1, 0, 0, 0.5f);
			Gizmos.DrawSphere(mainMenuTarget.position, 0.5f);
			Gizmos.DrawLine(mainMenuTarget.position, mainMenuTarget.position + mainMenuTarget.forward * 2);
		}

		if (levelSelectionTarget != null)
		{
			Gizmos.color = new Color(0, 1, 0, 0.5f);
			Gizmos.DrawSphere(levelSelectionTarget.position, 0.5f);
			Gizmos.DrawLine(levelSelectionTarget.position, levelSelectionTarget.position + levelSelectionTarget.forward * 2);
		}
	}

	void OnGameStateChange(GameState state)
	{
		switch (state)
		{
			case GameState.Main_Menu:
				StartCoroutine(TransitionCamera(mainMenuTarget));
				break;

			case GameState.Level_Selection:
				StartCoroutine(TransitionCamera(levelSelectionTarget));
				break;

			case GameState.Game:
				StartCoroutine(TransitionCamera(Player.CameraTarget, true));
				break;
		}
	}

	IEnumerator TransitionCamera(Transform target, bool parentAtEnd = false)
	{
		mainCamera.transform.SetParent(null);

		float timer = 0;
		Vector3 initialPos = mainCamera.transform.position;
		Quaternion initialRot = mainCamera.transform.rotation;

		while (timer < camTransitionDuration)
		{
			float percent = transitionCurve.Evaluate(timer / camTransitionDuration);

			mainCamera.transform.position = Vector3.Lerp(initialPos, target.position, percent);
			mainCamera.transform.rotation = Quaternion.Lerp(initialRot, target.rotation, percent);

			timer += Time.deltaTime;
			yield return null;
		}

		mainCamera.transform.position = target.position;
		mainCamera.transform.rotation = target.rotation;

		if (parentAtEnd)
			mainCamera.transform.SetParent(target);
	}
}