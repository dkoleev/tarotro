using MessagePipe;
using Tarotro.Game.Core;
using Tarotro.Game.Data;
using Tarotro.Game.Logic.Rng;
using Tarotro.Game.Messages;
using Tarotro.Game.Utils;
using VContainer;
using VContainer.Unity;
#if UNITY_EDITOR || DEBUG
using QFSW.QC;
using Tarotro.Game.Dev;
#endif

namespace Tarotro.Game.Logic {
    public class GameLifetimeScope : LifetimeScope {
        protected override void Configure(IContainerBuilder builder) {
            builder.RegisterEntryPoint<Boot>();
            
            builder.Register<GameLogger>(Lifetime.Singleton).As<IGameLogger>();
            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.Register<Battle>(Lifetime.Singleton);
            builder.Register<BattleProgressionManager>(Lifetime.Singleton);
            builder.Register<ScoreManager>(Lifetime.Singleton);
            builder.Register<SaveManager>(Lifetime.Singleton);
            builder.Register<AutoSaveHandler>(Lifetime.Singleton);
            builder.Register<ConfigLoader>(Lifetime.Singleton);
            builder.Register<GameData>(Lifetime.Singleton);
            builder.Register<GameRng>(Lifetime.Singleton);
            
            builder.RegisterBuildCallback(container => container.Resolve<AutoSaveHandler>());

#if UNITY_EDITOR || DEBUG
            builder.Register<DevConsoleCommands>(Lifetime.Singleton);
            builder.RegisterBuildCallback(container =>
            {
                var debugCommands = container.Resolve<DevConsoleCommands>();
                QuantumRegistry.RegisterObject(debugCommands);
            });
#endif
            
            //ScoreManager is registered as Lifetime.Singleton, but VContainer only creates singletons when something first
            //So call this for force resolve
            //For experiment
            // builder.RegisterBuildCallback(container => container.Resolve<ScoreManager>());

            var options = builder.RegisterMessagePipe();
            builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));
            builder.RegisterMessageBroker<EnemyDiedMessage>(options);
        }
    }
}
