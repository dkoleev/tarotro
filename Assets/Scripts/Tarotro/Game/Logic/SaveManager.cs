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

        private readonly IGameLogger _logger;

        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        [Inject]
        public SaveManager(IGameLogger logger) {
            _logger = logger;
        }

        public async UniTask SaveAsync(GameSaveData data, CancellationToken ct = default) {
            data.Version = SaveVersion;
            var bytes = MemoryPackSerializer.Serialize(data);
            await File.WriteAllBytesAsync(SavePath, bytes, ct);
            _logger.Info($"Game saved ({bytes.Length} bytes)", "Save");
        }

        public async UniTask<GameSaveData> LoadAsync(CancellationToken ct = default) {
            if (!HasSave()) {
                _logger.Info("No save file found", "Save");
                return null;
            }

            var bytes = await File.ReadAllBytesAsync(SavePath, ct);
            var data = MemoryPackSerializer.Deserialize<GameSaveData>(bytes);

            if (data == null) {
                _logger.Error("Failed to deserialize save data", "Save");
                return null;
            }

            _logger.Info($"Game loaded (version {data.Version}, {bytes.Length} bytes)", "Save");
            return data;
        }

        public bool HasSave() {
            return File.Exists(SavePath);
        }

        public void DeleteSave() {
            if (!File.Exists(SavePath)) return;

            File.Delete(SavePath);
            _logger.Info("Save file deleted", "Save");
        }
    }
}
