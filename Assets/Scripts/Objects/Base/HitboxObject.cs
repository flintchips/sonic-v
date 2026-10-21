using Objects.Stage;
using UnityEngine;
using VEngine.Entity;
using VEngine.Entity.Constants;
using VEngine.Utilities.Helpers;
using VEngine.Collision;
using System.Collections.Generic;

namespace Objects.Base
{
	public class HitboxObject : StageObject
	{
		public bool active = true;
		public Vector2 hitboxOffset;
		public List<Entity> collidingEntities = new List<Entity>();
		
		protected override void ObjectFixedUpdate()
		{
			position += velocity / 16f;
			if (active)
				HandleHitboxCollision();
			else
			{
				if(collidingEntities.Count > 0)
					collidingEntities = new List<Entity>();
			}
				
		}
		
		protected void HandleHitboxCollision()
		{
			foreach (var entity in Utilities.Stage.Instance.loadedEntities)
			{
				if (entity.collisionMode == CollisionState.None) continue;
				
				Vector2 rectsize = new Vector2(entity.hitbox.WidthRadius, entity.hitbox.HeightRadius);
				if(entity.ModeVector.x == 1)
					rectsize = new Vector2(rectsize.y, rectsize.x);
				
				Rect entityRect = HitboxToRect(entity.hitbox, entity.position);
				Rect objectRect = HitboxToRect(hitbox, position);
				
				if(entityRect.Overlaps(objectRect))
				{
					if(!collidingEntities.Contains(entity))
					{
						collidingEntities.Add(entity);
						OnEnter(entity);
						OnStay(entity);
					}
					else
						OnStay(entity);
				}
				else
				{
					if(collidingEntities.Contains(entity))
					{
						OnExit(entity);
						collidingEntities.Remove(entity);
					}
						
				}
			}
		}
		
		protected virtual void OnEnter(Entity entity)
		{
			
		}
		
		protected virtual void OnStay(Entity entity)
		{
			
		}
		
		protected virtual void OnExit(Entity entity)
		{
			
		}
		
		private Rect HitboxToRect(Hitbox box, Vector2 pos)
		{
			return new Rect(pos.x - PixelUnitHelper.P2U(box.WidthRadius + 1), 
							pos.y - PixelUnitHelper.P2U(box.HeightRadius  + 1), 
									PixelUnitHelper.P2U(box.WidthRadius * 2 + 1), 
									PixelUnitHelper.P2U(box.HeightRadius * 2 + 1));
		}
	}
}