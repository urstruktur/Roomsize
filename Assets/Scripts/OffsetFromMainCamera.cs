using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * offsets this camera from the main camera by the initial y offset
 **/
public class OffsetFromMainCamera : MonoBehaviour {

    private float initialYPos;
    private Camera cam;

	// Use this for initialization
	void Start () {
        initialYPos = this.transform.position.y;
        cam = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        //Vector3 pos = Camera.main.transform.position;
        //pos.y += initialYPos;
        //this.transform.position = pos;
        cam.fieldOfView = Camera.main.fieldOfView;
        this.transform.rotation = Camera.main.transform.rotation;
    }
}
