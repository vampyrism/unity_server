using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Asserts.Server
{
    public class Bow : Weapon
    {

        [SerializeField] private Transform projectile;
        public Bow() {
            this.weaponName = "bow";
            this.weaponIndex = 1;
            this.weaponDamage = 11f;
            this.reloadSpeed = 0.3f;
            this.isRanged = true;
        }

        public override void MakeAttack(Vector2 clickPosition, Vector2 spawnPosition, uint playerId) {
            Vector2 attackDirection = (clickPosition - (Vector2) spawnPosition).normalized;
            Transform projectileTransform = Instantiate(projectile, spawnPosition, Quaternion.identity);
            projectileTransform.GetComponent<Assets.Server.Projectile>().Setup(attackDirection, weaponDamage, playerId);
        }

    }
}
