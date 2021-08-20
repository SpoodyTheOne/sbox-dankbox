using System;
using System.Collections.Generic;

namespace Sandbox.Tools
{
	[Library( "tool_stacker", Title = "Stacker", Description = "stack props in a direction", Group = "construction" )]
	public partial class StackerTool : BaseTool
	{
		private Prop target;
		private PreviewEntity[] previewModels;
		private string Model;

		private int dir = 0;

		private Vector3 currentDir = Vector3.Forward;

		private Vector3[] Directions;

		[ConVar.ClientData( "stacker_amount" )]
		public static int StackAmount { get; set; } = 1;

		[ConVar.ClientData( "stacker_weld" )]
		public static string StackWeld { get; set; } = "1";

		public override void CreatePreviews()
		{
			previewModels = new PreviewEntity[24];

			int i = 0;

			foreach ( PreviewEntity p in previewModels )
			{
				var previewModel = p;
				if ( TryCreatePreview( ref previewModel, Model ) )
				{
					previewModel.RelativeToNormal = false;
					previewModel.OffsetBounds = true;
					previewModel.PositionOffset = -previewModel.CollisionBounds.Center;
					previewModel.RenderAlpha = 0f;
					previewModels[i] = previewModel;
					i++;
				}
			}
		}

		private void ChangeDir()
		{
			Log.Info( Directions );

			dir++;
			if ( dir > Directions.Length - 1 )
			{
				dir = 0;
			}

			currentDir = Directions[dir];
		}

		//called every tick serverside and clientside.
		//get target prop using traces and stuff here
		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				if ( Input.Pressed( InputButton.Reload ) )
				{
					Log.Info( currentDir );
					ChangeDir();
				}

				Reset();

				if ( Input.Down( InputButton.Run ) )
				{
					if ( Input.Pressed( InputButton.Attack1 ) )
					{
						Log.Info("increase");
						int amount = int.Parse( ConsoleSystem.GetValue( "stacker_amount" ) );
						Owner.GetClientOwner().SendCommandToClient("stacker_amount " + Math.Clamp( amount + 1, 1, 24 ));
						return;
					}
					else if ( Input.Pressed( InputButton.Attack2 ) )
					{
						int amount = int.Parse( ConsoleSystem.GetValue( "stacker_amount" ) );
						Owner.GetClientOwner().SendCommandToClient("stacker_amount " + Math.Clamp( amount - 1, 1, 24 ));
						return;
					}

				}

				var startPos = Owner.EyePos;
				var dir = Owner.EyeRot.Forward;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					.Ignore( Owner )
					.Run();

				if ( !tr.Hit || !tr.Body.IsValid() || !tr.Entity.IsValid() || tr.Entity.IsWorld )
				{
					Reset();
					return;
				}

				if ( tr.Entity.PhysicsGroup == null || tr.Entity.PhysicsGroup.BodyCount > 1 )
				{
					Reset();
					return;
				}

				if ( tr.Entity is not Prop prop )
				{
					Reset();
					return;
				}

				Model = prop.GetModelName();

				int k = 1;

				int stackamount = int.Parse( ConsoleSystem.GetValue( "stacker_amount" ) );

				foreach ( PreviewEntity previewModel in previewModels )
				{
					previewModel.Position = prop.CollisionPosition + prop.Transform.NormalToWorld( prop.GetModel().RenderBounds.Maxs * currentDir * 2 * prop.Scale * k );
					previewModel.Rotation = prop.Rotation;
					previewModel.SetModel( Model );
					previewModel.ResetInterpolation();
					previewModel.RenderColor = prop.RenderColor;
					previewModel.RenderAlpha = 0.5f;
					previewModel.Scale = prop.Scale;
					previewModel.GlowActive = true;
					previewModel.GlowState = GlowStates.GlowStateOn;
					previewModel.GlowColor = Color.Red;
					k++;
					if ( k > stackamount )
						break;
				}

				if ( Input.Pressed( InputButton.Attack1 ) )
				{

					for ( int i = 0; i < stackamount; i++ )
					{
						var stacked = new Prop();
						stacked.SetModel( prop.GetModelName() );
						stacked.Rotation = prop.Rotation;
						stacked.Position = previewModels[0].Position + prop.Transform.NormalToWorld( prop.GetModel().RenderBounds.Maxs * currentDir * 2 * prop.Scale * i );
						stacked.AddCollisionLayer( CollisionLayer.All );
						stacked.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
						stacked.RenderColor = prop.RenderColor;
						stacked.RenderAlpha = prop.RenderAlpha;
						stacked.Scale = prop.Scale;
						stacked.CopyMaterialGroup( prop );
						stacked.PhysicsBody.BodyType = prop.PhysicsBody.BodyType;

						//weld
						if ( ConsoleSystem.GetValue( "stacker_weld" ) == "1" )
							stacked.Weld( prop.Root as Prop );

						SandboxGame.UndoList[Owner.GetClientOwner().SteamId].Insert( 0, stacked );
					}
				}
				else
				{
					return;
				}

				CreateHitEffects( tr.EndPos );
			}
		}

		private void Reset()
		{
			target = null;

			foreach ( PreviewEntity previewModel in previewModels )
			{
				previewModel.RenderAlpha = 0f;
				previewModel.GlowActive = false;
			}
		}

		public override void Activate()
		{
			base.Activate();

			Directions = new Vector3[] {
				Vector3.Up/2,
				Vector3.Down/2,
				Vector3.Right,
				Vector3.Left,
				Vector3.Forward,
				Vector3.Backward
			};

			currentDir = Directions[dir];

			Reset();
		}

		public override void Deactivate()
		{
			base.Deactivate();

			Reset();
		}
	}
}