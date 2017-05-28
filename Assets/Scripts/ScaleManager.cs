using System;
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

    public GameObject doorsillSmall;
    public GameObject doorsillBig;

    public GameObject doorsillSmallExit;

    public GameObject testObject;

   // [Range(1, 6)]
    private int depthSmall = 1;
    private int depthBig = 2;

    [Range(0.1f, 1f)]
    public float doorSize = 0.1f;

    private GameObject[] rooms;
    private Rigidbody[][] associations;

    private Quaternion dimensionalRotation;
    private Vector3[] dimensionalTranslation;

    private Matrix4x4 dimensionalTransformation;

    private GameObject player;
    private FirstPersonController fpc;

    private bool inNormalSize = true;

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

        
        Vector3 t = doorsillBig.transform.localPosition - doorsillSmall.transform.localPosition;
        t.Scale(Vector3.one * (1 / doorSize));
        t = doorsillSmall.transform.rotation * t; // change to relative rotation to doorsillBig
        dimensionalTransformation = Matrix4x4.TRS(t,
                                                  doorsillSmall.transform.rotation, //Quaternion.Inverse(doorsillSmall.transform.rotation) * doorsillBig.transform.rotation,
                                                  Vector3.one * (1 / doorSize));

        if(testObject != null)
        {
            GameObject o1 = Instantiate(testObject);

            o1.transform.localPosition = (o1.transform.position + ExtractTranslationFromMatrix(ref dimensionalTransformation));
            o1.transform.localRotation = ExtractRotationFromMatrix(ref dimensionalTransformation);
            o1.transform.localScale = Vector3.Scale(o1.transform.localScale, ExtractScaleFromMatrix(ref dimensionalTransformation));

        }


        rooms = new GameObject[depthBig + depthSmall];

        // create BIG room copy and set position, scale & rotation correctly
        rooms[0] = new GameObject();
        rooms[0].name = "roomCopy" + 1;
        rooms[0].transform.position = doorsillSmall.transform.position;
        Instantiate(initialRoom, rooms[0].transform, true);
        rooms[0].transform.localScale = new Vector3(1 / doorSize, 1 / doorSize, 1 / doorSize);
        rooms[0].transform.position = doorsillBig.transform.position;
        rooms[0].transform.localRotation = Quaternion.Inverse(doorsillSmall.transform.rotation);
        InvertKinematic(rooms[0].transform);

        Vector3 doorsillSmall0 = rooms[0].transform.position;
        Vector3 doorsillBig0 = - rooms[0].transform.position + doorsillSmall.transform.rotation * (doorsillBig.transform.position * (1/doorSize));

        GameObject test = new GameObject("doorsillSmall0");
        test.transform.position = doorsillSmall0;

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

        // create bigger rigidbodys
        //Rigidbody[] bodiesCopyBigger = rooms[0].GetComponentsInChildren<Rigidbody>();
        /*
        rooms[2] = new GameObject();
        rooms[2].name = "roomCopy" + 3;
        rooms[2].transform.position = doorsillSmall0;
        Instantiate(initialRoom, rooms[2].transform, true);
        rooms[2].transform.localScale = new Vector3(1 / doorSize / doorSize, 1 / doorSize / doorSize, 1 / doorSize / doorSize);
        rooms[2].transform.position = doorsillBig0;
        */

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
        // check for active room
        Vector3 relativePosition = player.transform.position - doorsillBig.transform.position;
        
        if(relativePosition.z > 0)
        {
            if (!inNormalSize)
            {
                Debug.Log("normal size");
                inNormalSize = true;
                fpc.m_WalkSpeed = fpc.m_WalkSpeed / 2;
                InvertActivation();
            }
        }
        else{
            if (inNormalSize)
            {
                Debug.Log("small size");
                inNormalSize = false;
                fpc.m_WalkSpeed = fpc.m_WalkSpeed * 2;
                InvertActivation();
            }
        }
        
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
}
