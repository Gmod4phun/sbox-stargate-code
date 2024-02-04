public class AtlantisTransporterTrigger : ModelCollider, Component.ITriggerListener
{
    private AtlantisTransporter Transporter => GameObject.Parent.Components.Get<AtlantisTransporter>();

    public void OnTriggerEnter( Collider other )
    {

    }

    public void OnTriggerExit( Collider other )
    {

    }
}
