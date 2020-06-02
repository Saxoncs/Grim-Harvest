﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyMovement : MonoBehaviour
{

    private Vector3 startingLocation;
    private Vector2 startingCoordinates;
    private Vector3 currentLocation;

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
    //The amount of time the enemy will stop for before changing directions
    [SerializeField] private float waitTime;
    //checks whether the enemy is waiting or not, should be replaced with a state system
    private bool waiting;
    // Hitbox data for the player
    private Hitbox hitbox;

    //track the player
    [SerializeField] GameObject player;

    private Vector3 playerLocation;

    //determines if the enemy is vulnerable or not
    private bool vulnerable = true;

    //how long an enemy remains invulnerable after being hit
    [SerializeField] private float hitInvulnerablility;

    //Enemy health
    [SerializeField] private float health;

    private void Awake()
    {
        startingLocation = transform.position;
        startingCoordinates.x = transform.position.x;
        startingCoordinates.y = transform.position.z;

        

    }

    // Start is called before the first frame update
    void Start()
    {
        waiting = false;
        PickDestination();
    }

    // Update is called once per frame
    void Update()
    {
        currentLocation = transform.position;
        playerLocation = player.transform.position;

        if (health <= 0)
        {
            Death();
        }

        //Check for player in range
        if (CheckForPlayer())
        {
            //Move towards player if in range
            MoveToPlayer();
        }
        else
        {
            //Patrol randomly if player not in range
            Patrol();
        }




    }

    bool CheckForPlayer()
    {
        if (Vector3.Distance(playerLocation, currentLocation) < aggroRange) return true;
        else return false;
    }

    void MoveToPlayer()
    {
        if (!waiting)
        {
            //If you have reached the player, stop for a moment
            if (Vector3.Distance(playerLocation, currentLocation) < 0.1)
            {
                StartCoroutine(StopandWait());
            }

            //Look at the destination
            transform.LookAt(playerLocation);
            //move forward
            transform.Translate(Vector3.forward * Time.deltaTime * movespeed);
        }
    }

    void Patrol()
    {
        if (!waiting)
        {
            //If you have reached your previous destination, pick a new destination
            if (Vector3.Distance(randomDestination, currentLocation) < 0.1)
            {
                StartCoroutine(StopandWait());
                PickDestination();
            }



            //Look at the destination
            transform.LookAt(randomDestination);
            //move forward
            transform.Translate(Vector3.forward * Time.deltaTime * movespeed);

        }
    }


    IEnumerator StopandWait()
    {
        //start clock
        float elapsedTime = 0;

        //start waiting
        waiting = true;

        //clock ticks
        while (waitTime > elapsedTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;

        }

        //finish waiting
        waiting = false;


    }

    void PickDestination()
    {

        randomDirection = startingCoordinates + Random.insideUnitCircle * roamDistance;
        randomDestination.x = randomDirection.x;
        randomDestination.y = startingLocation.y; //this sets the destination y coordinate to be the same as the starting destination, so the creature will always move left and right of where it started, will need to be reworked for the sake of walking up and down planes.
        randomDestination.z = randomDirection.y;
    }

    //what happens when an enemy collides with a hit box
    void OnTriggerEnter(Collider attackingHitbox)
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
            StartCoroutine(DamageInvulnerability());
        }





    }
    
    //make target invulnerable for a brief period after taking a hit
    IEnumerator DamageInvulnerability()
    {
        //make self invulnerable and start a clock
        vulnerable = false;
        float elapsedTime = 0;

        //clock ticks
        while (hitInvulnerablility > elapsedTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;

        }

        //once clock is finished make self vulnerable again
        vulnerable = true;

    }

    void Death()
    {
        Destroy(gameObject);
    }

    //required method for drawing with gizmo's, should be locked away when working without debug mode
    void OnDrawGizmos()
    {

        Color gizmoColour = Color.red;
        gizmoColour.g = 0.5f;
        gizmoColour.a = 0.2f;

        //debug mode to show where the sphere of patrol is going to be
        Gizmos.color = gizmoColour;
        Gizmos.DrawSphere(startingLocation, roamDistance);
    }


}