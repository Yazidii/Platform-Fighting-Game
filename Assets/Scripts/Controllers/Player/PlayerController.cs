using UnityEngine;
using System.Collections;

/*
 TO DO:
 - Code attack collider behaviour
 - Make game controller
 - Move quality settings to game controller
 - When dashing the moment sliding down or up, character stays in animation while dashing
 - When dashing and hitting a small object, get thrown upwards and suddenly stop, probably due to X velocity
*/


public class PlayerController : MonoBehaviour 
{

	// Player properties
	private float moveSpeed = 15.0f;
	private float acceleration = 1.75f; 
	private float turnConstant = 1.5f; // multiply by acceleration if turning
	private float jumpSpeed = 20.0f; 
	private float dashSpeed = 40f;
	private float dashInterval = 0.7f;
	private float unBufferDashTime = 0.5f; // Time until dash command is discarded
	private float wallJumpSpeed = 15.0f; // Velocity of player when jumping off a wall
	private float wallStickTime = 0.1f; 
	private float bulletSpeed = 200;
	
	//Player States
	private bool ascending = false;
	private bool descending = false;
	private bool running = false;
	
	//Aiming states
	private bool isAiming = false;
	private bool prevIsAiming = false;
	
	//Player Speed
	private float resultingVelocity; //Used to set horizontal velocity after controls are parsed
	private float resultingYVelocity; //Used to set vertical velocity after controls are parsed
	private float currentYVelocity; //Velocity when entering frame
	private float currentVelocity; //X Velocity when entering frame
	private float finalAcceleration; // used to set acceleration to be applied in the frame
	private int horizontalInput = 0;
	
	//Abort Jump variables
	private int abortJumpFrames = 5; //Number of frames until aborting jump
	private int framesPassed = 0;
	private bool abortJump = false;
	//private bool bufferJump = false;
	
	//Attacking
	private int bufferedAttacks = 0;
	private int currentAttack = 0; // Attack on entering frame
	private int previousAttack = 0; // Attack on previous frame
	private bool isAttacking = false;
	private float attackScaleX;

	//Dash/roll
	private bool dashBuffer = false;
	private bool isDashing = false;
	private bool canDash = true;
	
	//WallJumping
	private int wallDirection = 0; //Which side of the player is the wall on
	private bool onWall = false;
	private bool prevOnWall = false; //Used to determine the frame which the player touches a wall
	private bool stuckToWall = false;
	
	//Controlling
	private bool canControl = false; //Can the player be controllerd
	
	//Collider properties
	private float localScaleX; //Rigidbody hitbox size
	private Vector3 yFromCenter; //Generate bottom of player collider
	private Vector3 xFromCenter; //Generate edge of player collider
	private int facing = 1; //Player orientation at start of frame
	private int finalFacing = 1; //Player orientation after conditions are set
	//Raycast Size
	private float distToGround = 0.01f; // Distance from player bottom to ground to count grounded
	
	// Other objects
	//private GameObject groundedOn = null;
	
	// Player components
	private Transform graphics; //Graphics container
	private Animator animator;
	private Transform attackHitboxes;
	private PolygonCollider2D[] attackHitbox = new PolygonCollider2D[3];
	
	// Gun Controller
	private GunController gun; //Gun container
	
	// Use this for initialization
	void Start() 
	{
		//TO DO - Move over to game controller
		//Quality settings 
		Application.targetFrameRate = 60;
		QualitySettings.vSyncCount = 0;
		
		
				
		//Contained objects
		graphics = transform.GetChild(0);
		animator = graphics.GetComponent<Animator>();
		gun = transform.GetChild(1).GetComponent<GunController>();
		gun.BulletSpeed = bulletSpeed;
		
		attackHitboxes = transform.GetChild(3);
		//Get player width for raycasting purposes
		localScaleX = graphics.localScale.x;
		attackScaleX = attackHitboxes.localScale.x;
		
		//Do not allow player rigidbody to rotate
		transform.GetComponent<Rigidbody2D>().fixedAngle =  true;
		
		//Distance from center to edge of player collider(Not entirely accurate as player center is offset)
		yFromCenter = new Vector3(0, transform.GetComponent<Collider2D>().bounds.extents.y, 0);
		xFromCenter = new Vector3(transform.GetComponent<Collider2D>().bounds.extents.x, 0, 0);
		

		attackHitbox[0] = attackHitboxes.GetChild(0).GetComponent<PolygonCollider2D>();
		attackHitbox[1] = attackHitboxes.GetChild(1).GetComponent<PolygonCollider2D>();
		attackHitbox[2] = attackHitboxes.GetChild(2).GetComponent<PolygonCollider2D>();
		attackHitbox[0].enabled = false;
		attackHitbox[1].enabled = false;
		attackHitbox[2].enabled = false;
		
		//Initialization routine, useless at the moment
		StartCoroutine("Initialize");

	}
	
	
	/// <summary>
	/// Updates the physics step.
	/// </summary>
	void FixedUpdate() 
	{
		//Process physical properties based on input
		ProcessMovement();
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update() 
	{
		
		//Check dashing state
		if (canControl) {
		isDashing = animator.GetCurrentAnimatorStateInfo(0).IsName("Dash");
		canControl = !isDashing;
		}
		
		//First check whether the attack animation is happening
		ProcessAttackInfo();
		
		//Check if player is stuck on a wall
		ProcessWallState();
		
		//Process input from the player
		ProcessInput();
		
		//Set gun State
		SetGunState();
		
		//Set animation and physical states
		SetGeneralProperties();
		
		
		//Other
	}
	
	/// <summary>
	/// Processes the movement.
	/// </summary>
	void ProcessMovement() 
	{
		//Read current velocity and acceleration
		currentVelocity = transform.GetComponent<Rigidbody2D>().velocity.x;
		currentYVelocity = transform.GetComponent<Rigidbody2D>().velocity.y;
		
		//Initialize output velocity as current
		resultingVelocity = currentVelocity;
		resultingYVelocity = currentYVelocity;
		
		finalAcceleration = acceleration;
		
		//When holding horizontal input
		if (horizontalInput != 0) 
		{	
			if (stuckToWall && wallDirection == -horizontalInput)
				StartCoroutine(StickToWall());
			//If turning, multiply acceleration by constant
			if (currentVelocity * horizontalInput < 0)
				finalAcceleration = acceleration * turnConstant;
			//Accelerate or decelerate to movespeed
			if (currentVelocity * horizontalInput < moveSpeed)
				resultingVelocity = currentVelocity + finalAcceleration * horizontalInput;
			if (currentVelocity * horizontalInput > moveSpeed)
				resultingVelocity = currentVelocity - finalAcceleration * horizontalInput;
		}
		//When horizontal input is 0 decelerate at rate of acceleration
		else 
		{
			running = false;
			if (currentVelocity != 0 && (IsGrounded() || isDashing))
			{
				resultingVelocity = currentVelocity - (currentVelocity/Mathf.Abs(currentVelocity)) * acceleration;
				if (Mathf.Abs(resultingVelocity) <= acceleration) 
				{
					resultingVelocity = 0;
				}
			}
		}
		
		//Force player to stick to wall	
		if (stuckToWall) {
			resultingVelocity = 0;
		}
		
		//Abort jump routine
		if (abortJump) {
			framesPassed++;
		}
		
		if (abortJump && framesPassed >= abortJumpFrames) {
			if (ascending){	
				resultingYVelocity = 0;
			}
			abortJump = false;
		}
		
		//Set velocity on each physics step
		transform.GetComponent<Rigidbody2D>().velocity = new Vector2(resultingVelocity, resultingYVelocity);
	}
	
	/// <summary>
	/// Processes the attack info.
	/// </summary>
	void ProcessAttackInfo() 
	{
	
		attackHitbox[0].enabled = false;
		attackHitbox[1].enabled = false;
		attackHitbox[2].enabled = false;
		//Get previous attack
		previousAttack = currentAttack;
		
		//Set current attack
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
		{
			currentAttack = 1;
			isAttacking = true;
		}
		else
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
		{
			currentAttack = 2;
			isAttacking = true;
		}
		else
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack3"))
		{
			currentAttack = 3;
			isAttacking = true;
		}
		else {
			isAttacking = false;
			currentAttack = 0;
		}
		
		if (currentAttack != 0)
			attackHitbox[currentAttack - 1].enabled = true;
		
		//If attack changed, remove one buffer
		if (previousAttack != currentAttack && bufferedAttacks > 0) {
			bufferedAttacks--;
		}
		
		if (previousAttack != currentAttack) {
			finalFacing = facing;
		}
		
		attackHitboxes.localScale = new Vector2(facing * attackScaleX, attackHitboxes.localScale.y);
		attackHitboxes.position = new Vector2(transform.position.x + facing * Mathf.Abs(attackHitboxes.position.x - transform.position.x), attackHitboxes.position.y);
	}
	
	/// <summary>
	/// Processes the wall state.
	/// </summary>
	void ProcessWallState()
	{
	
		//Cast rays for ground/wall checks
		GroundRayCast();
		RightWallRayCast();
		LeftWallRayCast();
		prevOnWall = onWall;		
		
		//Check if character is touching a wall
		wallDirection = returnOnWall();
		if (wallDirection == 0){
			onWall = false;
		}
		else{
			onWall = true;
			abortJump = false;
		}
		//If just touched wall, stick character to wall
		if (!IsGrounded() && onWall && !prevOnWall){
			stuckToWall = true;
			abortJump = false;
			dashBuffer = false;
			bufferedAttacks = 0;
		} 
		
		if (!onWall && prevOnWall){
			stuckToWall = false;
			StopCoroutine(StickToWall());
		}
	}
	
	/// <summary>
	/// Processes the input.
	/// </summary>
	void ProcessInput()
	{	
		//Read current velocity and acceleration
		currentVelocity = transform.GetComponent<Rigidbody2D>().velocity.x;
		currentYVelocity = transform.GetComponent<Rigidbody2D>().velocity.y;
		resultingVelocity = currentVelocity;
		resultingYVelocity = currentYVelocity;

		//Character Controls 
		if (canControl) 
		{
			if (Input.GetAxis("Horizontal") != 0 && !isAttacking) 
			{
				//When holding move right button
				if (Input.GetAxis("Horizontal") > 0) 
				{
					horizontalInput = 1;
					facing = 1;
				}
				else 
				if (Input.GetAxis("Horizontal") < 0) 
				{
					horizontalInput = -1;
					facing = -1;
				}
				running = true;
			}
			else 
			{
				horizontalInput = 0;
				running = false;
			}

			//Jumping handler
			if (Input.GetButtonDown("Vertical") && !dashBuffer && !isAttacking) 
			{
				//Jump
				if (IsGrounded())
				{
					resultingYVelocity = currentYVelocity + jumpSpeed;
					if (resultingYVelocity > jumpSpeed) resultingYVelocity = jumpSpeed;
				}
				
				if (!IsGrounded() && onWall)
				{
					resultingYVelocity = jumpSpeed;
					resultingVelocity = wallJumpSpeed * -wallDirection;
					stuckToWall = false;
					facing = -wallDirection;
				}
			}

			//Abort Jump if user releases button
			if (ascending && Input.GetButtonUp("Vertical")) 
			{
				framesPassed = 0;
				abortJump = true;
			}
		}
		else 
		{
			horizontalInput = 0;
		}
		
		//Dash handler
		if (Input.GetButtonDown("Dash") && canDash) 
		{
			//Set Dash buffer
			dashBuffer = true;
			StartCoroutine("UnbufferDash");
		}
		
		//Attack buffer
		if (Input.GetButtonDown("Attack")) 
		{
			if (bufferedAttacks < 1) bufferedAttacks++;
		}
		
		//Aiming control
		prevIsAiming = isAiming;
		isAiming = false;
		if (IsGrounded() && !isDashing && !isAttacking)
		{
			if (Input.GetAxis("Aim") != 0 || Input.GetAxis("GunVertical") != 0 || Input.GetAxis("GunHorizontal") != 0)
			{
				isAiming = true;
			}
		}
			
	}
	
	/// <summary>
	/// Sets the state of the gun.
	/// </summary>
	void SetGunState()
	{
		gun.IsAiming = isAiming;
		gun.PrevIsAiming = prevIsAiming;
		gun.Facing = facing;
		gun.CanControl = canControl;
		gun.ProcessGunInfo();
		facing = gun.Facing;
		canControl = gun.CanControl;
		
	}

	/// <summary>
	/// Sets the general properties.
	/// </summary>
	void SetGeneralProperties()
	{
	
		//If player is not locked to face the side of the attack, he should face the direction he is pressing
		if (!isAttacking) 
		{
			finalFacing = facing;
		}
		
		//Face player outwards from the wall
		if (onWall && !IsGrounded())
			finalFacing = wallDirection;
		
		//Set air state
		if (transform.GetComponent<Rigidbody2D>().velocity.y > 0)
		{
			ascending = true;
			descending = false;
		}
		else if (transform.GetComponent<Rigidbody2D>().velocity.y < 0)
		{
			ascending = false;
			descending = true;
		} 
		else 
		{
			ascending = false;
			descending = false;
		}
		
		
		//Set sprite orientation
		graphics.localScale = new Vector2(localScaleX * finalFacing, graphics.localScale.y);
		
		//If can dash and has dashing buffer set
		//Only fires when grounded, not attacking, pressed dash button, and set time since last dash passed
		//
		if (IsGrounded() && !isAttacking && dashBuffer && canDash) 
		{
			dashBuffer = false;
			StartCoroutine("DashInterval");
			animator.SetTrigger("Dash");
			resultingVelocity = facing * dashSpeed;
			isDashing = true;
		}
		
		// Set animation state properties
		animator.SetBool("OnWall", onWall);
		animator.SetBool("Running", running);
		animator.SetBool("Ascending", ascending);
		animator.SetBool("Descending", descending);
		animator.SetBool("Grounded", IsGrounded());
		animator.SetBool("Dashing", isDashing);
		
		//Attack Controller
		animator.SetInteger("Attacks", bufferedAttacks);
		animator.SetBool("Attacking", isAttacking);
		//Set velocity
		//Only used for jumps and dash which should happen once per button press
		transform.GetComponent<Rigidbody2D>().velocity = new Vector2(resultingVelocity, resultingYVelocity);
	}
	
	/// <summary>
	/// Casts the ground rays.
	/// </summary>
	void GroundRayCast() 
	{
		//Raycasting
		//Ground collision
		Debug.DrawLine(transform.position - yFromCenter - new Vector3(-0.1f,0.09f,0), transform.position - yFromCenter - new Vector3(-0.1f,0.1f,0), Color.white);
		Debug.DrawLine(transform.position - yFromCenter + xFromCenter - new Vector3(-0.02f,0.09f,0), transform.position - yFromCenter + xFromCenter - new Vector3(-0.02f,0.1f,0), Color.white);
		Debug.DrawLine(transform.position - yFromCenter - xFromCenter - new Vector3(-0.17f,0.09f,0), transform.position - yFromCenter - xFromCenter - new Vector3(-0.17f,0.1f,0), Color.white);
	}
	
	void LeftWallRayCast() 
	{
		//Raycasting
		//Ground collision
		Debug.DrawLine(transform.position - xFromCenter - new Vector3(-0.07f,0,0), transform.position - xFromCenter - new Vector3(-0.06f,0,0), Color.white);
		//Debug.DrawLine(transform.position - yFromCenter + xFromCenter - new Vector3(-0.02f,0.09f,0), transform.position - yFromCenter + xFromCenter - new Vector3(-0.02f,0.1f,0), Color.white);
		//Debug.DrawLine(transform.position - yFromCenter - xFromCenter - new Vector3(-0.17f,0.09f,0), transform.position - yFromCenter - xFromCenter - new Vector3(-0.17f,0.1f,0), Color.white);
	}	
	
	void RightWallRayCast() 
	{
		//Raycasting
		//Ground collision
		Debug.DrawLine(transform.position + xFromCenter + new Vector3(0.08f,0,0), transform.position + xFromCenter + new Vector3(0.09f,0,0), Color.white);
		//Debug.DrawLine(transform.position - yFromCenter + xFromCenter - new Vector3(-0.02f,0.09f,0), transform.position - yFromCenter + xFromCenter - new Vector3(-0.02f,0.1f,0), Color.white);
		//Debug.DrawLine(transform.position - yFromCenter - xFromCenter - new Vector3(-0.17f,0.09f,0), transform.position - yFromCenter - xFromCenter - new Vector3(-0.17f,0.1f,0), Color.white);
	}
	
	int returnOnWall()
	{
		RaycastHit2D left = Physics2D.Raycast(transform.position - xFromCenter - new Vector3(-0.07f,0,0), -Vector2.right, distToGround);
		RaycastHit2D right = Physics2D.Raycast(transform.position + xFromCenter + new Vector3(0.09f,0,0), Vector2.right, distToGround);
		
		if (left.collider) 
		{
			return -1;
		}
		else 
			if (right.collider) 
			{
				return 1;
			}
			else return 0;
	
	}

	//Check if body is grounded
	bool IsGrounded() 
	{
		return(Physics2D.Raycast(transform.position - yFromCenter - new Vector3(-0.1f,0.09f,0), -Vector2.up, distToGround).collider || 
		        Physics2D.Raycast(transform.position - yFromCenter + xFromCenter - new Vector3(-0.05f,0.09f,0), -Vector2.up, distToGround).collider || 
		        Physics2D.Raycast(transform.position - yFromCenter - xFromCenter - new Vector3(-0.15f,0.09f,0), -Vector2.up, distToGround).collider);
	}


	//Coroutine section
	//
	//Wait for game init
	IEnumerator Initialize() 
	{
		canControl = false;
		yield return new WaitForSeconds(0.5f);
		canControl = true;
		yield return null;
	}
	
	//Wait for next dash
	IEnumerator DashInterval() 
	{
		canDash = false;
		yield return new WaitForSeconds(dashInterval);
		canDash = true;	
		yield return null;
	}
	
	IEnumerator UnbufferDash() 
	{
		yield return new WaitForSeconds(unBufferDashTime);
		dashBuffer = false;
		yield return null;
	}
	
	//Stick to wall
	IEnumerator StickToWall() 
	{
		yield return new WaitForSeconds(wallStickTime);
		yield return null;
		stuckToWall = false;
		yield return null;
	}
	

	//GetSet
	
}
