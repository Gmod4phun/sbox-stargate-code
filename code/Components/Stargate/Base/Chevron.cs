namespace Sandbox.Components.Stargate
{
	public class Chevron : PropertyChangeComponent, Component.ExecuteInEditor
	{
		private float _selfillumscale = 0;

		[Property]
		public SkinnedModelRenderer ChevronModel { get; set; }

		[Property]
		public PointLight ChevronLight { get; set; }

		[Property]
		public bool On { get; set; }

		[Property, OnChange( nameof( OnOpenChanged ) )]
		public bool Open { get; set; }

		public Stargate Gate => GameObject.Parent.Components.Get<Stargate>( FindMode.EnabledInSelfAndDescendants );

		[Property]
		public int Number { get; set; } = 0;

		public void OnOpenChanged( bool oldValue, bool newValue )
		{
			if ( ChevronModel.IsValid() && ChevronModel.SceneModel.IsValid() )
			{
				ChevronModel.SceneModel.SetAnimParameter( "Open", newValue );

				Stargate.PlaySound( this, Gate.GetSound( newValue ? "chevron_open" : "chevron_close" ) );
			}
		}

		protected override void OnPreRender()
		{
			base.OnPreRender();

			var so = ChevronModel.SceneObject;
			if ( so.IsValid() )
			{
				_selfillumscale = _selfillumscale.Approach( On ? 1 : 0, Time.Delta * 5 );
				so.Attributes.Set( "selfillumscale", _selfillumscale );
				so.Batchable = false;
			}

			if ( ChevronLight.IsValid() )
			{
				// ChevronLight.Enabled = On; // disable chevron lights for now
			}
		}

		public async void TurnOn( float delay = 0 )
		{
			if ( delay > 0 )
			{
				await Task.DelaySeconds( delay );
				if ( !this.IsValid() ) return;
			}

			On = true;
			ChevronModel.MaterialGroup = "On";
		}

		public async void TurnOff( float delay = 0 )
		{
			if ( delay > 0 )
			{
				await Task.DelaySeconds( delay );
				if ( !this.IsValid() ) return;
			}

			On = false;
			ChevronModel.MaterialGroup = "default";
		}

		public async void SetOpen( bool open, float delay = 0 )
		{
			if ( delay > 0 )
			{
				await Task.DelaySeconds( delay );
				if ( !this.IsValid() ) return;
			}

			Open = open;
		}

		public void PlayOpenSound( float delay = 0 )
		{
			Stargate.PlaySound( this, Gate.GetSound( "chevron_open" ), delay );
		}

		public void PlayCloseSound( float delay = 0 )
		{
			Stargate.PlaySound( this, Gate.GetSound( "chevron_close" ), delay );
		}

		public void ChevronOpen( float delay = 0 )
		{
			// PlayOpenSound(delay);
			SetOpen( true, delay );
		}

		public void ChevronClose( float delay = 0 )
		{
			// PlayCloseSound(delay);
			SetOpen( false, delay );
		}
	}
}
