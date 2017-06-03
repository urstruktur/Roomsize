﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class ScaleManager : MonoBehaviour {

    // the room, where the player is, determines which rigidbodys are kinematic
    //

    public GameObject initialRoom;
    private GameObject originalBigDoor;

    // public GameObject doorsillSmall;
    // public GameObject doorsillBig;
    //  public GameObject doorsillSmallExit;
    //  public GameObject testObject;

    // [Range(1, 6)]
    private int depthSmall = 1;
    private int depthBig = 2;
    private int depthBigStatic = 1;

    [Range(0.1f, 1f)]
    public float doorSize = 0.1f;

    private GameObject[] rooms;
    private Rigidbody[][] assocs;

    private GameObject player;
    private FirstPersonController fpc;

    static public int currentRoom = 0;
    static public bool inNormalSize = true;

    void Start() {
        // find player, set variable
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if(players.Length >= 1)
        {
            player = players[0];
            fpc = player.GetComponent<FirstPersonController>();
        }
        else{
            Debug.LogError("No GameObject with Player tag!");
        }
      
        // collect rigidbodies 
        Rigidbody[] initialBodies = initialRoom.GetComponentsInChildren<Rigidbody>();
        
        // initalize association array
        assocs = new Rigidbody[initialBodies.Length][];
        for(int i = 0; i < initialBodies.Length; i++)
        {
            assocs[i] = new Rigidbody[depthBig+depthSmall+1];
            assocs[i][0] = initialBodies[i];
        }

        // create scaled rooms
        rooms = new GameObject[depthBig + depthSmall];
        GameObject originalSmallDoor = GetFirstChildWithTag(initialRoom, "SmallDoor");
        originalBigDoor = GetFirstChildWithTag(initialRoom, "BigDoor");

        // --- INSTANTIATE DOWNSCALED ROOMS ---
        GameObject previousRoom = initialRoom;
        for(int roomNr = 0; roomNr < depthSmall; roomNr++)
        {
            // instantiate
            GameObject bigDoor = GetFirstChildWithTag(previousRoom, "BigDoor");
            GameObject smallDoor = GetFirstChildWithTag(previousRoom, "SmallDoor");
            rooms[roomNr] = new GameObject();
            rooms[roomNr].transform.position = bigDoor.transform.position;
            GameObject instance = Instantiate(previousRoom, rooms[roomNr].transform, true);
            rooms[roomNr].transform.localScale = new Vector3(doorSize, doorSize, doorSize);
            rooms[roomNr].transform.position = smallDoor.transform.position;
            rooms[roomNr].transform.localRotation = originalSmallDoor.transform.rotation;
            instance.transform.SetParent(null);
            Destroy(rooms[roomNr]);
            rooms[roomNr] = instance;
            rooms[roomNr].name = "room -" + (roomNr + 1);

            // scale light range
            Light[] lights = rooms[roomNr].GetComponentsInChildren<Light>();
            foreach (Light lightCopy in lights)
            {
                lightCopy.range = lightCopy.range * doorSize;
            }

            // associate rigidbodies
            Rigidbody[] bodies = rooms[roomNr].GetComponentsInChildren<Rigidbody>();
            for (int a = 0; a < initialBodies.Length; a++)
            {
                foreach (Rigidbody copiedBody in bodies)
                {
                    if (initialBodies[a].gameObject.name == copiedBody.gameObject.name)
                    {
                        assocs[a][roomNr + 1] = copiedBody;

                        // add marker
                        ScaleMarker marker = copiedBody.gameObject.AddComponent<ScaleMarker>();
                        marker.scaleLevel = -(roomNr + 1);
                        marker.bigDoorPosition = smallDoor.transform.position;

                        // set all kinematic
                        copiedBody.isKinematic = true;
                    }
                }
            }

            previousRoom = rooms[roomNr];
        }

        // --- INSTANTIATE UPSCALED ROOMS ---
        previousRoom = initialRoom;
        for (int roomNr = depthSmall; roomNr < depthBig+depthSmall; roomNr++)
        {
            // instantiate
            GameObject bigDoor = GetFirstChildWithTag(previousRoom, "BigDoor");
            GameObject smallDoor = GetFirstChildWithTag(previousRoom, "SmallDoor");
            rooms[roomNr] = new GameObject();
            rooms[roomNr].transform.position = smallDoor.transform.position;
            GameObject instance = Instantiate(previousRoom, rooms[roomNr].transform, true);
            rooms[roomNr].transform.localScale = new Vector3(1 / doorSize, 1 / doorSize, 1 / doorSize);
            rooms[roomNr].transform.position = bigDoor.transform.position;
            rooms[roomNr].transform.localRotation = Quaternion.Inverse(originalSmallDoor.transform.rotation);
            instance.transform.SetParent(null);
            Destroy(rooms[roomNr]);
            rooms[roomNr] = instance;
            rooms[roomNr].name = "room " + (roomNr - depthSmall + 1);

            // scale light range
            Light[] lights = rooms[roomNr].GetComponentsInChildren<Light>();
            foreach (Light lightCopy in lights)
            {
                lightCopy.range = lightCopy.range / doorSize;
            }



            // associate rigidbodies
            Rigidbody[] bodies = rooms[roomNr].GetComponentsInChildren<Rigidbody>();
            for(int a = 0; a < initialBodies.Length; a++) 
            {
                foreach (Rigidbody copiedBody in bodies) { 
                    if (initialBodies[a].gameObject.name == copiedBody.gameObject.name)
                    {
                        assocs[a][depthSmall + roomNr] = copiedBody;

                        // add marker
                        ScaleMarker marker = copiedBody.gameObject.AddComponent<ScaleMarker>();
                        marker.scaleLevel = (roomNr - depthSmall + 1);
                        marker.bigDoorPosition = smallDoor.transform.position;

                        // only invert kinematic in first room copy, others always kinematic
                        if (roomNr == depthSmall)
                        {
                            copiedBody.isKinematic = !copiedBody.isKinematic;
                        }else
                        {
                            copiedBody.isKinematic = true;
                        }
                    }
                }
            }

            // for each big room level that exceed depthBigStatic delete all non-rigidbodies
            if(roomNr - depthSmall + 1 > depthBigStatic)
            {
                Transform transforms = rooms[roomNr].GetComponentInChildren<Transform>();

                foreach(Transform t in transforms)
                {
                    if(t.GetComponent<Rigidbody>() == null && !HasRigidbodyChildren(t))
                    {
                        Destroy(t.gameObject);
                    }
                }
            }


            previousRoom = rooms[roomNr];
        }

       // Debug.Log(associations.Length + " associations saved.");
    }

    bool HasRigidbodyChildren(Transform parent)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>())
        {
            if (t.GetComponent<Rigidbody>() != null)
            {
                return true;
            }
        }

        return false;
    }

	void Update () {
        // check for active room
        Vector3 relativePosition = player.transform.position - originalBigDoor.transform.position;
        
        if(relativePosition.z > 0)
        {
            if (!inNormalSize)
            {
                Debug.Log("In original room.");
                inNormalSize = true;
                fpc.m_WalkSpeed = fpc.m_WalkSpeed / 2;
                //InvertActivation();
            }
        }
        else{
            if (inNormalSize)
            {
                Debug.Log("In big room.");
                inNormalSize = false;
                fpc.m_WalkSpeed = fpc.m_WalkSpeed * 2;
               // InvertActivation();
            }
        }

        // apply transformation on all associated rigidbodies
        
        foreach(Rigidbody[] association in assocs)
        {
            for(int n = 0; n < association.Length; n++)
            {
                if (!association[n].isKinematic && association[n].transform.hasChanged)
                {
                    for (int m = 0; m < association.Length; m++)
                    {
                        if(m != n)
                        {
                            association[m].transform.localPosition = association[n].transform.localPosition;
                            association[m].transform.localRotation = association[n].transform.localRotation;
                            association[m].transform.hasChanged = false;
                        }
                    }

                    association[n].transform.hasChanged = false;
                   
                    break; // no need to iterate through the rest
                }
            }
        }
        
        if (Input.GetKey("x"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }


    private void InvertActivation()
    {
        if (rooms[1].activeInHierarchy)
        {
            rooms[1].SetActive(false);

        }else
        {
            rooms[1].SetActive(true);
        }
    }
            
    // Sets all non-kinematic rigidbodys of children to kinematic rigidbodys and the other way round
    private void InvertKinematic(Transform t)
    {
        Rigidbody[] bodies = t.gameObject.GetComponentsInChildren<Rigidbody>();

        foreach(Rigidbody body in bodies)
        {
            body.isKinematic = !body.isKinematic;
        }
    }


    public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
    public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 translate;
        translate.x = matrix.m03;
        translate.y = matrix.m13;
        translate.z = matrix.m23;
        return translate;
    }

    public static GameObject GetFirstChildWithTag(GameObject parent, String tag){
        GameObject[] tagged = GameObject.FindGameObjectsWithTag(tag);
        foreach(GameObject o in tagged)
        {
            if (o.transform.IsChildOf(parent.transform))
            {
                return o;
            }
        }
        return null;
     }
}
