using UnityEngine;

namespace Game.DevConsole.Commands {
    public class TimeScaleCommand : IDevConsoleCommand {
        public string Name => "timescale";
        public string Description => "Get or set the game time scale";
        public string Usage => "timescale [value]";

        public string Execute(string[] args) {
            if (args.Length == 0)
                return $"Time.timeScale = {Time.timeScale}";

            if (!float.TryParse(args[0], out var scale) || scale < 0f)
                return "Invalid value. Usage: timescale <0.0 - 10.0>";

            scale = Mathf.Clamp(scale, 0f, 10f);
            Time.timeScale = scale;
            return $"Time.timeScale set to {scale}";
        }
    }
}
