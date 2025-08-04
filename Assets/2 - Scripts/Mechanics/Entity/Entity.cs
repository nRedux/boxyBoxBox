using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public interface IEntity
{
    public TQuality GetQuality<TQuality>() where TQuality: class;
}


[System.Serializable]
public class FakeHeight
{
    private const float SLEEP_VELOCITY = .001f;
    private const float COEF_RESTITUTION = .15f;

    public float Height;
    public bool ApplyGravity = true;

    public GameObject SmokeEffect;

    public Transform Graphic;

    private float _velocity;
    private bool _asleep = false;

    public void Initialize( Transform transform )
    {
        //Just for convenience if not assigned directly
        if( Graphic == null && transform.childCount > 0 )
            Graphic = transform.GetChild( 0 );
    }

    public void Update()
    {
        if( Graphic == null )
            return;

        UpdateGravity();

        Graphic.transform.localPosition = Vector3.zero + Vector3.up * Height;
    }

    private void UpdateGravity()
    {
        if( !ApplyGravity || _asleep )
            return;

        _velocity += Physics2D.gravity.y * Time.fixedDeltaTime * Time.fixedDeltaTime;
        Height = Height + _velocity;
        
        if( Height <= 0f )
        {
            _velocity = -_velocity * COEF_RESTITUTION;
            Height = float.Epsilon;
            if( Mathf.Abs( _velocity ) < SLEEP_VELOCITY )
                _asleep = true;

            if( SmokeEffect )
                SmokeEffect.SetActive( true );
        }
    }
}


public class Entity : MonoBehaviour, IEntity
{
    [SerializeField]
    private FakeHeight _heightOffset;

    private List<IIEntityQuality> _qualities;

    public float Height => _heightOffset.Height;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _heightOffset.Initialize(transform);
        _heightOffset.Update();
        InitializeQualities();
    }

    private void FixedUpdate()
    {
        _heightOffset.Update();
    }

    private void InitializeQualities()
    {
        _qualities = gameObject.GetDerivedComponents<IIEntityQuality>();
    }


    public TQuality GetQuality<TQuality>() where TQuality: class
    {
        return _qualities.FirstOrDefault( x => x.GetType() == typeof( TQuality ) ) as TQuality;
    }

}
