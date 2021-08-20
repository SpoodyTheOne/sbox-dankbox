using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Health : Panel
{
	public Label Label;

	public Health()
	{
		Label = Add.Label( "100", "value" );
		
		this.Style.BorderColor = "red";
		this.Style.BorderWidth = 2;
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;

		Label.Text = $"{player.Health.CeilToInt()}%";
	}
}
