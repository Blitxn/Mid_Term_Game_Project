using UnityEngine;

namespace NekoLongWayHome
{
    public class GlobalPlatformSpeed : MonoBehaviour
    {
        [Range(0.05f, 2f)]
        public float speed = 25f;

        void Awake()  { Oscillator.globalSpeed = speed; }
        void Update() { Oscillator.globalSpeed = speed; }
    }
}