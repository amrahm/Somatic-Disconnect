using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {
	void  Awake(){
		//:::HitCheck
		RaycastHit hit; //Where/What the bullet hit
		currPos = transform.position; //Sets variables for tracer position
		currRot = transform.forward;// and rotation
		if(Physics.Raycast (transform.position, transform.forward, out hit, maxDist)) { //If the raycast reaches something, make it be hit
			rayCastHit(hit);
		}else{
			tracerMake(hit, currPos, currRot);
			Destroy(gameObject, 0.1f);
		}
		Destroy(gameObject, 0.5f);
	}
	void rayCastHit(RaycastHit hit){
		bulletHoleAliveTime += bulletHoleAliveTimeRandomness * Random.value; //Slightly randomize how long bulletholes exist so they don't disappear in a pattern.
		Instantiate(sparks, hit.point + (hit.normal * (floatInFrontOfWall + 0.1f)), transform.rotation);
		tracerMake(hit, currPos, currRot);
		if(hit.collider.gameObject.name == "CubeLL"){ //\\Test code
			hit.collider.gameObject.AddComponent<Rigidbody>();
			hit.collider.gameObject.GetComponent<Rigidbody>().mass = 50f;
			hit.collider.gameObject.name = "CubeHit";
		}
		//:::RigidBody Hit
		if(hit.collider.gameObject.GetComponent<Rigidbody>()){
			rigidbodyHit(hit, hit.collider.GetComponent<Rigidbody>());
		}else if(hit.collider.gameObject.transform.parent && hit.collider.GetComponentInParent<Rigidbody>()){
			rigidbodyHit(hit, hit.collider.GetComponentInParent<Rigidbody>());
		}

		//:::Ricochet
		if (Vector3.Angle ((hit.point - transform.position), hit.normal) < (90 + richochetAngle + (Random.value - .5) * 15)) {
			StartCoroutine(startRicochet(hit));
			tracerMake(hit, hit.point, (hit.point - transform.position) + 1.5f * hit.normal * Vector3.Dot(-(hit.point - transform.position), hit.normal));
		}else {
			//:::MaterialCheck
			if(!noHoles){
				if(hit.transform.tag == "Level"){ //Must later change this to vary based on material rather than tag(and add more variety)
					GameObject bullethole = Instantiate(bulletHole, hit.point + (hit.normal * floatInFrontOfWall), Quaternion.LookRotation(hit.normal));
					bullethole.transform.parent = hit.transform;
					Destroy (bullethole, bulletHoleAliveTime);
				}
			}
			Destroy(gameObject, 0.1f);
		}
	}
	void rigidbodyHit(RaycastHit hit, Rigidbody rb){
		Vector3 hitDirec = (hit.point - transform.position).normalized;
		//:RelativeVelocityCheck
		float hitforceRelVCalc = Vector3.Dot(hitDirec, rb.velocity);
		if (hitforceRelVCalc > hitforce) {
			hitforce = 1;
			hitforceRelVCalc = 0;
		}

		if(hit.collider.gameObject.name == "Launch Cube"){ //Launch Cube Test. Kills Launch Cube when hit.
			hit.collider.gameObject.SendMessageUpwards("BulletHit");
		}
		//:AddForce
		Vector3 force = (hitDirec * 4f * (1.4f * hitforce - hitforceRelVCalc));
		rb.AddForceAtPosition(force, hit.point, ForceMode.Impulse);
		object[] HitInfo = {hit, force};
		rb.gameObject.SendMessageUpwards("OnBullet", HitInfo, SendMessageOptions.DontRequireReceiver);
	}
	IEnumerator startRicochet(RaycastHit hit){
		yield return 1; 
		//Debug.Log("Ricochet Attempt " + Time.time);
		RaycastHit ricochet; //Where/What the ricochet hit
		float rDot = Vector3.Dot(-(hit.point - currPos), hit.normal); //component in normal direction - used to reflect angle
		Vector3 rDirec = (hit.point - currPos) + 1.5f * hit.normal * rDot; //Reflected angle
		currPos = hit.point; //Sets variables for tracer position
		currRot = rDirec;// and rotation
		if(Physics.Raycast(hit.point, rDirec, out ricochet, maxDist)){ //If the raycast reaches something, make it be hit
			rayCastHit(ricochet);//re-call function, allowing INFINITE RICOCHET!!!
			//ricochetCount++;
		}else{
			tracerMake(ricochet, currPos, currRot);
			Destroy(gameObject, 0.1f);
		}
	}
	void tracerMake(RaycastHit hit, Vector3 position, Vector3 direction){
		if(tracersEnabled){
			GameObject tracer = Instantiate(bullet, position, Quaternion.Euler(direction));
			if(tracer.GetComponent<LineRenderer>()){
				if(hit.collider != null){
					tracer.GetComponent<LineRenderer>().SetPosition(1, (hit.point - position));
				}else{
					tracer.GetComponent<LineRenderer>().SetPosition(1, direction * 200f);
				}
			}else{
				tracer.transform.position += transform.forward * 21.5f;
			}
			Destroy(tracer, 0.05f);
		}
	}
	public float maxDist = 1000;
	public GameObject bulletHole;
	public bool noHoles;
	public bool tracersEnabled = true;
	public GameObject bullet;
	public GameObject sparks;
	public float hitforce = 20;
	public float bulletHoleAliveTime = 30;
	public float richochetAngle = 10;
	const float bulletHoleAliveTimeRandomness = 10;
	const float floatInFrontOfWall = 0.012f;
	Vector3 currPos;
	Vector3 currRot;
//	int ricochetCount;
}