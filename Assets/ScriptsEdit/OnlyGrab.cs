using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlyGrab : MonoBehaviour {

	public GameObject camObject;
	public Material selectedMat, markedMat;
	Vector3 position;

	GameObject selectedObj = null;

	GameObject markedObj = null;
	Material markedObjMat = null;

	void Update () {
		position =  camObject.transform.position;

		GameObject obj = DetectObject();

		if (selectedObj != null) {
			MarkObject (selectedObj, selectedMat);
		} else {
			MarkObject (obj, markedMat);
		}

		if (Input.GetMouseButtonDown (0)) {
			//Debug.Log ("Pressed left click.");
			GrabObject(obj);
		}

		if (Input.GetMouseButtonUp (0)) {
			DropObject();
			//Debug.Log ("Released left click.");
		}

	}

	void FixedUpdate(){
		DragObject();
	}

	void DragObject (){
		if (selectedObj != null) {
			Rigidbody rigid = selectedObj.GetComponent<Rigidbody> ();
			rigid.useGravity = false;

			Vector3 pos = selectedObj.transform.position;
			Vector3 goal = position + camObject.transform.forward * 1;

			Vector3 dir = (goal - pos).normalized;
			float dis = Vector3.Distance(goal, pos);

			Debug.DrawLine (position, goal);

			rigid.velocity = dir * dis * 5;

			//rigid.AddForce ( dis * dir);
		}
	}


	GameObject DetectObject(){
		Vector3 dir = camObject.transform.forward;

		RaycastHit[] sphereHit = Physics.SphereCastAll(position, 0.3f, dir, 3);
		List<GameObject> results = new List<GameObject> ();

		for(int i = 0; i < sphereHit.Length; i++){
			GameObject obj = sphereHit[i].collider.gameObject;
			Rigidbody rigid = obj.GetComponent<Rigidbody> ();

			if(rigid != null && obj.gameObject != this.gameObject){
				results.Add(obj);
			}
		}

		GameObject output = null;

		foreach(GameObject obj in results){
			if (output == null || Vector3.Distance (position, obj.transform.position) < Vector3.Distance (gameObject.transform.position, output.transform.position)) {
				output = obj;
			}
		}

		return output;
	}

	void MarkObject(GameObject obj, Material mat){

		if (markedObj != null) {
			markedObj.GetComponent<Renderer>().sharedMaterial = markedObjMat;
		}

		if (obj != null && mat != null) {
			Renderer render = obj.GetComponent<Renderer>();
			markedObj = obj;
			markedObjMat = render.sharedMaterial;

			render.sharedMaterial = mat;
		}
	}

	void GrabObject (GameObject obj){
		//Debug.DrawLine(transform.position, transform.position + rotation * Vector3.forward);
		selectedObj = obj;
	}

	void DropObject (){
		if (selectedObj != null) {
			Rigidbody rigid = selectedObj.GetComponent<Rigidbody> ();
			rigid.velocity = Vector3.zero;
			rigid.useGravity = true;

			selectedObj = null;
		}
	}
}
