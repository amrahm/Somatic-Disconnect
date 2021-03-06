//Add Comments explaining purpose of blocks of code and unclear variables and such
//Fix reloading switch
//Speed up reloading
//Reduce breath effect(it's annoying)
//Double place the gun on switch for increased accuracy

using System.Collections;
using ExtensionMethods;
using UnityEngine;

public class Gun : MonoBehaviour {
	#region Variables
	public Vector3 gunP;
	public Vector3 gunR;
	public Vector3 handP;
	public Vector3 handR;
	public Vector3 aimHandP;
	public Vector3 aimHandR;
	public Vector3 reloadingP;
	public Vector3 reloadingR;
	internal Vector3 handPT;
	internal Quaternion handRT;
	public float scale = 1;
	public bool  ammoContainer;
	public string ammoType;
	public int magSize = 30;
	public int currentMag = 30;
	public float reloadSpeed = 1f;
	public float fireRate = 6f;
	public float spread = 1f;
	public float recoil = 2f;
	public float recoilUp = 1f;
	public float recoilAngle = 2f;
	public float carryWeight = 1f;
	public float turnDelay = 0.1f;
	public float aimSpeed = 0.1f;
	public float aimSensitivityRatio = 1;
	public float zoomAngle = 55f;
	public float zoomSpeed = 0.2f;
	public float gunBobAmountX = .03f;
	public float gunBobAmountY = .03f;
	public float gunBobAmountXAiming = 0.01f;
	public float gunBobAmountYAiming = 0.01f;
	public float gunBobSprintAdd = 2f;
	public float startupDelay;

	public GameObject gunParts;
	public GameObject shootObject;
	public GameObject shotSound;
	public GameObject muzzle;
	public GameObject muzzleFlash;
	public GameObject muzzleLight;
	public GameObject muzzleSmoke;
	public AudioSource reloadSound;
	public Animation animationLayer;
	public string recoilAnimationName;
	public GameObject bulletObject;
	public GameObject shellShot;
	public GameObject shell;
	public float shellAngle;
	public float shellForce;
	public float shellLife = 7;
	public bool  beingHeld;
	public bool  unequipped;
	public GameObject[] firstEight= new GameObject[0];
	public GameObject[] secondEight= new GameObject[0];
	public GameObject[] thirdEight= new GameObject[0];
	public Material gunMetal;
	public Material shinyNumbers;

	internal GameObject rightHand;
	public bool  reloading;
    private int countToThrow = -1;
	internal Transform playerTransform;
	internal CapsuleScript capsuleS;
    private float waitTilNextFire;
    private const float defaultCameraAngle = 60f;
    private float gunMass;
	public bool holdSetting;
	
	internal GameObject capsule; 
	public int ammtype;
    private readonly int[][] numbers = new int[10][];
    private int one;
    private int two;
    private int three;
    private float startupDelayd;
    private Vector3 posV;
	public bool  aiming;
    private bool  aimingToggle;
    private bool scrolled;
    private float scrollTimer;
    private float reloadTime = 1.9f;
	#endregion
	#region Methods

    private Coroutine _switchSet;
    private Coroutine _aimSet;

    private IEnumerator AimSet(Vector3 position, Quaternion rotation){
		holdSetting = true;
		while(Vector3.Distance(handPT, position) > 0.02f || Quaternion.Angle(handRT, rotation) > 0.1f){
			handPT = Extensions.SharpInDamp(handPT, position, aimSpeed);
			handRT = Extensions.SharpInDamp(handRT, rotation, aimSpeed);
			yield return null;
		}
		holdSetting = false;
    }

    private void NumberChange(){
		one = int.Parse(currentMag.ToString().Substring(0,1));
		if(secondEight.Length == 7){
			if(currentMag >= 10)
				two = int.Parse(currentMag.ToString().Substring(1,1));
		}
		if(thirdEight.Length == 7){
			if(currentMag >= 100)
				three = int.Parse((currentMag%10).ToString().Substring(0,1));
		}
		for(int i=0; i<7; i++){
			for(int e=0; e<7; e++){
				if(currentMag >= 100){
					firstEight[e].GetComponent<Renderer>().material = numbers[one][e] == 1 ? shinyNumbers : gunMetal;
					secondEight[e].GetComponent<Renderer>().material = numbers[two][e] == 1 ? shinyNumbers : gunMetal;
					thirdEight[e].GetComponent<Renderer>().material = numbers[three][e] == 1 ? shinyNumbers : gunMetal;

				}else if(currentMag >= 10){
					if(thirdEight.Length == 7){
						firstEight[e].GetComponent<Renderer>().material = gunMetal;		
						secondEight[e].GetComponent<Renderer>().material = numbers[one][e] == 1 ? shinyNumbers : gunMetal;
						thirdEight[e].GetComponent<Renderer>().material = numbers[two][e] == 1 ? shinyNumbers : gunMetal;
					}else{
						firstEight[e].GetComponent<Renderer>().material = numbers[one][e] == 1 ? shinyNumbers : gunMetal;
						secondEight[e].GetComponent<Renderer>().material = numbers[two][e] == 1 ? shinyNumbers : gunMetal;
					}
				}else{
					if(thirdEight.Length == 7){
						firstEight[e].GetComponent<Renderer>().material = gunMetal;	
						secondEight[e].GetComponent<Renderer>().GetComponent<Renderer>().material = gunMetal;
						thirdEight[e].GetComponent<Renderer>().material = numbers[one][e] == 1 ? shinyNumbers : gunMetal;
					}else{
						if(secondEight.Length == 7){
							firstEight[e].GetComponent<Renderer>().GetComponent<Renderer>().material = gunMetal;
							secondEight[e].GetComponent<Renderer>().material = numbers[one][e] == 1 ? shinyNumbers : gunMetal;
						}else{
							firstEight[e].GetComponent<Renderer>().material = numbers[one][e] == 1 ? shinyNumbers : gunMetal;
						}
					}
				}

			}
		}

	}
	#endregion

    private void  Awake(){	
		reloadTime = 1.9f / reloadSpeed;
		gunMass = GetComponent<Rigidbody>().mass;
		numbers[0] = new [] { 1, 1, 1, 1, 1, 1, 0 };
		numbers[1] = new [] { 0, 0, 0, 1, 1, 0, 0 };
		numbers[2] = new [] { 0, 1, 1, 0, 1, 1, 1 };
		numbers[3] = new [] { 0, 0, 1, 1, 1, 1, 1 };
		numbers[4] = new [] { 1, 0, 0, 1, 1, 0, 1 };
		numbers[5] = new [] { 1, 0, 1, 1, 0, 1, 1 };
		numbers[6] = new [] { 1, 1, 1, 1, 0, 1, 1 };
		numbers[7] = new [] { 0, 0, 0, 1, 1, 1, 0 };
		numbers[8] = new [] { 1, 1, 1, 1, 1, 1, 1 };
		numbers[9] = new [] { 1, 0, 1, 1, 1, 1, 1 };
		if(currentMag > magSize)
			currentMag = magSize;
		NumberChange();
		if(!capsuleS){
			StartCoroutine(SwitchUnequip());
		}
	}

    private void Shoot(){
		if(Input.GetAxis("Fire") > 0 && !reloading && currentMag >= 1 && startupDelayd <= 0 && waitTilNextFire <= 0 && capsuleS.player){
			//When shooting
			waitTilNextFire = 1;
			if(_switchSet != null) StopCoroutine(_switchSet);
			_switchSet = StartCoroutine(capsuleS.gunHolding.SwitchSet());
			//Create bullet
			Instantiate(bulletObject, muzzle.transform.position, Quaternion.Euler(muzzle.transform.rotation.eulerAngles.x + (Random.value - 0.5f) * spread, muzzle.transform.rotation.eulerAngles.y + (Random.value - 0.5f) * spread, muzzle.transform.rotation.eulerAngles.z));
			currentMag -= 1;
			//Decrease ammo
			NumberChange();
			//Set ammo counter on gun
			//Muzzle flash, light, sound, and smoke
			GameObject muzzleFlashI = Instantiate(muzzleFlash, muzzle.transform.position, muzzle.transform.rotation);
			muzzleFlashI.transform.parent = muzzle.transform; //FIXME?
			GameObject muzzleLightI = Instantiate(muzzleLight, muzzle.transform.position, muzzle.transform.rotation);
			muzzleLightI.transform.parent = muzzle.transform;
			GameObject muzzleSmokeI = Instantiate(muzzleSmoke, muzzle.transform.position, muzzle.transform.rotation);
			GameObject shotSoundI = Instantiate(shotSound, transform.position, transform.rotation);
			shotSoundI.transform.parent = transform;
			if(shell && capsuleS.bulletShells){
				//Create shell if option on
				GameObject shellI = Instantiate(shell, shellShot.transform.position, new Quaternion(Random.Range(-30 + shellAngle, 30 + shellAngle), 0, Random.Range(0 + shellAngle, 30 + shellAngle), 0));
				shellI.GetComponent<Rigidbody>().AddRelativeForce(5 * capsule.GetComponent<Rigidbody>().velocity.x, shellForce + 4 * capsule.GetComponent<Rigidbody>().velocity.y, 5 * capsule.GetComponent<Rigidbody>().velocity.z);
				shellI.GetComponent<Rigidbody>().AddRelativeTorque(Random.Range(7000, 9000), Random.Range(10000, 50000), 0);
				Destroy(shellI, shellLife);
			}
			//Cleanup
			Destroy(muzzleFlashI, 0.05f);
			Destroy(muzzleLightI, 0.1f);
			Destroy(muzzleSmokeI, 4f);
			Destroy(shotSoundI, 1f);
			if(!string.IsNullOrEmpty(recoilAnimationName))
				animationLayer.CrossFade(recoilAnimationName, 0.1f);
		}

		waitTilNextFire -= Time.deltaTime * fireRate;

		if(startupDelay > 0){ //Shoot Delay for e.g. minigun
			if(Input.GetAxis("Fire") > 0 && startupDelayd >= 0){
				startupDelayd -= Time.deltaTime;
			}else if(startupDelayd <= startupDelay){
				startupDelayd += Time.deltaTime;
			}
		}
	}

    private void Aim(){
		aiming = Input.GetAxis("Aim") > 0 || aimingToggle;
		if(Input.GetButtonDown("T"))
			aimingToggle = !aimingToggle;
		if((Input.GetButtonDown("Aim") || Input.GetButtonDown("T") && aimingToggle) && !reloading){
			if(_aimSet != null)
				StopCoroutine(_aimSet);
			_aimSet = StartCoroutine(AimSet(aimHandP, Quaternion.Euler(aimHandR)));
			if(_switchSet != null)
				StopCoroutine(_switchSet);
			_switchSet = StartCoroutine(capsuleS.gunHolding.SwitchSet());
		}else if(Input.GetButtonUp("Aim") || Input.GetButtonDown("T") && !aimingToggle){
			if(_aimSet != null)
				StopCoroutine(_aimSet);
			_aimSet = StartCoroutine(AimSet(handP, Quaternion.Euler(handR)));
			if(_switchSet != null)
				StopCoroutine(_switchSet);
			_switchSet = StartCoroutine(capsuleS.gunHolding.SwitchSet());
		}
	}

    private void Reload(){
		if(Input.GetButtonDown("Reload") && !reloading && currentMag < magSize && capsuleS.currentAmmos[ammtype] > 0){
			reloadSound.Play();
			reloading = true;
			//TODO Reload anim
		}
		else
			if(reloading && reloadTime < 0){
				if(capsuleS.currentAmmos[ammtype] >= magSize - currentMag){
					capsuleS.currentAmmos[ammtype] -= magSize - currentMag;
					currentMag = magSize;
				}
				else
					if(capsuleS.currentAmmos[ammtype] < magSize - currentMag){
						currentMag += capsuleS.currentAmmos[ammtype];
						capsuleS.currentAmmos[ammtype] = 0;
					}
				NumberChange();
				reloadTime = 1.9f / reloadSpeed;
				reloading = false;
			}
	}
	internal IEnumerator SwitchToActive(){
		gunParts.SetActive(true);
		Destroy(GetComponent<Rigidbody>());
		foreach(Transform t in GetComponentsInChildren<Transform>()){
			t.gameObject.layer = 13;
		}
		beingHeld = true;
		unequipped = false;
		transform.parent = rightHand.transform;
		transform.localPosition = gunP;
		transform.localRotation = Quaternion.Euler(gunR);
		if(_aimSet != null) StopCoroutine(_aimSet);
		_aimSet = StartCoroutine(AimSet(aimHandP, Quaternion.Euler(aimHandR)));
		if(_aimSet != null) StopCoroutine(_aimSet);
		_aimSet = StartCoroutine(AimSet(handP, Quaternion.Euler(handR)));
		if(_switchSet != null) StopCoroutine(_switchSet);
		_switchSet = StartCoroutine(capsuleS.gunHolding.SwitchSet());
		yield break;
	}
	internal IEnumerator SwitchToInventory(){
		reloading = false;
		beingHeld = true;
		unequipped = true;
		transform.parent = rightHand.transform;
		transform.localPosition = gunP;
		transform.localRotation = Quaternion.Euler(gunR);
		gunParts.SetActive(false);
		yield break;
	}

    private IEnumerator SwitchUnequip(){
		gunParts.SetActive(true);
		reloading = false;
		foreach(Transform t in GetComponentsInChildren<Transform>()){
			t.gameObject.layer = 11;
		}
		foreach(Collider c in GetComponentsInChildren<Collider>()){
			c.isTrigger = false;
		}
		transform.parent = null;
		if(!GetComponent<Rigidbody>())
			gameObject.AddComponent<Rigidbody>();
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<Rigidbody>().useGravity = true;
		GetComponent<Rigidbody>().mass = gunMass;
		if(capsuleS){
			GetComponent<Rigidbody>().AddRelativeForce(0, capsuleS.throwGunUpForce, capsuleS.throwGunForwardsForce);
			capsuleS = null;
		}
		beingHeld = false;
		unequipped = false;
		yield break;
	}
	internal void Unequipped(){
		transform.position += new Vector3(0, .15f, .15f);
		StartCoroutine(SwitchUnequip());
	}
	internal IEnumerator ChangeGun(){
		capsuleS.currentGun.SendMessage("Unequipped");
		capsuleS.currentGun = GetComponent<Gun>();
		capsuleS.SetGun(GetComponent<Gun>());
		StartCoroutine(SwitchToActive());
		yield break;
	}
	internal IEnumerator LeechAmmo(){
		if(capsuleS.maxAmmos[ammtype] >= capsuleS.currentAmmos[ammtype] + currentMag){
			capsuleS.currentAmmos[ammtype] += currentMag;
			currentMag = 0;
		} else{
			currentMag -= capsuleS.maxAmmos[ammtype] - capsuleS.currentAmmos[ammtype];
			capsuleS.currentAmmos[ammtype] = capsuleS.maxAmmos[ammtype];
		}
		NumberChange();
		yield break;
	}

    private void  Update(){
		if(capsuleS){
			if(beingHeld && !unequipped){ //When gun is in hand
				Shoot();
				Aim();
				Reload();
			}

			if(Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) < 0.01f){
				scrollTimer -= Time.deltaTime;
			} else if(scrollTimer < 0){
				scrolled = true;
				scrollTimer = .5f;
			}

			if((Input.GetButtonDown("Switch Weapons") || scrolled) && !ammoContainer){
				scrolled = false;
				if(beingHeld && !unequipped){
					Gun tempGun = capsuleS.secondGun.GetComponent<Gun>();
					capsuleS.secondGun = GetComponent<Gun>();
					capsuleS.currentGun = tempGun;
					StartCoroutine(SwitchToInventory());
				} else if(beingHeld && unequipped){
					capsuleS.currentGun = GetComponent<Gun>();
					StartCoroutine(SwitchToActive());
				}
			}

			//Reload Stop
			if(!reloading && !ammoContainer){
				reloadSound.Stop();
			} else if(reloading && !ammoContainer){
				reloadSound.pitch = reloadSpeed;
				reloadTime -= Time.deltaTime;
			}
		}
	}
}