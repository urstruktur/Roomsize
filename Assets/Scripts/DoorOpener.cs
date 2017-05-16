using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

public class DoorOpener : MonoBehaviour
{
    [Header("Use \"Door\" tag to animate doors on trigger enter.")]

    public GameObject player;

    void OnTriggerEnter(Collider other)
    {
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");
        Debug.Log("door opening");

        foreach(GameObject door in doors)
        {
            Animator animator = door.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("isOpen", true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");

        foreach (GameObject door in doors)
        {
            Animator animator = door.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("isOpen", false);
            }
        }
    }
}