using UnityEngine;
using System.Collections;

public class GunController : MonoBehaviour {

	private float gunCountdown = 3.0f;
	private Transform[] gunCounter = new Transform[3]; 
	public Transform bullet;
	private Transform firedBullet;
	private SpriteRenderer gunSprite; 
	
	private float gunLocalScale;
	private float bulletSpeed = 200;
	//Aiming states
	private bool isAiming = false;
	private bool prevIsAiming = false;
	
	private int facing;
	private bool canControl;
	private float bulletLifeTime = 3.0f;
	
	

	// Use this for initialization
	void Start () {
		
		//Gun properties
		gunSprite = transform.GetComponent<SpriteRenderer>();
		gunLocalScale = transform.localScale.y;
		gunCounter[0] = transform.parent.Find("GunCounters").GetChild(0);
		gunCounter[1] = transform.parent.Find("GunCounters").GetChild(1);
		gunCounter[2] = transform.parent.Find("GunCounters").GetChild(2);
		gunCounter[0].GetComponent<Renderer>().enabled = false;
		gunCounter[1].GetComponent<Renderer>().enabled = false;
		gunCounter[2].GetComponent<Renderer>().enabled = false;
		
		StartCoroutine("GunCountdown");
	
	}
	
	/// <summary>
	/// Processes the gun info.
	/// </summary>
	public void ProcessGunInfo()
	{	
		gunSprite.GetComponent<Renderer>().enabled = false;
		transform.GetComponentInChildren<LineRenderer> ().enabled = false;
		float angle = GunOrientation();
		Vector3 targetRotation = new Vector3 (0, 0, angle);
		
		if (isAiming)
		{	
			
			if (prevIsAiming == false) 
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetRotation), 1);
			}
			
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetRotation), 5 * Time.fixedDeltaTime);
			PointLaser();
			
			
			canControl = false;
			gunSprite.GetComponent<Renderer>().enabled = true;
			
			if (Input.GetButtonDown("Fire") && gunCounter[2].GetComponent<Renderer>().enabled == true)
			{	
				for (int i = 0; i <= 2; i++)
					gunCounter[i].GetComponent<Renderer>().enabled = false;
				StartCoroutine("GunCountdown");
				FireGun();
			}
			
		}
		else
		{
			canControl = true;
		}
	}
	
	float GunOrientation()
	{	
		float angle;
		float cosGun = Input.GetAxis("GunHorizontal");
		float sinGun = -Input.GetAxis("GunVertical");
		Vector2 playerScreenPos;
		
		if (cosGun == 0 && sinGun == 0)
		{
			playerScreenPos = Camera.main.WorldToScreenPoint(new Vector2 (transform.position.x, transform.position.y));
			cosGun = Input.mousePosition.x - playerScreenPos.x;
			sinGun = Input.mousePosition.y - playerScreenPos.y;
		}
		
		angle = Mathf.Rad2Deg * Mathf.Atan2 (sinGun, cosGun);
		
		if (angle < 0) angle = 360 + angle;
		if (isAiming)
			if (angle > 90 && angle < 270) 
		{
			transform.localScale = new Vector2(transform.localScale.x , -gunLocalScale);
			facing = -1;
		}
		else 
		{
			transform.localScale = new Vector2(transform.localScale.x , gunLocalScale);
			facing = 1;
		}
		
		return angle;
		
	}
	
	void FireGun()
	{
		firedBullet = Instantiate(bullet, transform.position + transform.TransformDirection (Vector3.right) * 0.5f , transform.rotation) as Transform;
		
		// Give the cloned object an initial velocity along the current
		// object's Z axis
		firedBullet.GetComponent<Rigidbody2D>().velocity = transform.TransformDirection (Vector3.right * bulletSpeed);
	}
	
	void PointLaser()
	{
		// Plane.Raycast stores the distance from ray.origin to the hit point in this variable:
		//float distance = 0;
		Vector2 endPoint = new Vector2(transform.position.x + transform.TransformDirection (Vector3.right).x * 50f, transform.position.y + transform.TransformDirection (Vector3.right).y * 50f);
		// if the ray hits the plane...
		
		RaycastHit2D hit = Physics2D.Raycast(transform.position + transform.TransformDirection (Vector3.right) * 1.80f, transform.TransformDirection (Vector3.right));
		if (hit.collider != null) {
			//distance = Mathf.Abs(hit.point.y - transform.position.y);
			endPoint = hit.point;
		}
		
		//Physics2D.Raycast (new Vector2 (transform.position.x, transform.position.y), gunAngle, distance);
		//Debug.DrawLine(transform.position + transform.TransformDirection (Vector3.right) * 1.75f, endPoint, Color.white);
		//Debug.Log (distance);
		
		transform.GetComponentInChildren<LineRenderer> ().SetPosition(0, transform.position + transform.TransformDirection (Vector3.right) * 1.75f);
		transform.GetComponentInChildren<LineRenderer> ().SetPosition(1, endPoint);
		if (gunCounter[2].GetComponent<Renderer>().enabled)
			transform.GetComponentInChildren<LineRenderer> ().enabled = true;
		// get the hit point:
		
		//Debug.Log(distance);
	}
	
	//Reload gun
	IEnumerator GunCountdown()
	{
		yield return new WaitForSeconds(gunCountdown/3);
		gunCounter[0].GetComponent<Renderer>().enabled = true;
		yield return new WaitForSeconds(gunCountdown/3);
		gunCounter[1].GetComponent<Renderer>().enabled = true;
		yield return new WaitForSeconds(gunCountdown/3);
		gunCounter[2].GetComponent<Renderer>().enabled = true;
		yield return null;
	}
	

	
	//GETSET
	//Properties
	public float BulletSpeed
	{
		get
		{
			return bulletSpeed;
		}
		set
		{
			bulletSpeed = value;
		}
	}
	public bool IsAiming
	{
		get
		{
			return isAiming;
		}
		set
		{
			isAiming = value;
		}
	}
	public bool PrevIsAiming
	{
		get
		{
			return prevIsAiming;
		}
		set
		{
			prevIsAiming = value;
		}
	}
	public int Facing
	{
		get
		{
			return facing;
		}
		set
		{
			facing = value;
		}
	}
	public bool CanControl
	{
		get
		{
			return canControl;
		}
		set
		{
			canControl = value;
		}
	}
	public float BulletLifeTime
	{
		get
		{
			return bulletLifeTime;
		}
		set
		{
			bulletLifeTime = value;
		}
	}
}
