﻿using Assets.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngineInternal;

public class Player : Character
{
    // Variables regarding physics
    Rigidbody2D body;
    private Animator animator;

    // Variables regarding movement
    private float moveLimiter = 0.7f;
    [SerializeField] private float runSpeed = 1.0f;

    private float timestampForNextAction;


    private GameObject newItemPickedUp;
    [SerializeField] private List<GameObject> weaponsList;
    private List<bool> weaponsBoolList;
    private Weapon equippedWeapon = null;

    // Networking
    public Client Client { get; set; }

    // Start is called before the first frame update
    public void Start()
    {
        Debug.Log("Started player gameobject");
        body = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        base.X = body.position.x;
        base.Y = body.position.y;
        base.DX = body.velocity.x;
        base.DY = body.velocity.y;
    }

    public void Move(float horizontal, float vertical)
    {
        // Check for diagonal movement
        if (horizontal != 0 && vertical != 0)
        {
            // limit movement speed diagonally, so you move at 70% speed
            horizontal *= moveLimiter;
            vertical *= moveLimiter;
        }

        body.AddForce(new Vector2(horizontal * runSpeed, vertical * runSpeed), ForceMode2D.Impulse);
    }

    public override void DirectMove(float x, float y, float dx, float dy)
    {
        base.DX = dx;
        base.DY = dy;
        this.transform.position = new Vector2(x, y);
        //body.AddForce(new Vector2(dx, dy), ForceMode2D.Impulse);
    }

    public void GrabObject()
    {

    }

    public void TryToAttack(Vector2 attackDirection, int weaponId)
    {
        Debug.Log("Inside player TryToAttack (in server)");
        Debug.Log("weaponID: " + weaponId);
        equippedWeapon = weaponsList[weaponId].GetComponent<Weapon>();
        this.equippedWeapon.MakeAttack(this.ID, attackDirection, transform.position);
    }


    public override void TakeDamage(float damage)
    {
        Debug.Log("Player took " + damage + " damage!");
        currentHealth = currentHealth - damage;
        if (currentHealth <= 0)
        {
            GameState.instance.HandleKilledEntity(damage, this.ID);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

    }

    private void OnTriggerExit2D(Collider2D collision)
    {

    }
    private void FixedUpdate()
    {
        if (transform.hasChanged)
        {
            transform.hasChanged = false;

            base.X = body.position.x;
            base.Y = body.position.y;
            //base.DX = body.velocity.x;
            //base.DY = body.velocity.y;

            Assets.Server.MovementMessage m = new Assets.Server.MovementMessage(
            this.ID,
            0,
            0,
            base.X,
            base.Y,
            base.Rotation,
            base.DX,
            base.DY
            );

            UDPServer.getInstance().BroadcastMessage(m);
        }
    }

    public void ForceUpdateClientPosition()
    {
        base.X = body.position.x;
        base.Y = body.position.y;
        base.DX = body.velocity.x;
        base.DY = body.velocity.y;

        Assets.Server.MovementMessage m = new Assets.Server.MovementMessage(
            this.ID,
            0,
            0,
            base.X,
            base.Y,
            base.Rotation,
            base.DX,
            base.DY
        );

        this.Client.MessageQueue.Enqueue(m);
    }
}

