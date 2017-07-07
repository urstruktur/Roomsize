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
    private GameObject originalBigDoor;
    private GameObject originalSmallDoor;

    // [Range(1, 6)]
    private int depthSmall = 1; // how many levels of small rooms are instantiated
    private int depthBig = 2;  // how many levels of big rooms are instantiated
    private int depthBigStatic = 1;  // how many levels of big rooms have static gameObjects not removed

    private float fovBigRoom = 80;
    private float fovSmallRoom = 60;

    [Range(0.1f, 1f)]
    public float doorSize = 0.1f;

    private GameObject[] rooms;
    private Rigidbody[][] assocs;

    private GameObject player;
    private FirstPersonController fpc;

    public Material windowPortalMaterial;
    public Material windowPortalMaterialBig;

    public Camera sceneryCameraNormal;
    public Camera sceneryCameraBig;

    static public int currentRoom = 0;
    static public bool inNormalSize = true;

    float walkSpeedSlow;
    float walkSpeedFast;

    void Start() {
        // find player, set variable
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if(players.Length >= 1)
        {
            player = players[0];
            fpc = player.GetComponent<FirstPersonController>();
            // save walking speed
            walkSpeedSlow = fpc.m_WalkSpeed;
            walkSpeedFast = fpc.m_WalkSpeed * 2;
        }
        else{
            Debug.LogError("No GameObject with Player tag!");
        }

        // collect rigidbodies 
        Rigidbody[] initialBodies = initialRoom.GetComponentsInChildren<Rigidbody>();

        // create scaled rooms
        rooms = new GameObject[depthBig + depthSmall];
        originalSmallDoor = GetFirstChildWithTag(initialRoom, "SmallDoor");
        originalBigDoor = GetFirstChildWithTag(initialRoom, "BigDoor");

        // initalize association array
        assocs = new Rigidbody[initialBodies.Length][];
        for(int i = 0; i < initialBodies.Length; i++)
        {
            assocs[i] = new Rigidbody[depthBig+depthSmall+1];
            assocs[i][0] = initialBodies[i];

            // add marker if not available
            ScaleMarker marker = initialBodies[i].gameObject.GetComponent<ScaleMarker>();
            if(marker == null)
            {
                marker = initialBodies[i].gameObject.AddComponent<ScaleMarker>();
            }
            marker.ScaleLevel = 0;
            marker.BigDoorPosition = originalBigDoor.transform.position;
        }

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

                // if ambient: deactivate
                if(lightCopy.shadows == LightShadows.None)
                {
                    lightCopy.enabled = false;
                }

                if (lightCopy.type == LightType.Directional)
                {
                    lightCopy.enabled = false;
                }
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

                        // set marker
                        ScaleMarker marker = copiedBody.gameObject.GetComponent<ScaleMarker>();
                        marker.ScaleLevel = -(roomNr + 1);
                        marker.BigDoorPosition = smallDoor.transform.position;

                        // set all kinematic
                        copiedBody.isKinematic = true;
                    }
                }
            }

            // deactivate colliders
            Transform transforms = rooms[roomNr].GetComponentInChildren<Transform>();

            foreach (Transform t in transforms)
            {
                MeshCollider coll = t.GetComponent<MeshCollider>();
                if (coll  != null)
                {
                    coll.enabled = false;
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

            SetLayerRecursively(rooms[roomNr], LayerMask.NameToLayer("BigRoom"));

            // scale light range
            Light[] lights = rooms[roomNr].GetComponentsInChildren<Light>();
            foreach (Light lightCopy in lights)
            {
                lightCopy.range = lightCopy.range / doorSize;

               
                if (lightCopy.type == LightType.Directional) 
                {
                    lightCopy.cullingMask &= ~(1 << LayerMask.NameToLayer("NormalRoom")); // turn off normal room
                    lightCopy.cullingMask |= 1 << LayerMask.NameToLayer("BigRoom"); // turn on big room

                    if ((roomNr - depthSmall + 1) > 1)
                    {
                        // disable directional lights in all bigger levels than 1
                        lightCopy.enabled = false;
                    }
                }
            }

            // for each big room level that exceed depthBigStatic delete all non-rigidbodies
            if (roomNr - depthSmall + 1 > depthBigStatic)
            {
                Transform transforms = rooms[roomNr].GetComponentInChildren<Transform>();

                foreach (Transform t in transforms)
                {
                    if ((t.GetComponent<Rigidbody>() == null || t.tag == "Ignore") && !HasRigidbodyChildren(t))
                    {
                        Destroy(t.gameObject);
                    }
                }
            }

            // associate rigidbodies
            Rigidbody[] bodies = rooms[roomNr].GetComponentsInChildren<Rigidbody>();
            for(int a = 0; a < initialBodies.Length; a++) 
            {
                foreach (Rigidbody copiedBody in bodies) { 
                    if (initialBodies[a].gameObject.name == copiedBody.gameObject.name)
                    {
                        assocs[a][depthSmall + roomNr] = copiedBody;

                        // set marker
                        ScaleMarker marker = copiedBody.gameObject.GetComponent<ScaleMarker>();
                        marker.ScaleLevel = (roomNr - depthSmall + 1);
                        marker.BigDoorPosition = smallDoor.transform.position;

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

            // exchange window portal materials in scale level 1
            if(roomNr == 1)
            {
                Transform[] transforms = rooms[roomNr].GetComponentsInChildren<Transform>();
                foreach (Transform t in transforms)
                {
                    Renderer renderer = t.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        if (renderer.sharedMaterial == windowPortalMaterial)
                        {
                            renderer.sharedMaterial = windowPortalMaterialBig;
                        }
                    }
                }

                if (sceneryCameraBig != null)
                {
                    sceneryCameraBig.gameObject.transform.SetParent(rooms[roomNr].transform);
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
            if (t.GetComponent<Rigidbody>() != null && t.tag != "Ignore")
            {
                return true;
            }
        }

        return false;
    }


    void Update() {
        // check for active room
        Vector3 relativePosition = player.transform.position - originalBigDoor.transform.position;

        float fov = -relativePosition.z * 6 + fovSmallRoom + (fovBigRoom - fovSmallRoom) / 2f;

        if (fov > fovBigRoom)
        {
            fov = fovBigRoom;
        } else if (fov < fovSmallRoom)
        {
            fov = fovSmallRoom;
        }
        Camera.main.fieldOfView = fov;

        // set rotation of scenery camera relative to big room
        if(sceneryCameraBig != null)
        {
            sceneryCameraBig.transform.localRotation = Camera.main.transform.rotation;
            sceneryCameraBig.fieldOfView = Camera.main.fieldOfView;
        }
        //sceneryCameraBig.transform.localPosition = Camera.main.transform.localPosition;
        //Vector3 pos = sceneryCameraBig.transform.position;
        //pos.y += 100;
        //sceneryCameraBig.transform.position = pos;

        if (relativePosition.z > 0)
        {
            if (!inNormalSize)
            {
                Debug.Log("In original room.");
                inNormalSize = true;
                fpc.m_WalkSpeed = walkSpeedSlow;
                //InvertActivation();
                //SetCameraFOV(true);
            }
        }
        else {
            if (inNormalSize)
            {
                Debug.Log("In big room.");
                inNormalSize = false;
                fpc.m_WalkSpeed = walkSpeedFast;
                // InvertActivation();
                // SetCameraFOV(false);
            }
        }

        // apply transformation on all associated rigidbodies

        foreach (Rigidbody[] association in assocs)
        {
            for (int n = 0; n < association.Length; n++)
            {
                if (!association[n].isKinematic && association[n].transform.hasChanged)
                {
                    for (int m = 0; m < association.Length; m++)
                    {
                        if (m != n)
                        {
                            association[m].transform.localPosition = association[n].transform.localPosition;
                            association[m].transform.localRotation = association[n].transform.localRotation;
                            association[m].transform.hasChanged = false;
                        }
                    }
                    
                    // activate biggest scale equivalent
                    // if normal scale exits small door
                    Vector3 relativeToSmallDoor = association[0].transform.position - originalSmallDoor.transform.position;
                    //Debug.Log(relativeToSmallDoor);

                    if (relativeToSmallDoor.z < 0)
                    {
                        if (!association[association.Length - 1].gameObject.activeSelf)
                        {
                            Debug.Log(association[association.Length - 1].name + " set active.");
                            association[association.Length - 1].gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        if (association[association.Length - 1].gameObject.activeSelf)
                        {
                            //Debug.Log("Set inactive.");
                            association[association.Length - 1].gameObject.SetActive(false);
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
        if (rooms[0].activeInHierarchy)
        {
            rooms[0].SetActive(false);

        }else
        {
            rooms[0].SetActive(true);
        }
    }

    /*
    private void SetCameraFOV(bool normal)
    {
        LeanTween.value(Camera.main.gameObject, Camera.main.fieldOfView, normal ? fovBigRoom : fovSmallRoom, 0.8f).setOnUpdate((float val) =>
        {
            Camera.main.fieldOfView = val;
        }).setEase(LeanTweenType.easeInOutQuad);
    }*/
            
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

    public void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach(Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
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
