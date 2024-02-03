namespace Sandbox.Components.Stargate.Rings
{
    public class RingTransporter : Component
    {
        [Property]
        public SkinnedModelRenderer Renderer => Components.Get<SkinnedModelRenderer>( true );

        [Property]
        public ModelCollider Collider => Components.Get<ModelCollider>( true );

        [Property]
        public RingTransporter OtherTransporter;

        [Property]
        public string Address { get; set; }

        [Property]
        public Model RingModel { get; set; } = Model.Load( "models/sbox_stargate/rings_ancient/ring_ancient.vmdl" );

        [Property]
        public float PlatformHeightOffset { get; set; } = 0;

        [Property]
        public float RingsRestingHeightOffset { get; set; } = 0;

        private List<Ring> DeployedRings = new();

        private bool Busy = false;

        public RingTransporter FindClosest()
        {
            return Scene.GetAllComponents<RingTransporter>().Where( x => x != this ).OrderBy( x => x.Transform.Position.DistanceSquared( Transform.Position ) ).FirstOrDefault();
        }

        public bool IsObjectAllowedToTeleport( GameObject obj )
        {
            if ( obj.Tags.Has( "rings_no_teleport" ) )
                return false;

            if ( obj.Tags.Has( "player" ) && obj.Components.Get<PlayerController>().IsValid() )
                return true;

            if ( obj.Parent is not Scene _ )
                return false;

            if ( obj.Components.Get<Rigidbody>().IsValid() )
                return true;

            return false;
        }

        private Ring CreateRing()
        {
            var ring_object = new GameObject();
            ring_object.Transform.World = Transform.World;
            ring_object.Transform.Position += ring_object.Transform.Rotation.Up * RingsRestingHeightOffset;
            ring_object.SetParent( GameObject );

            var ring_component = ring_object.Components.Create<Ring>();
            var renderer = ring_object.Components.Create<ModelRenderer>();
            renderer.Model = RingModel;

            var collider = ring_object.Components.Create<ModelCollider>();
            collider.Model = renderer.Model;

            var body = ring_object.Components.Create<Rigidbody>();
            body.Gravity = false;
            body.PhysicsBody.EnableSolidCollisions = false;

            ring_object.Tags.Add( "rings_no_teleport", "ignoreworld", "ringsring" );

            return ring_component;
        }

        public bool AreAllRingsInDesiredPosition()
        {
            return DeployedRings.Where( ring => ring.IsInDesiredPosition ).Count() == DeployedRings.Count();
        }

        public bool AreAllRingsInRestingPosition()
        {
            return DeployedRings.Where( ring => ring.IsInRestingPosition ).Count() == DeployedRings.Count();
        }

        private void DeleteRings()
        {
            foreach ( var ring in DeployedRings )
                ring.GameObject.Destroy();

            DeployedRings.Clear();
        }

        private void DeployRings()
        {
            Sound.Play( "sounds/sbox_stargate/rings/ringtransporter.part1.sound", GameObject.Transform.Position );

            float[] delays = { 2, 2.5f, 3f, 3.4f, 3.7f };
            var tasks = new List<Task>();

            for ( var i = 0; i < 5; i++ )
            {
                var ring = CreateRing();
                DeployedRings.Add( ring );

                ring.TryToReachRestingPosition = true;
                ring.SetDesiredUpOffset( PlatformHeightOffset + 80 - i * 16 );
                ring.StartReachingDesired( delays[i] );
            }
        }

        private void RetractRings()
        {
            Sound.Play( "sounds/sbox_stargate/rings/ringtransporter.part2.sound", GameObject.Transform.Position );

            float[] delays = { 0, 0.3f, 0.6f, 0.9f, 1.2f };
            var tasks = new List<Task>();

            for ( var i = 0; i < 5; i++ )
            {
                var ring = DeployedRings[4 - i];
                ring.StartReachingResting( delays[i] + 0.6f );
            }
        }

        private void TeleportObjects( List<GameObject> objects, RingTransporter from, RingTransporter to )
        {
            foreach ( GameObject e in objects )
            {
                var platformHeightOffsetDiff = to.PlatformHeightOffset - from.PlatformHeightOffset;

                var localPos = from.Transform.World.PointToLocal( e.Transform.Position );
                var newPos = to.Transform.Local.PointToWorld( localPos + Vector3.Up * platformHeightOffsetDiff );

                var localRot = from.Transform.World.RotationToLocal( e.Transform.Rotation );
                var newRot = to.Transform.Local.RotationToWorld( localRot.RotateAroundAxis( Vector3.Up, 180 ) );

                if ( e.Components.Get<PlayerController>() is PlayerController ply )
                {
                    if ( ply.Components.Get<TeleportScreenoverlay>( FindMode.InDescendants ) is TeleportScreenoverlay overlay )
                    {
                        overlay.ActivateFor( 0.2f );
                    }

                    var DeltaAngleEH = to.Transform.Rotation.Angles() - from.Transform.Rotation.Angles();
                    ply.SetPlayerViewAngles( ply.EyeAngles + new Angles( 0, DeltaAngleEH.yaw, 0 ) );
                }

                e.Transform.Position = newPos;
                e.Transform.Rotation = newRot;
                e.Transform.ClearLerp();
            }
        }

        private void TeleportBothSides()
        {
            if ( !OtherTransporter.IsValid() )
                return;

            // get object lists before teleporting (otherwise we will just teleport them back)
            var ourObjects = Scene.GetAllObjects( true ).Where( x => IsObjectAllowedToTeleport( x ) && x.Transform.Position.DistanceSquared( Transform.Position ) <= 80 * 80 ).ToList();
            var otherObjects = Scene.GetAllObjects( true ).Where( x => IsObjectAllowedToTeleport( x ) && x.Transform.Position.DistanceSquared( OtherTransporter.Transform.Position ) <= 80 * 80 ).ToList();

            TeleportObjects( ourObjects, this, OtherTransporter );
            TeleportObjects( otherObjects, OtherTransporter, this );
        }

        private async void DoRings( RingTransporter other )
        {
            if ( Busy )
                return;

            if ( !other.IsValid() || other.Busy )
                return;

            OtherTransporter = other;
            OtherTransporter.OtherTransporter = this;

            Busy = true;
            OtherTransporter.Busy = true;

            Renderer.SceneModel.SetAnimParameter( "Open", true );
            OtherTransporter.Renderer.SceneModel.SetAnimParameter( "Open", true );

            DeployRings();
            OtherTransporter.DeployRings();

            while ( !AreAllRingsInDesiredPosition() || !OtherTransporter.AreAllRingsInDesiredPosition() )
            {
                await Task.Frame();
            }

            // await Task.WhenAll( DeployRings(), OtherTransporter.DeployRings() );

            DoParticleEffect();
            OtherTransporter.DoParticleEffect();

            DoLightEffect();
            OtherTransporter.DoLightEffect();

            DoRingGlowEffect();
            OtherTransporter.DoRingGlowEffect();

            await Task.DelaySeconds( 0.5f );

            TeleportBothSides();

            RetractRings();
            OtherTransporter.RetractRings();

            while ( !AreAllRingsInRestingPosition() || !OtherTransporter.AreAllRingsInRestingPosition() )
            {
                await Task.Frame();
            }

            DeleteRings();
            OtherTransporter.DeleteRings();

            // await Task.WhenAll( RetractRings(), OtherTransporter.RetractRings() );

            Renderer.SceneModel.SetAnimParameter( "Open", false );
            OtherTransporter.Renderer.SceneModel.SetAnimParameter( "Open", false );

            await Task.DelaySeconds( 1.5f );

            Busy = false;
            OtherTransporter.Busy = false;

            OtherTransporter.OtherTransporter = null;
            OtherTransporter = null;
        }

        private async void DoParticleEffect()
        {
            var particle = Components.Create<LegacyParticleSystem>();
            particle.Particles = ParticleSystem.Load( "particles/sbox_stargate/rings_transporter.vpcf" );

            var angles = Transform.Rotation.Angles();
            particle.ControlPoints = new()
            {
                new ParticleControlPoint()
                {
                    StringCP = "1",
                    Value = ParticleControlPoint.ControlPointValueInput.Vector3,
                    VectorValue = Transform.Rotation.Up * 80
                },
                new ParticleControlPoint()
                {
                    StringCP = "2",
                    Value = ParticleControlPoint.ControlPointValueInput.Vector3,
                    VectorValue = new Vector3( angles.roll, angles.pitch, angles.yaw )
                },
                new ParticleControlPoint()
                {
                    StringCP = "3",
                    Value = ParticleControlPoint.ControlPointValueInput.Vector3,
                    VectorValue = new Vector3( Transform.Scale.x, 0, 0 )
                }
            };

            await Task.DelaySeconds( 2f );

            particle.Destroy();
        }

        private async void DoLightEffect()
        {
            var light_object = new GameObject();
            light_object.Transform.World = Transform.World;
            light_object.SetParent( GameObject );
            light_object.Tags.Add( "rings_no_teleport" );

            var light = light_object.Components.Create<PointLight>( false );
            light.LightColor = Color.FromBytes( 255, 255, 255 ) * 10f;
            light.Radius = 300;
            light.Enabled = true;

            var lightDistance = 15f;
            var targetDistance = 85f;
            var timeToReachTargetMs = 350;
            var delayMs = 5;

            var distanceToTravel = targetDistance - lightDistance;
            var numSteps = timeToReachTargetMs / delayMs;
            var lightDistanceStep = distanceToTravel / numSteps;

            while ( lightDistance <= targetDistance )
            {
                light_object.Transform.World = Transform.World.WithPosition( Transform.Position + Transform.Rotation.Up * lightDistance );
                lightDistance += lightDistanceStep;
                await Task.Delay( delayMs );
            }

            light_object.Destroy();
        }

        private void DoRingGlowEffect()
        {
            foreach ( var ring in DeployedRings )
            {
                ring.SetGlowState( true );
                ring.SetGlowState( false, 0.75f );
            }
        }

        public async void DialRings( RingTransporter other, float delay = 0 )
        {
            if ( Busy )
                return;

            if ( !other.IsValid() || other.Busy )
                return;

            if ( delay > 0 )
                await Task.DelaySeconds( delay );

            DoRings( other );
        }
    }
}
