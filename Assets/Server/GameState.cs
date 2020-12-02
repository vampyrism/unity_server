
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
        private Int32 timeLeft;

        private GameState()
        {
            this.Entities = new Dictionary<UInt32, Entity>();
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

        // Game Updates

        /// <summary>
        /// Create <c>Player</c> entity and add it to the game state.
        /// </summary>
        /// <returns>Id of the created player.</returns>
        public UInt32 CreatePlayer()
        {
            return this.CreatePlayer(null);
        }

        public UInt32 CreatePlayer(Client client)
        {
            Player player = GameObject.Instantiate(Resources.Load("Player") as GameObject).GetComponent<Player>();
            player.Client = client;
            return AddEntity(player);
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

            if(Vector2.Distance(player.transform.position, new Vector2(x,y)) > 2)
            {
                Debug.Log("Player " + id 
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