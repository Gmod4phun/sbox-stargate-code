using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		[Property]
		public Stargate Gate { get; set; }

		[Property]
		public int Number { get; set; } = 0;

		public void OnOpenChanged(bool oldValue, bool newValue)
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

			if (ChevronLight.IsValid())
			{
				ChevronLight.Enabled = On;
			}
		}

		public async void TurnOn( float delay = 0 )
		{
			if ( delay > 0 )
			{
				await Task.DelayRealtimeSeconds( delay );
				if ( !this.IsValid() ) return;
			}

			On = true;
		}

		public async void TurnOff( float delay = 0 )
		{
			if ( delay > 0 )
			{
				await Task.DelayRealtimeSeconds( delay );
				if ( !this.IsValid() ) return;
			}

			On = false;
		}

		public async void SetOpen( bool open, float delay = 0 )
		{
			if ( delay > 0 )
			{
				await Task.DelayRealtimeSeconds( delay );
				if ( !this.IsValid() ) return;
			}

			Open = open;
		}

		public void PlayOpenSound(float delay = 0) {
			Stargate.PlaySound( this, Gate.GetSound( "chevron_open" ), delay );
		}

		public void PlayCloseSound(float delay = 0) {
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
