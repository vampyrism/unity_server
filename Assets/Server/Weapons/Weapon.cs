using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : Entity
{
    public string weaponName;
    public int weaponIndex;
    public float weaponDamage;
    public float reloadSpeed;
    public bool isRanged;

    public abstract void MakeAttack(Vector2 clickPosition, Vector2 myPosition, uint playerId);
}
