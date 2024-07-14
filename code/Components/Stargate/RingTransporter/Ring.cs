namespace Sandbox.Components.Stargate.Rings
{
	public class Ring : Component
	{
		[Property]
		public ModelRenderer Renderer => Components.Get<ModelRenderer>(true);

		[Property]
		public ModelCollider Collider => Components.Get<ModelCollider>(true);

		[Property]
		public Rigidbody Body => Components.Get<Rigidbody>(true);

		[Property]
		public RingTransporter Transporter => GameObject.Parent.Components.Get<RingTransporter>();

		[Property, Sync]
		public Transform DesiredPosition { get; set; }

		[Property, Sync]
		public Transform RestingPosition { get; set; }

		[Property, Sync]
		public bool TryToReachDesiredPosition { get; set; }

		[Property, Sync]
		public bool TryToReachRestingPosition { get; set; }

		[Property, Sync]
		public bool Glowing { get; set; } = false;

		private float _equalThreshold = 0.05f;
		private float _desiredOffset = 0;
		private float _selfIllumScale = 0;

		[Property, ReadOnly]
		public bool IsInDesiredPosition =>
			Transform.Position.AlmostEqual(DesiredPosition.Position, _equalThreshold)
			&& Transform
				.Rotation.Angles()
				.AsVector3()
				.AlmostEqual(DesiredPosition.Rotation.Angles().AsVector3(), _equalThreshold);

		[Property, ReadOnly]
		public bool IsInRestingPosition =>
			Transform.Position.AlmostEqual(RestingPosition.Position, _equalThreshold)
			&& Transform
				.Rotation.Angles()
				.AsVector3()
				.AlmostEqual(RestingPosition.Rotation.Angles().AsVector3(), _equalThreshold);

		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (Body.IsValid() && !IsProxy)
			{
				DesiredPosition = Transporter.Transform.World.WithPosition(
					Transporter.Transform.Position
						+ Transporter.Transform.Rotation.Up * _desiredOffset
				);
				RestingPosition = Transporter.Transform.World.WithPosition(
					Transporter.Transform.Position
						+ Transporter.Transform.Rotation.Up * Transporter.RingsRestingHeightOffset
				);

				if (Body.PhysicsBody.IsValid())
				{
					if (TryToReachDesiredPosition)
					{
						Body.PhysicsBody.SmoothMove(DesiredPosition, 0.4f, Time.Delta);
					}
					else if (TryToReachRestingPosition)
					{
						Body.PhysicsBody.SmoothMove(RestingPosition, 0.4f, Time.Delta);
					}
				}
			}

			if (Renderer.IsValid())
			{
				var so = Renderer.SceneObject;
				if (so.IsValid())
				{
					_selfIllumScale = _selfIllumScale.LerpTo(
						Glowing ? 1 : 0,
						Time.Delta * (Glowing ? 2f : 16f)
					);
					so.Batchable = false;
					so.Attributes.Set("selfillumscale", _selfIllumScale);
				}
			}
		}

		public void SetDesiredUpOffset(float offset)
		{
			_desiredOffset = offset;
		}

		public async void StartReachingDesired(float startDelay = 0)
		{
			if (startDelay > 0)
			{
				await Task.DelaySeconds(startDelay);
			}

			TryToReachRestingPosition = false;
			TryToReachDesiredPosition = true;

			Body.PhysicsBody.EnableSolidCollisions = true;
		}

		public async void StartReachingResting(float startDelay = 0)
		{
			if (startDelay > 0)
			{
				await Task.DelaySeconds(startDelay);
			}

			TryToReachRestingPosition = true;
			TryToReachDesiredPosition = false;

			Body.PhysicsBody.EnableSolidCollisions = false;
		}

		public async void SetGlowState(bool state, float delay = 0)
		{
			if (delay > 0)
				await Task.DelaySeconds(delay);

			Glowing = state;
		}
	}
}
