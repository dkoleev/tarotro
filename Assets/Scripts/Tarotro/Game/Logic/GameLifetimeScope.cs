using MessagePipe;
using Tarotro.Game.Core;
using Tarotro.Game.Data;
using Tarotro.Game.Logic.Motion;
using Tarotro.Game.Logic.Rng;
using Tarotro.Game.Logic.Sequencing;
using Tarotro.Game.Messages;
using Tarotro.Game.Presenters;
using Tarotro.Game.Utils;
using UnityEngine;
using VContainer;
using VContainer.Unity;
#if UNITY_EDITOR || DEBUG
using QFSW.QC;
using Tarotro.Game.Dev;
#endif

namespace Tarotro.Game.Logic {
    public class GameLifetimeScope : LifetimeScope {
        [SerializeField] private MotionTuning motionTuning;
        [SerializeField] private CardHandConfig cardHandConfig;

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
            builder.Register<CardHand>(Lifetime.Singleton);
            builder.Register<CardHandPresenter>(Lifetime.Singleton);

            if (motionTuning != null)
                builder.RegisterInstance(motionTuning);
            else
                builder.RegisterInstance(MotionTuning.Default);

            if (cardHandConfig != null)
                builder.RegisterInstance(cardHandConfig);
            builder.Register<EventQueue>(Lifetime.Singleton);
            builder.Register<MotionSystem>(Lifetime.Singleton);
            builder.Register<GameLoopRunner>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            builder.RegisterBuildCallback(container => container.Resolve<AutoSaveHandler>());
            builder.RegisterBuildCallback(container => {
                var cardHandPresenter = container.Resolve<CardHandPresenter>();
                var runner = container.Resolve<GameLoopRunner>();
                cardHandPresenter.SetViewCallbacks(runner.RegisterView, runner.UnregisterView);
            });

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
