using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.Utilities;

//A more mature implementation would employ pooling.

public class BoxSpawner : MonoBehaviour
{
    private World _world;

    public Entity BoxPrefab;
    public int MaxBoxes;
    public float DelayBetween;

    private float _delayTimer;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _world = FindFirstObjectByType<World>();

        _delayTimer = DelayBetween;

        if( _world == null || _spriteRenderer == null || BoxPrefab == null )
            enabled = false;
    }


    private Vector3 GetSpawnlocation()
    {
        var bounds = _spriteRenderer.localBounds;
        bounds.center = transform.TransformPoint( bounds.center );
        Vector3 x = Vector3.Lerp( bounds.min, bounds.min + Vector3.right * bounds.size.x * transform.lossyScale.x, Random.value );
        Vector3 y = Vector3.Lerp( bounds.min, bounds.min + Vector3.up * bounds.size.y * transform.lossyScale.y, Random.value );
        Vector3 withinBounds = transform.TransformVector( x + y );
        
        return bounds.min + x + y;
    }


    private void Update()
    {

        _delayTimer -= Time.deltaTime;
        if( _delayTimer <= 0f && _world.BoxCount <= MaxBoxes )
        {
            _delayTimer = DelayBetween;
            SpawnBox();
        }
        
        /*
        if( Input.GetKeyDown( KeyCode.B ) )
        {
            SpawnBox();
        }
        */
    }

    private void SpawnBox()
    {
        if( BoxPrefab == null )
            return;

        var instance = Instantiate<Entity>( BoxPrefab );
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
