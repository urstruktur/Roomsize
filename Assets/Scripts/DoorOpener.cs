using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

public class DoorOpener : MonoBehaviour
{
    //[Header("Use \"Door\" tag to animate doors on trigger enter.")]
    
    void OnTriggerEnter(Collider other)
    {
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");
        if(other.gameObject.tag == "Player")
        foreach(GameObject door in doors)
        {
            Animator animator = door.GetComponent<Animator>();

            if (animator != null)
            {
                LeanTween.cancel(animator.gameObject);
                LeanTween.value(animator.gameObject, animator.GetFloat("openness"), ScaleManager.inNormalSize ? 1 : -1, 0.8f).setOnUpdate((float val) =>
                {
                   animator.SetFloat("openness", val);
                }).setEase(LeanTweenType.easeOutQuad);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");

        if (other.gameObject.tag == "Player")
        foreach (GameObject door in doors)
        {
            Animator animator = door.GetComponent<Animator>();
            if (animator != null)
            {
                LeanTween.cancel(animator.gameObject);
                LeanTween.value(animator.gameObject, animator.GetFloat("openness"), 0, 0.8f).setOnUpdate((float val) =>
                {
                    animator.SetFloat("openness", val);
                }).setEase(LeanTweenType.easeOutQuad);
            }
        }
    }
}