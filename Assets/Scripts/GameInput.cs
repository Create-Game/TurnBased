using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameInput: MonoBehaviour
{
	public static bool useState;
	static bool leftClicked;
	static bool dragging;
	static bool rotating;
	static bool wasDragging;
	static bool listenForDrag;
	static float dragThreshold = .1f;
	static Vector3 startDragMousePosition;
	static Vector3 lastFrameDragMousePosition;

	public static void UpdateCharacterInput()
	{
		leftClicked = false;
		if (dragging)
			wasDragging = true;

		if (Input.GetMouseButtonUp(1))
		{
			if (!wasDragging)
				useState = !useState;
			wasDragging = false;
		}

		if (Input.GetMouseButtonDown(0))
		{
			leftClicked = true;
		}
	}

	public static void UpdateCameraInput()
	{
		if (Input.GetMouseButtonDown(1))
		{
			startDragMousePosition = Input.mousePosition;
			lastFrameDragMousePosition = startDragMousePosition;

			listenForDrag = true;
		}
		else if (Input.GetMouseButtonUp(1))
		{
			dragging = false;
			listenForDrag = false;
		}

		if (listenForDrag)
		{
			if (!dragging)
			{
				if ((Input.mousePosition - startDragMousePosition).magnitude >= dragThreshold)
				{
					dragging = true;
				}
			}
			else
			{
				Vector3 diff = Input.mousePosition - lastFrameDragMousePosition;
				cameraMoveDelta = new Vector3(diff.x, 0f, diff.y);
				lastFrameDragMousePosition = Input.mousePosition;
			}
		}

		rotating = Input.GetMouseButton(2) && !dragging && !listenForDrag;

		if (Input.GetMouseButtonDown(2))
			lastFrameDragMousePosition = Input.mousePosition;

		if (rotating)
		{
			cameraRotationDelta = Input.mousePosition.x - lastFrameDragMousePosition.x;
			lastFrameDragMousePosition = Input.mousePosition;
		}
		else
		{
			cameraRotationDelta = 0f;
		}

		cameraZoomDelta = Input.GetAxis("cameraZoom");
	}

	public static Vector3 cameraMoveDelta;
	public static float cameraRotationDelta;
	public static float cameraZoomDelta;

	public static bool GetMove()
	{
		return !useState && leftClicked;
	}

	public static bool GetUse()
	{
		return useState && leftClicked;
	}

	public static bool CameraMove()
	{
		return dragging;
	}

	public static bool CameraRotate()
	{
		return rotating;
	}

	public static bool CameraZoom()
	{
		return Mathf.Abs(cameraZoomDelta) > Mathf.Epsilon;
	}
}
