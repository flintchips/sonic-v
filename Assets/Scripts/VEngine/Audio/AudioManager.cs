using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using System.Collections.Generic;

namespace VEngine.Audio
{
	public class AudioManager : MonoBehaviour
	{
		private static AudioManager Instance { get; set; }
		private static EventInstance _musicReference;
		private static List<EventInstance> _persistantEvents = new List<EventInstance>();

		public void Awake()
		{
			if (Instance != null)
			{
				Debug.LogError("More than one audio manager found in the scene!");
			}

			Instance = this;
		}

		public void Update()
		{
			// remove any persistant events that are done playing to save size in the list
			int p = 0; // position in the list the loop should use after removing something from the list
			int count = _persistantEvents.Count;
			for (int i = 0; i < count; i++)
			{
				if (HasStopped(_persistantEvents[p]))
				{
					_persistantEvents.RemoveAt(p);
				}
				else
				{
					p++;
				}
			}
		}

		public static EventInstance GetPersistantEventInstance(string eventName)
		{
			var fullPath = "event:/" + eventName;
			for (int i = 0; i < _persistantEvents.Count; i++)
			{
				string eventPath = "";
				EventDescription dsc;
				_persistantEvents[i].getDescription(out dsc);
				dsc.getPath(out eventPath);

				if (eventPath == fullPath)
				{
					return _persistantEvents[i];
				}
			}
			return default(EventInstance);
		}

		public static bool HasStopped(EventInstance instance)
		{
			FMOD.Studio.PLAYBACK_STATE state;
			instance.getPlaybackState(out state);
			return state == FMOD.Studio.PLAYBACK_STATE.STOPPED;
		}

		public static void InitializeLevelMusic(string eventRef)
		{
			var fullPath = "event:/" + eventRef;
			_musicReference = RuntimeManager.CreateInstance(fullPath);
			_musicReference.start();
		}

		public static void PlayOneShot(string eventRef)
		{
			var fullPath = "event:/" + eventRef;
			RuntimeManager.PlayOneShot(fullPath);
		}

		public static void PlayPersistant(string eventRefName)
		{
			var fullPath = "event:/" + eventRefName;
			bool exists = false;
			for (int i = 0; i < _persistantEvents.Count; i++)
			{
				string eventPath = "";
				EventDescription dsc;
				_persistantEvents[i].getDescription(out dsc);
				dsc.getPath(out eventPath);

				if (eventPath == null || eventPath != fullPath)
				{
					_persistantEvents[i] = RuntimeManager.CreateInstance(fullPath);
				}
				else
				{
					exists = true;
				}
			}

			if (!exists)
			{
				_persistantEvents.Add(RuntimeManager.CreateInstance(fullPath));
			}

			for (int i = 0; i < _persistantEvents.Count; i++)
			{
				string eventPath = "";
				EventDescription dsc;
				_persistantEvents[i].getDescription(out dsc);
				dsc.getPath(out eventPath);

				if (eventPath == fullPath)
				{
					_persistantEvents[i].stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
					_persistantEvents[i].start();
				}
			}
		}

		public static void StopPersistant(string eventRefName)
		{
			var fullPath = "event:/" + eventRefName;
			for (int i = 0; i < _persistantEvents.Count; i++)
			{
				string eventPath = "";
				EventDescription dsc;
				_persistantEvents[i].getDescription(out dsc);
				dsc.getPath(out eventPath);

				if (eventPath == fullPath)
				{
					_persistantEvents[i].stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
				}
			}
		}


	}
}