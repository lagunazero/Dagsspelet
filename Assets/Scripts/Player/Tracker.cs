using UnityEngine;
using System.Collections;

public class Tracker : MonoBehaviour {
	
	public Environment currentEnvironment;
	private Bounds envBounds;
	
	public float moveSpeed = 0.1f;
	public Vector3 mouseLastPos;
	
	public void Start()
	{
		if(currentEnvironment != null)
		{
			currentEnvironment.gameObject.SetActive(true);
			currentEnvironment.transform.Translate(0, 1, 0);
			envBounds = currentEnvironment.transform.renderer.bounds;
		}
	}
	
	public void ChangeEnvironment(Environment destination)
	{
		if(destination != currentEnvironment && destination != null)
		{
			currentEnvironment.transform.Translate(0, -1, 0);
			currentEnvironment.gameObject.SetActive(false);
			//todo: play transition effects?
			currentEnvironment = destination;
			currentEnvironment.gameObject.SetActive(true);
			currentEnvironment.transform.Translate(0, 1, 0);
			envBounds = currentEnvironment.transform.renderer.bounds;
			camera.transform.position = Vector3.zero; //todo: set position based on from and to where you came
		}
	}
	
	public void Update()
	{
		if(currentEnvironment.canMove)
		{
			if(Input.GetMouseButtonDown(0))
			{
				mouseLastPos = Input.mousePosition;
			}
			else if(Input.GetMouseButton(0))
			{
				Vector3 move = (mouseLastPos - Input.mousePosition) * moveSpeed;
				move.z = 0;
				float xClamp = (envBounds.extents.x - camera.orthographicSize * camera.aspect);
				float yClamp = (envBounds.extents.z - camera.orthographicSize);
				
				if(transform.position.x + move.x > xClamp)
					move.x = xClamp - transform.position.x;
				else if(transform.position.x + move.x < -xClamp)
					move.x = -xClamp - transform.position.x;

				//Confusing here because of the y and z mixup
				if(transform.position.z + move.y > yClamp)
					move.y = yClamp - transform.position.z;
				else if(transform.position.z + move.y < -yClamp)
					move.y = -yClamp - transform.position.z;

				transform.Translate(move);
				mouseLastPos = Input.mousePosition;
			}
		}
	}
	
	/*
	public void OnMouseDown()
	{
		if(mouseStartPos == null)
			mouseStartPos = Input.mousePosition;
	}

	public void OnMouseUp()
	{
		mouseStartPos = null;
	}
	*/
}
