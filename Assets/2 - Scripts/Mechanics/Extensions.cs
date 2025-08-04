using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public static class Extensions
{
    
    public static List<TSource> GetDerivedComponents<TSource>( this GameObject gameObject ) where TSource: class
    {
        var monoBehaviors = gameObject.GetComponents<MonoBehaviour>();
        return monoBehaviors.Where( x => typeof( TSource ).IsAssignableFrom( x.GetType() ) ).Select( x => x as TSource).ToList();
    }



}


public static class MathfX
{

    public static float EaseOutBounce( float x )
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if( x < 1 / d1 )
        {
            return n1 * x * x;
        }
        else if( x < 2 / d1 )
        {
            return n1 * ( x -= 1.5f / d1 ) * x + 0.75f;
        }
        else if( x < 2.5 / d1 )
        {
            return n1 * ( x -= 2.25f / d1 ) * x + 0.9375f;
        }
        else
        {
            return n1 * ( x -= 2.625f / d1 ) * x + 0.984375f;
        }
    }
}
