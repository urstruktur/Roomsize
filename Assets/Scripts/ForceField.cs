using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Vector3 force = this.transform.position - other.transform.position;
            force.Normalize();
            other.GetComponent<Rigidbody>().AddForce(force);
        }
    }
}
