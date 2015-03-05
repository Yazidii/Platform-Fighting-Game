using UnityEngine;
using System.Collections;

public class BulletCollision : MonoBehaviour {
	

	void Awake ()
	{
	
		transform.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
	
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{

	}
	
	void OnTriggerEnter2D (Collider2D other)
	{	
		Debug.Log ("Bullet " + transform.name + " hits " + other.name);
		
		if (other.tag == "Enemy")
		{
			Destroy(other.gameObject);
		}
	}
}
