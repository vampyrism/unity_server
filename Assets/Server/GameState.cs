
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

// This class keeps track of all entities such as players and items in the game.

namespace Assets.Server
{
    public sealed class GameState
    {
        public static readonly GameState instance = new GameState();

        // Keep track of all entities (players, items, etc.)
        public Dictionary<UInt32, Entity> Entities { get; private set; }

        // Time left of current day/night cycle
        public DayNightCycle dayNightCycle;

        private GameState()
        {
            this.Entities = new Dictionary<UInt32, Entity>();
            this.dayNightCycle = new DayNightCycle(10, 10);
        }


        public void FixedUpdate()
        {
            // Update game time
            dayNightCycle.FixedUpdate();
        }

        public void SpawnEnemies()
        {
            // Check to see how many enemies we need to fill up the map with
            int alive = 0;
            foreach (KeyValuePair<UInt32, Entity> kvp in GameState.instance.Entities) 
            {
                if (kvp.Value is Enemy && ((Enemy) kvp.Value).IsAlive()) {
                    alive += 1;
                }
            }
            Debug.Log("alive: " + alive);

            int enemies = 2 + (int) Math.Floor(Time.realtimeSinceStartup / 100);

            for (int i = 0; i < enemies - alive; i++)
            {
                UInt32 id = CreateEnemy(45, 45);
                EntityUpdateMessage message = new EntityUpdateMessage(
                    EntityUpdateMessage.Type.ENEMY,
                    EntityUpdateMessage.Action.CREATE,
                    id
                );
                UDPServer.getInstance().BroadcastMessage(message);
            }
        }

        /// <summary>
        /// Add entity to the game state.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        /// <returns>Id of the added entity.</returns>
        public UInt32 AddEntity(Entity entity)
        {
            Entities.Add(entity.ID, entity);
            return entity.ID;
        }

        /// <summary>
        /// Get <c>Entity<c> by id.
        /// </summary>
        /// <param name="id">ID of entity.</param>
        /// <returns><c>Entity</c> with id.</returns>
        public Entity GetEntity(UInt32 id)
        {
            Entity entity;
            try
            {
                entity = Entities[id];
            }
            catch (KeyNotFoundException)
            {
                // TODO: Not sure about best way to handle this.
                entity = null;
                Debug.Log("Entity " + id + " not found.");
            }

            return entity;
        }

        /// <summary>
        /// Check to see if game state contains <c>Entity</c> with id.
        /// </summary>
        /// <param name="id">Id of entity.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public bool ContainsEntity(UInt32 id)
        {
            return Entities.ContainsKey(id);
        }

        public void RemoveEntity(UInt32 id)
        {
            if (ContainsEntity(id))
            {
                Entities.Remove(id);
            }
        }

        // Game Updates

        /// <summary>
        /// Create <c>Player</c> entity and add it to the game state.
        /// </summary>
        /// <returns>Id of the created player.</returns>
        public UInt32 CreatePlayer()
        {
            return this.CreatePlayer(null, 0f, 0f);
        }

        public UInt32 CreatePlayer(Client client)
        {
            return this.CreatePlayer(client, 0f, 0f);
        }

        public UInt32 CreatePlayer(float x, float y)
        {
            return this.CreatePlayer(null, x, y);
        }

        public UInt32 CreatePlayer(Client client, float x, float y) {
            Player player = GameObject.Instantiate(Resources.Load("Player") as GameObject, new Vector3(x, y), Quaternion.identity).GetComponent<Player>();
            player.Client = client;
            return AddEntity(player);
        }

        /// <summary>
        /// Create <c>Enemy</c> entity and add it to the game state.
        /// </summary>
        /// <returns>Id of the created enemy.</returns>
        public UInt32 CreateEnemy() {
            return this.CreateEnemy(0f, 0f);
        }
        public UInt32 CreateEnemy(float x, float y) {
            Enemy enemy = GameObject.Instantiate(Resources.Load("Enemy") as GameObject, new Vector3(x, y), Quaternion.identity).GetComponent<Enemy>();
            return AddEntity(enemy);
        }

        /// <summary>
        /// Create <c>Weapon</c> entity and add it to the game state.
        /// </summary>
        /// <returns>Id of the created weapon.</returns>
        public UInt32 CreateWeapon(string itemName) {
            return this.CreateWeapon(0f, 0f, itemName);
        }
        public UInt32 CreateWeapon(float x, float y, string itemName) {
            Weapon weapon = GameObject.Instantiate(Resources.Load<GameObject>(itemName), new Vector3(x, y), Quaternion.identity).GetComponent<Weapon>();
            return AddEntity(weapon);
        }

        public UInt32 CreateBow(float x, float y) {
             return this.CreateWeapon(x, y, "Bow");
        }
        public UInt32 CreateCrossbow(float x, float y) {
            return this.CreateWeapon(x, y, "Crossbow");
        }


        /// <summary>
        /// Move <c>Player</c> with id by coordinates and velocity.
        /// </summary>
        /// <param name="id">Id of <c>Player</c> entity.</param>
        /// <param name="seq">Sequence number</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="dx">X velocity.</param>
        /// <param name="dy">Y velocity.</param>
        public void PlayerMove(UInt32 id, UInt16 seq, float x, float y, float dx, float dy)
        {
            Player player = (Player) GetEntity(id);

            if(!player)
            {
                return;
            }

            if(player.LastUpdate < seq || Math.Abs(player.LastUpdate - seq) > UInt16.MaxValue / 4)
            {
                player.LastUpdate = seq;
            } 
            else
            {
                return;
            }

            if(Vector2.Distance(player.transform.position, new Vector2(x,y)) > 2)
            {
                Debug.Log("Player " + id
                    + " (server pid=" + player.ID + " "
                    + " moved too fast (distance=" + Vector2.Distance(player.transform.position, new Vector2(x, y)));
                player.ForceUpdateClientPosition();
                return;
            }

            player.DirectMove(x, y, dx, dy);
            /*if (seq > player.LastUpdate)
            {
                player.DirectMove(x, y, dx, dy);
                player.LastUpdate = seq;
            }
            else if (Math.Abs(seq - player.LastUpdate) > UInt16.MaxValue / 4)
            {
                player.DirectMove(x, y, dx, dy);
                player.LastUpdate = seq;
            }*/
        }

        /// <summary>
        /// <c>Player</c> attack towards a direction.
        /// </summary>
        /// <param name="playerId">Id of attacking <c>Player</c>.</param>
        /// <param name="weaponId">weaponId of attacking <c>Player</c>.</param>
        /// <param name="clickPositionX">X position of the attack <c>Player</c>.</param>
        /// <param name="clickPositionY">Y position of the attack<c>Player</c>.</param>
        public void PlayerAttack(UInt32 playerId, short weaponId, float clickPositionX, float clickPositionY)
        {
            //Debug.Log("Start of PlayerAttack");
            Vector2 clickPosition;
            clickPosition.x = clickPositionX;
            clickPosition.y = clickPositionY;

            Player player = (Player) GetEntity(playerId);

            AttackMessage AttackInit = new AttackMessage(0, playerId, 0, 0, 0, weaponId, 0, 0, clickPositionX, clickPositionY, 1);
            //Debug.Log("Broadcasting in PlayerATtack");
            UDPServer.getInstance().BroadcastMessage(AttackInit);

            player.TryToAttack(clickPosition, weaponId);
        }

        /// <summary>
        /// <c>Player</c> attack towards a direction.
        /// </summary>
        /// <param name="playerId">Id of attacking <c>Player</c>.</param>
        /// <param name="weaponId">weaponId of attacking <c>Player</c>.</param>
        /// <param name="clickPositionX">X position of the attack <c>Player</c>.</param>
        /// <param name="clickPositionY">Y position of the attack<c>Player</c>.</param>
        public void EnemyAttack(UInt32 enemyID, UInt32 targetPlayerID) {
            //Debug.Log("Start of EnemyAttack");

            Enemy enemy = (Enemy)GetEntity(enemyID);
            Player player = (Player)GetEntity(targetPlayerID);

            AttackMessage AttackInit = new AttackMessage(0, enemyID, 0, 0, targetPlayerID, 0, 0, 0, player.X, player.Y, 1);
            Debug.Log("Broadcasting in EnemyAttack");
            UDPServer.getInstance().BroadcastMessage(AttackInit);
        }

        /// <summary>
        /// A character takes damage.
        /// </summary>
        /// <param name="targetCharacterID">Id of damaged <c>Player</c>.</param>
        /// <param name="damageAmount">Amount of damage that the character takes.</param>
        public void AttackValid(UInt32 targetCharacterID, float damageAmount)
        {
            Character targetEntity = (Character) GetEntity(targetCharacterID);
            targetEntity.TakeDamage(damageAmount);
            AttackMessage newAttack = new AttackMessage(0, targetCharacterID, 0, 0, 0, 0, 1, damageAmount, 0, 0, 0);
            //targetEntity.Client.MessageQueue.Enqueue(newAttack);
            UDPServer.getInstance().BroadcastMessage(newAttack);
        }

        public void DestroyEntityID(uint entityID) {
            if (Entities.TryGetValue(entityID, out Entity e)) {
                RemoveEntity(entityID);
                Server.instance.TaskQueue.Enqueue(new Action(() => {
                    Server.instance.ServerDestroyEntity(e);
                }));
            }
            else {
                Debug.Log("Trying to destroy entity ID: " + entityID + ", but couldn't find it.");
            }
        }
    }
}
