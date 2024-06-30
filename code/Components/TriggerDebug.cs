public class TriggerDebug : ModelCollider, Component.ITriggerListener
{
    public List<Collider> UniqueColliders { get; set; } = new();

    TimeSince LastUpdate = 0;
    protected override void OnUpdate()
    {
        base.OnUpdate();

        // if ( LastUpdate > 2 )
        // {
        // LastUpdate = 0;
        // Log.Info( $"unique colliders overtime: {UniqueColliders.Count()}" );
        // Log.Info( $"Currently touching: {Touching.Count()}" );
        // }
    }

    public new void OnTriggerEnter( Collider other )
    {
        Log.Info( $"something entered the trigger, we are being touched by {Touching.Count()} colliders" );

        if ( !UniqueColliders.Contains( other ) )
        {
            UniqueColliders.Add( other );
        }
    }

    public new void OnTriggerExit( Collider other )
    {
        Log.Info( $"something exited the trigger, we are being touched by {Touching.Count()} colliders" );
    }
}
