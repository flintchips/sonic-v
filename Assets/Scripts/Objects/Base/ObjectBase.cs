using Objects.Player;
using UnityEngine;

namespace Objects.Base
{
	public class ObjectBase : MonoBehaviour
	{
		public bool loaded;
		public bool keepLoaded;
		public Vector2 position, velocity;
		public float rotation;
		protected static float PlayerX => PlayerVars.Leader.position.x;

		// virtual methods
		protected virtual void ObjectAwake()
		{
		}

		protected virtual void ObjectStart()
		{
		}

		protected virtual void ObjectUpdate()
		{
		}

		protected virtual void ObjectFixedUpdate()
		{
		}
		
		protected virtual void LateObjectFixedUpdate()
        {
        }

		protected virtual void ObjectOnLoad()
		{
		}
	}
}