﻿using Common.Application;
using UnityEngine;

namespace AsteraX.Application.Tasks.Game
{
    public class MovePlayerShip : IApplicationTask
    {
        public Vector2 Movement { get; set; }
    }
}