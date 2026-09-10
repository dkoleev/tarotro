using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Data;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Core {
    public class ConfigLoader {
        private readonly GameData _gameData;
        private const string TarotCardConfigPath = "Configs/tarot_cards.json";
        private const string EnemiesConfigPath = "Configs/enemies.json";

        public ConfigLoader(GameData gameData) {
            _gameData = gameData;
        }

        public async UniTask Load() {
            var handle = Addressables.LoadAssetAsync<TextAsset>(TarotCardConfigPath);
            var jsonFile = await handle.ToUniTask();
            _gameData.TarotCards = JsonConvert.DeserializeObject<Dictionary<string, TarotCardData>>(jsonFile.text);

            handle = Addressables.LoadAssetAsync<TextAsset>(EnemiesConfigPath);
            jsonFile = await handle.ToUniTask();
            _gameData.Enemies = JsonConvert.DeserializeObject<Dictionary<string, EnemyData>>(jsonFile.text);

            Addressables.Release(handle);
        }
    }
}
