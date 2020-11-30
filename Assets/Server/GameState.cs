﻿
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

        public static GameState GetInstance()
        {
            return instance;
        }

        // Attributes

        // Keep track of all entities (players, items, ...)
        private Dictionary<UInt32, Entity> entities;

        // When the game started, to keep track of day/night
        private Int32 startTime;

        private GameState()
        {
            entities = new Dictionary<UInt32, Entity>();
        }

        // The implementation is as of now theoretical it 
        // consists of mainly psuedo-code and sample methods.

        // Game Updates
    
        public void AddEntity(UInt32 id, Entity entity)
        {
            entities.Add(id, entity);
        }

        public UInt32 CreatePlayer()
        {
            Player newPlayer = GameObject.Instantiate(Resources.Load("Player") as GameObject);
            clients.Add(newPlayer.ID, client);
            AddEntity(newPlayer.ID, newPlayer);
            return newPlayer.ID;
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