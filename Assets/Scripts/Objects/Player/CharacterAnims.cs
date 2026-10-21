using System.Collections.Generic;
using Objects.Player.Constants;
using UnityEngine;
using UnityEditor;
using Animation = VEngine.Animation.Animation;

namespace Objects.Player
{
	[System.Serializable]
	[CreateAssetMenu(fileName = "CharacterAnims", menuName = "Sonic-V/CharacterAnims", order = 0)]
	public class CharacterAnims : ScriptableObject
	{
		public CharacterType character;
		public List<Animation> animations;
	}
}