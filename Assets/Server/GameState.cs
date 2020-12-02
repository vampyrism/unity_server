
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
        private Dictionary<UInt32, Entity> entities;

        // Time left of current day/night cycle
        private Int32 timeLeft;

        private GameState()
        {
            entities = new Dictionary<UInt32, Entity>();
        }


        public void FixedUpdate() 
        {
            // Update game time
        }

        /// <summary>
        /// Add entity to the game state.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        /// <returns>Id of the added entity.</returns>
        public UInt32 AddEntity(Entity entity)
        {
            entities.Add(entity.ID, entity);
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
                entity = entities[id];
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
            return entities.ContainsKey(id);
        }

        // Game Updates

        /// <summary>
        /// Create <c>Player</c> entity and add it to the game state.
        /// </summary>
        /// <returns>Id of the created player.</returns>
        public UInt32 CreatePlayer()
        {
            return this.CreatePlayer(0f, 0f);
        }
        public UInt32 CreatePlayer(float x, float y) {
            Player player = GameObject.Instantiate(Resources.Load("Player") as GameObject, new Vector3(x, y), Quaternion.identity).GetComponent<Player>();
            return AddEntity(player);
        }

        /// <summary>
        /// Move <c>Player</c> with id by coordinates and velocity.
        /// </summary>
        /// <param name="id">Id of <c>Player</c> entity.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="dx">X velocity.</param>
        /// <param name="dy">Y velocity.</param>
        public void PlayerMove(UInt32 id, float x, float y, float dx, float dy)
        {
            Entity player = GetEntity(id);
            player.DirectMove(x, y, dx, dy);
        }

        /// <summary>
        /// Make <c>Player</c> attack another <c>Player</c>.
        /// </summary>
        /// <param name="playerId">Id of attacking <c>Player</c>.</param>
        /// <param name="targetId">Id of attacked <c>Player</c>.</param>
        public bool PlayerAttack(UInt32 playerId, UInt32 targetId)
        {
            Entity player = GetEntity(playerId);
            Entity target = GetEntity(targetId);
            // target.takeDamage(...)
            // client.messageQueue.Enqueue( new AttackMessage(...) )
            // ...
            throw new NotImplementedException();
        }
    }
}