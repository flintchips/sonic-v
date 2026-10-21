using VEngine.Entity.Constants;

namespace VEngine.Utilities.Helpers
{
    public abstract class CollisionHelper
    {
        public static Direction CollisionModeToDirection(CollisionState mode)
        {
            return mode switch
            {
                CollisionState.Floor => Direction.Down,
                CollisionState.LWall => Direction.Left,
                CollisionState.Roof => Direction.Up,
                CollisionState.RWall => Direction.Right,
                _ => Direction.Down
            };
        }

        public static CollisionState CollisionModeReversed(CollisionState mode)
        {
            return mode switch
            {
                CollisionState.Floor => CollisionState.Roof,
                CollisionState.LWall => CollisionState.RWall,
                CollisionState.Roof => CollisionState.Floor,
                CollisionState.RWall => CollisionState.LWall,
                _ => CollisionState.None
            };
        }
    }
}