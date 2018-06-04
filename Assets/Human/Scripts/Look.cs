/*Try making it so that only the upper body turns up to a certain point, then the legs turn with. When walking, turn the whole body and the upper body in opposite directions,
So that essentially only the legs are turning, and do this till the legs are in the same direction.


Add Comments explaining purpose of blocks of code and unclear variables and such */

using UnityEngine;
using ExtensionMethods;
public class Look : MonoBehaviour {
	#region Variables
	public GameObject man, head, neck, spine, aimPos, aimPosPre;
	public CapsuleScript capsuleS;
	public float sensitivity = 1f;
	public float lookSpeed, lookFactor;
	public float upBound = -133f;
	public float downBound = 133f;

	public float currentTargetCameraAngle = 60;
	public float aimSensitivity = 1;
	public float smoothY, smoothX;
	float manV, headV, ratioZoomV;
	public float turnedY, turnedY2, turnedX, turnedX2, turnedXSpine, turnedXSpine2;
	public bool X, Y;
	#endregion

    private void Update(){
		if(capsuleS.player)
			smoothX = aimSensitivity * sensitivity * Input.GetAxis("Mouse X") * 15f;
		if(X){
			if((turnedXSpine2 < 55 || smoothX < 0) && (turnedXSpine2 > -55 || smoothX > 0) && !capsuleS.walking){
				turnedXSpine2 += smoothX;
			} else{
				turnedX2 += smoothX;
			}
		}
		turnedX2 = turnedX2 % 360;
		turnedX = Extensions.SharpInDampAngle(turnedX, turnedX2, lookSpeed * 3, lookFactor) % 360;
		turnedXSpine = Extensions.SharpInDampAngle(turnedXSpine, turnedXSpine2, lookSpeed * 3, lookFactor);
		man.transform.localEulerAngles = new Vector3(0, turnedX, 0); //FIXME This line uses so much CPU???


		if(capsuleS.walking && Mathf.Abs(turnedXSpine2) > 0.01f){ //Rotate legs back to facing forward when walking/jumping
			float turnedXSpineLate = turnedXSpine2;
			turnedXSpine2 = Extensions.SharpInDampAngle(turnedXSpine2, 0, lookSpeed/2);
			turnedX2 += (turnedXSpineLate - turnedXSpine2) % 360;
		}


		if(capsuleS.player)
			smoothY = -aimSensitivity*sensitivity*Input.GetAxis("Mouse Y") * 15f;
		if(Y)
			turnedY2 += smoothY;
		turnedY2 = Mathf.Clamp(turnedY2, upBound, downBound) % (downBound + 1);
		turnedY = Extensions.SharpInDampAngle(turnedY, turnedY2, lookSpeed * 4, lookFactor) % (downBound + 1);

		//Aim
//		GetComponent<Camera>().fieldOfView = Mathf.SmoothDamp(GetComponent<Camera>().fieldOfView, currentTargetCameraAngle, ref ratioZoomV, capsuleS.currentGun.GetComponent<Gun>().zoomSpeed);
//		aimPos.transform.Rotate(-smoothX * transform.up * 50, Space.World);
//		aimPosPre.transform.localEulerAngles = new Vector3(aimPosPre.transform.localRotation.eulerAngles.x + (Mathf.DeltaAngle(turnedX2, turnedX) + Mathf.DeltaAngle(turnedXSpine2,turnedXSpine)) / 4, 0, 0);
	}

    private void LateUpdate (){
		aimPos.transform.localPosition -= new Vector3(0, smoothX / 250, 0);
		spine.transform.Rotate(Vector3.up * turnedXSpine, Space.World);
		head.transform.localEulerAngles = new Vector3(turnedY/1.5f - capsuleS.crouchAmount*15, 0, 0);
	}
}