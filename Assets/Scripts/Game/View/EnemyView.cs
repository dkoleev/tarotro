using TMPro;
using UnityEngine;

namespace Game.View {
    public class EnemyView : MonoBehaviour {
        [SerializeField] private TMP_Text healthText;

        public void SetHealth(int amount) {
            healthText.text = amount.ToString();
        }

        public void UpdateHealth(int amount)
        {
            healthText.text = amount.ToString();
        }

        public void PlayDeathAnimation()
        {
            
        }
    }
}
