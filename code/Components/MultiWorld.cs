public class MultiWorld : Component
{
    [Property]
    public int WorldIndex { get; private set; } = -1;

    public IEnumerable<GameObject> GetAllChildrenRecursive()
    {
        foreach ( var child in GameObject.Children )
        {
            yield return child;
            foreach ( var grandChild in child.Components.Get<MultiWorld>().GetAllChildrenRecursive() )
            {
                yield return grandChild;
            }
        }
    }

    protected override void OnStart()
    {
        base.OnStart();

        Tags.Add( MultiWorldSystem.GetWorldTag( WorldIndex ) );
        MultiWorldSystem.Init();
    }
}
