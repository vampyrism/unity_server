using Assets.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Enemy : Character
{
    Rigidbody2D body;

    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float enemyReach = 1.5f;
    [SerializeField] private float enemyDamage = 2f;
    [SerializeField] private float enemyAttackSpeed = 0.5f;

    private Vector2 oldPosition;
    private int stuckCount;

    private float timestampForNextAttack;


    [SerializeField] private int currentPathIndex;
    private List<Vector3> pathVectorList;

    private List<Transform> targetList;
    private Transform currentTarget;
    [SerializeField] private bool vampireIsDayDebugOverride = false;
    [SerializeField] private bool vampireIsDay = true;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();

        // Removed while using a simpler way of finding all players
        /*
        GameObject[] PlayerGameObjectList = GameObject.FindGameObjectsWithTag("Player");
        targetList = new List<Transform>();
        foreach (GameObject OtherPlayerGameObject in PlayerGameObjectList) {
            targetList.Add(OtherPlayerGameObject.transform);
        }
        */

        pathVectorList = null;
        currentPathIndex = 0;
        oldPosition = transform.position;

        timestampForNextAttack = Time.time;

        // Set the initial path
        vampireIsDay = GameState.instance.dayNightCycle.IsDay;
        UpdatePath();
    }

    void Update()
    {
        if (vampireIsDay == false) {
            // Night behaviour
            if (currentTarget != null) {
                // If we have a target we check if we are close enough to attack it
                if (Vector3.Distance(transform.position, currentTarget.position) < enemyReach) {
                    // If we are close enough we try to attack it
                    if (Time.time >= timestampForNextAttack) {
                        GameState.instance.EnemyAttack(this.ID, currentTarget.GetComponent<Character>().ID);

                        IEnumerator coroutine = DelayDamage(0.5f, currentTarget.GetComponent<Character>().ID);
                        StartCoroutine(coroutine);

                        timestampForNextAttack = Time.time + enemyAttackSpeed;
                        body.velocity = new Vector3(0, 0, 0);
                        pathVectorList = null;
                    }
                }
                else if (pathVectorList != null) {
                    // If we are not close enough to attack we move towards it
                    HandleMovement();
                }
                else {
                    // If we have no path we update path
                    UpdatePath();
                }
            }
            else {
                // If we have no target we update path
                UpdatePath();
            }
        }
        else {
            // Day Behaviour
            if (pathVectorList != null) {

                HandleMovement();
            }
            else {
                UpdatePath();
            }
        }
    }

    private IEnumerator DelayDamage(float waitTime, UInt32 targetPlayerID) {
        yield return new WaitForSeconds(waitTime);
        GameState.instance.AttackValid(targetPlayerID, this.enemyDamage);
    }

    public void HandleMovement() {
        Vector3 currentPathTarget = pathVectorList[currentPathIndex];
        /*
        if(Vector2.Distance((Vector2) transform.position, this.currentTarget.position) < 1)
        {
            body.velocity = new Vector2(0f, 0f);
            return;
        }
        */

        // Solves enemy getting stuck in corners by lerping it past it
        if (Vector2.Distance((Vector2) transform.position, oldPosition) < 0.0001) {
            stuckCount += 1;
        } else {
            stuckCount = 0;
        }
        if (stuckCount > 3) {
            StartCoroutine(LerpPosition((Vector2)currentPathTarget, Vector3.Distance(transform.position, currentPathTarget) /runSpeed));

            stuckCount = 0;
        }

        Debug.DrawLine(transform.position, currentPathTarget, Color.green);

        // If we are far enough from the tile we are moving towards, then we continue to move towards it.
        if (Vector3.Distance(transform.position, currentPathTarget) > minDistance) {
            Vector3 moveDir = (currentPathTarget - transform.position).normalized;
            body.velocity = new Vector2(moveDir.x * runSpeed, moveDir.y * runSpeed);

        } else {
            currentPathIndex++;
            // If the enemy is far away from the target it moves 7 tiles before retargeting.
            if (currentPathIndex >= pathVectorList.Count || currentPathIndex > 7) {
                UpdatePath();
            }
        }

        oldPosition = transform.position;
    }

    public Vector3 GetPosition() {
        return transform.position;
    }

    public override void TakeDamage(float damage) {
        //Debug.Log("Enemy took " + damage + " damage!");
        currentHealth = currentHealth - damage;
        if (currentHealth <= 0) {
            Destroy(gameObject);
        }
        vampireIsDay = false;
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public void RemovePlayerFromTargets(Transform removedPlayer) {
        if (currentTarget == removedPlayer) {
            currentTarget = null;
        }
        targetList.Remove(removedPlayer);
    }


    private void UpdatePath() {
        if (vampireIsDay == false) {
            // Night behaviour
            // Find the closest target
            SelectTarget();

            // If we didn't find any targets, return
            if (currentTarget == null) {
                body.velocity = new Vector2(0, 0);
                return;
            }

            currentPathIndex = 0;
            pathVectorList = Pathfinding.Instance.FindPath(GetPosition(), currentTarget.position);
        } else {
            // Day behaviour
            Vector3 randomTarget = new Vector3(UnityEngine.Random.Range(42, 52), UnityEngine.Random.Range(39, 47), 0);
            currentPathIndex = 0;
            pathVectorList = Pathfinding.Instance.FindPath(GetPosition(), randomTarget);
        }
    }
    private void SelectTarget() {
        float closestDistance = float.MaxValue;

        GameObject[] PlayerGameObjectList = GameObject.FindGameObjectsWithTag("Player");
        targetList = new List<Transform>();
        foreach (GameObject OtherPlayerGameObject in PlayerGameObjectList) {
            targetList.Add(OtherPlayerGameObject.transform);
        }

        foreach (Transform target in targetList) {
            float targetDistance = Vector2.Distance(transform.position, target.position);
            if (targetDistance < closestDistance) {
                closestDistance = targetDistance;
                currentTarget = target;
            }
        }
    }

    IEnumerator LerpPosition(Vector2 targetPosition, float duration) {
        float time = 0;
        Vector2 startPosition = transform.position;

        while (time < duration) {
            transform.position = Vector2.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }

    public float GetEnemyDamage() {
        return this.enemyDamage;
    }

    public void SetVampireDay(bool isDay) {
        if (vampireIsDayDebugOverride != true) {
            vampireIsDay = isDay;
        }
    }

    private void FixedUpdate()
    {
        if (transform.hasChanged)
        {
            transform.hasChanged = false;

            base.X = body.position.x;
            base.Y = body.position.y;
            base.DX = body.velocity.x;
            base.DY = body.velocity.y;

            Assets.Server.MovementMessage m = new Assets.Server.MovementMessage(
            0,
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
    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.GetComponent<Enemy>()) {
            if (pathVectorList != null) {
                Vector3 currentPathTarget = pathVectorList[currentPathIndex];
                StartCoroutine(LerpPosition((Vector2)currentPathTarget, Vector3.Distance(transform.position, currentPathTarget) / runSpeed));
            }
        }
    }
}
