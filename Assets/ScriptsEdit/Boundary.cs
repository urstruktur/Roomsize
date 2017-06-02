using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary{

	public Vector3 center;
	public Vector3 size;

	GameObject obj;

	public Boundary(GameObject obj){
		Calculate (obj);
	}

	public void Calculate(GameObject obj){
		MeshFilter[] filters = obj.GetComponentsInChildren<MeshFilter> ();
		List<Vector3> points = new List<Vector3> ();

		for (int i = 0; i < filters.Length; i++) {

			Matrix4x4 localToWorldMatrix = filters [i].transform.localToWorldMatrix;

			Bounds bound = filters [i].sharedMesh.bounds;
			Vector3 center = bound.center;
			Vector3 size = bound.size;

			Vector3 position = new Vector3 (center.x - size.x / 2, center.y - size.y / 2, center.z - size.z / 2);
			Vector3 corner = localToWorldMatrix.MultiplyPoint3x4 (position);
			points.Add (corner);

			position = new Vector3 (center.x + size.x / 2, center.y - size.y / 2, center.z - size.z / 2);
			corner = localToWorldMatrix.MultiplyPoint3x4 (position);
			points.Add (corner);

			position = new Vector3 (center.x - size.x / 2, center.y - size.y / 2, center.z + size.z / 2);
			corner = localToWorldMatrix.MultiplyPoint3x4 (position);
			points.Add (corner);

			position = new Vector3 (center.x + size.x / 2, center.y - size.y / 2, center.z + size.z / 2);
			corner = localToWorldMatrix.MultiplyPoint3x4 (position);
			points.Add (corner);

			position = new Vector3 (center.x - size.x / 2, center.y + size.y / 2, center.z - size.z / 2);
			corner = localToWorldMatrix.MultiplyPoint3x4 (position);
			points.Add (corner);

			position = new Vector3 (center.x + size.x / 2, center.y + size.y / 2, center.z - size.z / 2);
			corner = localToWorldMatrix.MultiplyPoint3x4 (position);
			points.Add (corner);

			position = new Vector3 (center.x - size.x / 2, center.y + size.y / 2, center.z + size.z / 2);
			corner = localToWorldMatrix.MultiplyPoint3x4 (position);
			points.Add (corner);

			position = new Vector3 (center.x + size.x / 2, center.y + size.y / 2, center.z + size.z / 2);
			corner = localToWorldMatrix.MultiplyPoint3x4 (position);
			points.Add (corner);

		}

		Matrix4x4 rotation = Matrix4x4.Inverse(obj.transform.localToWorldMatrix);

		Vector3 min = new Vector3(float.NaN, float.NaN, float.NaN);
		Vector3 max = new Vector3(float.NaN, float.NaN, float.NaN);

		foreach (Vector3 point in points) {
			Vector3 position =  rotation.MultiplyPoint3x4 (point);

			min = new Vector3(
				(min.x != min.x || position.x < min.x) ? position.x : min.x,
				(min.y != min.y || position.y < min.y) ? position.y : min.y,
				(min.z != min.z || position.z < min.z) ? position.z : min.z
			);

			max = new Vector3(
				(max.x != max.x || position.x > max.x) ? position.x : max.x,
				(max.y != max.y || position.y > max.y) ? position.y : max.y,
				(max.z != max.z || position.z > max.z) ? position.z : max.z
			);
		}

		this.center = (max - min)/2 + min;
		this.size = max - min;

		this.obj = obj;
	}

	public void DrawBounds(){
		UnityEditor.Handles.color = new Color (1, 0, 0, 0.5f);
		UnityEditor.Handles.matrix = obj.transform.localToWorldMatrix;
		UnityEditor.Handles.DrawWireCube (center, size);
	}
}
