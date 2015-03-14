using UnityEngine;
using System.Collections;

public class BulletCollision : MonoBehaviour {
	
	public float bulletLifeTime;
	
	void Awake ()
	{
		transform.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		bulletLifeTime = GameObject.Find("Gun").GetComponent<GunController>().BulletLifeTime;
		StartCoroutine ("FiredBulletEvent");
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
	
	IEnumerator FiredBulletEvent()
	{
		yield return new WaitForSeconds(bulletLifeTime);
		Destroy(this.gameObject);
		yield return null;
	}
}
