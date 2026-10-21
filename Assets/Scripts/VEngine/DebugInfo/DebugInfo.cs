using System.Globalization;
using Objects.Player;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace VEngine.DebugInfo
{
	public class DebugInfo : MonoBehaviour
	{
		public Text debugText;
		public float deltaTime;

		private void Update()
		{
			deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
			var fps = 1.0f / deltaTime;
			var fpsText = $"FPS {Mathf.Ceil(fps).ToString(CultureInfo.InvariantCulture)}\n";
			var playerStateText = $"Cur State: {PlayerVars.Leader.CurrentState}\n";
			var ringCountText = $"Rings: {Stage.Instance.ringCount}\n";
			var timeText = $"Time: {Mathf.FloorToInt(Time.time)}\n";
			debugText.text = fpsText + playerStateText + ringCountText + timeText;
		}
	}
}