using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Rigidbody))]
public class Dragbody : MonoBehaviour {

	public AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

	public Size size;
	public enum Size{
		normal,
		tiny,
		large
	}


	Rigidbody _rigid;
	public Rigidbody rigid{
		get {
			if (_rigid == null)
				_rigid = GetComponent<Rigidbody> ();
			return _rigid;
		}

	}

	Vector3 _center;
	public Vector3 center{
		get{
			if (_center == null)
				Calculate ();

			Vector3 output = transform.localToWorldMatrix.MultiplyPoint3x4(_center);
			return output;
		}
	}

	Vector3 _size;
	/*public Vector3 size{
		get{
			if (_size == null)
				Calculate ();

			Matrix4x4 matrix = transform.localToWorldMatrix;

			Vector3 output = new Vector3 (_size.x * matrix [0, 0], _size.y * matrix [1, 1], _size.z * matrix [2, 2]);//transform.localToWorldMatrix.MultiplyPoint3x4(_center);
			return output;
		}
	}*/

	public void Update(){
		Calculate ();
	}

	public void Calculate(){

		MeshFilter[] filters = gameObject.GetComponentsInChildren<MeshFilter> ();
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

		Matrix4x4 inverse = Matrix4x4.Inverse(transform.localToWorldMatrix);

		Vector3 min = new Vector3(float.NaN, float.NaN, float.NaN);
		Vector3 max = new Vector3(float.NaN, float.NaN, float.NaN);

		foreach (Vector3 point in points) {
			Vector3 position =  inverse.MultiplyPoint3x4 (point);

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

		//transform.localToWorldMatrix.MultiplyPoint3x4( 
		_center = (max - min)/2 + min;
		_size = max - min;
	}



	#if UNITY_EDITOR
	void OnDrawGizmos(){
		Matrix4x4 inverse = Matrix4x4.Inverse(transform.localToWorldMatrix);

		//Vector3 center = inverse.MultiplyPoint3x4 (this.center);
		//Vector3 size = _size;

		UnityEditor.Handles.color = new Color (1, 0, 0, 0.5f);
		UnityEditor.Handles.matrix = transform.localToWorldMatrix;
		UnityEditor.Handles.DrawWireCube (_center, _size);

		UnityEditor.Handles.color = new Color (1, 1, 1, 1f);
		UnityEditor.Handles.DrawWireCube (_center, Vector3.one * 0.1f);
	}
	#endif


}
