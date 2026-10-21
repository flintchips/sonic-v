
using Objects.Player.Constants;
namespace Objects.Player.States
{
	public class Hurt : Utilities.BaseState<PlayerState, PlayerObject>
	{
		public Hurt() : base(PlayerState.Hurt) { }
		public override void HandleState()
		{

		}
	}
}