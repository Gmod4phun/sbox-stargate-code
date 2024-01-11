namespace Sandbox.Components.Stargate.Rings
{
    public class Ringtransporter : Component, IUse
    {
        [Property]
        public ModelRenderer Renderer => Components.Get<ModelRenderer>( true );

        [Property]
        public ModelCollider Collider => Components.Get<ModelCollider>( true );

        [Property]
        public Ringtransporter OtherTransporter;

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
            collider.Enabled = false;

            var physics = ring_object.Components.Create<Rigidbody>();
            physics.Gravity = false;

            ring_object.Tags.Add( "rings_no_teleport", "ignoreworld" );

            return ring_component;
        }

        private async Task DeployRings()
        {
            Sound.Play( "sounds/sbox_stargate/rings/ringtransporter.part1.sound", GameObject.Transform.Position );

            float[] delays = { 2, 2.5f, 3f, 3.4f, 3.7f };
            var tasks = new List<Task>();

            for ( var i = 0; i < 1; i++ )
            {
                var ring = CreateRing( -4 );
                DeployedRings.Add( ring );

                tasks.Add( ring.MoveToPosition( Transform.Position + Transform.Rotation.Up * (80 - i * 16), delays[i] ) );
            }

            while ( DeployedRings.Where( ring => ring.IsInDesiredPosition ).Count() != DeployedRings.Count() )
            {
                await Task.Frame();
            }

            foreach ( var ring in DeployedRings )
            {
                ring.BreakAsyncLoop();
            }

            await Task.WhenAll( tasks );

            Log.Info( "all rings deployed" );
        }

        private async Task RetractRings()
        {
            Sound.Play( "sounds/sbox_stargate/rings/ringtransporter.part2.sound", GameObject.Transform.Position );

            float[] delays = { 0, 0.3f, 0.6f, 0.9f, 1.2f };
            var tasks = new List<Task>();

            for ( var i = 0; i < 1; i++ )
            {
                var ring = DeployedRings[4 - i];
                tasks.Add( ring.MoveToPosition( Transform.Position - Transform.Rotation.Up * 16, delays[i] + 0.6f, true ) );
            }

            while ( DeployedRings.Where( ring => ring.IsInDesiredPosition ).Count() != DeployedRings.Count() )
            {
                await Task.Frame();
            }

            foreach ( var ring in DeployedRings )
            {
                ring.BreakAsyncLoop();
            }

            await Task.WhenAll( tasks );

            Log.Info( "all rings retracted" );

            foreach ( var ring in DeployedRings )
                ring.GameObject.Destroy();

            DeployedRings.Clear();
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

        public async void DoRings()
        {
            if ( Busy )
                return;

            var closestRings = FindClosest();
            if ( !closestRings.IsValid() || closestRings.Busy )
                return;

            OtherTransporter = closestRings;
            OtherTransporter.OtherTransporter = this;

            Busy = true;
            OtherTransporter.Busy = true;

            Collider.Enabled = false;
            OtherTransporter.Collider.Enabled = false;

            await Task.WhenAll( DeployRings(), OtherTransporter.DeployRings() );

            TeleportBothSides();

            await Task.WhenAll( RetractRings(), OtherTransporter.RetractRings() );

            Busy = false;
            OtherTransporter.Busy = false;

            Collider.Enabled = true;
            OtherTransporter.Collider.Enabled = true;

            OtherTransporter.OtherTransporter = null;
            OtherTransporter = null;
        }

        public bool OnUse( GameObject user )
        {
            DoRings();

            return false;
        }

        public bool IsUsable( GameObject user )
        {
            return true;
        }
    }
}
