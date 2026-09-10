using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Data;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Core {
    public class ConfigLoader {
        private const string TarotCardConfigPath = "Configs/tarot_cards.json";

        public async UniTask<Dictionary<string, TarotCardData>> LoadTarotCards() {
            AsyncOperationHandle<TextAsset> handle = default;
            try {
                handle = Addressables.LoadAssetAsync<TextAsset>(TarotCardConfigPath);
                var jsonFile = await handle.ToUniTask();

                return JsonConvert.DeserializeObject<Dictionary<string, TarotCardData>>(jsonFile.text);

                // GameLogger.Info($"✅ Loaded {_quests.Count} quest entries", "⏳ Loading");
            }
            catch (System.Exception e) {
                return null;
                // GameLogger.Error($"❌ Failed to load fish_data.json: {e.Message}", "⏳ Loading");
            }
            finally {
                if (handle.IsValid()) {
                    Addressables.Release(handle);
                }
            }
        }
    }
}
