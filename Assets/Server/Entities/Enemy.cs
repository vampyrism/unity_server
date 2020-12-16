﻿using Assets.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Enemy : Character
{
    Rigidbody2D body;

    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float enemyReach = 2f;
    [SerializeField] private float enemyDamage = 2f;
    [SerializeField] private float enemyAttackSpeed = 0.5f;

    private Vector2 oldPosition;
    private int stuckCount;

    private float timestampForNextAttack;


    [SerializeField] private int currentPathIndex;
    private List<Vector3> pathVectorList;

    private List<Transform> targetList;
    private Transform currentTarget;

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
        UpdatePath();
    }

    void Update()
    {
        // If we don't have a target or no path to the target, then we run UpdatePath
        if (pathVectorList != null && currentTarget != null) {
            // If we have a target and a path, then we check if we are close enough to the target
            Vector3 currentPathPosition = pathVectorList[currentPathIndex];
            if (Vector3.Distance(transform.position, currentTarget.position) < enemyReach) {
                // If we are close enough we try to attack it
                if (Time.time >= timestampForNextAttack) {
                    //currentTarget.GetComponent<Character>().TakeDamage(enemyDamage);
                    timestampForNextAttack = Time.time + enemyAttackSpeed;
                    body.velocity = new Vector3(0, 0, 0);
                }
            }
            else {
                HandleMovement();
            }
        } else {
            UpdatePath();
        }
    }

    public void HandleMovement() {
        if(Vector2.Distance((Vector2) transform.position, this.currentTarget.position) < 1)
        {
            body.velocity = new Vector2(0f, 0f);
            return;
        }

        if (Vector2.Distance((Vector2) transform.position, oldPosition) < 0.05) {
            stuckCount += 1;
        } else {
            stuckCount = 0;
        }

        Vector3 currentPathPosition = pathVectorList[currentPathIndex];

        if (stuckCount > 8) {
            StartCoroutine(LerpPosition((Vector2)currentPathPosition, Vector3.Distance(transform.position, currentPathPosition)/runSpeed));
                
            stuckCount = 0;
        }


        //transform.right = targetPosition - transform.position;
        Debug.DrawLine(transform.position, currentPathPosition, Color.green);

        if (Vector3.Distance(transform.position, currentPathPosition) > minDistance) {
            Vector3 moveDir = (currentPathPosition - transform.position).normalized;
            body.velocity = new Vector2(moveDir.x * runSpeed, moveDir.y * runSpeed);

        } else {
            currentPathIndex++;
            // If the enemy is far away from the target it moves 7 tiles before retargeting.
            if (currentPathIndex >= pathVectorList.Count || currentPathIndex > 7) {
                //Found the position of the list, stop moving or find new target
                pathVectorList = null;
                body.velocity = new Vector3(0,0,0);
            }
        }

        oldPosition = transform.position;
    }

    public Vector3 GetPosition() {
        return transform.position;
    }

    public override void TakeDamage(float damage) {
        Debug.Log("Enemy took " + damage + " damage!");
        currentHealth = currentHealth - damage;
        if (currentHealth <= 0) {
            Destroy(gameObject);
        }
    }

    public void RemovePlayerFromTargets(Transform removedPlayer) {
        if (currentTarget == removedPlayer) {
            currentTarget = null;
        }
        targetList.Remove(removedPlayer);
    }


    private void UpdatePath() {
        if (currentTarget == null) {
            SelectTarget();
            
        } else if (Vector2.Distance(transform.position, currentTarget.position) > enemyReach*2) {
            SelectTarget();
        }

        // If we didn't find any targets, return
        if (currentTarget == null) return;

        currentPathIndex = 0;
        pathVectorList = Pathfinding.Instance.FindPath(GetPosition(), currentTarget.position);
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
}
