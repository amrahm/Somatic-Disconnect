using UnityEngine;

public class Grounded : MonoBehaviour {
    public bool feetCheck;

    public void OnTriggerStay(Collider colliders) {
        if(colliders.gameObject.layer != 13) feetCheck = true;
    }

    public void OnTriggerExit(Collider colliders) {
        if(colliders.gameObject.layer != 13) feetCheck = false;
    }
}