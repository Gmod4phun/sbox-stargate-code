public class BasePlayer : Component
{
    [Property]
    public CameraComponent Camera { get; set; }

    [Property, Sync]
    public int CurrentWorldIndex { get; set; } = 0;

    [Property] public CharacterController Controller => Components.Get<CharacterController>();
}
