using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MemoryPack;
using Tarotro.Game.Data.Save;
using Tarotro.Game.Utils;
using UnityEngine;
using VContainer;

namespace Tarotro.Game.Logic {
    public class SaveManager {
        private const int SaveVersion = 1;
        private const string SaveFileName = "save.bin";
        private const string TempSuffix = ".tmp";
        private const string BackupSuffix = ".bak";

        private readonly IGameLogger _logger;

        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        private string TempPath => SavePath + TempSuffix;
        private string BackupPath => SavePath + BackupSuffix;

        [Inject]
        public SaveManager(IGameLogger logger) {
            _logger = logger;
        }

        public async UniTask<bool> SaveAsync(GameSaveData data, CancellationToken ct = default) {
            data.Version = SaveVersion;

            try {
                var bytes = MemoryPackSerializer.Serialize(data);

                await File.WriteAllBytesAsync(TempPath, bytes, ct);

                if (File.Exists(SavePath)) {
                    File.Replace(TempPath, SavePath, BackupPath);
                } else {
                    File.Move(TempPath, SavePath);
                }

                _logger.Info($"Game saved ({bytes.Length} bytes)", "Save");
                return true;
            } catch (Exception ex) {
                _logger.Error($"Failed to save game: {ex.Message}", "Save");
                CleanupTempFile();
                return false;
            }
        }

        public async UniTask<GameSaveData> LoadAsync(CancellationToken ct = default) {
            var data = await TryLoadFile(SavePath, ct);
            if (data != null) return data;

            _logger.Warning("Main save failed, trying backup", "Save");
            data = await TryLoadFile(BackupPath, ct);
            if (data != null) return data;

            _logger.Error("All save files are corrupted or missing", "Save");
            return null;
        }

        public bool HasSave() {
            return File.Exists(SavePath) || File.Exists(BackupPath);
        }

        public void DeleteSave() {
            DeleteIfExists(SavePath);
            DeleteIfExists(BackupPath);
            DeleteIfExists(TempPath);
            _logger.Info("Save files deleted", "Save");
        }

        private async UniTask<GameSaveData> TryLoadFile(string path, CancellationToken ct) {
            if (!File.Exists(path)) return null;

            try {
                var bytes = await File.ReadAllBytesAsync(path, ct);
                var data = MemoryPackSerializer.Deserialize<GameSaveData>(bytes);
                if (data == null) return null;

                _logger.Info($"Loaded from {Path.GetFileName(path)} (v{data.Version}, {bytes.Length} bytes)", "Save");
                return data;
            } catch (Exception ex) {
                _logger.Error($"Failed to load {Path.GetFileName(path)}: {ex.Message}", "Save");
                return null;
            }
        }

        private void CleanupTempFile() {
            DeleteIfExists(TempPath);
        }

        private static void DeleteIfExists(string path) {
            if (File.Exists(path)) {
                File.Delete(path);
            }
        }
    }
}
