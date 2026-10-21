using UnityEngine;

namespace Objects.Player.Input
{
	public class PlayerInput : MonoBehaviour
	{
		private static PlayerObject playerObject => PlayerVars.Leader != null ? PlayerVars.Leader : null;
		public ControlInput control = new ControlInput();
		public void InputUpdate()
		{
			UpdateControl();
			playerObject.control = control;
		}

		private void UpdateControl()
		{
			if (playerObject == null) return;
			if (UnityEngine.Input.GetKeyDown(KeyCode.A) || UnityEngine.Input.GetKeyDown(KeyCode.S))
				control.jumpPress = true;
			if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
				control.q = true;

			control.jumpHold = UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.S);
			control.up = UnityEngine.Input.GetKey(KeyCode.UpArrow);
			control.down = UnityEngine.Input.GetKey(KeyCode.DownArrow);
			control.left = UnityEngine.Input.GetKey(KeyCode.LeftArrow);
			control.right = UnityEngine.Input.GetKey(KeyCode.RightArrow);

			if (!control.left || !control.right) return;
			control.left = false;
			control.right = false;
		}
	}
}