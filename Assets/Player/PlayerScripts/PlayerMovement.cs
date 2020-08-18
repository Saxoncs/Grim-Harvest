    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
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

    //Combined these variables create a total dash length
    float dashSpeed = 40;
    float dashDuration = 0.5f;

    //Character's base move speed
    float moveSpeed = 10;

    //Character's base jump height
    float jumpHeight = 10;

    //Character's base jump deceleration
    float gravity = 14;

    //combines with gravity creates character's fall speed
    float fallMultiplier = 2;

    //controls class manages when a player is pressing a button / key
    PlayerControls controls;

    //this will manage the player's movement
    Rigidbody rb;

    //handles player collisions
    Collider coll;

    //distance the player is going to move next frame
    [SerializeField] Vector3 movement;

    //camera objects
    [SerializeField] GameObject cameraFocus;
    Transform cameraTransform;

    Vector3 verticalMovement;
    Vector3 forwardMovement;
    Vector3 rightMovement;

    #endregion


    // Called before Start
    void Awake()
    {
        //create a playerControls object with which to bind inputs
        controls = new PlayerControls();

        //handles the character's movement and interactions in physical space
        rb = GetComponent<Rigidbody>();

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
        controls.PlayerMovement.Dash.performed += ctx => DashInput();

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
        MovePlayer();
    }


    //handles a character's movement
    void MovePlayer()
    {

        //If not stunned apply the player's input
        if (state == State.Free && isGrounded())
        {

            MovementInput();
            //Look where you're going
            Vector3 facing = movement;
            facing.y = 0;

            //I don't 100% know why but I need the vector3.up there to prevent the character from looking into the sky or ground and ensure they face forward
            if (facing != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(facing, Vector3.up);
            }
            


        }


        //// apply gravity, the gravity is stronger as a player falls
        if (!isGrounded())
        {
            if (verticalMovement.y < 0) verticalMovement.y -= gravity * fallMultiplier * Time.deltaTime;
            else verticalMovement.y -= gravity * Time.deltaTime;
        }
        else if (verticalMovement.y < 0) verticalMovement.y = 0;


        //add all inputs together and move based on state
        switch (state)
        {
            case State.Free:
                movement = (Vector3.ClampMagnitude(forwardMovement + rightMovement, 1) * moveSpeed) + verticalMovement;
                break;

            case State.Blocking:
            case State.Attacking:
                movement = verticalMovement;
                break;

            case State.Dashing:
                movement = (Vector3.ClampMagnitude(forwardMovement + rightMovement, 1) * dashSpeed) + verticalMovement;
                break;

            default:
                break;
        }


        //actually move the character based on the values above, this will need to be replaced by a rigidbody movement call, replace the rb.velocity with movement once gravity is fixed
        rb.velocity = movement;
    }


    #region Input Handling

    //Gets movement inputs and converts them to relative values based on camera rotation, unlike other input commands this is called when a command needs a direction not as the input is registered.
    void MovementInput()
    {
        //create vectors for the forward and right movement inputs of the player
        forwardMovement = cameraTransform.forward * moveInput.y;
        rightMovement = cameraTransform.right * moveInput.x;

        //Remove vertical component of the character movement
        rightMovement.y = 0;
        forwardMovement.y = 0;


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
            StartCoroutine(Dash());
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

        while (dashTime < dashDuration && state == State.Dashing)
        {
            dashTime += Time.deltaTime;
            yield return null;



        }

        //continue dash if the player leaves the ground while dashing
        while (isGrounded() && state == State.Dashing)
        {
            yield return null;
        }


        //if the player is still considered to be dashing swap them back to free, is needs the if statement to prevent the player from being reassigned to free if the dash is interrupted by hitstun or a special
        if (state == State.Dashing) state = State.Free;

        Debug.Log("Dash Complete");

        Hold();




    }

    //stop a player from being able to input movement
    public void Hold()
    {
        movement.x = 0;
        movement.z = 0;
    }

    void JumpInput()
    {
        Debug.Log("jump input recognized");

        ////if able and willing, jump! could add dash to the options here to allow for a dash jump
        if (isGrounded())
        {
            switch (state)
            {
                case State.Free:
                case State.Dashing:
                    verticalMovement.y = jumpHeight;
                    break;

                default:
                    break;
            }
        }
    }


    #endregion

    #region Player state trackers

    //enum keeps track of what the player's state is so that it can shift in and out
    [SerializeField] public enum State { Free, Stunned, Dashing, Rooted, Attacking, Dodging, Blocking }
    [SerializeField] public enum Weapon { SnS, GreatScythe, TwinScythe }

    //checks if the player is grounded using colliders
    bool isGrounded()
    {
        //start a raycast from the player position, aim it dow, send it as far as the the of the character (with a slight margin of error)
        return Physics.Raycast(transform.position, Vector3.down, coll.bounds.extents.y + 0.1f);
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
