namespace Sandbox.Components.Stargate.Rings
{
    public class Ring : Component
    {
        [Property]
        public ModelRenderer Renderer => Components.Get<ModelRenderer>( true );

        [Property]
        public ModelCollider Collider => Components.Get<ModelCollider>( true );

        [Property]
        public Rigidbody Body => Components.Get<Rigidbody>( true );

        [Property]
        public Ringtransporter Transporter => GameObject.Parent.Components.Get<Ringtransporter>();

        private Vector3 _desiredPosition;
        public bool IsInDesiredPosition => Transform.Position.AlmostEqual( _desiredPosition, 0.1f );

        private bool ShouldBreakAsyncLoop = false;

        public void BreakAsyncLoop()
        {
            ShouldBreakAsyncLoop = true;
        }

        public async Task MoveToPosition( Vector3 pos, float startDelay = 0, bool retracting = false )
        {
            _desiredPosition = pos;

            if ( startDelay > 0 )
            {
                await Task.DelaySeconds( startDelay );
            }

            if ( !retracting )
                Collider.Enabled = true;

            var t = global::Transform.Zero.WithScale( 1 );
            Body.PhysicsBody.UseController = true;

            while ( !ShouldBreakAsyncLoop )
            {
                // Transform.Position = Transform.Position.LerpTo( _desiredPosition, Time.Delta * 5f );
                // Transform.Rotation = Transporter.Transform.Rotation;

                // Body.PhysicsBody.Move( t.WithPosition( _desiredPosition ).WithRotation( Transporter.Transform.Rotation ), Time.Delta * 0.5f );
                Body.PhysicsBody.SmoothMove( t.WithPosition( _desiredPosition ).WithRotation( Transporter.Transform.Rotation ), Time.Delta, Time.Delta * 0.5f );

                // disable ring collider when it nears the base (to avoid rings bumping to each other when retracting)
                if ( retracting && Collider.Enabled && Transform.Position.DistanceSquared( _desiredPosition ) <= 256 )
                {
                    Collider.Enabled = false;
                }

                await Task.Frame();
            }

            ShouldBreakAsyncLoop = false;
        }
    }
}
