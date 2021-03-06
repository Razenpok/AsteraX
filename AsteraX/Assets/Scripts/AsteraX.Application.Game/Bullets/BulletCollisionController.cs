using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AsteraX.Application.Game.Asteroids;
using AsteraX.Application.Tasks.Game;
using AsteraX.Application.Tasks.UI;
using AsteraX.Domain.Game;
using AsteraX.Infrastructure;
using Common.Application;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using static AsteraX.Application.Tasks.Game.SpawnAsteroids;

namespace AsteraX.Application.Game.Bullets
{
    public class BulletCollisionController : MonoBehaviour
    {
        private IAsyncRequestHandler<Command> _commandHandler;

        [Inject]
        public void Construct(IAsyncRequestHandler<Command> commandHandler)
        {
            _commandHandler = commandHandler;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<AsteroidInstance>(out var asteroid))
            {
                Destroy(gameObject);
                var request = new Command {AsteroidId = asteroid.Id};
                _commandHandler.Handle(request).Forget();
            }
        }

        public class Command : IAsyncRequest
        {
            public long AsteroidId { get; set; }
        }

        public class CommandHandler : AsyncRequestHandler<Command>
        {
            private readonly ILevelRepository _levelRepository;
            private readonly IGameSessionRepository _gameSessionRepository;
            private readonly IApplicationTaskPublisher _taskPublisher;

            public CommandHandler(
                ILevelRepository levelRepository,
                IGameSessionRepository gameSessionRepository,
                IApplicationTaskPublisher taskPublisher)
            {
                _levelRepository = levelRepository;
                _gameSessionRepository = gameSessionRepository;
                _taskPublisher = taskPublisher;
            }

            protected override async UniTask Handle(Command command, CancellationToken ct)
            {
                var gameSession = _gameSessionRepository.Get();
                gameSession.CollideAsteroidWithBullet(command.AsteroidId);
                _gameSessionRepository.Save();

                _taskPublisher.PublishTask(new DestroyAsteroid
                {
                    Id = command.AsteroidId
                });

                if (gameSession.GetAsteroids().Count > 0)
                {
                    return;
                }

                var level = _levelRepository.GetLevel();
                gameSession.StartLevel(level);
                _gameSessionRepository.Save();

                var asteroids = gameSession.GetAsteroids();

                var showLoadingScreen = new ShowLoadingScreen
                {
                    Id = (int) level.Id,
                    Asteroids = level.AsteroidCount,
                    Children = level.AsteroidChildCount
                };
                var spawnAsteroids = new SpawnAsteroids
                {
                    Asteroids = ToSpawnAsteroidsDto(asteroids)
                };

                await _taskPublisher.PublishAsyncTask(showLoadingScreen, ct);
                _taskPublisher.PublishTask(spawnAsteroids);
                await _taskPublisher.PublishAsyncTask(new HideLoadingScreen(), ct);
                _taskPublisher.PublishTask(new EnablePlayerInput());
                _taskPublisher.PublishTask(new UnpauseGame());
            }

            private static List<AsteroidDto> ToSpawnAsteroidsDto(IEnumerable<Asteroid> asteroids)
            {
                return asteroids.Select(a => new AsteroidDto
                {
                    Id = a.Id,
                    Size = a.Size,
                    Children = ToSpawnAsteroidsDto(a.Children)
                }).ToList();
            }
        }
    }
}