using System.Collections.Generic;
using Objects.Player.Constants;
using UnityEngine;
using UnityEditor;

namespace VEngine.Animation
{
	[AddComponentMenu("SonicV/VAnimation")]
	[System.Serializable]
	public class Animation
	{
		[HideInInspector] private string name => animationID.ToString();
		public AnimationType animationID;

		public List<Sprite> frames;

		public Animation(AnimationType animationID, List<Sprite> frames)
		{
			this.animationID = animationID;
			this.frames = frames;
		}
	}
}