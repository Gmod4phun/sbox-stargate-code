public class PlayerPressController : Component
{
	PlayerController PlayerController => Components.Get<PlayerController>(FindMode.InSelf);

	protected override void OnUpdate()
	{
		if (!IsProxy)
		{
			if (!PlayerController.IsValid())
				return;

			UpdateLookAt();
		}
	}

	public void UpdateLookAt()
	{
		if (PlayerController.Pressed.IsValid())
		{
			UpdatePressed();
		}
		else
		{
			UpdateHovered();
		}
	}

	private void UpdatePressed()
	{
		if (!string.IsNullOrWhiteSpace(PlayerController.UseButton))
		{
			bool flag = Input.Down(PlayerController.UseButton);
			if (flag && PlayerController.Pressed is IPressable pressable)
			{
				flag = pressable.Pressing(
					new IPressable.Event
					{
						Ray = PlayerController.EyeTransform.ForwardRay,
						Source = this
					}
				);
			}

			if (
				GetDistanceFromGameObject(
					PlayerController.Pressed.GameObject,
					PlayerController.EyePosition
				) > PlayerController.ReachLength
			)
			{
				flag = false;
			}

			if (!flag)
			{
				PlayerController.StopPressing();
			}
		}
	}

	private float GetDistanceFromGameObject(GameObject obj, Vector3 point)
	{
		float num = Vector3.DistanceBetween(obj.WorldPosition, PlayerController.EyePosition);
		foreach (
			Collider componentsInChild in PlayerController.Pressed.GetComponentsInChildren<Collider>()
		)
		{
			float num2 = Vector3.DistanceBetween(
				componentsInChild.FindClosestPoint(PlayerController.EyePosition),
				PlayerController.EyePosition
			);
			if (num2 < num)
			{
				num = num2;
			}
		}

		return num;
	}

	private void UpdateHovered()
	{
		SwitchHovered(TryGetLookedAt());
		if (PlayerController.Hovered is IPressable pressable)
		{
			pressable.Look(
				new IPressable.Event
				{
					Ray = PlayerController.EyeTransform.ForwardRay,
					Source = this
				}
			);
		}

		if (Input.Pressed(PlayerController.UseButton))
		{
			PlayerController.StartPressing(PlayerController.Hovered);
		}
	}

	private void SwitchHovered(Component obj)
	{
		IPressable.Event e = new IPressable.Event
		{
			Ray = PlayerController.EyeTransform.ForwardRay,
			Source = this
		};
		if (PlayerController.Hovered == obj)
		{
			if (PlayerController.Hovered is IPressable pressable)
			{
				pressable.Look(e);
			}

			return;
		}

		if (PlayerController.Hovered is IPressable pressable2)
		{
			pressable2.Blur(e);
			PlayerController.Hovered = null;
		}

		PlayerController.Hovered = obj;
		if (PlayerController.Hovered is IPressable pressable3)
		{
			pressable3.Hover(e);
			pressable3.Look(e);
		}
	}

	private Component TryGetLookedAt()
	{
		var start = Scene.Camera.WorldPosition;
		var end = start + Scene.Camera.WorldRotation.Forward * PlayerController.ReachLength;

		var eyeTrace = Scene
			.Trace.Ray(start, end)
			.WithWorld(GameObject)
			.IgnoreGameObjectHierarchy(GameObject)
			.Run();

		if (!eyeTrace.Hit || !eyeTrace.GameObject.IsValid())
		{
			return null;
		}

		Component foundComponent = null;
		ISceneEvent<PlayerController.IEvents>.PostToGameObject(
			GameObject,
			delegate(PlayerController.IEvents x)
			{
				foundComponent = x.GetUsableComponent(eyeTrace.GameObject) ?? foundComponent;
			}
		);
		if (foundComponent.IsValid())
		{
			return foundComponent;
		}

		var collider = eyeTrace.Collider;
		if (!collider.IsValid())
		{
			return null;
		}

		foreach (var component in collider.GetComponents<IPressable>())
		{
			if (
				component.CanPress(
					new IPressable.Event
					{
						Ray = PlayerController.EyeTransform.ForwardRay,
						Source = this
					}
				)
			)
			{
				return component as Component;
			}
		}

		return null;
	}
}
