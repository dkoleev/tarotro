using Game.Data;
using Game.DevConsole.Commands;
using UnityEngine;
using VContainer;

namespace Game.DevConsole {
    public static class DevConsoleInstaller {
        public static void Install(IContainerBuilder builder) {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return;
#endif
            builder.RegisterBuildCallback(container => {
                var console = container.Resolve<DevConsole>();

                console.RegisterCommand(new HelpCommand(console));
                console.RegisterCommand(new ClearCommand(console));
                console.RegisterCommand(new TimeScaleCommand());
                console.RegisterCommand(new FpsCommand());
                console.RegisterCommand(new GameInfoCommand(container.Resolve<GameData>()));

                var go = new GameObject("DevConsole");
                Object.DontDestroyOnLoad(go);
                var view = go.AddComponent<DevConsoleView>();
                view.Initialize(console);
            });
        }
    }
}
