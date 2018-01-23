using UnityEngine;
using System.Collections;
using ExtensionMethods;

public class DampTest : MonoBehaviour {
	public bool smooth;
	public bool In;
	public Transform target;
	public float speed;
	public float factor;
	Vector3 sV;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButton("T")){
			if(smooth)
				transform.position = Vector3.SmoothDamp(transform.position, target.position, ref sV, .4f);
			else if(In)
				transform.position = Extensions.SharpInDamp(transform.position, target.position, speed, factor);
			else
				transform.position = Extensions.SharpOutDamp(transform.position, target.position, .4f, Time.deltaTime);
		}
	}
}
