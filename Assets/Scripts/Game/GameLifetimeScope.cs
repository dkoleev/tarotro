using VContainer;
using VContainer.Unity;

namespace Game {
    public class GameLifetimeScope : LifetimeScope {
        protected override void Configure(IContainerBuilder builder) {
            builder.RegisterEntryPoint<Boot>();
            builder.Register<SceneLoader>(Lifetime.Singleton);
        }
    }
}