﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This is the base class for enemies in the game, it will contain virtual functions for all of the basic operations an enemy will perform
public class Enemy : MonoBehaviour
{

    //Declaring some important variables
    private Vector3 startingLocation;
    private Vector2 startingCoordinates;
    private Vector3 currentLocation;

    //track the player
    [SerializeField] GameObject player;
    private Vector3 playerLocation;

    //determines if the enemy is vulnerable or not
    private bool vulnerable = true;

    //how long an enemy remains invulnerable after being hit
    [SerializeField] private float hitInvulnerablility;

    //Enemy health
    [SerializeField] private float health;

    //The amount of time the enemy will stop for before changing directions
    [SerializeField] private float waitTime;

    //checks whether the enemy is waiting or not, should be replaced with a state system
    private bool waiting;

    // Hitbox data for the player
    private Hitbox hitbox;





    //gets the location of the enemy ASAP, and marks it as the enemy startingLocation
    void Awake()
    {

        startingLocation = transform.position;
        startingCoordinates.x = transform.position.x;
        startingCoordinates.y = transform.position.z;
    }

    // Start is called before the first frame update, stop the enemy from waiting and pick a destination
    void Start()
    {
        waiting = false;
    }

    // Update is called once per frame, it takes not of the player and enemy location and performs a few functions
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

    //what the enemy does when its health reaches 0
    protected virtual void Death()
    {

    }

    //how the enemy searches for the player, and consequently what will cause it to change behavior
    protected virtual bool CheckForPlayer()
    {
        return false;
    }

    //how the enemy threatens the player once it knows where it is (might need a rename)
    protected virtual void MoveToPlayer()
    {

    }

    //what the enemy does when it is unaware of a player
    protected virtual void Patrol()
    {

    }


    //tells the enemy to stop for a while
    protected IEnumerator StopandWait()
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

    //make enemy invulnerable for a brief period after taking a hit
    protected IEnumerator DamageInvulnerability()
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

}