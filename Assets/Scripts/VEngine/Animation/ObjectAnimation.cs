using System.Collections.Generic;
using UnityEngine;

namespace VEngine.Animation
{
    [System.Serializable]
    [AddComponentMenu("SonicV/ObjectAnimation")]
    public class ObjectAnimation
    {
        public string name;
        public List<Sprite> frames;

        public ObjectAnimation(string name, List<Sprite> frames)
        {
            this.name = name;
            this.frames = frames;
        }
    }
}