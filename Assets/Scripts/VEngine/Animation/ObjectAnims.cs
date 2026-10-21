using System.Collections.Generic;
using UnityEngine;

namespace VEngine.Animation
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "ObjectAnims", menuName = "Sonic-V/ObjectAnims", order = 0)]
    public class ObjectAnims : ScriptableObject
    {
        public List<ObjectAnimation> animations;
    }
}