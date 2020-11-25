using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : Entity
{
    [SerializeField] public float maxHealth = 5;
    [SerializeField] public float currentHealth = 5;
    public abstract void TakeDamage(float damage);
}
