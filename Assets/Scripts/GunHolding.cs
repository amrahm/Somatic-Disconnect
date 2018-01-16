//Add Comments explaining purpose of blocks of code and unclear variables and such
using UnityEngine;
using System.Collections;
using ExtensionMethods;
public class GunHolding : MonoBehaviour {
	#region Variables
	public bool continuousPositionSet;
	public float headRot;
	public float holdSmooth;
	public float handDist;
	public CapsuleScript capsuleS;
	public GameObject head, man, hand, forearm, upperArm, ShoulderRr, cameraObject, aimPos, aimPosPre;
	public float forearmX, forearmY, forearmZ;
	public float forearmUp, forearmSide, forearmForward;
	public float forearmAimChangeUp, forearmAimChangeY, forearmAimChangeForward;
	public float reloadingX, reloadingY, reloadingZ;
	public float reloadingUp, reloadingSide, reloadingForward;
	public float armRotDiv;
	public Vector3 upperArmInitPos;
	public Transform upperArmAimPos;
	public float uArm;

	float reloadingXD, reloadingYD, reloadingZD;
	float reloadingUpD, reloadingSideD, reloadingForwardD;
	float holdHeight, holdSide, holdForward;
	Vector3 posV;

	public float armHX, armHY, armHZ;
	public float armLX, armLY, armLZ;
	public float armUX, armUY, armUZ;
	Vector3 fArmV;
	#endregion

	void Awake (){
		upperArmInitPos = new Vector3(-0.6148456f , 0f, 0f);
		StartCoroutine(SwitchSet());
	}
	void OnEnable(){
		StartCoroutine(SwitchSet());
	}
	public IEnumerator SwitchSet(){
		do{
			aimPosPre.transform.localPosition = capsuleS.currentGun.GetComponent<Gun>().handPT;
			aimPosPre.transform.localRotation = capsuleS.currentGun.GetComponent<Gun>().handRT;
			yield return null;
		} while(capsuleS.currentGun.GetComponent<Gun>().holdSetting || continuousPositionSet);
		yield break;
	}
	void Update (){
//		aimPos.transform.position = aimPosPre.transform.position; //Makes foreArm follow camera
		aimPos.transform.position = Extensions.SharpInDamp(aimPos.transform.position, aimPosPre.transform.position, 2.5f); //Makes foreArm follow camera
		//vvv Makes hand follow camera
		aimPos.transform.rotation = Quaternion.Slerp(aimPos.transform.rotation, aimPosPre.transform.rotation, Quaternion.Angle(aimPos.transform.rotation, aimPosPre.transform.rotation) * Time.deltaTime / holdSmooth);
	}
}