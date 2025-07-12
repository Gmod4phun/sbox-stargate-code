namespace Sandbox.Components.Stargate
{
	public class StargateIrisGoauld : StargateIrisAtlantis
	{
		[Property]
		public Shield Shield => Components.Get<Shield>(FindMode.InSelf);

		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (!Shield.IsValid())
				return;

			Shield.AlphaMul = Alpha;
		}
	}
}
