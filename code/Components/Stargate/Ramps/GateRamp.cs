namespace Sandbox.Components.Stargate.Ramps
{
    public abstract class GateRamp : Component
    {
        public virtual int GateSlots => 1;

        [Property]
        public List<Stargate> Gates { get; protected set; }

        [Property]
        public Vector3 StargatePositionOffset { get; protected set; }

        [Property]
        public Angles StargateRotationOffset { get; protected set; }

        public bool HasFreeSlot() => Gates.Count < GateSlots;

        public static GateRamp GetClosest( Vector3 position, float max = -1f )
        {
            var ramps = GameManager.ActiveScene.GetAllComponents<GateRamp>().Where( x => x.HasFreeSlot() );
            if ( !ramps.Any() )
            {
                return null;
            }

            var firstRamp = ramps.First();
            var closestDistance = position.DistanceSquared( firstRamp.Transform.Position );
            GateRamp closestRamp = firstRamp;

            foreach ( GateRamp ramp in ramps )
            {
                var curRampDistance = position.DistanceSquared( ramp.Transform.Position );
                if ( curRampDistance < closestDistance )
                {
                    closestDistance = curRampDistance;
                    closestRamp = ramp;
                }
            }

            return closestRamp;
        }
    }
}
