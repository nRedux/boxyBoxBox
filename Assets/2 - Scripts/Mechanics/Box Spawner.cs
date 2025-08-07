using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.Utilities;

//A more mature implementation would employ pooling.

public class BoxSpawner : MonoBehaviour
{

    [SerializeField]
    private Entity[] _boxPrefabs;
    [SerializeField]
    private int _maxBoxes;
    [SerializeField]
    private float _delayBetwen;


    private World _world;
    private float _delayTimer;
    private SpriteRenderer _spriteRenderer;


    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _world = FindFirstObjectByType<World>();

        _delayTimer = _delayBetwen;

        if( _world == null || _spriteRenderer == null || _boxPrefabs.Length == 0 )
            enabled = false;
    }


    private void Update()
    {
        if( !_world.Playing )
            return;
        _delayTimer -= Time.deltaTime;
        if( _delayTimer <= 0f && _world.BoxCount <= _maxBoxes )
        {
            _delayTimer = _delayBetwen;
            SpawnBox();
        }
    }


    /// <summary>
    /// Get location to spawn box
    /// </summary>
    /// <returns>Location within spawner SpriteRenderer</returns>
    private Vector3 GetSpawnlocation()
    {
        var bounds = _spriteRenderer.localBounds;
        bounds.center = transform.TransformPoint( bounds.center );
        Vector3 x = Vector3.Lerp( bounds.min, bounds.min + Vector3.right * bounds.size.x * transform.lossyScale.x, Random.value );
        Vector3 y = Vector3.Lerp( bounds.min, bounds.min + Vector3.up * bounds.size.y * transform.lossyScale.y, Random.value );
        Vector3 withinBounds = transform.TransformVector( x + y );
        
        return bounds.min + x + y;
    }


    /// <summary>
    /// Spawn a new box instance
    /// </summary>
    private void SpawnBox()
    {
        if( _boxPrefabs == null )
            return;

        var instance = Instantiate<Entity>( _boxPrefabs[Random.Range(0, _boxPrefabs.Length) ] );
        var sortable = instance.GetQuality<Sortable>();
        if( sortable == null )
        {
            Destroy( instance.gameObject );
            enabled = false;
            Debug.Log( "Invalid Entity setup for BoxSpawner." );
        }
        instance.transform.position = GetSpawnlocation();
        _world.AddBox( instance );
    }

}
