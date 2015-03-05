using UnityEngine;
using System.Collections;

public class Enemy_Type1 : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		StartCoroutine ("MoveRight");	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
	
	IEnumerator MoveRight() 
	{
		transform.GetComponent<Rigidbody2D>().velocity = new Vector2(5, 0);
		yield return new WaitForSeconds(1);
		StartCoroutine("MoveLeft");
		yield return null;
	}
	
	IEnumerator MoveLeft ()
	{
		transform.GetComponent<Rigidbody2D>().velocity = new Vector2(-5, 0);
		yield return new WaitForSeconds(1);
		StartCoroutine("MoveRight");
		yield return null;
	}
}
