using System.Collections.Generic;
using UnityEngine;

using static GameManager;

/// <summary>Player controller</summary>
public class Player : MonoBehaviour
{
	const float crossFadeDuration = 1f / 6f;

	public static Transform Transform => instance.transform;
	public static Transform CameraTarget => instance.cameraTarget;
	public static int CollectedPieces => instance.collectedPieces.Count;
	public static float PlayerHeight => instance.height;

	static Player instance;

	[Header("Settings")]
	public KeyCode[] left;
	public KeyCode[] right;
	[Space]
	public float turnSpeed;
	public float height;
	public float pieceDistance;
	[Space]
	public float openMouthDistance;
	public float maxOpenMouthAngle;
	public float minOpenMouthAngle;
	public float openMouthSpeed;

	[Header("Scene references")]
	public Animator anim;
	public Rigidbody rigid;
	public Transform cameraTarget;

	List<SnakePiece> collectedPieces;
	List<Vector3> previousPositions;
	Vector3 centerPoint;
	float currentSpeed;
	float sideInput;
	float animOffsetPercent;
	float openPercent;
	int indexPieceDistance;
	int totalPieces;
	bool blockInput;

	public void Init()
	{
		instance = this;

		collectedPieces = new List<SnakePiece>();
		previousPositions = new List<Vector3>();
		sideInput = 0;
		blockInput = true;

		rigid.isKinematic = true;
	}

	public void StartGame()
	{
		currentSpeed = DifficultyManager.UpdateSpeed(0);
		blockInput = false;

		rigid.isKinematic = false;

		totalPieces = DifficultyManager.GetCurrentDifficultySetting().GetTotalPieces(MapsManager.SpawnedMap.size);
		centerPoint = MapsManager.SpawnedMap.transform.position;
	}

	void Update()
	{
		if (blockInput)
			return;

		ManageInput();
		AnimateMouth();
	}

	void LateUpdate()
	{
		if (blockInput)
			return;

		// turn
		if (sideInput != 0)
			transform.RotateAround(transform.position, transform.up, turnSpeed * sideInput * Time.deltaTime);

		// move
		transform.RotateAround(centerPoint, transform.right, -currentSpeed * Time.deltaTime);

		AnimateTail();
	}

	void ManageInput()
	{
		sideInput = 0;

		foreach (KeyCode key in left)
		{
			if (Input.GetKey(key))
			{
				sideInput--;
				break;
			}
		}

		foreach (KeyCode key in right)
		{
			if (Input.GetKey(key))
			{
				sideInput++;
				break;
			}
		}
	}

	void AnimateMouth()
	{
		if (MapsManager.CurrentPickup == null)
			return;

		Vector3 target = MapsManager.CurrentPickup.piece.transform.position;

		float distance = Vector3.Distance(target, transform.position);
		float angle = Vector3.Angle(Vector3.ProjectOnPlane(target - transform.position, transform.up), transform.forward);
		bool needEat = distance <= openMouthDistance;

		openPercent = Mathf.MoveTowards(openPercent, Mathf.InverseLerp(maxOpenMouthAngle, minOpenMouthAngle, angle), openMouthSpeed * Time.deltaTime);

		if (openPercent > 0)
			anim.Play("Eat", 0, openPercent);
		else if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
			anim.Play("Idle");
	}

	// TODO : Add snake pieces tail continous colliders

	void AnimateTail()
	{
		// animate pieces
		if (collectedPieces.Count != 0)
		{
			int endCount = previousPositions.Count;

			for (int i = 0; i < collectedPieces.Count; i++)
			{
				SnakePiece currentPiece = collectedPieces[i];
				int targetIndex = indexPieceDistance * (i + 1);

				currentPiece.transform.position = previousPositions[endCount - targetIndex];
				currentPiece.transform.LookAt(i == 0 ? transform : collectedPieces[i - 1].transform);
			}
		}

		previousPositions.Add(transform.position);

		// manage previous positions
		if (indexPieceDistance == 0)
		{
			if (previousPositions.Count < 1)
				return;

			if (Vector3.Distance(transform.position, previousPositions[0]) >= pieceDistance)
				indexPieceDistance = previousPositions.Count - 1;
		}
		else
		{
			int desiredCount = indexPieceDistance * (collectedPieces.Count + 1);

			while (previousPositions.Count > desiredCount)
				previousPositions.RemoveAt(0);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.transform.parent == null)
			return;

		SnakePiece piece = other.GetComponentInParent<SnakePiece>();

		if (piece != null)
		{
			if (collectedPieces.Contains(piece))
			{
				anim.Play("Die");
				GameManager.ChangeState(GameState.End_Menu);
				blockInput = true;
				return;
			}

			collectedPieces.Add(piece);
			piece.GetComponentInParent<Pickup>().Collect((float)collectedPieces.Count / totalPieces);

			piece.transform.position = previousPositions[0];
			piece.transform.LookAt(collectedPieces.Count > 1 ? collectedPieces[collectedPieces.Count - 2].transform : transform);

			if (totalPieces == collectedPieces.Count)
			{
				currentSpeed = 0;
				GameManager.ChangeState(GameState.End_Menu);
			}
			else
			{
				currentSpeed = DifficultyManager.UpdateSpeed(collectedPieces.Count);
				MapsManager.SpawnPickUp();
			}
		}
	}
}