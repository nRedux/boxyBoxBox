using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IEntity
{
    public TQuality GetQuality<TQuality>() where TQuality: class;
}


public class Entity : MonoBehaviour, IEntity
{

    [SerializeField]
    private FakeHeight _heightOffset;

    private List<IIEntityQuality> _qualities = null;

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


    /// <summary>
    /// Find all of the quality components attached
    /// </summary>
    private void InitializeQualities()
    {
        _qualities = gameObject.GetDerivedComponents<IIEntityQuality>();
    }


    /// <summary>
    /// Try to get a quality on this entity
    /// </summary>
    /// <typeparam name="TQuality">The concrete type you're interested in.</typeparam>
    /// <returns>The component requested if it exists, and null otherwise.</returns>
    public TQuality GetQuality<TQuality>() where TQuality: class
    {
        return _qualities.FirstOrDefault( x => x.GetType() == typeof( TQuality ) ) as TQuality;
    }

}
