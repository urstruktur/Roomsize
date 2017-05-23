using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private Rigidbody[][] associations;

    void Start () {
        rooms = new GameObject[depth];

        // create BIG room copy and set position, scale & rotation correctly
        rooms[0] = new GameObject();
        rooms[0].name = "roomCopy" + 1;
        rooms[0].transform.position = doorsillSmall.transform.position;
        Instantiate(initialRoom, rooms[0].transform, true);
        rooms[0].transform.localScale = new Vector3(1 / doorSize, 1 / doorSize, 1 / doorSize);
        rooms[0].transform.position = doorsillBig.transform.position;
        rooms[0].transform.localRotation = Quaternion.Inverse(doorsillSmall.transform.rotation);
        InvertKinematic(rooms[0].transform);

        // create SMALL room copy and set position, scale & rotation correctly
        rooms[1] = new GameObject();
        rooms[1].name = "roomCopy" + 2;
        rooms[1].transform.position = doorsillBig.transform.position;
        Instantiate(initialRoom, rooms[1].transform, true);
        rooms[1].transform.localScale = new Vector3(doorSize, doorSize, doorSize);
        rooms[1].transform.position = doorsillSmall.transform.position;
        rooms[1].transform.localRotation = doorsillSmall.transform.rotation;
        InvertKinematic(rooms[1].transform);

        // save associations
        Rigidbody[] bodiesInitial = initialRoom.GetComponentsInChildren<Rigidbody>();
        Rigidbody[] bodiesCopyBig = rooms[0].GetComponentsInChildren<Rigidbody>();
        Rigidbody[] bodiesCopySmall = rooms[1].GetComponentsInChildren<Rigidbody>();

        associations = new Rigidbody[bodiesInitial.Length][];

        for (int i = 0; i < bodiesInitial.Length; i++)
        {
            Rigidbody bigAssociation = null;
            foreach (Rigidbody bodyCopyBig in bodiesCopyBig) {
                if (bodiesInitial[i].gameObject.name == bodyCopyBig.gameObject.name)
                {
                    bigAssociation = bodyCopyBig;
                }
            }
            Rigidbody smallAssociation = null;
            foreach (Rigidbody bodyCopySmall in bodiesCopySmall)
            {
                if (bodiesInitial[i].gameObject.name == bodyCopySmall.gameObject.name)
                {
                    smallAssociation = bodyCopySmall;
                }
            }
            associations[i] = new Rigidbody[]{ bodiesInitial[i], bigAssociation, smallAssociation };
        }

        // change light range of copy
        Light[] lightCopies = rooms[0].GetComponentsInChildren<Light>();
        foreach(Light lightCopy in lightCopies)
        {
            lightCopy.range = lightCopy.range / doorSize;
        }

        // change light range of copy
        lightCopies = rooms[1].GetComponentsInChildren<Light>();
        foreach (Light lightCopy in lightCopies)
        {
            lightCopy.range = lightCopy.range * doorSize;
        }

        Debug.Log(associations.Length + " associations saved.");
    }

	void Update () {
        foreach (Rigidbody[] entry in associations)
        {
            Rigidbody a = entry[0];
            Rigidbody b = entry[1];
            Rigidbody c = entry[2];

            if (!a.isKinematic && a.transform.hasChanged)
            {
                b.transform.localPosition = a.transform.localPosition;
                b.transform.localRotation = a.transform.localRotation;
                a.transform.hasChanged = false;

                c.transform.localPosition = a.transform.localPosition;
                c.transform.localRotation = a.transform.localRotation;
            }
            else if(!b.isKinematic && b.transform.hasChanged)
            {
                a.transform.localPosition = b.transform.localPosition;
                a.transform.localRotation = b.transform.localRotation;
                b.transform.hasChanged = false;

                c.transform.localPosition = b.transform.localPosition;
                c.transform.localRotation = b.transform.localRotation;
            }
        }

        if (Input.GetKey("escape"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
