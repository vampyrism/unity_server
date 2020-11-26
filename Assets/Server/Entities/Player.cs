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

    // Variables regarding movement
    private float moveLimiter = 0.7f;
    [SerializeField] private float runSpeed = 1.0f;
    [SerializeField] private UInt32 entity_id; 

    public float x { get; private set; } = 0.0f;
    public float y { get; private set; } = 0.0f;
    public float vx { get; private set; } = 0.0f;
    public float vy { get; private set; } = 0.0f;

    private float timestampForNextAction;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        this.entity_id = base.ID; // TODO: Debugging, remove.
        body = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        x = body.position.x;
        y = body.position.y;
        vx = body.velocity.x;
        vy = body.velocity.y;
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

        body.velocity = new Vector2(horizontal * runSpeed, vertical * runSpeed);
    }

    public void GrabObject()
    {

    }

    public void TryToAttack(Vector2 targetPosition)
    {

    }

    public override void TakeDamage(float damage)
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

    }

    private void OnTriggerExit2D(Collider2D collision)
    {

    }
}

