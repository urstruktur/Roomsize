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

	GameObject selectedObj = null;

	Vector3 _center = Vector3.zero;
	Vector3 center {
		get{
			return  selectedObj.transform.position + /*selectedObj.transform.rotation **/ _center;
		}
		set{
			_center = value;
		}
	}


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

	GameObject DetectObject(){
		Vector3 forward = Camera.main.transform.forward;
		Vector3 origin = Camera.main.transform.position;

		RaycastHit[] sphereHit = Physics.SphereCastAll(origin, 0.3f, forward, range);
		List<GameObject> results = new List<GameObject> ();

		for(int i = 0; i < sphereHit.Length; i++){
			GameObject obj = sphereHit[i].collider.transform.root.gameObject;
			Rigidbody rigid = obj.GetComponent<Rigidbody> ();

			if(rigid != null && obj.transform.root != gameObject.transform && !rigid.isKinematic){
				results.Add(obj);

			}
		}

		GameObject output = null;

		foreach(GameObject obj in results){
			if (output == null || Vector3.Distance (origin, obj.transform.position) < Vector3.Distance (gameObject.transform.position, output.transform.position)) {
				output = obj;
			}
		}

		return output;
	}

	void GrabObject(GameObject obj){
		selectedObj = obj;

		if (selectedObj != null) {
			center = CenterObject(selectedObj);

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
		if (selectedObj != null) {
			Rigidbody rigid = selectedObj.GetComponent<Rigidbody> ();
			rigid.useGravity = false;

			Vector3 origin = Camera.main.transform.position;

			Vector3 forward = Camera.main.transform.forward;
			Vector3 upward = Camera.main.transform.up;

			Vector3 pos = center;


			Vector3 axis_forward = Vector3.Cross (Vector3.up, Vector3.Cross (forward, Vector3.up)).normalized;
			if (axis_forward.magnitude == 0) {
				axis_forward =  Vector3.Cross (Vector3.up, Vector3.Cross (upward, Vector3.up)).normalized;
			}

			float distance = (axis_forward * radius).magnitude/Vector3.Project (forward, (axis_forward * radius)).magnitude;;

			distance = Mathf.Min (distance, maxRadius);
			Vector3 goal = origin + forward * distance;
			if (goal.x != goal.x) {
				goal = origin + forward * maxRadius;
			}
				
			//Debug.DrawLine (origin, origin + forward, new Color(1,0,0,0.6f));
			//Debug.DrawLine (origin, origin + axis_forward * radius, new Color(0,0,1,0.6f));
			//Debug.DrawLine (origin, goal, new Color(0,1,0,1f));

			//Vector3 goal
			rigid.velocity = (goal - pos).normalized * Vector3.Distance(pos, goal) / Time.fixedDeltaTime;


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

		Debug.DrawLine (origin, origin + Camera.main.transform.forward * range, Color.red);

		UnityEditor.Handles.DrawWireDisc (origin, Vector3.up, radius);

		if(selectedObj != null){
			CalculateBounds (selectedObj);

			UnityEditor.Handles.color = new Color (1, 1, 0, 0.5f);
			UnityEditor.Handles.DrawWireCube (center, Vector3.one * 0.2f);
		}
	}

	Vector3 CenterObject(GameObject obj){

		Bounds bounds = obj.transform.GetComponent<Renderer>().bounds;

		Renderer[] renders = obj.GetComponentsInChildren<Renderer> ();
		foreach (Renderer render in renders){
			if (render.gameObject.activeSelf != true)
				continue;

			bounds.Encapsulate (render.bounds.min);
			bounds.Encapsulate (render.bounds.max);
		}

		return bounds.center - obj.transform.position;
	}


	//FLUFF

	private void CalculateBounds(GameObject obj){

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
	}

	public void rotateRigidBodyAroundPointBy(Rigidbody rb, Vector3 origin, Vector3 axis, float angle){
		Quaternion q = Quaternion.AngleAxis(angle, axis);
		rb.MovePosition(q * (rb.transform.position - origin) + origin);
		rb.MoveRotation(rb.transform.rotation * q);
	}
}
