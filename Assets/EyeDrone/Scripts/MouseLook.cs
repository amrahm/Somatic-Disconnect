using System.Collections;
using ExtensionMethods;
using UnityEngine;

public class MouseLook : MonoBehaviour {
	#region Variables
	public GameObject target, thisCamera, powerSlider, eye, eyeBall, jet, jetBall;
	public LayerMask aimRayLayerMask;
	public ParticleSystem jetParticles;
	public GameObject cannon, barrel, laser;
	public GameObject mainCamera, player;
	public Material eyeShot;
	public MouseLookTestHelper script;
	public MouseLookTestHelper script2;
	public bool glidey;
	public float glideySpeed = 15;
	public bool lowHover;
	public float maxWalkSpeed = 20f;
	public float kick = 50;
	public float sprintSpeed = 2.5f;
	public float timeToFullSprint = 0.4f;
	public float horizSprintRatio = 1f;
	public float acceleration = 30f;
	public float accelerationHoriz = 30f;
	public float deceleration = 1;
	public float turnSpeed = 50;
	public float hoverHeight = 2;
	public float maxHoverHeight = 30;
	public float hoverForce = 65f;
	public float maxVerticalSpeed = 24;
	public bool clamped = true;
	public 	bool gravity = true;
	public float mass = 0.2f;
	public float maxPush = 750;
	public float minPush = 100;
	public float aimSpeed = 1;
	public float t = 2;
	public float shotSize = 0.2957171f;
	public bool isControlled;
    private bool blind;
	[HideInInspector]
	public float vertInput, horizInput, turnInput;
//	[HideInInspector]
	public float cannonAngle;
    private bool moving;
    private float mouseSensitivity;
	public float push;
    private bool shouldShoot;
	public float pGain = 2600; // the proportional gain
	public float iGain = 40; // the integral gain
	public float dGain = 260; // differential gain
    private float integrator = 19.6f; // error accumulator
    private float lastError;
    private float curPos = 2; // actual Pos
    private float force = 785; // current force
    private Rigidbody rb;
    private float sprintVol1;
    private float sprint = 1f;
    private float velForward, velRight;
    private Vector3 fwdVec, rightVec;
    private Vector3 tangentVec, tangentRightVec;
    private Coroutine aim;
	#endregion

    private IEnumerator Aim(){
		Ray ray = new Ray(laser.transform.position, laser.transform.forward);
		RaycastHit hit;
		float angle = 0;
		if(Physics.Raycast(ray, out hit, 75, aimRayLayerMask)){
			angle = hit.distance * 1000 / push;
//			Debug.Log(hit.collider.gameObject.name);
		}
//		Debug.Log(angle);
		while(Mathf.Abs(cannonAngle - angle) > 0.002f){
			cannonAngle = Extensions.SharpInDamp(cannonAngle, angle, aimSpeed);
			yield return new WaitForFixedUpdate();
		}
	}

    private void Awake(){
		mouseSensitivity = mainCamera.GetComponent<Look>().sensitivity;
		push = (maxPush - minPush) / 2;
		rb = GetComponent<Rigidbody>();
	}

    private void Walk(){
		velForward = Vector3.Dot(rb.velocity, transform.forward); //Current fwd speed. Used to see if Kick is necessary.
		velRight = Vector3.Dot(rb.velocity, transform.right);//Similar to ^

		fwdVec = (transform.forward * (acceleration) * vertInput * Time.deltaTime) * sprint;
		rightVec = (transform.right * (accelerationHoriz) * horizInput * Time.deltaTime) * sprint;
		//			Debug.DrawRay(tf.position, fwdVec.normalized, Color.blue);
		//			Debug.DrawRay(tf.position, rightVec.normalized, Color.cyan);
		if(Mathf.Abs(vertInput) > 0 && Mathf.Abs(horizInput) > 0){
			if(Mathf.Abs(velForward) < maxWalkSpeed * sprint / 1.5f){
				rb.velocity += fwdVec;
			}
			if(Mathf.Abs(velRight) < maxWalkSpeed * sprint / 1.5f){
				rb.velocity += rightVec;
			}
		}else{
			if(Mathf.Abs(velForward) < maxWalkSpeed * sprint){
				rb.velocity += fwdVec;
				if(Mathf.Abs(horizInput) < 0.1f)
					rb.velocity -= transform.right * velRight * deceleration / 5;
			}
			if(Mathf.Abs(velRight) < maxWalkSpeed * sprint){
				rb.velocity += rightVec;
				if(Mathf.Abs(vertInput) < 0.1f)
					rb.velocity -= transform.forward * velForward * deceleration / 5;
			}
		}

		//::Kick - If pressing walk from standstill, gives a kick so walking is more responsive.
		if(vertInput > 0 && velForward < maxWalkSpeed / 3){
			rb.AddRelativeForce(0, 0, kick * 30);
			//Debug.Log("kickf"); //\\\\\\\\\\\\\\\\\\\\\\\\\\
		}else if(vertInput < 0 && velForward > -maxWalkSpeed / 3){
			rb.AddRelativeForce(0, 0, -kick * 30);
			//Debug.Log("kickb"); //\\\\\\\\\\\\\\\\\\\\\\\\\\
		}
		if(horizInput > 0 && velRight < maxWalkSpeed / 3){
			rb.AddRelativeForce(kick * 30, 0, 0);
			//Debug.Log("kickr"); //\\\\\\\\\\\\\\\\\\\\\\\\\\
		}else if(horizInput < 0 && velRight > -maxWalkSpeed / 3){
			rb.AddRelativeForce(-kick * 30, 0, 0);
			//Debug.Log("kickl"); //\\\\\\\\\\\\\\\\\\\\\\\\\\
		}
	}

    private void Sprint(float sprintAxis, bool horizontalPressed){
		if(sprintAxis > 0){
			if(vertInput > 0 && sprint <= sprintSpeed && rb.velocity.magnitude > 1 && !horizontalPressed){
				sprint = Mathf.SmoothDamp(sprint, sprintSpeed, ref sprintVol1, timeToFullSprint);
			}
			else
				if((sprint <= sprintSpeed * horizSprintRatio || sprint >= sprintSpeed * horizSprintRatio + .1) && rb.velocity.magnitude > 1){
					sprint = Mathf.SmoothDamp(sprint, sprintSpeed * horizSprintRatio, ref sprintVol1, timeToFullSprint / 2);
				}
				else{
					sprint = Mathf.SmoothDamp(sprint, 1, ref sprintVol1, 0.1f);
				}
		}
		else{
			sprint = Mathf.SmoothDamp(sprint, 1, ref sprintVol1, 0.2f);
		}
	}

    private void Update(){
		eye.gameObject.transform.LookAt(target.transform);
		if(Input.GetKeyDown("o")){
			gravity = !gravity;
		}
		if(Input.GetKeyDown("p") || shouldShoot){
			shouldShoot = false;
			if(gravity){	
				MouseLookTestHelper shot = Instantiate(script, cannon.transform.position, gameObject.transform.rotation);
				shot.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
				shot.script = GetComponent<MouseLook>();
				shot.guy = target;
				shot.barrelT = barrel.transform;
				shot.maxForce = maxPush;
				shot.push = push;
				shot.size = shotSize;
				shot.t = t;
				shot.aimSpeed = aimSpeed;
				shot.gravity = gravity;
				shot.clamped = clamped;
				shot.gameObject.GetComponent<Rigidbody>().mass = mass;
//				if(aim != null) StopCoroutine(aim);
//				aim = StartCoroutine(Aim());
			} else{
				MouseLookTestHelper shot2 = Instantiate(script2, cannon.transform.position, gameObject.transform.rotation);
				shot2.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
				shot2.script = GetComponent<MouseLook>();
				shot2.barrelT = barrel.transform;
				shot2.guy = target;
				shot2.maxForce = maxPush;
				shot2.push = push;
				shot2.size = shotSize;
				shot2.t = t;
				shot2.aimSpeed = aimSpeed;
				shot2.gravity = gravity;
				shot2.clamped = clamped;
				shot2.gameObject.GetComponent<Rigidbody>().mass = mass;
			}	
		}
		if(Input.GetKeyDown("y")){
			mainCamera.GetComponent<Camera>().enabled = !mainCamera.GetComponent<Camera>().enabled;
			mainCamera.GetComponent<AudioListener>().enabled = !mainCamera.GetComponent<AudioListener>().enabled;
			mainCamera.GetComponent<FlareLayer>().enabled = !mainCamera.GetComponent<FlareLayer>().enabled;
			thisCamera.SetActive(!thisCamera.activeSelf);
			player.GetComponent<CapsuleScript>().player = !player.GetComponent<CapsuleScript>().player;
			isControlled = !isControlled;
		}
		if(isControlled){
			Sprint(Input.GetAxis("Sprint"), Input.GetButton("Horizontal"));
			if(!glidey)	Walk();
			moving = Mathf.Abs(vertInput) > 0.01f || Mathf.Abs(horizInput) > 0.01f;
			if(Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.01f){
				push += Input.GetAxis("Mouse ScrollWheel") * 400;
				push = Mathf.Clamp(push, minPush, maxPush);
				float pushFrac = (push + minPush) / maxPush - minPush / maxPush;
				powerSlider.transform.localScale = new Vector3(1, 1, pushFrac);
				powerSlider.transform.localPosition = new Vector3(-0.27f, 0.001f, (1 - pushFrac) * 5);
			}
			vertInput = Input.GetAxis("Vertical");
			horizInput = Input.GetAxis("Horizontal");
			turnInput = Input.GetAxis("Lean");

			if(Input.GetKeyDown("u")){
				if(lowHover){
					hoverHeight = transform.position.y;
				} else{
					Ray ray = new Ray(transform.position, -transform.up);
					RaycastHit hit;
					if(Physics.Raycast(ray, out hit, maxHoverHeight)){
						hoverHeight = hit.distance;
					}
				}
				lowHover = !lowHover;
			}
			if(Input.GetButton("Jump")){
				var ps = jetParticles.velocityOverLifetime;
				ps.z = 6;
			} else if(Input.GetButton("Crouch")){
				var ps = jetParticles.velocityOverLifetime;
				ps.z = -6;
			} else if(Input.GetButtonUp("Jump") || Input.GetButtonUp("Crouch")){
				var ps = jetParticles.velocityOverLifetime;
				ps.z = 0;
			}
			hoverHeight += (Input.GetAxis("Jump") - Input.GetAxis("Crouch")) * Mathf.Pow(Mathf.Max(Mathf.Min(hoverHeight, 20), 1), 0.6f) / (40);
			if(lowHover){
				hoverHeight = Mathf.Clamp(hoverHeight, 0.3f, maxHoverHeight);
			}

			if(Input.GetButtonDown("Fire")){
				shouldShoot = true;
			}
		}
	}

    private void FixedUpdate(){
		Quaternion jBR = Quaternion.FromToRotation(-transform.up, Vector3.down);
		float downDot = Vector3.Dot(transform.up, Vector3.down);
		float rightDot = Vector3.Dot(transform.right, Vector3.down);
		if(downDot > -0.3f && downDot < 0.3f){
			jBR = Quaternion.Euler(-transform.up) * Quaternion.AngleAxis(-50 * rightDot / Mathf.Abs(rightDot), transform.forward);

		}else if(downDot > 0.3f){
			jBR = Quaternion.Euler(Vector3.right) * Quaternion.AngleAxis(30, transform.right);
		}
		jet.transform.rotation = jBR;
		jetBall.transform.rotation = Quaternion.Slerp(jetBall.transform.rotation, jBR, Quaternion.Angle(jetBall.transform.rotation, jBR) * Time.fixedDeltaTime / 10);


		Vector3 jetDir = -jet.transform.up;
		Vector3 posVec = transform.position - jet.transform.position;
		rb.AddTorque(-Vector3.Cross(jetDir, posVec) * hoverForce * 3);
		if(Vector3.Dot(jet.transform.up, Vector3.down) < .5){
			if(lowHover){
				Ray ray = new Ray(transform.position, -transform.up);
				RaycastHit hit;
				if(Physics.Raycast(ray, out hit, hoverHeight)){
					float proportionalHeight = (hoverHeight - hit.distance) / hoverHeight;
					Vector3 appliedHoverForce = jet.transform.up * proportionalHeight * hoverForce;
					rb.AddForce(appliedHoverForce, ForceMode.Acceleration);
				} else{
					rb.AddForce(hoverForce * 20 * Vector3.down);
				}
			} else{
				curPos = transform.position.y;
				float error = hoverHeight - curPos; // generate the error signal
				integrator += error * Time.fixedDeltaTime; // integrate error
				float diff = (error - lastError) / Time.fixedDeltaTime; // differentiate error
				lastError = error;
				force = error * pGain + integrator * iGain + diff * dGain; // calculate the force summing the 3 errors with respective gains:
				if(Mathf.Abs(Vector3.Dot(rb.velocity, transform.up)) < maxVerticalSpeed){
					rb.AddForce(force * jet.transform.up); // apply the force to accelerate the rigidbody:
				}
			}
		}

		if(glidey) rb.AddRelativeForce(horizInput * glideySpeed, 0f, vertInput * glideySpeed, ForceMode.Acceleration);
		rb.AddRelativeTorque(0f, turnInput * turnSpeed * mouseSensitivity, 0f, ForceMode.Acceleration);
	}

    private void LateUpdate(){
		if(isControlled){
			jetBall.transform.Rotate(transform.right * vertInput * 6, Space.World);
			jetBall.transform.Rotate(transform.forward * -horizInput * 6, Space.World);
		}
	}
	public void OnBullet(object[] hitInfo){
		RaycastHit hit = (RaycastHit)hitInfo[0];
		if(hit.collider.gameObject == eyeBall){
			eyeBall.GetComponent<Renderer>().material = eyeShot;
			blind = true;
		}
	}
}