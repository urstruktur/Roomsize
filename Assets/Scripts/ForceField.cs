using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour {

    public bool noForceInX = false;
    public bool noForceInY = false;
    public bool noForceInZ = false;

    public float multiplier = 1.0f;

   

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            Vector3 force = other.transform.position - this.transform.position;
            force.Normalize();
            if (noForceInX)
            {
                force.x = 0;
            }
            if (noForceInY)
            {
                force.y = 0;
            }
            if (noForceInZ)
            {
                force.z = 0;
            }

            ImpactReceiver script = other.gameObject.GetComponent<ImpactReceiver>();
            if (script != null) script.AddImpact(force, multiplier);
        }
    }
}
