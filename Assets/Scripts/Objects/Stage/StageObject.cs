using Objects.Base;
using UnityEditor;
using UnityEngine;
using VEngine.Collision;
using VEngine.Utilities.Helpers;

namespace Objects.Stage
{
	[ExecuteAlways]
	public class StageObject : ObjectBase
	{
		public Hitbox outerbox, innerbox, hitbox;

		public void Start()
		{
			position = transform.position;
			if (!Application.isPlaying) return;

			if (keepLoaded)
			{
				loaded = true;
				Utilities.Stage.Instance.loadedObjects.Add(this);
				ObjectAwake();
				ObjectOnLoad();
			}
			else
			{
				if (!(Mathf.Abs(PlayerX - position.x) <= 24)) return;
				loaded = true;
				Utilities.Stage.Instance.loadedObjects.Add(this);
				ObjectAwake();
				ObjectOnLoad();
			}
		}

		public void StageFixedUpdate()
		{
			if (!Application.isPlaying) return;

			if (!loaded)
			{
				if (!(Mathf.Abs(PlayerX - position.x) <= 24)) return;
				loaded = true;
				Utilities.Stage.Instance.loadedObjects.Add(this);
				ObjectOnLoad();
			}
			else
			{
				ObjectFixedUpdate();

				if (Mathf.Abs(PlayerX - position.x) > 24)
				{
					loaded = false;
				}
			}
		}
		
		public void StageLateFixedUpdate()
		{
			if (!Application.isPlaying) return;
			
			if(loaded) LateObjectFixedUpdate();
		}

		public void Update()
		{
#if UNITY_EDITOR

			if (Application.isPlaying) return;

			if (!Selection.Contains(gameObject)) return;
			var objPosition = transform.position;
			
			// outerbox
			Debug.DrawLine(
				new Vector3(objPosition.x - PixelUnitHelper.P2U(outerbox.WidthRadius),
					objPosition.y + PixelUnitHelper.P2U(outerbox.HeightRadius), 0),
				new Vector3(objPosition.x - PixelUnitHelper.P2U(outerbox.WidthRadius),
					objPosition.y - PixelUnitHelper.P2U(outerbox.HeightRadius), 0), Color.yellow);
			Debug.DrawLine(
				new Vector3(objPosition.x + PixelUnitHelper.P2U(outerbox.WidthRadius),
					objPosition.y + PixelUnitHelper.P2U(outerbox.HeightRadius), 0),
				new Vector3(objPosition.x + PixelUnitHelper.P2U(outerbox.WidthRadius),
					objPosition.y - PixelUnitHelper.P2U(outerbox.HeightRadius), 0), Color.yellow);
			Debug.DrawLine(
				new Vector3(objPosition.x - PixelUnitHelper.P2U(outerbox.WidthRadius),
					objPosition.y + PixelUnitHelper.P2U(outerbox.HeightRadius), 0),
				new Vector3(objPosition.x + PixelUnitHelper.P2U(outerbox.WidthRadius),
					objPosition.y + PixelUnitHelper.P2U(outerbox.HeightRadius), 0), Color.yellow);
			Debug.DrawLine(
				new Vector3(objPosition.x - PixelUnitHelper.P2U(outerbox.WidthRadius),
					objPosition.y - PixelUnitHelper.P2U(outerbox.HeightRadius), 0),
				new Vector3(objPosition.x + PixelUnitHelper.P2U(outerbox.WidthRadius),
					objPosition.y - PixelUnitHelper.P2U(outerbox.HeightRadius), 0), Color.yellow);
			
			// hitbox		
			Debug.DrawLine(
				new Vector3(objPosition.x - PixelUnitHelper.P2U(hitbox.WidthRadius),
					objPosition.y + PixelUnitHelper.P2U(hitbox.HeightRadius), 0),
				new Vector3(objPosition.x - PixelUnitHelper.P2U(hitbox.WidthRadius),
					objPosition.y - PixelUnitHelper.P2U(hitbox.HeightRadius), 0), Color.magenta);
			Debug.DrawLine(
				new Vector3(objPosition.x + PixelUnitHelper.P2U(hitbox.WidthRadius),
					objPosition.y + PixelUnitHelper.P2U(hitbox.HeightRadius), 0),
				new Vector3(objPosition.x + PixelUnitHelper.P2U(hitbox.WidthRadius),
					objPosition.y - PixelUnitHelper.P2U(hitbox.HeightRadius), 0), Color.magenta);
			Debug.DrawLine(
				new Vector3(objPosition.x - PixelUnitHelper.P2U(hitbox.WidthRadius),
					objPosition.y + PixelUnitHelper.P2U(hitbox.HeightRadius), 0),
				new Vector3(objPosition.x + PixelUnitHelper.P2U(hitbox.WidthRadius),
					objPosition.y + PixelUnitHelper.P2U(hitbox.HeightRadius), 0), Color.magenta);
			Debug.DrawLine(
				new Vector3(objPosition.x - PixelUnitHelper.P2U(hitbox.WidthRadius),
					objPosition.y - PixelUnitHelper.P2U(hitbox.HeightRadius), 0),
				new Vector3(objPosition.x + PixelUnitHelper.P2U(hitbox.WidthRadius),
					objPosition.y - PixelUnitHelper.P2U(hitbox.HeightRadius), 0), Color.magenta);
#endif
		}
	}
}