namespace Sandbox.Components.Stargate.Rings
{
    public class Ringtransporter : Component
    {
        [Property]
        public SkinnedModelRenderer Renderer => Components.Get<SkinnedModelRenderer>( true );

        [Property]
        public ModelCollider Collider => Components.Get<ModelCollider>( true );

        [Property]
        public Ringtransporter OtherTransporter;

        [Property]
        public string Address { get; set; }

        private List<Ring> DeployedRings = new();

        private bool Busy = false;

        public Ringtransporter FindClosest()
        {
            return Scene.GetAllComponents<Ringtransporter>().Where( x => x != this ).OrderBy( x => x.Transform.Position.DistanceSquared( Transform.Position ) ).FirstOrDefault();
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

        private Ring CreateRing( float heightOffset = 0 )
        {
            var ring_object = new GameObject();
            ring_object.Transform.World = Transform.World;
            ring_object.Transform.Position += ring_object.Transform.Rotation.Up * heightOffset;
            ring_object.SetParent( GameObject );

            var ring_component = ring_object.Components.Create<Ring>();
            var renderer = ring_object.Components.Create<ModelRenderer>();
            renderer.Model = Model.Load( "models/sbox_stargate/rings_ancient/ring_ancient.vmdl" );

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
                var ring = CreateRing( -4 );
                DeployedRings.Add( ring );

                ring.TryToReachRestingPosition = true;
                ring.SetDesiredUpOffset( 80 - i * 16 );
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

        private void TeleportBothSides()
        {
            if ( !OtherTransporter.IsValid() )
                return;

            // get object lists before teleporting (otherwise we will just teleport them back)
            var ourObjects = Scene.GetAllObjects( true ).Where( x => IsObjectAllowedToTeleport( x ) && x.Transform.Position.DistanceSquared( Transform.Position ) <= 80 * 80 ).ToList();
            var otherObjects = Scene.GetAllObjects( true ).Where( x => IsObjectAllowedToTeleport( x ) && x.Transform.Position.DistanceSquared( OtherTransporter.Transform.Position ) <= 80 * 80 ).ToList();

            foreach ( GameObject e in ourObjects )
            {
                var localPos = Transform.World.PointToLocal( e.Transform.Position );
                var newPos = OtherTransporter.Transform.Local.PointToWorld( localPos );

                var localRot = Transform.World.RotationToLocal( e.Transform.Rotation );
                var newRot = OtherTransporter.Transform.Local.RotationToWorld( localRot.RotateAroundAxis( Vector3.Up, 180 ) );

                e.Transform.Position = newPos;
                e.Transform.Rotation = newRot;
                e.Transform.ClearLerp();

                if ( e.Components.Get<PlayerController>() is PlayerController ply )
                {
                    var DeltaAngleEH = OtherTransporter.Transform.Rotation.Angles() - Transform.Rotation.Angles();
                    ply.SetPlayerViewAngles( ply.EyeAngles + new Angles( 0, DeltaAngleEH.yaw, 0 ) );
                }
            }

            foreach ( GameObject e in otherObjects )
            {
                var localPos = OtherTransporter.Transform.World.PointToLocal( e.Transform.Position );
                var newPos = Transform.Local.PointToWorld( localPos );

                var localRot = Transform.World.RotationToLocal( e.Transform.Rotation );
                var newRot = OtherTransporter.Transform.Local.RotationToWorld( localRot );

                e.Transform.Position = newPos;
                e.Transform.Rotation = newRot;
                e.Transform.ClearLerp();
            }
        }

        private async void DoRings( Ringtransporter other )
        {
            if ( Busy )
                return;

            if ( !other.IsValid() || other.Busy )
                return;

            OtherTransporter = other;
            OtherTransporter.OtherTransporter = this;

            Busy = true;
            OtherTransporter.Busy = true;

            Collider.Enabled = false;
            OtherTransporter.Collider.Enabled = false;

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

            Collider.Enabled = true;
            OtherTransporter.Collider.Enabled = true;

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

        public async void DialRings( Ringtransporter other, float delay = 0 )
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
