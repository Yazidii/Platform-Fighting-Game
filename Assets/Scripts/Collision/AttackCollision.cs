using UnityEngine;
using System.Collections;

public class AttackCollision : MonoBehaviour {
	
	private bool canHit = true;
	private bool isEnabled = false;
	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		isEnabled = transform.GetComponent<PolygonCollider2D>().enabled;
		if (isEnabled == false) 
		{
			canHit = true;
		}
	}
	
	void OnTriggerEnter2D (Collider2D other)
	{	
		if (canHit)
		{
			if (other.tag == "Enemy")
			{
				canHit = false;
				string attackNumber = transform.tag;
				Debug.Log ("Attack " + attackNumber + " hits " + other.name);
				Destroy(other.gameObject);
			}
		}
	}
}
