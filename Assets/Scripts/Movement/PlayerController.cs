using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class PlayerController : MonoBehaviour {

	Vector3 gravity = new Vector3 (0, -1, 0);

	Quaternion yaw = Quaternion.identity;
	Quaternion pitch = Quaternion.identity;

	Vector2 movement = Vector2.zero;

	float speed = 5;
	float sensitivity = 3;

	CursorLockMode cursor = CursorLockMode.Locked;

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
	}

	// Update is called once per frame
	void FixedUpdate () {
		UseInputs();

		//GRAVITY
		AddForce (gravity, 9.81f);

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

	void UseInputs(){

		//CAMERA
		float horizontal = Input.GetAxis ("Mouse X") * sensitivity;
		float vertical = Input.GetAxis ("Mouse Y") * sensitivity;

		yaw = Quaternion.AngleAxis (horizontal, Vector3.up) * yaw; //HORIZONTAL
		pitch = Quaternion.AngleAxis (vertical, Vector3.right) * pitch; //VERTICAL


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

		//Dragbody body = !drag.hasObject ? drag.DetectObject () : null;


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


}
