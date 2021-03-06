﻿using System;
using System.Linq;
using AsteraX.Domain.Game;
using FluentAssertions;
using NUnit.Framework;

namespace AsteraX.Domain.Tests
{
    public class GameSessionTests
    {
        [Test]
        public void Game_session_cannot_be_created_with_negative_jumps()
        {
            const int initialJumps = -1;

            Func<GameSession> act = () => new GameSession(initialJumps);

            act.Should().Throw("Number of jumps is negative");
        }

        [Test]
        public void Collision_of_asteroid_and_player_ship_destroys_player_ship()
        {
            var (session, asteroid) = CreateGameSessionWithOneAsteroid();

            session.CollideAsteroidWithPlayerShip(asteroid.Id);

            session.IsPlayerAlive.Should().BeFalse();
            session.DomainEvents.Should().ContainSingle(e => e is PlayerShipDestroyedEvent);
        }

        [Test]
        public void Collision_of_asteroid_and_player_ship_destroys_asteroid()
        {
            var (session, asteroid) = CreateGameSessionWithOneAsteroid();

            session.CollideAsteroidWithPlayerShip(asteroid.Id);

            session.GetAsteroids().Should().NotContain(asteroid);
        }

        [Test]
        public void Collision_of_asteroid_and_player_ship_decreases_jumps()
        {
            const int initialJumps = 3;
            var (session, asteroid) = CreateGameSessionWithOneAsteroid(initialJumps);

            session.CollideAsteroidWithPlayerShip(asteroid.Id);

            const int expectedJumps = 2;
            session.Jumps.Should().Be(expectedJumps);
        }

        [Test]
        public void Collision_of_asteroid_and_player_ship_doesnt_decrease_jumps_below_zero()
        {
            const int initialJumps = 0;
            var (session, asteroid) = CreateGameSessionWithOneAsteroid(initialJumps);

            session.CollideAsteroidWithPlayerShip(asteroid.Id);

            const int expectedJumps = 0;
            session.Jumps.Should().Be(expectedJumps);
        }

        [Test]
        public void Collision_of_asteroid_and_player_ship_causes_game_over_when_there_are_no_jumps_remaining()
        {
            const int initialJumps = 0;
            var (session, asteroid) = CreateGameSessionWithOneAsteroid(initialJumps);

            session.CollideAsteroidWithPlayerShip(asteroid.Id);

            session.IsOver.Should().BeTrue();
            session.DomainEvents.Should().ContainSingle(e => e is GameOverEvent);
        }

        [Test]
        public void Collision_of_asteroid_and_player_ship_doesnt_cause_game_over_when_there_are_jumps_remaining()
        {
            const int initialJumps = 3;
            var (session, asteroid) = CreateGameSessionWithOneAsteroid(initialJumps);

            session.CollideAsteroidWithPlayerShip(asteroid.Id);

            session.IsOver.Should().BeFalse();
            session.DomainEvents.Should().NotContain(e => e is GameOverEvent);
        }

        [Test]
        public void Collision_of_asteroid_and_player_ship_doesnt_increase_score()
        {
            var (session, asteroid) = CreateGameSessionWithOneAsteroid();

            session.CollideAsteroidWithPlayerShip(asteroid.Id);

            const int expectedScore = 0;
            session.Score.Should().Be(expectedScore);
        }

        [Test]
        public void Collision_of_asteroid_and_bullet_destroys_asteroid()
        {
            var (session, asteroid) = CreateGameSessionWithOneAsteroid();

            session.CollideAsteroidWithBullet(asteroid.Id);

            session.GetAsteroids().Should().NotContain(asteroid);
        }

        [Test]
        public void Collision_of_asteroid_and_bullet_increases_score_by_asteroid_score()
        {
            var (session, asteroid) = CreateGameSessionWithOneAsteroid();

            session.CollideAsteroidWithBullet(asteroid.Id);

            var expectedScore = asteroid.Score;
            session.Score.Should().Be(expectedScore);
        }

        [Test]
        public void Player_is_alive_after_respawn()
        {
            var (session, asteroid) = CreateGameSessionWithOneAsteroid();
            session.CollideAsteroidWithPlayerShip(asteroid.Id);

            session.RespawnPlayer();

            session.IsPlayerAlive.Should().BeTrue();
        }

        private static (GameSession, Asteroid) CreateGameSessionWithOneAsteroid()
        {
            const int initialJumps = 3;
            return CreateGameSessionWithOneAsteroid(initialJumps);
        }

        private static (GameSession, Asteroid) CreateGameSessionWithOneAsteroid(int jumps)
        {
            var session = new GameSession(jumps);
            var level = CreateLevelWithOneAsteroid();
            session.StartLevel(level);
            var asteroid = session.GetAsteroids().First();
            return (session, asteroid);
        }

        private static Level CreateLevelWithOneAsteroid()
            => new Level(1, 1, 0);
    }
}