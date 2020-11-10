    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Temp Variables
    #endregion

    #region Declaring Variables


    //Tracks the state the player is in in terms of their movement options
    public State state;
    //Tracks the weapon the player is using
    public Weapon weapon;

    // Variables that keep track of what buttons the player is pressing
    Vector2 moveInput;
    bool specialInput;

    //Handles the special moves
    private PlayerSpecials playerSpecials;
    //Handles the attacks
    private PlayerAttacks playerAttacks;


    //Character's base move speed
    [SerializeField] float moveSpeed = 10;

    //Combined these variables create a total dash length
    [SerializeField] float dashSpeed = 40;
    [SerializeField] float dashDuration = 0.5f;

    //Character's aerial movespeed
    [SerializeField] float driftSpeed = 4;

    //Character's jump heights
    [SerializeField] float jumpHeight = 10;
    [SerializeField] float doubleJumpHeight = 6;

    //Character's base jump deceleration
    [SerializeField] float gravity = 14;
    //combines with gravity creates character's fall speed
    float fallMultiplier = 2;

    //amount of time the character can wallrun for
    [SerializeField] float wallrunDuration = 1;
    //Time the character spends kicking off a wall during walljump
    [SerializeField] float wallJumpDuration = 0.5f;

    // Number of times the player can airdash without touching the ground
    [SerializeField] float airdashLimit = 2;

    //Number of times the player has airdashed currently
    [SerializeField] float airdashCount = 0;


    //controls class manages when a player is pressing a button / key
    PlayerControls controls;

    //this will manage the player's movement
    Rigidbody rb;

    //handles player collisions
    Collider coll;

    //object that triggers wallrunning
    [SerializeField] GameObject wallCheck;

    //camera object
    [SerializeField] GameObject cameraFocus;
    //camera transform
    Transform cameraTransform;

    //Tracks the result of the most recent isGrounded() check, should only be used if a check has already been performed this frame
    [SerializeField] bool isCurrentlyGrounded;
    //Determines if the player is allowed to double jump or not
    bool candoubleJump;
    //Determines if the player is currently in contact with a climbable wall
    bool inContactWithWall;
    //Tracks whether or not the player can wallrun
    private bool canWallRun = true;


    //How a character's upward velocity is expected to change
    Vector3 verticalVelocity;
    //How a character's forward velocity is expected to change (forward being defined as the direction the camera is facing)
    Vector3 forwardVelocity;
    //How a character's right velocity is expected to change (right being defined as the 90 degrees from the direction the camera is facing)
    Vector3 rightVelocity;
    //the velocity the player is expected to have next frame
    [SerializeField] Vector3 velocity;
    //The velocity that the player had in the previous frame
    Vector3 previousVelocity;

    #endregion


    // Called before Start
    void Awake()
    {
        //create a playerControls object with which to bind inputs
        controls = new PlayerControls();

        //handles the character's movement and interactions in physical space
        rb = GetComponent<Rigidbody>();

        //Handles the character's collisions with terrain and hit boxes
        coll = GetComponent<Collider>();

        //get location of the camera focus, this may be redundant if we go the fixed camera route
        cameraTransform = cameraFocus.GetComponent<Transform>();

        //Get the specific player movement and attacks class attached to the current player.
        playerAttacks = GetComponent<PlayerAttacks>();
        playerSpecials = GetComponent<PlayerSpecials>();

        #region Input Bindings

        //Bind walk input to a vector between (1,1) and (-1,-1)
        controls.PlayerMovement.Walk.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        //Cancel walk input by returning vector to (0,0)
        controls.PlayerMovement.Walk.canceled += ctx => moveInput = Vector2.zero;

        //Tracks when the player presses the attack button
        controls.PlayerMovement.Attack.performed += ctx => AttackInput();

        //Tracks whether the player is pressing the special button or not
        controls.PlayerMovement.Special.performed += ctx => SpecialInput();
        controls.PlayerMovement.Special.canceled += ctx => CancelSpecialInput();

        //Tracks when the player is pressing the dash button
        controls.PlayerMovement.Dash.started += ctx => DashInput();

        //Tracks when the player is presses the jump button
        controls.PlayerMovement.Jump.performed += ctx => JumpInput();
        #endregion

    }

    // Start is called before the first frame update
    void Start()
    {
        //sets the player to be free to move by default
        state = State.Free;

        //sets the weapon to SnS by default
        weapon = Weapon.SnS;
    }

    // Update is called once per frame
    void Update()
    {
        //Move player according to inputs and physics
        MovePlayer();
    }


    //handles a character's movement
    void MovePlayer()
    {

        //If not stunned apply the player's input to the character
        if (state == State.Free)
        {


            //If the player is grounded change the player's facing
            if (isGrounded())
            {
                //Grab Movement inputs an store them in the forwardMovement and rightMovement variables
                MovementInput();
                //Look where you're going
                ChangeFacing(velocity);
            }
            //otherwise the player is airborne, save their previous velocity and manipulate that to allow for momentum maybe I should do this math in the final case section and add a grounded and not grounded switch
            //else
            //{
            //    previousVelocity = velocity;
            //}


        }

        // apply gravity, the gravity is stronger as a player falls
        if (state != State.Wallrun)
        {
            if (!isCurrentlyGrounded)
            {
                if (verticalVelocity.y < 0) verticalVelocity.y -= gravity * fallMultiplier * Time.deltaTime;
                else verticalVelocity.y -= gravity * Time.deltaTime;
            }
            else if (verticalVelocity.y < 0) verticalVelocity.y = 0;
        }

        //clamp the forward and right movement together so that holding diagonal doesn't move faster than holding straight
        Vector3 clampedVelocity = Vector3.ClampMagnitude(forwardVelocity + rightVelocity, 1);


        //add all inputs together and move based on state and whether or not the player is grounded
        switch (state)
        {
            case State.Free:
            case State.Walljump:
                velocity = (clampedVelocity * moveSpeed) + verticalVelocity;
                break;

            case State.Blocking:
            case State.Attacking:
                velocity = verticalVelocity;
                break;

            case State.Dashing:
                velocity = (clampedVelocity * dashSpeed) + verticalVelocity;
                break;

            case State.Wallrun:
                //convert all movement inputs to upwards movement, in future I should mess with this to allow for more control
                velocity.y = (Mathf.Abs(clampedVelocity.x + clampedVelocity.z)) * moveSpeed;
                break;

            default:
                Debug.Log("Player's current state has no movement implementation");
                break;
        }

        //actually move the character based on the values above
        rb.velocity = velocity;
    }

    #region Input Handling

    //Gets movement inputs and converts them to relative values based on camera rotation, unlike other input commands this is called when a command needs a direction not as the input is registered.
    void MovementInput()
    {
        //create vectors for the forward and right movement inputs of the player
        forwardVelocity = cameraTransform.forward * moveInput.y;
        rightVelocity = cameraTransform.right * moveInput.x;

        //Remove vertical component of the character movement
        rightVelocity.y = 0;
        forwardVelocity.y = 0;
    }

    //handles any part of an attack input that needs to work within playerMovement, might prove unnecessary
    void AttackInput()
    {
        playerAttacks.Attack();
    }

    //this will handle any part of the special input the player movement script might need, it may ultimately prove unnecessary
    void SpecialInput()
    {
        playerSpecials.Special();
    }

    //This will handle any part of canceling the special input required in playerMovement, it may ultimately prove unnecessary
    void CancelSpecialInput()
    {
        playerSpecials.Cancel();
    }

    //Called whenever the dash button is pressed
    void DashInput()
    {
        Debug.Log("dash input recognized");

        if (state == State.Free && isGrounded())
        {
            //start Dash
            StartCoroutine(Dash());
        }
        else if ((state == State.Free || state == State.Dashing) && airdashCount < airdashLimit)
        {
            //Start Airdash
            StartCoroutine(Airdash());
        }


    }

    void JumpInput()
    {
        

        //if able and willing, jump! could add dash to the options here to allow for a dash jump
        if (isGrounded())
        {
            Jump();
        }
        else if (state == State.Wallrun)
        {
            StartCoroutine(WallJump());
        }
        else if (candoubleJump)
        {
            DoubleJump();
        }
    }

    #endregion

    #region Action Coroutines

    IEnumerator WallRun()
    {
        Debug.Log("Commencing Wallrun");
        // counts how long the player has been attached to the wall
        float wallTime = 0;
        state = State.Wallrun;

        //so long as the run time is not expired and the player is still in the wallrun state
        while (wallTime < wallrunDuration && state == State.Wallrun && !isGrounded() && inContactWithWall)
        {
            //find our heading
            MovementInput();

            wallTime += Time.deltaTime;
            yield return null;
        }

        // finish wall run, if the player is still considered to be wallrunning swap the m back to free, needs to be an if statement to prevent the player from being reassigned to free if they are stunned out of a wallrun
        if (state == State.Wallrun)
        {
            state = State.Free;

            Debug.Log("Wallrun time expired");

            Hold();
        }
    }

    IEnumerator Dash()
    {
        //counts how long a dash has lasted
        float dashTime = 0;
        //let the rest of the class know the character is dashing
        state = State.Dashing;

        //find our heading
        MovementInput();


        Debug.Log("Starting to Dash");

        

        //continue to dash until the dash time ends of the player's state changes
        while (dashTime < dashDuration && state == State.Dashing)
        {
            ChangeFacing(velocity);
            dashTime += Time.deltaTime;
            yield return null;
        }

        //continue to dash if the player leaves the ground while dashing
        while (!isGrounded() && state == State.Dashing)
        {
            yield return null;
        }


        //finish dashing, if the player is still considered to be dashing swap them back to free, it needs the if statement to prevent the player from being reassigned to free if the dash is interrupted by hitstun or a special
        if (state == State.Dashing)
        {
            state = State.Free;

            Debug.Log("Dash Complete");

            Hold();
        }

    }

    //Currently a clone of dash, needs to be reworked to redirect player.
    IEnumerator Airdash()
    {
        //add one to the airdash count
        airdashCount += 1;
        //track how long a dash has lasted
        float dashTime = 0;
        //let the rest of the class know the character is dashing
        state = State.Dashing;

        //find our heading
        MovementInput();

        Debug.Log("Starting to Dash");



        //continue to dash until the dash time ends of the player's state changes
        while (dashTime < dashDuration && state == State.Dashing)
        {
            ChangeFacing(velocity);
            dashTime += Time.deltaTime;
            yield return null;       
            //Hold for dash duration gravity
            verticalVelocity.y = 0;
        }

        //continue to dash until the player is grounded
        while (!isGrounded() && state == State.Dashing)
        {
            yield return null;
        }


        //finish dashing, if the player is still considered to be dashing swap them back to free, it needs the if statement to prevent the player from being reassigned to free if the dash is interrupted by hitstun or a special
        if (state == State.Dashing)
        {
            state = State.Free;

            Debug.Log("Dash Complete");

            Hold();
        }

    }



    IEnumerator WallJump()
    {
        // push off from the wall by sending force in the opposite direction of the wall (this could potentially be shortcut by simply sending the player backwards, I don't know how often a player will be wallrunning up something while not facing it or if that's even a good idea.) also go up too.
        Debug.Log("attempting wall push off");
        state = State.Free;

        //Face away from the wall
        ChangeFacing(-velocity);

        //Launch up
        verticalVelocity.y = jumpHeight;
        // and forwards
        forwardVelocity = transform.forward / 2;

        // allow the user to wallrun again
        canWallRun = true;

        //start a clock for the walljump 
        float wallJumpTime = 0;

        //set state to Walljump
        state = State.Walljump; 

        //if the state hasn't been changed and the time  isn't up
        while (wallJumpTime < wallJumpDuration && state == State.Walljump)
        {
            //Tick timer
            wallJumpTime += Time.deltaTime;
            //wait for next frame
            yield return null;
        }

        //if still wall jumping after the timer ends 
        if(state == State.Walljump)
        {
            //set the state back to free
            state = State.Free;

            Debug.Log("Walljump Complete");

            Hold();
        }

    }

    //stop a player from being able to input movement
    public void Hold()
    {
        velocity.x = 0;
        velocity.z = 0;
    }
    
    void Jump()
    {
        switch (state)
        {
            case State.Free:
            case State.Dashing:
                verticalVelocity.y = jumpHeight;
                Debug.Log("attempting to jump");
                break;

            default:
                break;
        }
    }

    void DoubleJump()
    {
        switch (state)
        {
            case State.Free:
                verticalVelocity.y = doubleJumpHeight;
                candoubleJump = false;
                Debug.Log("attempting to double jump");
                break;

            default:
                break;
        }
    }


    #endregion

    #region Player state trackers

    //enum keeps track of what the player's state is so that it can shift in and out
    [SerializeField] public enum State { Free, Stunned, Dashing, Rooted, Attacking, Dodging, Blocking, Wallrun, Walljump }
    [SerializeField] public enum Weapon { SnS, GreatScythe, TwinScythe }

    //checks if the player is grounded using colliders
    bool isGrounded()
    {
        //start a raycast from the player position, aim it down, send it as far as the the of the character (with a slight margin of error)
        if (Physics.Raycast(transform.position, Vector3.down, coll.bounds.extents.y + 0.1f))
        {
            isCurrentlyGrounded = true;
            candoubleJump = true;
            canWallRun = true;
            airdashCount = 0;
            return true;
        }
        isCurrentlyGrounded = false;
        return false;
    }


    void ChangeFacing(Vector3 facing)
    {
        facing.y = 0;
        
        if (facing != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(facing, Vector3.up);
        }
    }

    #endregion

    #region Collisions

    // if a collision is detected and the player is airborn change the state to Wallrun, this SHOULD us the wallcheck collider but if not I'll have to look into how to specify, this is also likely to cause a ton of issues with state tracking so it'll need a switch statement to determine it's priority over other things like stunned
    void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag == "Terrain")
        {
            inContactWithWall = true;
            //if the player isn't wallrunning but can
            if (!isGrounded() && canWallRun)
            {
                canWallRun = false;
                Debug.Log("wall check successful");
                StartCoroutine(WallRun());
            }

        }
    }

    // 
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Terrain")
        {
            inContactWithWall = false;
        }

    }


    #endregion

    #region enable and disable character

    private void OnEnable()
    {
        controls.PlayerMovement.Enable();
    }

    private void OnDisable()
    {
        controls.PlayerMovement.Disable();
    }

    #endregion

}
