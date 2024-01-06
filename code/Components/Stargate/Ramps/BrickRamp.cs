namespace Sandbox.Components.Stargate.Ramps
{
    public partial class BrickRamp : GateRamp
    {
        public const string Model = "models/sbox_stargate/ramps/brick/brick.vmdl";

        [Property]
        public Vector3 SpawnOffset { get; private set; } = new( 0, 0, 70 );

        // public Vector3[] StargatePositionOffset => new[] { new Vector3( 0, 0, 95 ) };
        // public Angles[] StargateRotationOffset => new[] { Angles.Zero };

        public List<Stargate> Gate { get; set; } = new();

        [Property]
        public ModelRenderer RampModel { get; set; }

        public BrickRamp()
        {
            StargatePositionOffset = new Vector3( 0, 0, 95 );
        }
    }
}
