using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ScaleManager : MonoBehaviour {

    // the room, where the player is, determines which rigidbodys are kinematic
    // 

    public GameObject initialRoom;

    public GameObject doorsillSmall;
    public GameObject doorsillBig;

    [Range(1, 6)]
    public int depth = 2;

    [Range(0.1f, 1f)]
    public float doorSize = 0.1f;

    private GameObject[] rooms;

    void Start () {
        rooms = new GameObject[depth];

        rooms[0] = new GameObject();
        rooms[0].name = "roomCopy" + 1;
        rooms[0].transform.position = doorsillSmall.transform.position;
        Instantiate(initialRoom, rooms[0].transform, true);
        rooms[0].transform.localScale = new Vector3(1 / doorSize, 1 / doorSize, 1 / doorSize);
        rooms[0].transform.position = doorsillBig.transform.position;
        rooms[0].transform.localRotation = Quaternion.Inverse(doorsillSmall.transform.rotation);
    }
	
	void Update () {
		
	}
}
