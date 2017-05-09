using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicSim : MonoBehaviour {

	//Force (default) 	|   Accelerate an object over 1 time step, based on its mass.
	//					|   Units: Newtons = kg * m/s^2
	//					|
	//Acceleration  	|   Accelerate an object over 1 time step, ignoring its mass. (like gravity)
	//					|   Units: m/s^2
	//					|
	//Impulse         	|   Instantaneously propel an object, based on its mass
	//					|   Units: N * s = kg * m/s
	//					|
	//VelocityChange 	|   Instantaneously propel an object, ignoring its mass
	//					|   (like body.velocity = foo, except multiple scripts can stack)
	//					|   Units:  m/s


	Rigidbody rigid;

	// Use this for initialization
	void Awake () {
		rigid = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		Vector3 gravitation = new Vector3(0,-1, 0) * 9.81f;

		AddForce(new Vector3 (0, -1, 0), 9.81f, rigid.mass);

		//rigid.velocity += 
		//rigid.AddForce (gravitation * rigid.mass, ForceMode.Force);

		//rigid.velocity += rigid.mass * grav * Mathf.Pow(Time.fixedDeltaTime, 1);
		//rigid.velocity += Vector3.forward * 20 * Mathf.Pow(Time.fixedDeltaTime, 1);
	}

	/*void AddForce(Vector3 dir, float force){
		float  acceleration =  force / rigid.mass ;
		rigid.velocity += dir * acceleration * Mathf.Pow(Time.fixedDeltaTime, 1);
	}*/
		
	Vector3 GetForce(Vector3 dir, float force, float mass = 1){
		float acceleration = force / mass;
		return dir * acceleration * Time.fixedDeltaTime;
	}

	void AddForce(Vector3 dir, float force, float mass = 1){
		rigid.velocity += GetForce (dir, force, mass);
	}

}