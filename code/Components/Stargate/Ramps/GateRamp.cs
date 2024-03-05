namespace Sandbox.Components.Stargate.Ramps
{
    public class GateRamp : Component, Component.ExecuteInEditor
    {
        [Property]
        public virtual int GateSlots { get; protected set; } = 1;

        public IEnumerable<Stargate> Gates => GameObject.Components.GetAll<Stargate>( FindMode.InChildren );

        [Property]
        public Vector3 StargatePositionOffset { get; protected set; }

        [Property]
        public Angles StargateRotationOffset { get; protected set; }

        public IEnumerable<ParticleEmitter> SmokeEmitters => GameObject.Components.GetAll<ParticleEmitter>( FindMode.InChildren );

        public bool HasFreeSlot() => Gates.Count() < GateSlots;

        public static GateRamp GetClosest( Vector3 position, float max = -1f )
        {
            var ramps = Game.ActiveScene.GetAllComponents<GateRamp>().Where( x => x.HasFreeSlot() );
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

        protected override void OnUpdate()
        {
            base.OnUpdate();

            var hasActiveGate = Gates.Any( gate => gate.IsValid() && !gate.Idle );

            foreach ( var emitter in SmokeEmitters )
            {
                if ( emitter.IsValid() )
                    emitter.Enabled = hasActiveGate;
            }
        }
    }
}
