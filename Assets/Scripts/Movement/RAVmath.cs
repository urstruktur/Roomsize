using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RAVmath{

	public static Vector3 NearestPointOnLine(Ray ray, Vector3 point){
		
		Vector3 v = point - ray.origin;
		float d = Vector3.Dot(v, ray.direction);
		return  ray.origin + ray.direction * d;
	}




}
