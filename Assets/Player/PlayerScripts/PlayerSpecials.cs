using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpecials : MonoBehaviour
{

    //used to reference the player's current state
    private PlayerMovement playerMovement;


    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //if a player is blocking continue to hold them
        if (playerMovement.state == PlayerMovement.State.Blocking)
        {
            //reduces movement.x and y to 0
            playerMovement.Hold();
        }
    }


    //called when the PlayerMovement script passes a special input through to this class
    public void Special()
    {
        //if the player is pressing special manage their input
            Debug.Log("special input recognized");

        //using a switch statement to neaten up the code, first checks the weapon equipped then a nested switch checks for the state of the player
        switch(playerMovement.weapon)
        {
            case PlayerMovement.Weapon.SnS:
                switch(playerMovement.state)
                {
                    case PlayerMovement.State.Free:
                    case PlayerMovement.State.Dashing:
                        playerMovement.state = PlayerMovement.State.Blocking;
                        break;

                    //end of state check switch
                    default:
                        break;
                }
                break;
            
            //end of the weapon check switch
            default:
                Debug.Log("Error: Player weapon not found");
                break;
        }
    }


    //Called when the player movement script passes a canceled special through to this class
    public void Cancel()
    {
        Debug.Log("special input canceled");
        //If the player was blocking, stop them blocking
        if (playerMovement.state == PlayerMovement.State.Blocking) playerMovement.state = PlayerMovement.State.Free;
    }

}
