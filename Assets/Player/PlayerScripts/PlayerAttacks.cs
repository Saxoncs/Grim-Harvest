using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacks : MonoBehaviour
{

    //A reference to the player movement script attached to this player, can be used to check state
    private PlayerMovement playerMovement;

    [SerializeField] GameObject snsJab;


    // Start is called before the first frame update
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void Attack()
    {
        //if the player is pressing special manage their input
        Debug.Log("attack input recognized");

        //using a switch statement to neaten up the code, first checks the weapon equipped then a nested switch checks for the state of the player
        switch (playerMovement.weapon)
        {
            case PlayerMovement.Weapon.SnS:
                switch (playerMovement.state)
                {
                    case PlayerMovement.State.Free:
                        StartCoroutine(SnSJab());
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


    //All attacks should be handled when called by the PlayerMovement script, it might be better to separate the player movement script from the input handling by creating a PlayerController or PlayerState script.


    IEnumerator SnSJab()
    {
        //set the parameters for the move
        float startup = 0.1f;
        float endlag = 0.4f;
        float range = 1f;
        float active = 0.2f;

        //start player attack
        playerMovement.state = PlayerMovement.State.Attacking;
        //stop movement
        playerMovement.Hold();

        //startup clock
        float elapsedTime = 0;
        while (elapsedTime < startup && playerMovement.state == PlayerMovement.State.Attacking)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        


        //spawn hitbox in front of player at range distance away
        if (playerMovement.state == PlayerMovement.State.Attacking)
        {
            Vector3 location = transform.position + (transform.forward * range);
            Instantiate(snsJab, location, transform.rotation);
        }

        //linger for endlag
        elapsedTime = 0;
        while (elapsedTime < endlag && playerMovement.state == PlayerMovement.State.Attacking)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //if the player is still attacking, set them free once more
        if (playerMovement.state == PlayerMovement.State.Attacking) playerMovement.state = PlayerMovement.State.Free; 


    }
 

}
