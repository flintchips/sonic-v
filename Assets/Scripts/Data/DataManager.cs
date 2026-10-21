using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utilities;

namespace VEngine.Data
{
	public class DataManager : Singleton<DataManager>
	{		
		public static GameData Data;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void OnBeforeSceneLoad()
		{
			if(FindObjectOfType(typeof(DataManager)) != null) return;
			GameObject newObj = new GameObject("DataManager");
            newObj.AddComponent<DataManager>();
            Data = LoadData();
			DontDestroyOnLoad(newObj);
			
		}
		
		public static void SaveData(GameData data)
		{
			FileHandler.SaveToJSON(data, "GameData.Json");
			Data = data;
		}
		
		public static GameData LoadData()
		{
			Data = FileHandler.ReadFromJSON<GameData>("GameData.Json");
			return Data;
		}
	}
}

