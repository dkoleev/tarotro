using UnityEngine;

namespace Game
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private int _health;

        public int Health => _health;
    }
}
