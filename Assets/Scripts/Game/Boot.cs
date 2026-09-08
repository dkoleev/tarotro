using System;
using UnityEngine;

namespace Game {
    public class Boot : MonoBehaviour {
        private void Start() {
            try {
                Steamworks.SteamClient.Init(252490);
            }
            catch (Exception e) {
                Debug.LogException(e);
            }
        }
    }
}
