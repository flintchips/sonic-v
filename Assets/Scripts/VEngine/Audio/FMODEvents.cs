using UnityEngine;

namespace VEngine.Audio
{
    public class FMODEvents : MonoBehaviour
    {
        private static FMODEvents _instance;

        public void Awake()
        {
            Instantiate();
        }

        public void OnEnable()
        {
            Instantiate();
        }

        private void Instantiate()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(this);
            }
        }
    }
}