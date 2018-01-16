using UnityEngine;
using System.Collections;

public class Grounded : MonoBehaviour {


	public bool  feetCheck = false;
	public void  OnTriggerStay ( Collider colliders  ){
		if(colliders.gameObject.layer != 13){
			feetCheck = true;
		}
	}
	public void  OnTriggerExit ( Collider colliders  ){
		if(colliders.gameObject.layer != 13){
			feetCheck = false;
		}
	}
}