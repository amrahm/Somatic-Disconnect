//Add Comments explaining purpose of blocks of code and unclear variables and such
using UnityEngine;
using System.Collections;
using ExtensionMethods;
[RequireComponent(typeof(Animator))]

public class PlayerAnimation : MonoBehaviour {
	#region Variables
	public bool player = true;
	public bool ikActive;
    public Transform lookObj;
	public float posY, capsY, xR, zR;
	public float walkAnimMult, leanMult, crouchMult;
	public bool reloading;
	public GameObject capsule, head, aimPos;
	public Vector3 vel;
	public float turnDirection;
	protected Animator anims;
	public CapsuleScript capsuleS;
	public PlayerPhysics playerPhysics;
	public float crouchAmount;
	public float stepSpeed = 2;
	float step, stepv;
	public int forwardSpeedFloat, sideSpeedFloat, turnSpeedFloat, stepFloat, groundedBool, walkingBool, sprintFloat, reloadBool, reloadSpeedFloat, crouchAmountFloat;
	#endregion
	void Awake(){
		anims = GetComponent<Animator>();
		forwardSpeedFloat = Animator.StringToHash("forwardSpeed");
		sideSpeedFloat = Animator.StringToHash("sideSpeed");
		turnSpeedFloat = Animator.StringToHash("turnSpeed");
		stepFloat = Animator.StringToHash("step");
		groundedBool = Animator.StringToHash("grounded");
		walkingBool = Animator.StringToHash("walking");
		sprintFloat = Animator.StringToHash("sprint");
		reloadBool = Animator.StringToHash("reload");
		reloadSpeedFloat = Animator.StringToHash("reloadSpeed"); 
		crouchAmountFloat = Animator.StringToHash("crouchAmount");
	}
	void Update(){
		float vIn;
		float hIn;
		float mX;
		if(capsuleS.player){
//			vIn = Input.GetAxis("Vertical");
//			hIn = Input.GetAxis("Horizontal");
			vIn = capsuleS.velForward / 2 + playerPhysics.stepBack * 10;
			hIn = capsuleS.velRight / 2 + playerPhysics.stepLeft * 10;
			mX = Input.GetAxis("Mouse X");
		} else{
			vIn = capsuleS.velForward / 5 + playerPhysics.stepBack * 10;
			hIn = capsuleS.velRight / 5 +  playerPhysics.stepLeft * 10;
			mX = 0;
		}
		if(capsuleS.grounded){
			anims.SetBool(groundedBool, true);
			anims.SetFloat(forwardSpeedFloat, vIn); //*vel.magnitude*walkAnimMult
			anims.SetFloat(sideSpeedFloat, hIn); //*vel.magnitude*walkAnimMult
			turnDirection = Extensions.SharpInDamp(turnDirection, Mathf.Clamp(mX*12, -15, 15), .3f); // set our animator's float parameter 'Speed' equal to the speed of turning sideways				
			anims.SetFloat(turnSpeedFloat, turnDirection);
			anims.SetFloat(stepFloat, step);
		}else{
			anims.SetBool(groundedBool, false);
			anims.SetFloat(forwardSpeedFloat, 0);
			anims.SetFloat(sideSpeedFloat, 0); //*vel.magnitude*walkAnimMult
			anims.SetFloat(turnSpeedFloat, turnDirection);
			turnDirection = Extensions.SharpInDamp(turnDirection, 0, .2f); // set our animator's float parameter 'Speed' equal to the speed of turning sideways				
		}

		if(vIn > .01f){
			step += Time.deltaTime*stepSpeed*vIn;
		}else if(vIn < -.01f){
			step -= Time.deltaTime*stepSpeed*vIn;
		}else if(step > .1f){
			step = Mathf.SmoothDamp(step, 0, ref stepv, .5f);
		}
//		Debug.Log(vIn);
		if(step >= 5){
			step = 0;
		}else if(step < 0){
			step = 4.99f;
		}

		if((Mathf.Abs(vIn) > 0.2f || Mathf.Abs(hIn) > 0.2f) && capsuleS.grounded){
			anims.SetBool(walkingBool, true);
		}else{
			anims.SetBool(walkingBool, false);
		}
		if(Input.GetButton("Sprint") && capsuleS.grounded){
			anims.SetFloat(sprintFloat, capsuleS.sprint);
		}else{
			anims.SetFloat(sprintFloat, 1);
		}

		if(capsuleS.currentGun.GetComponent<Gun>().reloading){
			anims.SetBool(reloadBool, true);
			anims.SetFloat(reloadSpeedFloat, capsuleS.currentGun.GetComponent<Gun>().reloadSpeed);
		}else{
			anims.SetBool(reloadBool, false);
		}

//		xR = Mathf.SmoothDamp(xR, capsuleS.velForward*.4f*leanMult, ref xRV, .1f);
//		if(Mathf.Abs(Input.GetAxis("Mouse X")) > .5 && Mathf.Abs(hIn) < .2f){
//			zR = Mathf.SmoothDamp(zR, capsuleS.velRight*leanMult, ref zRV, .1f);
//		}else{
//			zR = Mathf.SmoothDamp(zR, -capsuleS.velRight*.4f*leanMult, ref zRV, .1f);
//		}
//		vel = capsule.transform.GetComponent<Rigidbody>().velocity;
		anims.SetLayerWeight(4, playerPhysics.crouchAmount*.5f + capsuleS.crouchAmount*.4f);
		anims.SetFloat(crouchAmountFloat, playerPhysics.crouchAmount + capsuleS.crouchAmount*5);
	}
	void OnAnimatorIK(){ //a callback for calculating IK
		if(anims){
			if(ikActive){ //if the IK is active, set the position and rotation directly to the goal.
				if(lookObj != null) { // Set the look target position, if one has been assigned
					anims.SetLookAtWeight(1);
					anims.SetLookAtPosition(lookObj.position);
				}
				if(aimPos != null){ // Set the right hand target position and rotation, if one has been assigned
					anims.SetIKPositionWeight(AvatarIKGoal.RightHand,1);
					anims.SetIKRotationWeight(AvatarIKGoal.RightHand,1);  
					anims.SetIKPosition(AvatarIKGoal.RightHand, aimPos.transform.position);
					anims.SetIKRotation(AvatarIKGoal.RightHand, aimPos.transform.rotation);
				}
			} else{ //if the IK is not active, set the position and rotation of the hand and head back to the original position         
                anims.SetIKPositionWeight(AvatarIKGoal.RightHand,0);
                anims.SetIKRotationWeight(AvatarIKGoal.RightHand,0); 
				anims.SetLookAtWeight(0);
			}
		}
	}
}