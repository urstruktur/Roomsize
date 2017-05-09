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

    public GameObject doorsillSmallExit;

    [Range(1, 6)]
    public int depth = 2;

    [Range(0.1f, 1f)]
    public float doorSize = 0.1f;

    private GameObject[] rooms;
    private Dictionary<Rigidbody, Rigidbody> associations;

    void Start () {
        rooms = new GameObject[depth];
        associations = new Dictionary<Rigidbody, Rigidbody>();

        // create room copy and set position, scale & rotation correctly
        rooms[0] = new GameObject();
        rooms[0].name = "roomCopy" + 1;
        rooms[0].transform.position = doorsillSmall.transform.position;
        Instantiate(initialRoom, rooms[0].transform, true);
        rooms[0].transform.localScale = new Vector3(1 / doorSize, 1 / doorSize, 1 / doorSize);
        rooms[0].transform.position = doorsillBig.transform.position;
        rooms[0].transform.localRotation = Quaternion.Inverse(doorsillSmall.transform.rotation);
        InvertKinematic(rooms[0].transform);

        // save associations
        Rigidbody[] bodiesInitial = initialRoom.GetComponentsInChildren<Rigidbody>();
        Rigidbody[] bodiesCopy = rooms[0].GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody bodyInitial in bodiesInitial)
        {
            foreach (Rigidbody bodyCopy in bodiesCopy)
            {
                if(bodyInitial.gameObject.name == bodyCopy.gameObject.name)
                {
                    associations.Add(bodyInitial, bodyCopy);
                }
            }
        }

        // change light range of copy
        Light[] lightCopies = rooms[0].GetComponentsInChildren<Light>();
        foreach(Light lightCopy in lightCopies)
        {
            lightCopy.range = lightCopy.range / doorSize;
        }

        Debug.Log(associations.Count + " associations saved.");
    }

	void Update () {
        foreach (KeyValuePair<Rigidbody, Rigidbody> entry in associations)
        {
            Rigidbody a = entry.Value;
            Rigidbody b = entry.Key;

            if (!a.isKinematic && a.transform.hasChanged)
            {
                b.transform.localPosition = a.transform.localPosition;
                b.transform.localRotation = a.transform.localRotation;
                a.transform.hasChanged = false;
            }else if(!b.isKinematic && b.transform.hasChanged)
            {
                a.transform.localPosition = b.transform.localPosition;
                a.transform.localRotation = b.transform.localRotation;
                b.transform.hasChanged = false;
            }
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

}
