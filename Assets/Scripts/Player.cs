using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static GameManager;

/// <summary>Player controller</summary>
public class Player : MonoBehaviour
{
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
	[Space]
	public float pickupAnimDuration;
	public float pickupSizeMult;
	public AnimationCurve pickupAlphaCurve;
	public AnimationCurve pickupSizeCurve;

	[Header("Scene references")]
	public Transform meshRoot;
	public Animator anim;
	public Rigidbody rigid;
	public Transform cameraTarget;
	public Transform startPickup;
	public Transform pickupFX;
	public AnimateSpriteColor pickupFXColor;

	List<SnakePiece> collectedPieces;
	List<Vector3> previousPositions;
	Coroutine pickupRoutine;
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
		currentSpeed = DifficultyManager.GetCurrentDifficultySetting().GetSpeed(MapsManager.SpawnedMap.size, 0);
		blockInput = false;

		rigid.isKinematic = false;

		totalPieces = DifficultyManager.GetCurrentDifficultySetting().GetTotalPieces(MapsManager.SpawnedMap.size);
		centerPoint = MapsManager.SpawnedMap.transform.position;
	}

	public static void CleanPlayer()
	{
		CameraTarget.GetChild(0).SetParent(null);
		instance.collectedPieces.ForEach(item => Destroy(item.gameObject));
		Destroy(instance.gameObject);
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
		if (!blockInput)
		{
			// turn
			if (sideInput != 0)
				transform.RotateAround(transform.position, transform.up, turnSpeed * sideInput * Time.deltaTime);

			// move
			transform.RotateAround(centerPoint, transform.right, -currentSpeed * Time.deltaTime);
		}

		AnimateTail();
	}

	void ManageInput()
	{
		sideInput = 0;

		if (Application.platform == RuntimePlatform.Android)
		{
			if (Input.touchCount != 0)
				sideInput = Mathf.Sign(Input.GetTouch(0).position.x - Screen.width / 2);
		}
		else
		{
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
	}

	void AnimateMouth()
	{
		if (MapsManager.CurrentPickup == null)
			return;

		Vector3 target = MapsManager.CurrentPickup.piece.transform.position;

		float distance = Vector3.Distance(target, transform.position);
		float angle = Vector3.Angle(Vector3.ProjectOnPlane(target - transform.position, transform.up), transform.forward);

		float targetPercent = Mathf.InverseLerp(maxOpenMouthAngle, minOpenMouthAngle, angle);

		if (distance <= openMouthDistance)
			targetPercent = 1;

		openPercent = Mathf.MoveTowards(openPercent, targetPercent, openMouthSpeed * Time.deltaTime);

		if (openPercent > 0)
			anim.Play("Eat", 0, openPercent);
		else if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
			anim.Play("Idle");
	}

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

				currentPiece.UpdateLine(
					i == 0 ? meshRoot.position : collectedPieces[i - 1].backPoint,
					i == 0 ? 0 : collectedPieces[i - 1].targetLineWidth,
					i == 0
				);
			}
		}

		if (blockInput)
			return;

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

	void AnimateTailReduced()
	{
		if (collectedPieces.Count == 0)
			return;

		collectedPieces[0].UpdateLine(meshRoot.position, 0, true);
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.transform.parent == null || blockInput)
			return;

		SnakePiece piece = other.GetComponentInParent<SnakePiece>();

		// collided with piece
		if (piece != null)
		{
			// piece was in tail
			if (collectedPieces.Contains(piece))
			{
				GameOver();
				return;
			}

			collectedPieces.Add(piece);
			piece.GetComponentInParent<Pickup>().Collect((float)collectedPieces.Count / totalPieces);

			pickupFXColor.offset = piece.colorAnim.offset;

			if (pickupRoutine != null)
				StopCoroutine(pickupRoutine);

			pickupRoutine = StartCoroutine(PickupVFXAnim());

			piece.transform.position = previousPositions[0];
			piece.transform.LookAt(collectedPieces.Count > 1 ? collectedPieces[collectedPieces.Count - 2].transform : transform);

			if (totalPieces == collectedPieces.Count)
			{
				currentSpeed = 0;
				blockInput = true;

				GameManager.ChangeState(GameState.End_Menu);
				anim.Play("Win");
				SoundsManager.PlaySound("Win");
			}
			else
			{
				currentSpeed = DifficultyManager.GetCurrentDifficultySetting().GetSpeed(MapsManager.SpawnedMap.size, collectedPieces.Count);
				MapsManager.SpawnPickUp();
			}
		}
		else if (other.CompareTag("Finish")) // collided with line
			GameOver();
	}

	IEnumerator PickupVFXAnim()
	{
		float timer = 0;
		int posIndex = 0;

		float step = pickupAnimDuration / (collectedPieces.Count + 1);
		SpriteRenderer sprite = pickupFX.GetComponent<SpriteRenderer>();

		pickupFX.transform.rotation = Quaternion.LookRotation(-transform.forward, transform.up);
		Vector3 previousTarget = Vector3.zero;
		Vector3 currentTarget = Vector3.zero;

		while (timer < pickupAnimDuration)
		{
			timer += Time.deltaTime;
			float localPercent = (timer - (step * posIndex)) / step;
			float percent = timer / pickupAnimDuration;

			// pos
			switch (posIndex)
			{
				case 0:
					currentTarget = transform.position;
					pickupFX.position = Vector3.Lerp(startPickup.position, currentTarget, localPercent);
					break;

				case 1:
					previousTarget = collectedPieces.Count > 0 ? collectedPieces[0].transform.position : previousPositions[0];
					currentTarget = previousTarget;

					pickupFX.position = Vector3.Lerp(transform.position, previousTarget, localPercent);
					break;

				case 2:
				case 3:
					currentTarget = collectedPieces[posIndex - 1].transform.position;
					pickupFX.position = Vector3.Lerp(previousTarget, currentTarget, localPercent);
					break;
			}

			// scale
			pickupFX.localScale = Vector3.one * pickupSizeCurve.Evaluate(percent) * pickupSizeMult;

			// orientation
			pickupFX.transform.rotation = Quaternion.RotateTowards(pickupFX.transform.rotation, Quaternion.LookRotation(currentTarget - pickupFX.transform.position, transform.up), 4 * currentSpeed * Time.deltaTime);

			// alpha
			Color color = sprite.color;
			color.a = pickupAlphaCurve.Evaluate(percent);
			sprite.color = color;

			if (localPercent >= 1)
			{
				if (posIndex >= 2)
					previousTarget = collectedPieces[posIndex - 1].transform.position;

				posIndex++;
			}

			yield return null;
		}

		pickupRoutine = null;
	}

	public void GameOver()
	{
		SoundsManager.PlaySound("Lose");
		anim.Play("Die");
		GameManager.ChangeState(GameState.End_Menu);
		blockInput = true;
	}
}