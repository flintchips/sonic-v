using UnityEngine;
using UnityDebug = UnityEngine.Debug;

namespace VEngine.Utilities.Integrations.Discord
{
    public class DiscordRichPresence : MonoBehaviour
    {
        public long applicationID;
        [Space] public string details = "making new bugs to fix later";
        public string state = "";
        [Space] public string largeImage = "image";
        public string largeText = "SonicV";

        private long time;
        private global::Discord.Discord discord;
        private static DiscordRichPresence _instance;

        private void Awake()
        {
            if (!_instance)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (Application.isPlaying)
                    Destroy(gameObject);
            }
        }

        public void Update()
        {
            try
            {
                _instance.discord.RunCallbacks();
            }
            catch
            {
                if (Application.isPlaying)
                    Destroy(gameObject);
            }
        }

        private void Start()
        {
            try
            {
                state = "Running Test";
                discord = new global::Discord.Discord(applicationID,
                    (System.UInt64)global::Discord.CreateFlags.NoRequireDiscord);
                time = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
            catch
            {
                Destroy(gameObject);
            }
        }

        private void LateUpdate()
        {
            UpdateStatus();
        }

        private void OnApplicationQuit()
        {
            try
            {
                state = "In The Editor";
                if (discord != null)
                {
                    UpdateStatus();
                }
            }
            catch
            {
                Destroy(gameObject);
            }
        }

        private void UpdateStatus()
        {
            try
            {
                var activityManager = discord.GetActivityManager();
                var activity = new global::Discord.Activity
                {
                    Details = details,
                    Assets =
                    {
                        LargeImage = largeImage,
                        LargeText = largeText
                    },
                    Timestamps =
                    {
                        Start = time
                    }
                };

                activityManager.UpdateActivity(activity, (res) =>
                {
                    if (res != global::Discord.Result.Ok) UnityDebug.LogWarning("Connection to Discord has failed!");
                });
            }
            catch
            {
                if (Application.isPlaying)
                    Destroy(gameObject);
            }
        }
    }
}