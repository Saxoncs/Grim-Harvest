using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float cameraDistance;
    [SerializeField] GameObject player;
    [SerializeField] Vector3 currentLocation;
    //The length of the deadzone at which the camera won't readjust its zoom.
    [SerializeField] private float zoomDeadzone;
    [SerializeField] float pivotSpeed;


    private Vector2 moveInput;
    private PlayerControls controls;
    [SerializeField] private Vector3 playerLocation;
    private Vector3 previousPlayerLocation;

    private Transform camera;
    

    

    //how much and in what direction the player has moved
    [SerializeField] Vector3 playerMovement;

    void Awake()
    {

        #region Input bindings
        controls = new PlayerControls();
        //Bind move input to a vector between (1,1) and (-1,-1)
        controls.CameraMovement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        //Cancel move input by returning vector to (0,0), helps to make movement of camera snappier
        controls.CameraMovement.Move.canceled += ctx => moveInput = Vector2.zero;

        //Registers whenever the zoom button is pressed
        controls.CameraMovement.Zoom.performed += ctx => ZoomInput();
        #endregion

        //camera should be maintained as the first child on the index
        camera = transform.GetChild(0);
    }

    void Start()
    {
        //Set location of camera focus
        transform.position = player.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        currentLocation = transform.position;
        playerLocation = player.transform.position;



        //if the camera's position is out of the acceptable camera distancing bounds, commence fix
        if (camera.position.x - cameraDistance > zoomDeadzone)
        {
            AdjustZoom();
        }

        MoveInput();


    }

    void AdjustZoom()
    {
        //move camera to within distance of player
        if (Vector3.Distance(playerLocation, currentLocation) > -cameraDistance)
        {
            //move back
            camera.Translate(Vector3.forward * Time.deltaTime * 20);

        }
        else if (Vector3.Distance(playerLocation, currentLocation) < -cameraDistance)
        {
            //move forward
            camera.Translate(Vector3.forward * Time.deltaTime * -20);

        }
    }

    void LateUpdate()
    {
        FollowPlayer();

    }

    void FollowPlayer()
    {
        //set focuses location to the location of the player
        transform.position = player.transform.position;
    }


    private void MoveInput()
    {
        if(moveInput != Vector2.zero)
        {
            transform.Rotate(moveInput.y * Time.deltaTime * pivotSpeed, 0, 0, Space.Self);
            transform.Rotate(0, moveInput.x * Time.deltaTime * pivotSpeed, 0, Space.World);

        }
    }

    private void ZoomInput()
    {
        Debug.Log("Zoom input detected");
        cameraDistance += 50;
    }

    #region Enable and Disable Camera

    private void OnEnable()
    {
        controls.CameraMovement.Enable();
    }

    private void OnDisable()
    {
        controls.CameraMovement.Disable();
    }


    #endregion

}
