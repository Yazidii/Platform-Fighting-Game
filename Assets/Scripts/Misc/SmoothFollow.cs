// Smooth Follow from Standard Assets
// Converted to C# because I fucking hate UnityScript and it's inexistant C# interoperability
// If you have C# code and you want to edit SmoothFollow's vars ingame, use this instead.
using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour {
	// The target we are following
	public Transform target;
	// The distance in the x-z plane to the target
	public float distance = -30.0f;
	// How much we
	public float xDamping = 6f;
	public float yDamping = 5.0f;
	// When the camera stops
	public float deadZone = 4f;
	public float yDeadZone = 4f;
	
	// Place the script in the Camera-Control group in the component menu
	[AddComponentMenu("Camera-Control/Smooth Follow")]
	
	void LateUpdate () {
		// Early out if we don't have a target
		if (!target) return;
		
		// Calculate the current rotation angles
		float wantedXPosition = target.position.x;
		float wantedYPosition = target.position.y;
		
		float currentXPosition = transform.position.x;
		float currentYPosition = transform.position.y;

		if (currentXPosition < (wantedXPosition - deadZone)) {
						wantedXPosition = wantedXPosition - deadZone;
						currentXPosition =  wantedXPosition;
				}
		else 
			if (currentXPosition > (wantedXPosition + deadZone)){
						wantedXPosition = wantedXPosition + deadZone;
						currentXPosition = wantedXPosition;
				}
		
		if (currentYPosition < (wantedYPosition - yDeadZone)) {
			wantedYPosition = wantedYPosition - yDeadZone;
			
			// Damp the Y movement
			currentYPosition = wantedYPosition;
		}
		else 
		if (currentYPosition > (wantedYPosition + yDeadZone)){
			wantedYPosition = wantedYPosition + yDeadZone;
			
			// Damp the Y movement
			currentYPosition = wantedYPosition;
		}

						


		transform.position = new Vector3 (currentXPosition, currentYPosition, distance);

	}
} 