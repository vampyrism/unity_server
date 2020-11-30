
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
        // Singleton

        public static readonly GameState instance = new GameState();

        public static GameState GetInstance()
        {
            return instance;
        }

        // Attributes

        // Keep track of all entities (players, items, ...)
        private Dictionary<UInt32, Entity> entities;

        private GameState()
        {
            entities = new Dictionary<UInt32, Entity>();
        }

        // The implementation is as of now theoretical so the functions 
        // consists of mainly psuedo kod.

        // Game Updates
    
        public void AddEntity(UInt32 id, Entity entity)
        {
            entities.Add(id, entity);
        }

        public void CreatePlayer(Client client)
        {
            Player newPlayer = GameObject.Instantiate(Resources.Load("Player") as GameObject);
            clients.Add(newPlayer.entity_id, client);
            AddEntity(newPlayer.entity_id, newPlayer);
        }

        public void PlayerMove(UInt32 id, float x, float y)
        {
            Player player;
            entities.TryGetValue(id, out player);
            // player.x = x
            // player.y = y
            // ...
        }

        public void PlayerAttack(UInt32 playerId, UInt32 targetId)
        {
            Player player;
            Player target;
            entities.TryGetValue(playerId, out player);
            entities.TryGetValue(targetId, out target);
            // target.takeDamage(...)
            // client.messageQueue.Enqueue( new AttackMessage(...) )
            // ...
        }
    }
}