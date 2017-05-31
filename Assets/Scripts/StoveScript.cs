using cakeslice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveScript : MonoBehaviour
{

    public GameObject deathZone;
    private GameObject[] heatingPlateOn;
    private GameObject[] heatingPlateOff;
    public GameObject[] switches;

    static public bool isOn = false;
    // Update is called once per frame
    void Update()
    {
        if(heatingPlateOff == null)
        {
            heatingPlateOff = GameObject.FindGameObjectsWithTag("HeaterOff");
            heatingPlateOn = GameObject.FindGameObjectsWithTag("HeaterOn");

            foreach (GameObject o in heatingPlateOff)
            {
                o.GetComponent<MeshRenderer>().enabled = !isOn;
            }

            foreach (GameObject o in heatingPlateOn)
            {
                o.GetComponent<MeshRenderer>().enabled = isOn;
            }
        }

        Vector3 forward = Camera.main.transform.forward;
        Vector3 origin = Camera.main.transform.position;

        RaycastHit[] sphereHit = Physics.SphereCastAll(origin, 0.5f, forward, 0.8f);

        bool highlight = false;

        foreach (RaycastHit r in sphereHit)
        {
            GameObject result = r.transform.gameObject;
            foreach(GameObject s in switches)
            {
                if (s.Equals(result))
                {
                    Outline o = s.GetComponent<Outline>();
                    o.eraseRenderer = false;
                    highlight = true;


                }else if (!highlight)
                {
                    Outline o = s.GetComponent<Outline>();
                    o.eraseRenderer = true;
                }
            }
        }

        if (Input.GetMouseButtonDown(0) && highlight)
        {
            isOn = !isOn;

            foreach(GameObject o in heatingPlateOff)
            {
                o.GetComponent<MeshRenderer>().enabled = !isOn;
            }

            foreach (GameObject o in heatingPlateOn)
            {
                o.GetComponent<MeshRenderer>().enabled = isOn;
            }

        }
    }
}
