using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyMovement : Enemy
{


    // The range at which the bad boy will aggro to the good boy
    [SerializeField] private float aggroRange;
    // Enemy movespeed
    [SerializeField] private float movespeed;
    // Distance the enemy is able to roam
    [SerializeField] public float roamDistance;
    //distance at which the enemy can detect the player
    [SerializeField] private float sightLength;
    //the destination of the enemy as a 2d vector
    Vector2 randomDirection;
    //the destination as a 3d vector
    [SerializeField] Vector3 randomDestination;



    // Start is called before the first frame update
    void Start()
    {
        PickDestination();
    }


    //overrides from enemy class
    protected override bool CheckForPlayer()
    {
        if (Vector3.Distance(playerLocation, currentLocation) < aggroRange) return true;
        else return false;
    }

    //overrides from enemy class
    protected override void MoveToPlayer()
    {
        if (!waiting)
        {
            //If you have reached the player, stop for a moment
            if (Vector3.Distance(playerLocation, currentLocation) < 0.1)
            {
                StartCoroutine(StopandWait(thinkTime));
            }

            //set the destination to the x and z coordinates of the player
            Vector3 destination = playerLocation;
            //set the destination's y axis the the current enemy location
            destination.y = startingLocation.y;

            //Look at the destination
            transform.LookAt(destination);
            
            //move forward
            transform.Translate(Vector3.forward * Time.deltaTime * movespeed);
        }
    }

    // overrides from enemy class
    protected override void Patrol()
    {
        if (!waiting)
        {
            //If you have reached your previous destination, pick a new destination
            if (Vector3.Distance(randomDestination, currentLocation) < 0.1)
            {
                StartCoroutine(StopandWait(thinkTime));
                PickDestination();
            }

            //Look at the destination
            transform.LookAt(randomDestination);
            //move forward
            transform.Translate(Vector3.forward * Time.deltaTime * movespeed);

        }
    }


    void PickDestination()
    {

        randomDirection = startingCoordinates + Random.insideUnitCircle * roamDistance;
        randomDestination.x = randomDirection.x;
        randomDestination.y = startingLocation.y; //this sets the destination y coordinate to be the same as the starting destination, so the creature will always move left and right of where it started, will need to be reworked for the sake of walking up and down planes.
        randomDestination.z = randomDirection.y;
    }

    //what happens when an enemy collides with a hit box
    protected override void OnTriggerEnter(Collider attackingHitbox)
    {
        Debug.Log("Collision Detected");

        hitbox = attackingHitbox.GetComponent<Hitbox>();
        //check the hitbox is valid
        if (hitbox.type == "player" && vulnerable == true)
        {
            Debug.Log("I'm hit!");

            //Deal damage
            health -= hitbox.damage;

            //Assign knockback


            //Make invulnerable for a period
            StartCoroutine(DamageInvulnerability(hitInvulnerability));
        }

    }
    
    //make target invulnerable for a brief period after taking a hit
    IEnumerator DamageInvulnerability(float invulnTime)
    {
        //make self invulnerable and start a clock
        vulnerable = false;
        float elapsedTime = 0;

        //clock ticks
        while (invulnTime > elapsedTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;

        }

        //once clock is finished make self vulnerable again
        vulnerable = true;

    }

    //required method for drawing with gizmo's, should be locked away when working without debug mode
    void OnDrawGizmos()
    {

        Color gizmoColour = Color.red;
        gizmoColour.g = 0.5f;
        gizmoColour.a = 0.2f;

        //debug mode to show where the sphere of patrol is going to be
        Gizmos.color = gizmoColour;
        Gizmos.DrawSphere(transform.position, roamDistance);
    }


}
