using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class Extensions
{
    
    private static int DEPTH_ORDER_START = 10000;
    private static int DEPTH_ORDER_DENSITY = 10;


    /// <summary>
    /// Identify components which derive from the given type
    /// </summary>
    /// <typeparam name="TSource">Base type</typeparam>
    /// <param name="gameObject">The game object to hunt within</param>
    /// <returns>List of found components</returns>
    public static List<TSource> GetDerivedComponents<TSource>( this GameObject gameObject ) where TSource: class
    {
        var monoBehaviors = gameObject.GetComponents<MonoBehaviour>();
        return monoBehaviors.Where( x => typeof( TSource ).IsAssignableFrom( x.GetType() ) ).Select( x => x as TSource).ToList();
    }


    /// <summary>
    /// Helper linq style operation to perform an action on a collection
    /// </summary>
    /// <typeparam name="TSource">Collection template type</typeparam>
    /// <param name="collection">The collection</param>
    /// <param name="action">The action to perform</param>
    /// <exception cref="System.ArgumentNullException">If either argument is null, is thrown</exception>
    public static void Do<TSource>( this IEnumerable<TSource> collection, System.Action<TSource> action )
    {
        if( collection == null )
            throw new System.ArgumentNullException( $"Argument {nameof(collection)} cannot be null" );
        if( action == null )
            throw new System.ArgumentNullException( $"Argument {nameof( action )} cannot be null" );
        foreach( var item in collection )
            action( item );
    }


    /// <summary>
    /// Simple depth sorting algo for sprites
    /// </summary>
    /// <param name="camera">Camera we're interacting with</param>
    /// <param name="transform">Transform we want to find a depth value for</param>
    /// <returns>A depth value derived from screen space</returns>
    public static int GetDepthSortOrder( this Camera camera, Transform transform )
    {
        var screenPoint = camera.WorldToScreenPoint( transform.position );
        return DEPTH_ORDER_START - Mathf.FloorToInt( screenPoint.y ) / DEPTH_ORDER_DENSITY;
    }
}