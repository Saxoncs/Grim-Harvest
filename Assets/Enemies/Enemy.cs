using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This is the base class for enemies in the game, it will contain virtual functions for all of the basic operations an enemy will perform
public class Enemy : MonoBehaviour
{

    //Declaring some important variables
    protected Vector3 startingLocation;
    protected Vector2 startingCoordinates;
    protected Vector3 currentLocation;

    protected Rigidbody rb;

    //track the player
    [SerializeField] GameObject player;
    protected Vector3 playerLocation;

    //determines if the enemy is vulnerable or not
    protected bool vulnerable = true;

    //how long an enemy remains invulnerable after being hit
    [SerializeField] protected float hitInvulnerablility;

    //Enemy health
    [SerializeField] protected float health;

    //The amount of time the enemy will stop for before changing directions
    [SerializeField] protected float thinkTime;

    //checks whether the enemy is waiting or not, should be replaced with a state system
    protected bool waiting;

    // Hitbox data for the player
    protected Hitbox hitbox;


    float gravity = 14;
    protected Vector3 movement = new Vector3();

    [SerializeField] protected bool isEffectedByGravity;




    //gets the location of the enemy ASAP, and marks it as the enemy startingLocation
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startingLocation = transform.position;

        startingCoordinates.x = startingLocation.x;
        startingCoordinates.y = startingLocation.z;

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
            MoveToPlayer();
        }
        else
        {
            Patrol();
        }

    }

    // Move in the direction specified by movement, this is as of yet unimplemented
    protected virtual void Move()
    {
        if(isEffectedByGravity)
        {
            movement.y += gravity;
        }
        rb.velocity = (movement);

        //the old way for reference
        //transform.Translate(Vector3.forward * Time.detlaTime * movespeed);
    }

    //what the enemy does when its health reaches 0
    protected virtual void Death()
    {
        Destroy(gameObject);
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
    protected IEnumerator StopandWait(float waitTime)
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
    protected IEnumerator DamageInvulnerability(float invulnerablilityDuration)
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
    protected virtual void OnTriggerEnter(Collider attackingHitbox)
    {
        Debug.Log("Collision Detected");

        hitbox = attackingHitbox.GetComponent<Hitbox>();
        //check the hitbox is valid
        if (hitbox.type == "player" && vulnerable == true)
        {
            Debug.Log("I'm hit!");
        }

    }

}
