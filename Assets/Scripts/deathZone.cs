using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class deathZone : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {
        if (StoveScript.isOn && other.tag == "Player")
        {
            Vector3 direction = other.transform.position - this.transform.position;
            direction.Normalize();
            ImpactReceiver script = other.gameObject.GetComponent<ImpactReceiver>();
            if (script != null) script.AddImpact(direction, 50f);
            Debug.Log("enter");
        }
    }
}
