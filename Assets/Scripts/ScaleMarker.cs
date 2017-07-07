using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleMarker : MonoBehaviour {
    public int ScaleLevel { get; set; }
    public Vector3 BigDoorPosition { get; set; }
    public bool pickableInNormal = true;
    public bool pickableInBig = false;
}
