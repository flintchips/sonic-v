using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public static class FileHandler
{
	public static void SaveToJSON<T>(T toSave, string filename) 
	{
		string content = JsonUtility.ToJson(toSave, true);	
		WriteFile(GetPath(filename), content);
	}

	public static T ReadFromJSON<T>(string filename)
	{
		string content = ReadFile(GetPath(filename));

		if(string.IsNullOrEmpty(content) || content == "{}" )
		{
			Debug.Log("burh");
			return default;
		}

		T res = JsonUtility.FromJson<T>(content);

		return res;
	}

	private static string GetPath(string filename)
	{
		return Application.persistentDataPath + "/" + filename;
	}

	private static void WriteFile(string path, string content)
	{
		FileStream fileStream = new FileStream(path, FileMode.Create);

		using (StreamWriter writer = new StreamWriter(fileStream))
		{
			writer.Write(content);
		}
	}

	private static string ReadFile(string path)
	{
		if(File.Exists(path)){
			using (StreamReader reader = new StreamReader(path))
			{
				string content = reader.ReadToEnd();
				return content;
			}
		}
		return "";
	}
}