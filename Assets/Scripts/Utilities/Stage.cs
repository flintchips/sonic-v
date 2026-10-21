using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VEngine.Audio;
using VEngine.Entity;
using VEngine.Camera;
using System;
using UnityEngine.SceneManagement;

namespace Utilities
{
	public class Stage : MonoBehaviour
	{
		public static Stage Instance;
		public string levelMusic;
		public float songBpm = 120;
		public int curBeat;
		public float beatProgress;
		public int ringCount = 0;

		public List<Objects.Stage.StageObject> loadedObjects;
		public List<Entity> loadedEntities;

		public void Awake()
		{
			Instantiate();
			loadedEntities = FindObjectsOfType<Entity>().ToList();
		}

		public void OnEnable()
		{
			loadedObjects.Clear();

			Instantiate();

			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		public void OnDisable()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		public void CalculateBpmStuff()
		{
			float beatLength = (60 / songBpm);
			beatProgress = (Time.time % beatLength) / beatLength;
			curBeat = Mathf.FloorToInt(Time.time / beatLength);
		}

		void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			AudioManager.InitializeLevelMusic(levelMusic);
		}

		private void Instantiate()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else if (Instance != this)
			{
				Destroy(this);
			}
		}

		public void FixedUpdate()
		{
			CalculateBpmStuff();
			var tempIndex = 0;
			var loadedObjectsStore = loadedObjects;
			for (var i = 0; i < loadedObjectsStore.Count; i++)
			{
				if (loadedObjectsStore[i].loaded) continue;
				loadedObjects.RemoveAt(i - tempIndex);
				tempIndex++;
			}

			Objects.Stage.StageObject[] stageObjects = FindObjectsByType(typeof(Objects.Stage.StageObject), FindObjectsSortMode.None) as Objects.Stage.StageObject[];
			foreach (Objects.Stage.StageObject stageObject in stageObjects)
			{
				stageObject.StageFixedUpdate();
			}
			foreach (Objects.Stage.StageObject stageObject in stageObjects)
			{
				stageObject.StageLateFixedUpdate();
			}
		}
	}
}