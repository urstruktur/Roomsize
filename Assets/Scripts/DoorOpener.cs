using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DoorOpener : MonoBehaviour
{
    [Header("Use \"Door\" tag to animate doors on trigger enter.")]

    public GameObject player;

    void OnTriggerEnter(Collider other)
    {
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");

        foreach(GameObject door in doors)
        {
            Animation animation = door.GetComponent<Animation>();
            if (animation != null)
            {
                animation.Play("DoorOpening");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");

        foreach (GameObject door in doors)
        {
            Animation animation = door.GetComponent<Animation>();
            if (animation != null)
            {
                animation.Play("DoorClosing");
            }
        }
    }
}