using UnityEngine;



public class Grab : MonoBehaviour, IEntityAction
{
    private Grabbable _grabbed;

    public void Execute( IEntity entity  )
    {
        var grabbable = entity.GetQuality<Grabbable>();
        if( grabbable == null )
            return;

        _grabbed = grabbable as Grabbable;
    }

    public void Update()
    {
        if( _grabbed != null )
        {
            _grabbed.transform.position = transform.position;
        }
    }
}
