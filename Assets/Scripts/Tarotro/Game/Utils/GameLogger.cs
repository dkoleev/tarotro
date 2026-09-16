using System;
using System.IO;
using System.Text;
using UnityEngine;
using VContainer;

namespace Tarotro.Game.Utils {
    public class GameLogger : IGameLogger, IDisposable {
        private readonly StreamWriter _writer;
        private readonly bool _enableConsoleLogs;

        [Inject]
        public GameLogger() {
            _enableConsoleLogs =
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                true;
#else
                false;
#endif

            try {
                var logDir = Path.Combine(Application.persistentDataPath, "Logs");
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                CleanOldLogs(logDir, 7);

                var logPath = Path.Combine(logDir, $"{DateTime.Now:yyyy-MM-dd}.log");
                _writer = new StreamWriter(logPath, append: true, Encoding.UTF8) {
                    AutoFlush = false
                };
                _writer.WriteLine($"\n---- Game Log Started: {DateTime.Now} ----");
                _writer.Flush();
            } catch (Exception e) {
                UnityEngine.Debug.LogWarning($"[GameLogger] Failed to initialize file logging: {e.Message}");
            }
        }

        public void Info(string message, string category) =>
            Log(message, LogLevel.Info, category);

        public void Warn(string message, string category) =>
            Log(message, LogLevel.Warning, category);

        public void Error(string message, string category) =>
            Log(message, LogLevel.Error, category);

        public void Debug(string message, string category) =>
            Log(message, LogLevel.Debug, category);

        private void Log(string message, LogLevel level, string category) {
            WriteToFile(message, level, category);

            if (!_enableConsoleLogs) return;

            var formatted = FormatConsole(message, level, category);
            switch (level) {
                case LogLevel.Info:
                    UnityEngine.Debug.Log(formatted);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formatted);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(formatted);
                    break;
                case LogLevel.Debug:
#if UNITY_EDITOR
                    UnityEngine.Debug.Log($"<color=#888888>{formatted}</color>");
#endif
                    break;
            }
        }

        private void WriteToFile(string message, LogLevel level, string category) {
            if (_writer == null) return;

            try {
                _writer.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] [{category}] {message}");

                if (level >= LogLevel.Warning)
                    _writer.Flush();
            } catch {
                // ignored
            }
        }

        private static string FormatConsole(string message, LogLevel level, string category) {
            var color = level switch {
                LogLevel.Info => "#8BC34A",
                LogLevel.Warning => "#FFC107",
                LogLevel.Error => "#F44336",
                LogLevel.Debug => "#9E9E9E",
                _ => "#FFFFFF"
            };

            return $"<color={color}>[{level}] [{category}]</color> {message}";
        }

        private static void CleanOldLogs(string dir, int maxDays) {
            try {
                foreach (var file in Directory.GetFiles(dir, "*.log")) {
                    if (File.GetCreationTime(file) < DateTime.Now.AddDays(-maxDays))
                        File.Delete(file);
                }
            } catch {
                // ignored
            }
        }

        public void Dispose() {
            _writer?.Flush();
            _writer?.Dispose();
        }

        private enum LogLevel {
            Debug,
            Info,
            Warning,
            Error
        }
    }
}
