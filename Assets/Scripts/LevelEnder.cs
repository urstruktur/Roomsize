using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class LevelEnder : MonoBehaviour {

    public GameObject setActiveOnTrigger;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider collider)
    {
        if(gameObject.transform.lossyScale.magnitude >= 0.5)
        {
            Debug.Log("finish");
            if (collider.gameObject.GetComponent<FirstPersonController>() != null)
            {
                setActiveOnTrigger.SetActive(true);
            }
        }
    }
}
