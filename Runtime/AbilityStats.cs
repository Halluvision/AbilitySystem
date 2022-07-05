using UnityEngine;

namespace Halluvision.AbilitySystem
{
    public class AbilityStats : MonoBehaviour
    {
        [SerializeField]
        private float _energy;
        public float Energy { get { return _energy; } }

        public void ConsumeEnergy(float energyCost)
        {
            _energy -= energyCost;
        }
    }
}
