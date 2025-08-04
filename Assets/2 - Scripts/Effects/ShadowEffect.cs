using UnityEngine;

public class ShadowEffect : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer _spriteRenderer;
    [SerializeField]
    private Vector3 _offset;
    [SerializeField]
    private Material _material;
    [SerializeField]
    private Vector3 _scale = Vector3.one;
    [SerializeField]
    private Color _tint;

    private GameObject _shadow;
    private SpriteRenderer _shadowRenderer;
    private Entity _entity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        if( _spriteRenderer == null )
            _spriteRenderer = GetComponent<SpriteRenderer>();

        _entity = GetComponent<Entity>();

        if( _spriteRenderer == null )
            return;

        _shadow = new GameObject( "shadow" );
        _shadow.transform.parent = transform;
        _shadow.transform.localScale = _scale;
        _shadow.transform.localPosition = _offset;
        _shadow.transform.localRotation = Quaternion.identity;

        _shadowRenderer = _shadow.AddComponent<SpriteRenderer>();
        _shadowRenderer.sprite = _spriteRenderer.sprite;
        _shadowRenderer.material = _material;
        _shadowRenderer.color = _shadowRenderer.color * _tint;
        _shadowRenderer.sortingLayerName = _spriteRenderer.sortingLayerName;
        _shadowRenderer.sortingOrder = 1;
    }

    private void Update()
    {
        if( _entity != null )
        {
            _shadow.transform.localPosition = _offset + Vector3.down * _entity.Height;
        }

        if( _shadowRenderer.sprite != _spriteRenderer.sprite )
            _shadowRenderer.sprite = _spriteRenderer.sprite;
    }
}
