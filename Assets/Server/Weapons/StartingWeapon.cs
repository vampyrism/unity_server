using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingWeapon : Weapon
{
    public float attackRangeX = 0.8f;
    public float attackRangeY = 1.5f;
    public float weaponDistanceFromPlayer = 1;
    public float offsetInYDirection = 0.2f;
    public StartingWeapon() {
        this.weaponName = "startingWeapon";
        this.weaponIndex = 0;
        this.weaponDamage = 1f;
        this.reloadSpeed = 1f;
        this.isRanged = false;
    }

    public override void MakeAttack(uint attackingPlayerID, Vector2 clickPosition, Vector2 playerPosition) {
        Debug.Log("Inside StartingWeapon MakeAttack");
        Vector2 attackDirection = (clickPosition - (Vector2)playerPosition).normalized;
        Vector2 weaponBoxPosition = playerPosition + (attackDirection * weaponDistanceFromPlayer);
        weaponBoxPosition.y += offsetInYDirection;

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(weaponBoxPosition, new Vector2(attackRangeX, attackRangeY), AngleBetweenTwoPoints(clickPosition, playerPosition));
        DebugDrawBox(weaponBoxPosition, new Vector2(attackRangeX, attackRangeY), AngleBetweenTwoPoints(clickPosition, playerPosition), Color.green, 3);

        for (int i = 0; i < hitTargets.Length; i++) {
            Character hitCharacter = hitTargets[i].GetComponent<Character>();

            // Did we hit a character
            if (hitCharacter != null) {
                // Did we hit ourselves?
                if (hitCharacter.ID == attackingPlayerID) {
                    continue;
                }

                Assets.Server.GameState.instance.AttackValid(hitCharacter.ID, weaponDamage);
            }
            else if (hitTargets[i].name == "Collision_Default") {
                // Hit a wall
                Debug.Log("Hit the wall");
            }
        }
    }

    float AngleBetweenTwoPoints(Vector3 a, Vector3 b) {
        return (Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg) + 90;
    }


    void DebugDrawBox(Vector2 point, Vector2 size, float angle, Color color, float duration) {

        var orientation = Quaternion.Euler(0, 0, angle);

        // Basis vectors, half the size in each direction from the center.
        Vector2 right = orientation * Vector2.right * size.x / 2f;
        Vector2 up = orientation * Vector2.up * size.y / 2f;

        // Four box corners.
        var topLeft = point + up - right;
        var topRight = point + up + right;
        var bottomRight = point - up + right;
        var bottomLeft = point - up - right;

        // Now we've reduced the problem to drawing lines.
        Debug.DrawLine(topLeft, topRight, color, duration);
        Debug.DrawLine(topRight, bottomRight, color, duration);
        Debug.DrawLine(bottomRight, bottomLeft, color, duration);
        Debug.DrawLine(bottomLeft, topLeft, color, duration);
    }


}


