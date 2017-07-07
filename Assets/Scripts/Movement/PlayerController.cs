using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class PlayerController : MonoBehaviour {

	Vector3 gravity = new Vector3 (0, -1, 0);

	Quaternion yaw = Quaternion.identity;
	Quaternion pitch = Quaternion.identity;

	Vector2 movement = Vector2.zero;

	public float speed = 2;
	float sensitivity = 3;

	public float fallspeed = 3;

	Vector3 respawn;

	CursorLockMode cursor = CursorLockMode.Locked;

	[HideInInspector]
	public bool freeze = false;

	Rigidbody _rigid;
	public Rigidbody rigid{
		get {
			if (_rigid == null)
				_rigid = GetComponent<Rigidbody> ();
			return _rigid;
		}
	}

	GrabAndDrag _drag;
	public GrabAndDrag drag{
		get {
			if (_drag == null)
				_drag = GetComponent<GrabAndDrag> ();
			return _drag;
		}
	}

	public Quaternion View{
		get{
			return yaw * pitch;
		}
	}


	// Use this for initialization
	void Awake () {
		rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		rigid.useGravity = false;

		//RESPAWN POSITION
		respawn = transform.position;
	}

	// Update is called once per frame
	void FixedUpdate () {
		UseInputs();

		//GRAVITY
		AddForce (gravity, 9.81f);

		if (IsGrounded () && rigid.velocity.y < -fallspeed) {
			Respawn ();
		}

		transform.position = transform.position + View * Vector3.forward * movement.x * speed * Time.fixedDeltaTime;
		transform.position = transform.position + View * Vector3.right * movement.y * speed * Time.fixedDeltaTime;
	}

	void AddForce(Vector3 dir, float force, float mass = 1){
		float  acceleration =  force / mass ;
		rigid.velocity += dir * acceleration * Mathf.Pow(Time.fixedDeltaTime, 1);
	}

	void Update () {
		UseCursor ();
	}

	void LateUpdate () {
		Camera.main.transform.rotation = View;
	}

	float maxangle = 65;
	void UseInputs(){

		//CAMERA
		RotateView();

		//MOVEMENT
		movement = Vector2.zero;

		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) {
			float dir = Input.GetKey (KeyCode.W) ? 1 : -1;
			movement.x = dir;
		}

		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A)) {
			float dir = Input.GetKey (KeyCode.D) ? 1 : -1;
			movement.y = dir;
		}

		//MOUSE
		if (drag != null)
			drag.OnInput ();

		//JUMP
		if (IsGrounded() && Input.GetButton("Jump")){

			float jumpHeight = 2.0f;
			float verticalSpeed = Mathf.Sqrt(2 * jumpHeight * gravity.magnitude);
			rigid.velocity += gravity * -1 * verticalSpeed; //Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
		}

	}

	void RotateView(){
		if (freeze)
			return;

		float horizontal = Input.GetAxis ("Mouse X") * sensitivity;
		float vertical = Input.GetAxis ("Mouse Y") * sensitivity;

		yaw = Quaternion.AngleAxis (horizontal, Vector3.up) * yaw; //HORIZONTAL
		pitch = Quaternion.AngleAxis (vertical, -Vector3.right) * pitch; //VERTICAL

		//LOCK ANGLE
		float angle = Quaternion.Angle(Quaternion.identity, pitch);
		if (angle > maxangle) {
			float value = (angle - maxangle);
			Quaternion max = Quaternion.AngleAxis (value, Vector3.right);
			Quaternion min = Quaternion.AngleAxis (-value, Vector3.right);

			pitch = Mathf.Abs (Quaternion.Angle (max, pitch) - 45) > Mathf.Abs (Quaternion.Angle (min, pitch) - 45) ? pitch * max : pitch * min;
		}
	}

	void UseCursor(){
		if (Input.GetKeyDown (KeyCode.Escape) ){
			cursor = (CursorLockMode.Locked != cursor) ? CursorLockMode.Locked : CursorLockMode.None;

			Cursor.lockState = cursor;
			Cursor.visible = (CursorLockMode.Locked != cursor);
		}
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = (CursorLockMode.Locked != cursor);
	}

	void Respawn(){
		transform.position = respawn;
	}

	bool IsGrounded (){

		Ray ray = new Ray(transform.position, gravity);

		RaycastHit[] sphereHit = Physics.SphereCastAll(ray.origin, 0.1f, ray.direction, 0.01f);

		for (int i = 0; i < sphereHit.Length; i++) {

			Transform trans = sphereHit [i].collider.transform;

			if (trans.root == transform) {
				continue;
			}

			if(sphereHit [i].distance <= 0.001f) {
				return true;
			}

			//Debug.Log (root.name);
		}

		return false;
	}


}
