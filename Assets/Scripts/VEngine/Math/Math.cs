using UnityEngine;

namespace VEngine.Math
{
    public class Math : MonoBehaviour
    {
        public static float BitShift(float shift)
        {
            return (Mathf.Pow(2, -shift));
        }
    }
}