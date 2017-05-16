using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof (Rigidbody))]
//[RequireComponent(typeof (CapsuleCollider))]

public class PlayerController : MonoBehaviour {

	public enum Person{
		FirstPerson,
		ThirdPerson
	}
	public Person person = Person.FirstPerson;

	/*public bool _activeCamera2;
	/*public activeCamera{
		get {
			return _activeCamera2;
		}
		set {
			_activeCamera2 = activeCamera;
		}
	}*/

	Rigidbody _rigid;
	public Rigidbody rigid{
		get {
			if (_rigid == null) {
				_rigid = GetComponent<Rigidbody> ();
			}
			return _rigid;
		}
			
	}
		
	Camera _cam;
	public Camera cam{
		get {
			if (_cam == null) {

				GameObject obj = new GameObject("Camera");
				obj.transform.parent = transform;
				_cam = obj.AddComponent<Camera> ();
			}
			return _cam;
		}
	}

	// Use this for initialization
	void Awake () {
		Debug.Log ("EXECUTE");
		#if UNITY_EDITOR
		rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	/*void Movement(){
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) {
			float dir = Input.GetKey (KeyCode.W) ? 1 : -1;
			transform.position = transform.position + rotation * Vector3.forward * dir * movspeed * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A)) {
			float dir = Input.GetKey (KeyCode.D) ? 1 : -1;
			transform.position = transform.position + rotation * Vector3.right * dir * movspeed * Time.deltaTime;
		}
	}*/
}
