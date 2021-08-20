using Sandbox;

public partial class Prop : Sandbox.Prop
{
	protected override void OnDestroy()
	{
		base.OnDestroy();

		SandboxGame.PropDestroyed( this );
	}
}