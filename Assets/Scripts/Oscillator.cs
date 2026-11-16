using UnityEngine;

namespace NekoLongWayHome
{
    [RequireComponent(typeof(Rigidbody))]
    public class Oscillator : MonoBehaviour, IKinematicMover
    {
        public enum Mode
        {
            Sine,
            Punch
        }

        // One speed control for ALL platforms
        public static float globalSpeed = 1f;

        [Header("Movement Settings")]
        public Mode mode = Mode.Sine;

        [Tooltip("X = left/right, Y = up/down, Z = forward/back.")]
        public Vector3 offset = new Vector3(3f, 0f, 0f);

        [Tooltip("Seconds for one full back-and-forth at speed 1. Higher = slower.")]
        [Min(0.01f)]
        public float period = 25f;

        [Tooltip("Speed for this platform. 1 = normal.")]
        [Min(0.01f)]
        public float speed = 25f;

        [Range(0f, 1f)]
        public float phase;

        [Header("Sequence (one by one)")]
        public int stepIndex = 0;
        public int stepCount = 1;

        private Rigidbody rb;
        private Vector3 start;
        private Vector3 velocity;
        private float clock;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();

            rb.isKinematic = true;
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            start = rb.position;

            rb.position = Evaluate(0f);
        }

        // Extra-smooth easing: zero speed at both ends, no sudden changes
        static float SmootherStep(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * x * (x * (x * 6f - 15f) + 10f);
        }

        Vector3 Evaluate(float t)
        {
            float seqPhase = stepCount > 1 ? (float)stepIndex / stepCount : 0f;

            float cycle = Mathf.Repeat(
                t / Mathf.Max(period, 0.01f) + phase + seqPhase,
                1f
            );

            float f;

            if (mode == Mode.Sine)
            {
                // Smooth glide: -1 to 1 and back
                f = Mathf.Sin(cycle * 2f * Mathf.PI);
            }
            else
            {
                // Smooth move with short pauses at the ends
                if (cycle < 0.05f)
                    f = 0f;

                else if (cycle < 0.45f)
                    f = SmootherStep((cycle - 0.05f) / 0.40f);

                else if (cycle < 0.55f)
                    f = 1f;

                else if (cycle < 0.95f)
                    f = 1f - SmootherStep((cycle - 0.55f) / 0.40f);

                else
                    f = 0f;
            }

            return start + offset * f;
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            clock += dt * speed * globalSpeed;

            Vector3 next = Evaluate(clock);

            velocity = (next - rb.position) / dt;

            rb.MovePosition(next);
        }

        public Vector3 GetPointVelocity(Vector3 worldPoint)
        {
            return velocity;
        }
    }
}