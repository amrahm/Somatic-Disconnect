//Add Comments explaining purpose of blocks of code and unclear variables and such
//Fix Ammo System

using UnityEngine;
using UnityEngine.SceneManagement;
using ExtensionMethods;
using JetBrains.Annotations;

public class CapsuleScript : MonoBehaviour {
    #region Variables

    public bool player;
    public bool gameMode;
    public float maxWalkSpeed = 15f;
    public float acceleration = 30f;
    public float accelerationHoriz = 30f;
    public float verToHorDeceleration = 0.5f;
    public float kick = 50; //Gives player acceleration a little kick to make movement more responsive
    public float kickBack = 2; //kicks rigidbodies under player backwards
    public float sprintSpeed = 2.5f;
    public float horizSprintRatio = 1f;
    public float timeToFullSprint = 0.4f;
    public float jumpHeight = 10f;
    public float jetpackFuel; //Defines how long holding spacebar will allow a higher jump
    public float airControlWalkRatio = .05f;
    public float maxWalkSlope = 75f; //The steepest slope where one can still walk
    public float maxJumpSlope = 50f; //The steepest slope where one can jump
    private float _currSlope;
    private float _jumpHold;
    public bool crouchToggle = true;
    public float crouchSpeed = 0.2f;
    public float maxCrouchWalkSpeed = 3f;
    public GameObject spine, footL, footR, handR;
    public Grounded groundCheck;
    public PlayerPhysics playerPhysics;
    public Gun currentGun, secondGun;
    public GunHolding gunHolding;
    private float _gunWeight;
    internal float velForward, velRight;
    private Vector3 _jumpVec;
    private Vector3 _fwdVec, _rightVec;
    private Vector3 _tangentVec, _tangentRightVec;
    private bool _unCrouch, _isCrouching;

    private float _targetScaleV;
    private float _maxSpeed = 20f;
    internal float crouchAmount;
    private float _lastCrouchAmount;
    internal float sprint = 1f;
    private bool _crouchBeforeJump, _isReadyToJump, _isJumping;
    private bool _shouldJump;
    private float _jetpackFuelC;
    private bool _canJump;
    public float switchCheckDist = 1;
    public LayerMask switchCheckLayerMask;
    public float throwGunUpForce = 100;
    public float throwGunForwardsForce = 300;
    public bool bulletShells = true;
    [UsedImplicitly] public string[] ammoTypes = new string[3];
    public int[] maxAmmos = {500, 3000, 1000};
    internal readonly int[] currentAmmos = {500, 3000, 1000};
    private float _eTime;
    internal bool grounded;
    internal bool walking;
    private float _vIn;
    private float _hIn;
    private bool _frictionZero;
    private float _sprintVol1;
    private Rigidbody _rb;
    private Transform _spineTf;

    #endregion

    private void Walk() {
        _fwdVec = Quaternion.FromToRotation(_spineTf.forward, _tangentVec) *
                  (_spineTf.forward * sprint * (acceleration - _gunWeight * .7f) * _vIn * Time.deltaTime) * (1 + _currSlope / 100);
        _rightVec = Quaternion.FromToRotation(_spineTf.up, _tangentRightVec) *
                    (_spineTf.up * sprint * (accelerationHoriz - _gunWeight * .7f) * _hIn * Time.deltaTime) * (1 + _currSlope / 100);
        //			Debug.DrawRay(tf.position, fwdVec.normalized, Color.blue);
        //			Debug.DrawRay(tf.position, rightVec.normalized, Color.cyan);
        if(Mathf.Abs(_vIn) > 0 && Mathf.Abs(_hIn) > 0) {
            if(Mathf.Abs(velForward) < _maxSpeed * sprint / 1.5f) {
                _rb.velocity += _fwdVec;
            }
            if(Mathf.Abs(velRight) < _maxSpeed * sprint / 1.5f) {
                _rb.velocity += _rightVec;
            }
        } else {
            if(Mathf.Abs(velForward) < _maxSpeed * sprint) {
                _rb.velocity += _fwdVec;
                if(Mathf.Abs(_vIn) > 0.1f)
                    _rb.velocity -= _spineTf.up * velRight * verToHorDeceleration / 10;
            }

            if(Mathf.Abs(velRight) < _maxSpeed * sprint) {
                _rb.velocity += _rightVec;
                if(Mathf.Abs(_hIn) > 0.1f)
                    _rb.velocity -= _spineTf.forward * velForward * verToHorDeceleration / 10;
            }
        }

        if((Mathf.Abs(_vIn) > 0 || Mathf.Abs(_hIn) > 0) && (velForward + velRight) < _maxSpeed * 2) {
            if(!_frictionZero) {
                footL.GetComponent<Collider>().material.dynamicFriction = 0;
                footL.GetComponent<Collider>().material.staticFriction = 0;
                footR.GetComponent<Collider>().material.dynamicFriction = 0;
                footR.GetComponent<Collider>().material.staticFriction = 0;
                _frictionZero = true;
            }
        } else {
            if(_frictionZero) {
                footL.GetComponent<Collider>().material.dynamicFriction = 1;
                footL.GetComponent<Collider>().material.staticFriction = 1;
                footR.GetComponent<Collider>().material.dynamicFriction = 1;
                footR.GetComponent<Collider>().material.staticFriction = 1;
                _frictionZero = false;
            }
        }

        //			Debug.Log(rb.velocity.magnitude);
        //::Kick - If pressing walk from standstill, gives a kick so walking is more responsive.
        if(_vIn > 0 && velForward < _maxSpeed / 3) {
            _rb.AddForce(_spineTf.forward * kick * 30);
            //Debug.Log("kickf"); //\\\\\\\\\\\\\\\\\\\\\\\\\\
        } else if(_vIn < 0 && velForward > -_maxSpeed / 3) {
            _rb.AddForce(_spineTf.forward * -kick * 30);
            //Debug.Log("kickb"); //\\\\\\\\\\\\\\\\\\\\\\\\\\
        }

        if(_hIn > 0 && velRight < _maxSpeed / 3) {
            _rb.AddForce(_spineTf.up * kick * 30);
            //Debug.Log("kickr"); //\\\\\\\\\\\\\\\\\\\\\\\\\\
        } else if(_hIn < 0 && velRight > -_maxSpeed / 3) {
            _rb.AddForce(_spineTf.up * -kick * 30);
            //Debug.Log("kickl"); //\\\\\\\\\\\\\\\\\\\\\\\\\\
        }
    }

    private void RigidBodyKickBack(Rigidbody rigidBody, Collision collision, ContactPoint contactPoint) {
        float massMult = Mathf.Pow(2.71828f, -9 / (rigidBody.mass * _rb.mass * collision.relativeVelocity.magnitude / 140));
        rigidBody.AddForceAtPosition(-(_fwdVec + _rightVec) * 1300 * massMult * kickBack, contactPoint.point);
    }

    private void AirControl() {
        if((velForward < _maxSpeed * sprint && _vIn > 0) || (velForward > -_maxSpeed * sprint && _vIn < 0)) {
            _rb.velocity += _spineTf.forward * airControlWalkRatio * _vIn * Time.deltaTime * 10;
        }

        if((velRight < _maxSpeed * sprint && _hIn > 0) || (velRight > -_maxSpeed * sprint && _hIn < 0)) {
            _rb.velocity += _spineTf.up * airControlWalkRatio * _hIn * Time.deltaTime * 10;
        }
    }

    private void Sprint(float sprintAxis, bool horizontalPressed) {
        if(sprintAxis > 0) {
            if(_vIn > 0) {
                _unCrouch = _isCrouching;
                //Makes guy uncrouch if isCrouching
            }

            if(_vIn > 0 && sprint <= sprintSpeed && _rb.velocity.magnitude > 1 && !horizontalPressed) {
                sprint = Mathf.SmoothDamp(sprint, sprintSpeed, ref _sprintVol1, timeToFullSprint);
            } else if((sprint <= sprintSpeed * horizSprintRatio || sprint >= sprintSpeed * horizSprintRatio + .1) && _rb.velocity.magnitude > 1) {
                sprint = Mathf.SmoothDamp(sprint, sprintSpeed * horizSprintRatio, ref _sprintVol1, timeToFullSprint / 2);
            } else {
                sprint = Mathf.SmoothDamp(sprint, 1, ref _sprintVol1, 0.1f);
            }
        } else {
            sprint = Mathf.SmoothDamp(sprint, 1, ref _sprintVol1, 0.2f);
        }
    }

    private void Jump(bool jumpPress, bool jumpPressed, bool jumpRelease) {
        if(jumpPress && _canJump) {
            _unCrouch = true;
            _crouchBeforeJump = true;
            _isJumping = true;
//				shouldJump = true;
        }

        if(_crouchBeforeJump) {
            if(!_isReadyToJump) {
                crouchAmount = Extensions.SharpInDamp(crouchAmount, _jumpHold, 2.05f);
                if(jumpPressed)
                    _jumpHold += Time.deltaTime * 7; //FIXME Needs Time.deltaTime. So do other similar things.
            }

            if(crouchAmount > .5f || jumpRelease) {
                _isReadyToJump = true;
                _jumpHold = 0;
            }

            if(_isReadyToJump) {
                crouchAmount = Extensions.SharpInDamp(crouchAmount, 0f, 2.2f);
                if(crouchAmount < 0.1f) {
                    _shouldJump = true;
                    _isReadyToJump = false;
                    _crouchBeforeJump = false;
                }
            }
        }

        if(_shouldJump) {
            _jumpVec = (jumpHeight - (_gunWeight * .07f)) * -_spineTf.right * 55 * _rb.mass;
            _rb.AddForce(_jumpVec);
            _shouldJump = false;
        } else if(_shouldJump && _isCrouching) {
            _unCrouch = true;
            _crouchBeforeJump = false;
        }
    }

    private void JumpHolding(bool jumpPressed) {
        if(_isJumping && jumpPressed && _jetpackFuelC > 0f) {
            _jetpackFuelC -= Time.deltaTime * 2.5f;
            _jumpVec = (jumpHeight - (_gunWeight * .07f)) * -_spineTf.right * _rb.mass * 280 * Time.deltaTime;
            _rb.AddForce(_jumpVec);
        } else {
            _isJumping = false;
            _jetpackFuelC = jetpackFuel;
        }
    }

    private void EndJump() {
        _jumpHold = 0;
        _shouldJump = false;
        _isReadyToJump = false;
        _crouchBeforeJump = false;
    }

    private void Crouch(bool crouchPress, bool crouchPressed) {
        //:::Crouch	- Checks if should crouch
        if((crouchPress && crouchToggle && !_isCrouching) || (crouchPressed && !crouchToggle)) {
            _isCrouching = true;
            _crouchBeforeJump = false;
        } else if(((crouchPress && _isCrouching && crouchToggle) || (!crouchToggle && !crouchPressed) || _unCrouch)) {
            _isCrouching = false;
            _unCrouch = false;
        }

        //::If should crouch, crouch; else, stand up
        if(_isCrouching) {
            if(crouchAmount < 1.29f)
                crouchAmount = Mathf.SmoothDamp(crouchAmount, 1.3f, ref _targetScaleV, crouchSpeed);
            if(grounded) //Not sure why this is needed. Maybe without it you would slow down in midair?
                _maxSpeed = maxCrouchWalkSpeed;
        } else if(!_isCrouching || _unCrouch) {
            if(crouchAmount > 0.01f)
                crouchAmount = Mathf.SmoothDamp(crouchAmount, 0, ref _targetScaleV, crouchSpeed);
            _maxSpeed = maxWalkSpeed;
        }
    }

    private void GunSwitch(bool usePress, bool usePressed, bool useRelease) {
        //:::E Held/Tapped - If held, tells Gun being held to swap with gun on floor. If tapped, steals ammo.
        if(usePress) {
            _eTime = 0;
        } else if(usePressed) {
            //While being held, increase eTime
            _eTime += Time.deltaTime;
        } else if(useRelease) {
            //On release, check if held for long enough, and call function accordingly
            if(_eTime <= .3) {
                GunSwitchCheck(false);
            } else {
                GunSwitchCheck(true);
            }
        }
    }

    private void GunSwitchCheck(bool held) {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, switchCheckDist, switchCheckLayerMask);
        int i = 0;
        Gun gun = null;
        while(i < hitColliders.Length) {
            if(hitColliders[i].CompareTag("Gun Part")) {
                gun = hitColliders[i].GetComponentInParent<Gun>();
            } else if(hitColliders[i].CompareTag("Gun")) {
                gun = hitColliders[i].GetComponent<Gun>();
            }

            if(gun != null) {
                if(held) {
                    SetGun(gun);
                    StartCoroutine(gun.ChangeGun());
                    _gunWeight = currentGun.carryWeight + secondGun.carryWeight;
                } else {
                    SetGun(gun);
                    StartCoroutine(gun.LeechAmmo());
                    gun.capsuleS = null;
                }

                break;
            }

            i++;
        }
    }

    internal void SetGun(Gun gun) {
        gun.playerTransform = transform;
        gun.capsule = gameObject;
        gun.capsuleS = GetComponent<CapsuleScript>();
        gun.rightHand = handR;
    }

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
        _spineTf = spine.transform;
        if(gameMode) { //Hides and Locks Cursor
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        _gunWeight = currentGun.carryWeight + secondGun.carryWeight;
        SetGun(currentGun);
        SetGun(secondGun);
        StartCoroutine(currentGun.SwitchToActive());
        StartCoroutine(secondGun.SwitchToInventory());
    }

    private void Update() {
        if(player) {
            _vIn = Input.GetAxis("Vertical") + playerPhysics.stepBack;
            _hIn = Input.GetAxis("Horizontal") + playerPhysics.stepLeft;

            Sprint(Input.GetAxis("Sprint"), Input.GetButton("Horizontal"));
            JumpHolding(Input.GetButton("Jump"));
            Crouch(Input.GetButtonDown("Crouch"), Input.GetButton("Crouch"));
            GunSwitch(Input.GetButtonDown("Use"), Input.GetButton("Use"), Input.GetButtonUp("Use"));

            if(grounded) {
                Walk();
                Jump(Input.GetButtonDown("Jump"), Input.GetButton("Jump"), Input.GetButtonUp("Jump"));
            } else { //If not grounded:
                AirControl();
                EndJump();
                transform.position +=
                    Vector3.up * (crouchAmount - _lastCrouchAmount) / 3.0f; //Move player up when crouching in air so that it's like pulling up legs.
            }
        } else {
            _vIn = playerPhysics.stepBack;
            _hIn = playerPhysics.stepLeft;

            Sprint(0, false);
            JumpHolding(false);
            Crouch(false, false);
            GunSwitch(false, false, false);

            if(grounded) {
                Walk();
                Jump(false, false, false);
            } else { //If not grounded:
                AirControl();
                EndJump();
                //Move player up when crouching in air so that it's like pulling up legs.
                transform.position += Vector3.up * (crouchAmount - _lastCrouchAmount) / 3.0f;
            }
        }

        walking = (Mathf.Abs(velForward) > 0.1f || Mathf.Abs(velRight) > 0.1f);
    }

    private void LateUpdate() {
        _lastCrouchAmount = crouchAmount;

        //:::Level Reset
        if(Input.GetButton("L")) {
            ReloadCurrentScene();
        }
    }

    private void FixedUpdate() {
        //:::Grounded - Checks if touching ground
        grounded = groundCheck.feetCheck;
        //Sets velocities if not grounded
        velForward = Vector3.Dot(_rb.velocity, _spineTf.forward); //Current fwd speed. Used to see if Kick is necessary.
        velRight = Vector3.Dot(_rb.velocity, _spineTf.up); //Similar to ^
    }


    //:::Current Velocity and Slope Calculator
    private void OnCollisionStay(Collision collision) {
        _canJump = false;
        grounded = false;
        foreach(ContactPoint contactPoint in collision.contacts) {
            if(contactPoint.thisCollider.gameObject != footL && contactPoint.thisCollider.gameObject != footR) continue;

            _currSlope = Vector3.Angle(-_spineTf.right, contactPoint.normal);
            if(_currSlope > maxJumpSlope || _currSlope <= 0.8f || _currSlope <= 180.8f && _currSlope >= 179.2f) {
                //if on flat ground or in air
                _tangentVec = _spineTf.forward;
                _tangentRightVec = _spineTf.up;
            } else {
                _tangentVec = Vector3.Cross(_spineTf.up, contactPoint.normal);
                _tangentRightVec = -Vector3.Cross(_spineTf.forward, contactPoint.normal);
            }

//				Debug.DrawRay(transform.position, tangentVec, Color.blue);
            if(_currSlope < maxWalkSlope) {
                grounded = true;
                //Debug.Log(collision.relativeVelocity);
                velForward = Vector3.Dot(-collision.relativeVelocity, _spineTf.forward); //Current fwd speed. Used to see if Kick is necessary.
                velRight = Vector3.Dot(-collision.relativeVelocity, _spineTf.up); //Similar to ^
            }

            _canJump |= _currSlope < maxJumpSlope;

            //::KickBack
            if(collision.collider.GetComponent<Rigidbody>()) {
                RigidBodyKickBack(collision.collider.GetComponent<Rigidbody>(), collision, contactPoint);
            } else if(collision.collider.gameObject.transform.parent && collision.collider.GetComponentInParent<Rigidbody>()) {
                RigidBodyKickBack(collision.collider.GetComponentInParent<Rigidbody>(), collision, contactPoint);
            }
        }
    }

    public void ReloadCurrentScene() {
        // get the current scene name 
        string sceneName = SceneManager.GetActiveScene().name;

        // load the same scene
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}