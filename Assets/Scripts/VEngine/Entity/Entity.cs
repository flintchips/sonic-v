using Objects.Base;
using Objects.Player;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;
using VEngine.Collision;
using VEngine.Entity.Constants;

namespace VEngine.Entity
{
	public class Entity : ObjectBase
	{
		public float groundVel, angle;
		public bool onGround, groundedStore; // prev frame's onGround value

		[Header("Collision")]

		public bool onObject;
		public GameObject attachedObject;
		public SolidObject solidObject;
		[HideInInspector] public int collisionFlagH;
		[HideInInspector] public int collisionFlagV;
		public Hitbox outerbox, innerbox, hitbox;

		public int GroundModeWidthRadius => ModeVector.x == 0 ? outerbox.WidthRadius : outerbox.HeightRadius;

		public int GroundModeHeightRadius => ModeVector.x == 0 ? outerbox.HeightRadius : outerbox.WidthRadius;

		public Sensor[] sensors = new Sensor[10];
		[FormerlySerializedAs("tileCollisions")][HideInInspector] public TileCollisionType tileCollisionsType = TileCollisionType.Down;
		public CollisionLayer collisionLayer = CollisionLayer.Ground;
		public CollisionState collisionMode = CollisionState.Floor;
		public Flip direction = Flip.None;
		public Flip directionStore = Flip.None;
		public int substeps;
		[HideInInspector] public float stepDelta;
		[HideInInspector] public bool isLastSubstep;

		// vector in the downwards direction of whatever groundMode the entity is in
		public Vector2 ModeVector
		{
			get
			{
				return collisionMode switch
				{
					CollisionState.None => Vector2.zero,
					CollisionState.Floor => Vector2.down,
					CollisionState.LWall => Vector2.left,
					CollisionState.Roof => Vector2.up,
					CollisionState.RWall => Vector2.right,
					_ => Vector2.zero
				};
			}
		}

		// vector 90 degrees counter-clockwise of modeVector
		public Vector2 ModeVectorRight => new(-ModeVector.y, ModeVector.x);

		// run virtual methods 
		private void Awake()
		{
			if ((PlayerObject)this != null)
			{
				ObjectAwake();
				ObjectOnLoad();
				return;
			}

			if (keepLoaded)
			{
				loaded = true;
				Stage.Instance.loadedEntities.Add(this);
				ObjectAwake();
				ObjectOnLoad();
			}
			else
			{
				if (!(Mathf.Abs(PlayerX - position.x) <= 24)) return;
				loaded = true;
				Stage.Instance.loadedEntities.Add(this);
				ObjectAwake();
				ObjectOnLoad();
			}
		}

		private void Start() { ObjectStart(); }
		private void Update() { ObjectUpdate(); }

		private void FixedUpdate()
		{
			if (!loaded)
			{
				if (!(Mathf.Abs(PlayerX - position.x) <= 24)) return;
				loaded = true;
				Stage.Instance.loadedEntities.Add(this);
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
	}
}