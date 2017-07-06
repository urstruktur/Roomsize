using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabAndDrag : MonoBehaviour {

	//public GameObject selectedObj;
	PlayerController player;

	Storage selected = null;




	public bool hasObject{
		get{
			return (selected != null);
		}

	}

	public float range = 4;
	public float radius = 3;
	public float dampRotation = 3.0f;

	void Awake(){
		player = GetComponent<PlayerController> ();
	}

	// Update is called once per frame
	void FixedUpdate () {
		DragObject ();
	}

	Vector3 AxisForward(){
		Vector3 forward = player.View * Vector3.forward;
		Vector3 upward = player.View * Vector3.up; // center;

		//INCREASE LENGTH
		Vector3 axis_forward =  Vector3.Cross (transform.up, Vector3.Cross (transform.up, forward)).normalized;
		axis_forward = axis_forward.magnitude == 0 ? Vector3.Cross (forward, transform.right).normalized : axis_forward;

		//Debug.DrawLine (Camera.main.transform.position, Camera.main.transform.position - axis_forward, Color.blue);

		return axis_forward;
	}

	public void OnInput(){
		
		Vector3 origin = Camera.main.transform.position;
		Vector3 forward = player.View * Vector3.forward;

		System.Object obj = default(System.Object);

		if (!hasObject) {
			obj = RAVsys.DetectObject<Dragbody>(new Ray (origin, forward), range);
		}

		if (Input.GetMouseButtonDown (0)) {
			if(!hasObject)
				GrabObject(obj);
			else
				ReleaseObject();
		}
	}

	void GrabObject(System.Object obj){
		if (obj == null)
			return;

		selected = new Storage ();

		if(obj.GetType() == typeof(GameObject)){
			selected.obj = (GameObject)obj;
		}

		if (obj is UnityEngine.Component) {
			selected.obj = ((UnityEngine.Component)obj).gameObject;
		}

		selected.rigid = selected.obj.GetComponent<Rigidbody> ();
	
		//SAVE MASS
		selected.mass = selected.rigid.mass;

		selected.rigid.mass = 0;
		selected.rigid.useGravity = false;


		//if(obj != isType()
		
	}

	void ReleaseObject(){
		if (!hasObject)
			return;

		selected.rigid.mass = selected.mass;
		selected.rigid.useGravity = true;

		selected.rigid.velocity = player.View * Vector3.forward/70 /Time.fixedDeltaTime;

		selected = null;

		//rigid.useGravity = true;

		//rotation = Quaternion.identity;
		//selectedObj = null;
	}

	void DragObject(){

		if (!hasObject)
			return;

		Vector3 origin = Camera.main.transform.position;
		Vector3 forward = player.View * Vector3.forward; //Camera.main.transform.forward;

		float maxRadius = radius * 1.4f;

		Vector3 axis_forward = AxisForward();
		float distance =  Mathf.Min (radius / Vector3.Project(forward, (axis_forward * radius)).magnitude , maxRadius);

		// ROTATION
		//selectedObj.transform.rotation = Quaternion.LookRotation (axis_forward) * t_rotation * rotation;

		Vector3 goal = origin + player.View * Vector3.forward * (distance);
		Vector3 way = (goal - selected.center).normalized * Vector3.Distance (selected.center, goal);



		Debug.DrawLine (origin, goal, new Color(1,0,0,1f));
		Debug.DrawLine (selected.center, goal, new Color(0,0,1,1f));
		//Debug.Log ((way / Time.fixedDeltaTime).magnitude);

		//PREVENT MOVING THROUGH WALLS
		RaycastHit hit;
		if (selected.rigid.SweepTest (way.normalized, out hit, way.magnitude)) {
			way = (goal - selected.center).normalized * hit.distance;
		}

		//MOVE OBJECT
		selected.rigid.velocity = way/ Time.fixedDeltaTime;


		//ROTATION DAMPING
		if (selected.rigid.angularVelocity.magnitude >= 0.01f) {
			float value = ( 1/ selected.rigid.angularVelocity.magnitude) * (dampRotation);//* Time.fixedDeltaTime) ;//rigid.angularVelocity / ((dampRoation + 1) * 100 * Time.fixedDeltaTime);
			selected.rigid.angularVelocity -= selected.rigid.angularVelocity * selected.rigid.angularVelocity.magnitude * value * Time.fixedDeltaTime;
		} else {
			selected.rigid.angularVelocity = Vector3.zero;
		}
	}

	#if UNITY_EDITOR
	void OnDrawGizmos(){
		Vector3 origin = Camera.main.transform.position;

		UnityEditor.Handles.DrawWireDisc (origin, Vector3.up, radius);

		/*if(selectedObj != null && boundary != null){
			boundary.DrawBounds();
		}

		DragObject ();*/
	}
	#endif

	class Storage{
		public GameObject obj;
		public Rigidbody rigid;
		public float mass;

		bool init = true;

		Dragbody body;
		public Vector3 center {
			get {
				if (init) {
					body = obj.GetComponent<Dragbody> ();
					init = false;
				}

				if (body) {
					return body.center;
				} 

				return obj.transform.position;
			}
		}
	}
}
