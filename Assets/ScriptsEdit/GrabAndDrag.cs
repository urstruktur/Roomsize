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

	struct Tag{
		public Renderer render;
		public Material mat;

		public Tag(Renderer render, Material mat){
			this.render = render;
			this.mat = mat;
		}
	}

	GameObject selectedObj = null;

	Dictionary<string, List<Tag>> materials = new Dictionary<string, List<Tag>>();

	// Use this for initialization
	void Start () {
		
	}

	void FixedUpdate () {
		DragObject ();

	}

	// Update is called once per frame
	bool step = false;

	void Update () {

		GameObject detectedObj = null;
		if (selectedObj == null) {
			detectedObj = DetectObject ();
		}



		if (Input.GetMouseButtonDown (0)) {
			//Debug.Log ("Pressed left click.");
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

			//Debug.DrawLine (transform.position, transform.position +  horiz * new Vector3(1,0,0) *1.4f, Color.white);

			selectedObj.transform.rotation = pitch * yaw * selectedObj.transform.rotation;
		}
		if (Input.GetMouseButtonUp (1)) {
			UnityStandardAssets.Characters.FirstPerson.FirstPersonController controller = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
			controller.activeRotation = true;
		}

		if (Input.GetKeyDown (KeyCode.Q)) {
			//Debug.Log ("GO A STEP");
			step = true;
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
			GameObject obj = sphereHit[i].collider.gameObject;
			Rigidbody rigid = obj.GetComponent<Rigidbody> ();

			if(rigid != null && obj.transform.root != gameObject.transform && !rigid.isKinematic){
				results.Add(obj);
				//Debug.Log(sphereHit[i].collider.name);
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

	void GrabObject(GameObject obj){
		//Debug.DrawLine(transform.position, transform.position + rotation * Vector3.forward);
		selectedObj = obj;

		if (selectedObj != null) {
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

			Vector3 pos = selectedObj.transform.position;
			//Vector3 goal = origin + forward;

			//Vector3 dir = (goal - pos).normalized;


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


			//Vector3 goal = forward  * Vector3.Dot (forward, axis_forward * radius);

			//Debug.Log (axis_forward);

			//float dis = Vector3.Distance(goal, pos);

			Debug.DrawLine (origin, origin + forward, new Color(1,0,0,0.6f));
			Debug.DrawLine (origin, origin + axis_forward * radius, new Color(0,0,1,0.6f));

			//Debug.DrawLine (origin, pos, new Color(1,1,1,0.6f));

			Debug.DrawLine (origin, goal, new Color(0,1,0,1f));

			//Vector3 goal

			rigid.velocity = (goal - pos).normalized * Vector3.Distance(pos, goal) / Time.fixedDeltaTime;

			if (step || true) {
				//rigid.velocity = f * dis * 5;

				//step = false;
			} else {
				//rigid.velocity = Vector3.zero;
			}


			//Debug.Log ("#" + rigid.angularVelocity.magnitude + " ");

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


	void OnDrawGizmos(){
		Vector3 origin = Camera.main.transform.position;

		UnityEditor.Handles.DrawWireDisc (origin, Vector3.up, radius);
	}
}
