using UnityEngine;


/// <summary>
/// Simple setup for faking height
/// </summary>
[System.Serializable]
public class FakeHeight
{
    private const float SLEEP_VELOCITY = .001f;
    private const float COEF_RESTITUTION_MIN = .10f;
    private const float COEF_RESTITUTION_MAX = .25f;

    [SerializeField]
    private AudioSource _thudSFX;
    [SerializeField]
    private float _height = 10f;
    [SerializeField]
    private bool _applyGravity = true;
    [SerializeField]
    private GameObject _smokeEffect;
    [SerializeField]
    private Transform _graphic;

    private float _velocity;
    private bool _asleep = false;

    /// <summary>
    /// Current height
    /// </summary>
    public float Height => _height;


    public void Initialize( Transform transform )
    {
        //Just for convenience if not assigned directly (assumes first child - what could go wrong.)
        if( _graphic == null && transform.childCount > 0 )
            _graphic = transform.GetChild( 0 );
    }


    /// <summary>
    /// Update current position based on height value and update physics.
    /// </summary>
    public void Update()
    {
        if( _graphic == null )
            return;

        UpdateGravity();

        _graphic.transform.localPosition = Vector3.zero + Vector3.up * _height;
    }


    /// <summary>
    /// Simulate gravity on the faked height.
    /// </summary>
    private void UpdateGravity()
    {
        if( !_applyGravity || _asleep )
            return;

        _velocity += Physics2D.gravity.y * Time.fixedDeltaTime * Time.fixedDeltaTime;
        _height = _height + _velocity;
        
        //Thud
        if( _height <= 0f )
        {
            //Bouncey
            _velocity = -_velocity * Random.Range(COEF_RESTITUTION_MIN, COEF_RESTITUTION_MAX);
            _height = float.Epsilon;
            if( Mathf.Abs( _velocity ) < SLEEP_VELOCITY )
                _asleep = true;
            else if( _thudSFX != null )
                _thudSFX.Play();
            if( _smokeEffect )
                _smokeEffect.SetActive( true );
        }
    }
}
