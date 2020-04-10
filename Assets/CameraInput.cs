using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInput : MonoBehaviour
{
	[SerializeField]
	float maxZoomMoveSpeed;
	[SerializeField]
	float minZoomMoveSpeed;
	[SerializeField]
	float rotaionSpeed;
	[SerializeField]
	float maxZoom;
	[SerializeField]
	float minZoom;
	[SerializeField]
	float maxZoomSpeed;
	[SerializeField]
	float minZoomSpeed;

	private void Update()
	{
		GameInput.UpdateCameraInput();

		if (GameInput.CameraMove())
		{
			float t = Mathf.InverseLerp(minZoom, maxZoom, Camera.main.orthographicSize);
			float speed = Mathf.Lerp(minZoomMoveSpeed, maxZoomMoveSpeed, t);

			transform.Translate(-GameInput.cameraMoveDelta * speed * Time.deltaTime, Space.Self);
		}

		if (GameInput.CameraRotate())
		{
			transform.Rotate(0f, GameInput.cameraRotationDelta * rotaionSpeed * Time.deltaTime, 0f);
		}

		if (GameInput.CameraZoom())
		{
			float t = Mathf.InverseLerp(minZoom, maxZoom, Camera.main.orthographicSize);
			float zoomSpeed = Mathf.Lerp(minZoomSpeed, maxZoomSpeed, t);

			Camera.main.orthographicSize -= GameInput.cameraZoomDelta * zoomSpeed * Time.deltaTime;
			Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
		}
	}
}
