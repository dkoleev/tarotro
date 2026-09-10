using Game.Core;
using Game.Data;
using Game.Messages;
using MessagePipe;
using VContainer;
using VContainer.Unity;

namespace Game.Logic {
    public class GameLifetimeScope : LifetimeScope {
        protected override void Configure(IContainerBuilder builder) {
            builder.RegisterEntryPoint<Boot>();
            builder.Register<SceneLoader>(Lifetime.Singleton);
            builder.Register<Battle>(Lifetime.Singleton);
            builder.Register<ScoreManager>(Lifetime.Singleton);
            builder.Register<ConfigLoader>(Lifetime.Singleton);
            builder.Register<GameData>(Lifetime.Singleton);

            //ScoreManager is registered as Lifetime.Singleton, but VContainer only creates singletons when something first
            //So call this for force resolve
            //For experiment
            builder.RegisterBuildCallback(container => container.Resolve<ScoreManager>());

            var options = builder.RegisterMessagePipe();
            builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));
            builder.RegisterMessageBroker<EnemyDiedMessage>(options);
        }
    }
}
