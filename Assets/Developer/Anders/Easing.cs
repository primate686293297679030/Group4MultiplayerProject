using UnityEngine;

namespace EasingFunctions
{
    public class Ease
    {
        public static float InOutSine(float x)
        {
            return -(Mathf.Cos(Mathf.PI* x) - 1) / 2;
        }
    public static float Out(float x, int power)
        {
            return 1 - Mathf.Pow(1 - x, power);
        }
        public static float In(float x, int power)
        {
            return Mathf.Pow(x, power);
        }
        public static float InOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;

            return x < 0.5
                ? (Mathf.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
                : (Mathf.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
        }
    }
}

