using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabAndDrag : MonoBehaviour {

	public float range = 3;

	public float rotationSensitivity = 1;

	public float radius = 1;
	public float maxRadius = 2;
	public float dampRoation = 3.0f;

	public Material selectedMat, markedMat;

	public GameObject selectedObj = null;
	Boundary boundary = null;


	Dictionary<string, List<Tag>> materials = new Dictionary<string, List<Tag>>();
	struct Tag{
		public Renderer render;
		public Material mat;

		public Tag(Renderer render, Material mat){
			this.render = render;
			this.mat = mat;
		}
	}

	void FixedUpdate () {
		DragObject ();
	}

	void Update () {

		GameObject detectedObj = null;
		if (selectedObj == null) {
			detectedObj = DetectObject ();
		}
			
		if (Input.GetMouseButtonDown (0)) {

			if (selectedObj == null) {
				GrabObject (detectedObj);
			} else {
				ReleaseObject ();
			}
		}

		if (Input.GetMouseButtonDown (1) && selectedObj != null) {
			UnityStandardAssets.Characters.FirstPerson.FirstPersonController controller = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
			controller.activeRotation = false;
		}
		if (Input.GetMouseButton(1) && selectedObj != null) {
			
			float adjustX = Input.GetAxis ("Mouse X") * rotationSensitivity;
			float adjustY = Input.GetAxis ("Mouse Y") * rotationSensitivity;

			//Quaternion yaw = Quaternion.AngleAxis (adjustX, new Vector3(0,1,0)); //HORIZONTAL
			//Quaternion pitch = Quaternion.AngleAxis (adjustY, selectedObj.transform.rotation * yaw * new Vector3(1,0,0)); //VERTICAL

			Quaternion yaw = Quaternion.AngleAxis (adjustX, new Vector3(0,1,0)); //HORIZONTAL
			Quaternion pitch = Quaternion.AngleAxis (adjustY, new Vector3(1,0,0)); //VERTICAL

			// yaw *
			Quaternion rotation = pitch * selectedObj.transform.rotation;

			selectedObj.transform.rotation = rotation;
			//selectedObj.transform.position -= rotation * center;
		}
		if (Input.GetMouseButtonUp (1)) {
			UnityStandardAssets.Characters.FirstPerson.FirstPersonController controller = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
			controller.activeRotation = true;
		}

		MarkObject ("Mark", detectedObj, markedMat);
		/*if (selectedObj == null) {
			
		} else {
			MarkObject ("Selected", selectedObj, selectedMat);
		}*/
	}

	public static Vector3 DistanceToLine(Ray ray, Vector3 point){
		return Vector3.Cross(ray.direction, point - ray.origin);
	}

	GameObject DetectObject(){
		Vector3 forward = Camera.main.transform.forward;
		Vector3 origin = Camera.main.transform.position;

		RaycastHit[] sphereHit = Physics.SphereCastAll(origin, 0.3f, forward, range);
		//List<GameObject> results = new List<GameObject> ();

		float best = -1;
		GameObject result = null;

		for(int i = 0; i < sphereHit.Length; i++){

			Transform root = sphereHit [i].collider.transform;

			while (true) {
				if (root.GetComponent<Rigidbody>() != null || root.parent == null) {
					break;
				} else {
					root = root.parent;
				}
			}

			Rigidbody rigid = root.GetComponent<Rigidbody>();

			if(root == gameObject || rigid == null || rigid.isKinematic){
				continue;
			}

			Vector3 point = NearestPointOnLine(origin, forward, sphereHit [i].point);

			Vector3 ping = sphereHit [i].collider.ClosestPoint (point);
			point = NearestPointOnLine(origin, forward, ping);


			float distance = Mathf.Pow( Vector3.Distance (origin, point), 0.3f) + Mathf.Pow( Vector3.Distance (sphereHit [i].point, point),1);

			if(best == -1 || distance < best){
				result = root.gameObject;
				best = distance;
			}
				
			Debug.DrawLine (origin, ping, new Color (1, 1, 0, 0.1f));
			Debug.DrawLine (point, ping, new Color (0, 1, 0, 0.5f));

		}

		return result;
	}

	public static Vector3 NearestPointOnLine(Vector3 origin, Vector3 direction, Vector3 point)
	{
		direction.Normalize();//this needs to be a unit vector
		Vector3 v = point - origin;
		float d = Vector3.Dot(v, direction);
		return  origin + direction * d;
	}

	void GrabObject(GameObject obj){
		selectedObj = obj;

		if (selectedObj != null) {

			boundary = new Boundary (selectedObj);

			//center = CenterObject(selectedObj);

			Rigidbody rigid = selectedObj.GetComponent<Rigidbody> ();
			rigid.useGravity = false;
		}
	}

	void ReleaseObject(){
		if (selectedObj != null) {
			Rigidbody rigid = selectedObj.GetComponent<Rigidbody> ();
			rigid.velocity = Vector3.zero;
			rigid.useGravity = true;

			selectedObj = null;
		}
	}

	void DragObject(){

		if (selectedObj == null) {
			return;
		}

		Rigidbody rigid = selectedObj.GetComponent<Rigidbody> ();
		rigid.useGravity = false;

		Vector3 origin = Camera.main.transform.position;

		Vector3 forward = Camera.main.transform.forward;
		Vector3 upward = Camera.main.transform.up; // center;

		//INCREASE LENGTH
		Vector3 axis_forward =  Vector3.Cross (Vector3.up, Vector3.Cross (forward, Vector3.up)).normalized;
		axis_forward = axis_forward.magnitude == 0 ? axis_forward : Vector3.Cross (Vector3.up, Vector3.Cross (upward, Vector3.up)).normalized;
		float distance =  Mathf.Min (radius / Vector3.Project(forward, (axis_forward * radius)).magnitude , maxRadius);


		//Debug.DrawLine (origin, origin + forward * distance, new Color(1,1,0,0.6f));

		Vector3 goal = origin + forward * distance;

		//Debug.DrawLine (boundary.center, goal, new Color(0,1,0,1f));

		if (boundary == null) {
			boundary = new Boundary (selectedObj);
		}

		Debug.DrawLine (origin, boundary.Intersect(origin, forward), new Color(0,1,0,1f));

		/*if (goal.x != goal.x) {
			goal = origin + forward * maxRadius;
		}*/

		//Debug.DrawLine (origin, origin + forward, new Color(1,0,0,0.6f));
		//Debug.DrawLine (origin, origin + axis_forward * radius, new Color(0,0,1,0.6f));
		//Debug.DrawLine (origin, goal, new Color(0,1,0,1f));

		//Vector3 goal
		Vector3 center = boundary.center;
		rigid.velocity = ((goal - center).normalized * Vector3.Distance(center, goal) / Time.fixedDeltaTime)*0.1f;


		//ROTATION
		if (rigid.angularVelocity.magnitude >= 0.01f) {
			float value = ( 1/ rigid.angularVelocity.magnitude) * (dampRoation);//* Time.fixedDeltaTime) ;//rigid.angularVelocity / ((dampRoation + 1) * 100 * Time.fixedDeltaTime);
			rigid.angularVelocity -= rigid.angularVelocity * rigid.angularVelocity.magnitude * value * Time.fixedDeltaTime;
		} else {
			rigid.angularVelocity = Vector3.zero;
		}

		//Vector3 localAngularVelocity = transform.InverseTransformDirection(rigid.angularVelocity);
		//rigid.AddRelativeTorque(-localAngularVelocity * slowDownPower * Time.fixedDeltaTime);

		//rigid.AddForce ( dis * dir);
	}


	void MarkObject(string group, GameObject obj, Material mat){

		if (materials.ContainsKey (group)) {

			List<Tag> tags = materials[group];

			if (obj != null && tags[0].render.transform.root == obj.transform) {
				return;
			}
			foreach (Tag tag in tags) {
				tag.render.sharedMaterial = tag.mat;
			}
			materials.Remove (group);
		}

		if (obj != null && mat != null) {
			List<Tag> tags = new List<Tag> ();

			foreach (Transform child in obj.GetComponentsInChildren<Transform>()) {
				Renderer render = child.GetComponent<Renderer> ();
				if (render != null) {
					Tag tag = new Tag (render, render.sharedMaterial);
					tags.Add (tag);
					render.sharedMaterial = mat;
				}
			}
			materials.Add (group, tags);
		}
	}


	void OnDrawGizmos(){
		Vector3 origin = Camera.main.transform.position;

		Debug.DrawLine (origin, origin + Camera.main.transform.forward * range, new Color(1,0,0,0.1f));

		UnityEditor.Handles.DrawWireDisc (origin, Vector3.up, radius);

		if(selectedObj != null && boundary != null){
			boundary.DrawBounds();
		}

		DragObject ();
	}

	//FLUFF

	/*private void CalculateBounds(GameObject obj){

		Bounds bounds = obj.transform.GetComponent<Renderer>().bounds;

		//UnityEditor.Handles.matrix = obj.transform.localToWorldMatrix;//Matrix4x4.TRS(Vector3.zero, obj.transform.rotation, Vector3.one);
		UnityEditor.Handles.color = new Color (1, 0, 0, 0.5f);
		UnityEditor.Handles.DrawWireCube (bounds.center, bounds.size);

		Renderer[] renders = obj.GetComponentsInChildren<Renderer> ();
		foreach (Renderer render in renders){
			if (render.gameObject.activeSelf != true)
				continue;
			UnityEditor.Handles.DrawWireCube (render.bounds.center, render.bounds.size);

			bounds.Encapsulate (render.bounds.min);
			bounds.Encapsulate (render.bounds.max);
		}

		UnityEditor.Handles.color = new Color (1, 1, 1, 1f);
		UnityEditor.Handles.DrawWireCube (bounds.center, bounds.size);
	}*/

	public void rotateRigidBodyAroundPointBy(Rigidbody rb, Vector3 origin, Vector3 axis, float angle){
		Quaternion q = Quaternion.AngleAxis(angle, axis);
		rb.MovePosition(q * (rb.transform.position - origin) + origin);
		rb.MoveRotation(rb.transform.rotation * q);
	}

	public static Quaternion QuaternionFromMatrix(Matrix4x4 m) {
		return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
	}
}
