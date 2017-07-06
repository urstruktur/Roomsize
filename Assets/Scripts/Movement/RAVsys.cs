using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RAVsys{

	public static T DetectObject<T>(Ray ray, float range = float.PositiveInfinity){

		RaycastHit[] sphereHit = Physics.SphereCastAll(ray.origin, 0.3f, ray.direction, range);

		float value = float.PositiveInfinity;

		T output = default(T);

		for (int i = 0; i < sphereHit.Length; i++) {

			Transform root = sphereHit [i].collider.transform;
			T comp;

			while (true) {
				comp = root.GetComponent<T>();
				if (comp != null || root.parent == null) {
					break;
				} else {
					root = root.parent;
				}
			}

			Vector3 point = RAVmath.NearestPointOnLine(ray, sphereHit [i].point);

			Vector3 ping = sphereHit [i].collider.ClosestPoint (point);
			point = RAVmath.NearestPointOnLine(ray, ping);


			float distance = Mathf.Pow( Vector3.Distance (ray.origin, point), 0.3f) + Mathf.Pow( Vector3.Distance (sphereHit [i].point, point),1);

			if(distance < value){
				output = comp;
				value = distance;
			}

		}

		return output;
	}
}
