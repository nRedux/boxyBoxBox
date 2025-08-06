using UnityEngine;

[System.Serializable]
public class RendererSortReference
{
    public SpriteRenderer Renderer;
    public int DepthOffset;

    public void ApplyDepth( int depth )
    {
        Renderer.sortingOrder = depth + DepthOffset;
    }
}


/// <summary>
/// Could be optimized via central sort order manager - quick and dirty here
/// </summary>
[ExecuteAlways]
public class DepthSorter : MonoBehaviour
{
    [SerializeField]
    private RendererSortReference[] _spriteRendererHierarchy = null;

    private Camera _camera;
    

    private void Start()
    {
        UpdateOrder();
    }


    // Update is called once per frame
    void Update()
    {
        UpdateOrder();       
    }


    private void UpdateOrder( )
    {
        if( _camera == null )
            _camera = Camera.main;
        var order = _camera.GetDepthSortOrder( transform );
        _spriteRendererHierarchy.Do( x => x.ApplyDepth( order ) );
    }
}
