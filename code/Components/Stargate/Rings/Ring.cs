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

        [Property]
        public Transform DesiredPosition { get; set; }

        [Property]
        public Transform RestingPosition { get; set; }

        [Property]
        public bool TryToReachDesiredPosition { get; set; }

        [Property]
        public bool TryToReachRestingPosition { get; set; }

        private float _equalThreshold = 0.05f;
        private float _desiredOffset = 0;

        [Property, ReadOnly]
        public bool IsInDesiredPosition => Transform.Position.AlmostEqual( DesiredPosition.Position, _equalThreshold ) && Transform.Rotation.Angles().AsVector3().AlmostEqual( DesiredPosition.Rotation.Angles().AsVector3(), _equalThreshold );

        [Property, ReadOnly]
        public bool IsInRestingPosition => Transform.Position.AlmostEqual( RestingPosition.Position, _equalThreshold ) && Transform.Rotation.Angles().AsVector3().AlmostEqual( RestingPosition.Rotation.Angles().AsVector3(), _equalThreshold );

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if ( Body.IsValid() )
            {
                DesiredPosition = Transporter.Transform.World.WithPosition( Transporter.Transform.Position + Transporter.Transform.Rotation.Up * _desiredOffset );
                RestingPosition = Transporter.Transform.World.WithPosition( Transporter.Transform.Position - Transporter.Transform.Rotation.Up * 16 );

                if ( Body.PhysicsBody.IsValid() )
                {
                    if ( TryToReachDesiredPosition )
                    {
                        Body.PhysicsBody.SmoothMove( DesiredPosition, 0.4f, Time.Delta );
                    }
                    else if ( TryToReachRestingPosition )
                    {
                        Body.PhysicsBody.SmoothMove( RestingPosition, 0.4f, Time.Delta );
                    }
                }
            }
        }

        public void SetDesiredUpOffset( float offset )
        {
            _desiredOffset = offset;
        }

        public async void StartReachingDesired( float startDelay = 0 )
        {
            if ( startDelay > 0 )
            {
                await Task.DelaySeconds( startDelay );
            }

            TryToReachRestingPosition = false;
            TryToReachDesiredPosition = true;

            Body.PhysicsBody.EnableSolidCollisions = true;
        }

        public async void StartReachingResting( float startDelay = 0 )
        {
            if ( startDelay > 0 )
            {
                await Task.DelaySeconds( startDelay );
            }

            TryToReachRestingPosition = true;
            TryToReachDesiredPosition = false;

            Body.PhysicsBody.EnableSolidCollisions = false;
        }
    }
}
