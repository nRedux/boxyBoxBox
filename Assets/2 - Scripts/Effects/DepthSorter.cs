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
public class DepthSorter : MonoBehaviour
{
    [SerializeField]
    private RendererSortReference[] _spriteRendererHierarchy = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
    }


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
        var order = World.Instance.Camera.GetDepthSortOrder( transform );
        _spriteRendererHierarchy.Do( x => x.ApplyDepth( order ) );
    }
}
