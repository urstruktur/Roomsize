using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary{

	public Vector3 _center;
	public Vector3 center{
		get{
			Matrix4x4 localToWorldMatrix = obj.transform.localToWorldMatrix;
			return localToWorldMatrix.MultiplyPoint3x4(_center);
			//return _center;
		}

		/*set{
			_center = value;
		}*/
	}
	public Vector3 size = Vector3.zero;

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

		this._center = (max - min)/2 + min;
		this.size = max - min;

		this.obj = obj;
	}

	public Vector3 Intersect(Vector3 origin, Vector3 direction){
		Vector3 point = Vector3.zero;
		Vector3 size = this.size;

		Vector3 A = Vector3.zero;
		Vector3 B = Vector3.zero;

		Matrix4x4 matrix = obj.transform.localToWorldMatrix;
		float radius = 0;

		if (size.x > size.y && size.x > size.z) {
			
			radius = size.y > size.z ? size.y / 2 : size.z / 2;

			A = matrix.MultiplyPoint3x4(_center + new Vector3(size.x / 2 - radius, 0 ,0));
			B = matrix.MultiplyPoint3x4(_center - new Vector3(size.x / 2 - radius, 0 ,0));
		}

		if (size.y > size.x && size.y > size.z) {
			radius = size.x > size.z ? size.x / 2 : size.z / 2;

			A = matrix.MultiplyPoint3x4(_center + new Vector3(0, size.y / 2 - radius, 0));
			B = matrix.MultiplyPoint3x4(_center - new Vector3(0, size.y / 2 - radius, 0));
		}

		if (size.z > size.x && size.z > size.y) {
			radius = size.x > size.y ? size.x / 2 : size.y / 2;

			A = matrix.MultiplyPoint3x4(_center + new Vector3(0, 0, size.z / 2 - radius));
			B = matrix.MultiplyPoint3x4(_center - new Vector3(0, 0, size.z / 2 - radius));
		}

		Debug.DrawLine (center, A, Color.magenta);
		Debug.DrawLine (center, B, Color.yellow);

		Vector3 intersect;
		Vector3 other;

		ClosestPointsOnTwoLines(out intersect, out other, A, (A-B).normalized, origin, direction);

		//Debug.Log ("#2:" + A + " " + B + " h:" + ((A-B).magnitude + radius * 2));

		bool collide = false;

		//Debug.Log (center + ":" + radius + " h:" + (A-B).magnitude);

		int area = PointOnWhichSideOfLineSegment( A, B, intersect);

		if ((intersect - other).magnitude < radius) {
			collide = true;

			LineCylinderIntersection(center, (A-center).normalized, radius, origin, direction);


			if (collide == true) {
				Debug.DrawLine (intersect, other, Color.red);
			} else {
				Debug.DrawLine (intersect, other, Color.green);
			}
		}

		if ( area == 1 || area == 2) {

			Vector3[] results = LineCircleIntersection(area == 1 ? A : B, radius, origin, direction);

			if (results.Length > 0) {
				return results [0];
			}
		}


			

		return point;

		/*Matrix4x4 matrix = obj.transform.localToWorldMatrix;

		Vector3 center = _center;
		Vector3 size = this.size;


		Vector3[] anchor = new Vector3[8];

		anchor[0] = matrix.MultiplyPoint3x4(new Vector3 (center.x - size.x / 2, center.y - size.y / 2, center.z - size.z / 2));

		anchor[1] = matrix.MultiplyPoint3x4(new Vector3 (center.x + size.x / 2, center.y - size.y / 2, center.z - size.z / 2));

		anchor[2] = matrix.MultiplyPoint3x4(new Vector3 (center.x - size.x / 2, center.y - size.y / 2, center.z + size.z / 2));

		anchor[3] = matrix.MultiplyPoint3x4(new Vector3 (center.x + size.x / 2, center.y - size.y / 2, center.z + size.z / 2));


		Vector3 intersect = Vector3.zero;


		Vector3 planeNormal = Vector3.zero;
		Vector3 planePoint = Vector3.zero;

		PlaneFrom3Points (out planeNormal, out planePoint, anchor [0], anchor [1], anchor [2]);
		LinePlaneIntersection (out intersect, origin, direction, planePoint, planeNormal);


		Matrix4x4 inverse = Matrix4x4.Inverse(matrix);
		Vector3 intersect_ = inverse.MultiplyPoint3x4 (intersect);

		if(false){

			Debug.DrawLine (anchor [0], anchor [1], Color.yellow);
			Debug.DrawLine (anchor [1], anchor [2], Color.yellow);
			Debug.DrawLine (anchor [2], anchor [0], Color.yellow);
		}
		else{
			Debug.DrawLine (anchor [0], anchor [1], Color.cyan);
			Debug.DrawLine (anchor [1], anchor [2], Color.cyan);
			Debug.DrawLine (anchor [2], anchor [0], Color.cyan);
		}


		Vector3 point = intersect;*/


		return point;
	}


	static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planePoint, Vector3 planeNormal){

		float length;
		float dotNumerator;
		float dotDenominator;
		Vector3 vector;
		intersection = Vector3.zero;

		//calculate the distance between the linePoint and the line-plane intersection point
		dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
		dotDenominator = Vector3.Dot(lineVec, planeNormal);

		//line and plane are not parallel
		if(dotDenominator != 0.0f){
			length =  dotNumerator / dotDenominator;

			//create a vector from the linePoint to the intersection point
			vector = lineVec.normalized * length;

			//get the coordinates of the line-plane intersection point
			intersection = linePoint + vector;	

			return true;	
		}

		//output not valid
		else{
			return false;
		}
	}

	static void PlaneFrom3Points(out Vector3 planeNormal, out Vector3 planePoint, Vector3 pointA, Vector3 pointB, Vector3 pointC){

		planeNormal = Vector3.zero;
		planePoint = Vector3.zero;

		//Make two vectors from the 3 input points, originating from point A
		Vector3 AB = pointB - pointA;
		Vector3 AC = pointC - pointA;

		//Calculate the normal
		planeNormal = Vector3.Normalize(Vector3.Cross(AB, AC));

		//Get the points in the middle AB and AC
		Vector3 middleAB = pointA + (AB / 2.0f);
		Vector3 middleAC = pointA + (AC / 2.0f);

		//Get vectors from the middle of AB and AC to the point which is not on that line.
		Vector3 middleABtoC = pointC - middleAB;
		Vector3 middleACtoB = pointB - middleAC;

		//Calculate the intersection between the two lines. This will be the center 
		//of the triangle defined by the 3 points.
		//We could use LineLineIntersection instead of ClosestPointsOnTwoLines but due to rounding errors 
		//this sometimes doesn't work.
		Vector3 temp;
		ClosestPointsOnTwoLines(out planePoint, out temp, middleAB, middleABtoC, middleAC, middleACtoB);
	}

	static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){

		closestPointLine1 = Vector3.zero;
		closestPointLine2 = Vector3.zero;

		float a = Vector3.Dot(lineVec1, lineVec1);
		float b = Vector3.Dot(lineVec1, lineVec2);
		float e = Vector3.Dot(lineVec2, lineVec2);

		float d = a*e - b*b;

		//lines are not parallel
		if(d != 0.0f){

			Vector3 r = linePoint1 - linePoint2;
			float c = Vector3.Dot(lineVec1, r);
			float f = Vector3.Dot(lineVec2, r);

			float s = (b*f - c*e) / d;
			float t = (a*f - c*b) / d;

			closestPointLine1 = linePoint1 + lineVec1 * s;
			closestPointLine2 = linePoint2 + lineVec2 * t;

			return true;
		}

		else{
			return false;
		}
	}

	public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point){

		Vector3 lineVec = linePoint2 - linePoint1;
		Vector3 pointVec = point - linePoint1;

		float dot = Vector3.Dot(pointVec, lineVec);

		//point is on side of linePoint2, compared to linePoint1
		if(dot > 0){
			//point is on the line segment
			if(pointVec.magnitude <= lineVec.magnitude){
				return 0;
			}
			//point is not on the line segment and it is on the side of linePoint2
			else{
				return 2;
			}
		}
		//Point is not on side of linePoint2, compared to linePoint1.
		//Point is not on the line segment and it is on the side of linePoint1.
		else{
			return 1;
		}
	}

	public Vector3[] LineCylinderIntersection(Vector3 origin, Vector3 normal, float radius, Vector3 point, Vector3 direction){

		//Debug.DrawLine (origin, origin + normal, Color.white);

		Vector3 A = ProjectPointOnPlane(origin, normal, point);
		Vector3 dir = (A - ProjectPointOnPlane(origin, normal, point + direction));

		Debug.DrawLine (A, A + dir*12, Color.cyan);

		Vector3[] intersections = LineCircleIntersection(origin, radius, A, dir);

		if (intersections.Length == 1) {
			Debug.DrawLine (intersections[0], Vector3.zero, Color.blue);
		}
		if (intersections.Length == 2) {
			Debug.DrawLine (intersections[1], Vector3.zero, Color.blue);
		}


		//Debug.DrawLine (A, dir, Color.red);

		return new Vector3[0];
	}

	public Vector3[] LineCircleIntersection(Vector3 origin, float radius, Vector3 point, Vector3 direction){
		
		Vector3 centerToRayStart = point - origin;

		float a = Vector3.Dot(direction, direction);
		float b = 2 * Vector3.Dot(centerToRayStart, direction);
		float c = Vector3.Dot(centerToRayStart, centerToRayStart) - (radius * radius);

		float discriminant = (b * b) - (4 * a * c);
		if (discriminant >= 0)
		{
			//Ray did not miss
			discriminant = Mathf.Sqrt(discriminant);

			//How far on ray the intersections happen
			float t1 = (-b - discriminant) / (2 * a);
			float t2 = (-b + discriminant) / (2 * a);

			Vector3[] hitPoints = new Vector3[0];

			// && t1 <= 1  && t2 <= 1
			if (t1 >= 0  && t2 >= 0 ){
				//total intersection, return both points
				hitPoints = new Vector3[2];
				hitPoints[0] = point + (direction * t1);
				hitPoints[1] = point + (direction * t2);
			}
			else{
				//Only one intersected, return one point
				if (t1 >= 0)// && t1 <= 1)
				{
					hitPoints = new Vector3[1];
					hitPoints[0] = point + (direction * t1);
				}
				else if (t2 >= 0)// && t2 <= 1)
				{
					hitPoints = new Vector3[1];
					hitPoints[0] = point + (direction * t2);
				}
			}
			return hitPoints;
		}
		//No hits
		return new Vector3[0];
	}

	public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point){		

		//get vector from point on line to point in space
		Vector3 linePointToPoint = point - linePoint;

		float t = Vector3.Dot(linePointToPoint, lineVec);

		return linePoint + lineVec * t;
	}

	public static Vector3 ProjectPointOnPlane(Vector3 planePoint, Vector3 planeNormal, Vector3 point){

		float distance;
		Vector3 translationVector;

		//First calculate the distance from the point to the plane:
		distance = Vector3.Dot(planeNormal, (point - planePoint));

		//Reverse the sign of the distance
		distance *= -1;

		//Get a translation vector
		translationVector = planeNormal.normalized * distance;

		//Translate the point to form a projection
		return point + translationVector;
	}


	public void DrawBounds(){
		//UnityEditor.Handles.color = new Color (1, 0, 0, 0.5f);
		//UnityEditor.Handles.matrix = obj.transform.localToWorldMatrix;
		//UnityEditor.Handles.DrawWireCube (_center, size);

		//UnityEditor.Handles.color = new Color (1, 1, 1, 1f);
		//UnityEditor.Handles.DrawWireCube (_center, Vector3.one * 0.1f);
	}
}
