using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Tarotro.Game.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace Tarotro.Game.Core {
    public class ConfigLoader {
        private readonly GameData _gameData;
        private const string TarotCardConfigPath = "Configs/tarot_cards.json";
        private const string EnemiesConfigPath = "Configs/enemies.json";
        private const string BattleConfigPath = "Configs/battle.json";
        private const string CirclesConfigPath = "Configs/circles.json";
        private const string CharactersConfigPath = "Configs/characters.json";

        [Inject]
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

            handle = Addressables.LoadAssetAsync<TextAsset>(CirclesConfigPath);
            jsonFile = await handle.ToUniTask();
            _gameData.Circles = JsonConvert.DeserializeObject<Dictionary<CircleType, CircleData>>(jsonFile.text);

            handle = Addressables.LoadAssetAsync<TextAsset>(CharactersConfigPath);
            jsonFile = await handle.ToUniTask();
            _gameData.Characters = JsonConvert.DeserializeObject<Dictionary<string, CharacterData>>(jsonFile.text);
            
            handle = Addressables.LoadAssetAsync<TextAsset>(BattleConfigPath);
            jsonFile = await handle.ToUniTask();
            _gameData.Battle = JsonConvert.DeserializeObject<BattleData>(jsonFile.text);

            Addressables.Release(handle);
        }
    }
}
