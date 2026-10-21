using UnityEngine;
using VEngine.Data;
using UnityEngine.Rendering;

namespace Utilities
{
	public class ScreenManager : Singleton<ScreenManager>
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void OnBeforeSceneLoad()
		{
			if (FindObjectOfType(typeof(ScreenManager)) != null) return;
			var newObj = new GameObject("Screen Manager");
			newObj.AddComponent<ScreenManager>();
			DontDestroyOnLoad(newObj);
		}

		private GameData data;
		private bool firstUpdateFrame = false;
		public void Start()
		{
			firstUpdateFrame = true;
		}

		public void LateUpdate()
		{
			data = (DataManager.Data != null) ? DataManager.Data : new GameData();
			if (firstUpdateFrame)
			{
				WindowResize(data.windowSize);

				firstUpdateFrame = false;
			}
			// handle keeping the window the right aspect and resolution
			if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
			{
				if (Input.GetKeyDown(KeyCode.Return))
				{
					data.fullScreen = !data.fullScreen;
					Cursor.visible = false;
					WindowResize(data.windowSize);
					DataManager.SaveData(data);
				}
			}

			if (Input.GetKeyDown(KeyCode.Minus))
			{
				if (data.windowSize > 1)
					WindowResize(--data.windowSize);
				DataManager.SaveData(data);
			}

			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				QualitySettings.vSyncCount = QualitySettings.vSyncCount == 1 ? 0 : 1;
			}

			if (!Input.GetKeyDown(KeyCode.Equals)) return;
			if (216 * (data.windowSize + 1) < Screen.mainWindowDisplayInfo.height)
				WindowResize(++data.windowSize);
			DataManager.SaveData(data);
		}

		private void WindowResize(int windowSize)
		{
			if (data.fullScreen)
			{
				Screen.SetResolution(Screen.mainWindowDisplayInfo.width, Screen.mainWindowDisplayInfo.height, true);
			}
			else
			{
				Screen.SetResolution(windowSize * 384, windowSize * 216, false);
			}
		}
	}
}

