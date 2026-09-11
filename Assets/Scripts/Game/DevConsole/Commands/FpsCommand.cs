using UnityEngine;

namespace Game.DevConsole.Commands {
    public class FpsCommand : IDevConsoleCommand {
        public string Name => "fps";
        public string Description => "Get or set the target frame rate";
        public string Usage => "fps [value]";

        public string Execute(string[] args) {
            if (args.Length == 0)
                return $"Target: {Application.targetFrameRate}, Current: {Mathf.RoundToInt(1f / Time.unscaledDeltaTime)}";

            if (!int.TryParse(args[0], out var target) || target < -1)
                return "Invalid value. Use -1 for unlimited or a positive integer.";

            Application.targetFrameRate = target;
            return $"Target FPS set to {(target == -1 ? "unlimited" : target.ToString())}";
        }
    }
}
