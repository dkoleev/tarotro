using TMPro;
using UnityEngine;

namespace Game.View {
    public class EnemyView : MonoBehaviour {
        [SerializeField] private TMP_Text _healthText;

        public void SetHealth(int amount) {
            _healthText.text = amount.ToString();
        }

        public void UpdateHealth(int amount) {
            _healthText.text = amount.ToString();
        }

        public void PlayDeathAnimation() {
        }
    }
}
