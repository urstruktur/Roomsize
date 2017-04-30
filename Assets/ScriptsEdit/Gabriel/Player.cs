using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public GameObject camObject;
	float space = 1.6f;
	public Vector3 gravity = new Vector3 (0, -1, 0);


	float movspeed = 3;
	float sensitivity = 3;

	Vector3 position = Vector3.zero;
	Quaternion rotation = Quaternion.identity;


	//Vector3 forward = new Vector3 (0,0,1);
	//Vector3 upward = new Vector3 (0,1,0);

	Camera _cam;
	public Camera cam{
		get {
			if (_cam == null) {

				if (camObject != null) {
					_cam = camObject.GetComponent<Camera> ();
				} else {
					_cam = gameObject.AddComponent<Camera> ();
				}


			}
			return _cam;
		}
	}


	void Awake(){
		
	}

	// Use this for initialization
	void Start () {
		
	}
		
	CursorLockMode cursor = CursorLockMode.Locked;


	// Update is called once per frame
	void Update () {
		position = transform.position;

		CameraMovement();

		if (Input.GetMouseButtonDown (0)) {
			//Debug.Log ("Pressed left click.");
			GrabObject();
		}

		if (Input.GetMouseButtonUp (0)) {
			DropObject();
			//Debug.Log ("Released left click.");
		}

		if (Input.GetMouseButtonDown (1)) {
			Debug.Log ("Pressed right click.");
		}

		if (Input.GetMouseButtonUp (1)) {
			Debug.Log ("Released right click.");
		}

		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) {
			float dir = Input.GetKey (KeyCode.W) ? 1 : -1;
			transform.position = transform.position + rotation * Vector3.forward * dir * movspeed * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A)) {
			float dir = Input.GetKey (KeyCode.D) ? 1 : -1;
			transform.position = transform.position + rotation * Vector3.right * dir * movspeed * Time.deltaTime;
		}

		DragObject();

	}

	GameObject selection = null;

	void GrabObject (){
		//Debug.DrawLine(transform.position, transform.position + rotation * Vector3.forward);
		Vector3 dir = rotation * Vector3.forward;

		RaycastHit[] sphereHit = Physics.SphereCastAll(transform.position, 1f, dir, 100);
		List<GameObject> results = new List<GameObject> ();

		for(int i = 0; i<sphereHit.Length; i++){
			GameObject obj = sphereHit[i].collider.gameObject;
			Rigidbody rigid = obj.GetComponent<Rigidbody> ();

			if(rigid != null){
				results.Add(obj);
				//Debug.Log(sphereHit[i].collider.name);
			}
		}

		selection = null;

		foreach(GameObject obj in results){
			if (selection == null || Vector3.Distance (position, obj.transform.position) < Vector3.Distance (gameObject.transform.position, selection.transform.position)) {
				selection = obj;
			}
		}
	}

	void DragObject (){
		if (selection != null) {
			Rigidbody rigid = selection.GetComponent<Rigidbody> ();
			rigid.useGravity = false;

			Vector3 pos = selection.transform.position;
			Vector3 goal = position + rotation * Vector3.forward * space;

			Vector3 dir = (goal - pos).normalized;
			float dis = Vector3.Distance(goal, pos);

			Debug.DrawLine (position, goal);

			rigid.velocity = dir * dis * 5;

			//rigid.AddForce ( dis * dir);
		}
	}

	void DropObject (){
		if (selection != null) {
			Rigidbody rigid = selection.GetComponent<Rigidbody> ();
			rigid.velocity = Vector3.zero;
			rigid.useGravity = true;

			selection = null;
		}
	}

	void CameraMovement (){
		//Cursor
		if (Input.GetKeyDown (KeyCode.Escape) ){
			cursor = (CursorLockMode.Locked != cursor) ? CursorLockMode.Locked : CursorLockMode.None;

			Cursor.lockState = cursor;
			Cursor.visible = (CursorLockMode.Locked != cursor);
		}
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = (CursorLockMode.Locked != cursor);

		float adjustX = Input.GetAxis ("Mouse X") * sensitivity;
		float adjustY = Input.GetAxis ("Mouse Y") * sensitivity;

		Quaternion yaw = Quaternion.AngleAxis (adjustX, new Vector3(0,1,0)); //HORIZONTAL
		Quaternion pitch = Quaternion.AngleAxis (adjustY, rotation * yaw * new Vector3(1,0,0)); //VERTICAL

		//Debug.DrawLine (transform.position, transform.position +  horiz * new Vector3(1,0,0) *1.4f, Color.white);

		rotation = pitch * yaw * rotation;


		Quaternion grav = Quaternion.FromToRotation(new Vector3(0,-1,0), gravity.normalized);

	}

	void FixedUpdate(){
		cam.transform.position = transform.position;
		//transform.rotation = rotation;
		cam.transform.rotation = rotation;
		//Physics.Raycast
	}
}
