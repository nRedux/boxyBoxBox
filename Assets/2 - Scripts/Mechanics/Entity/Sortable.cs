using UnityEngine;


public enum SortType { Red, Blue}


/// <summary>
/// Entities with this quality attached are able to be sorted by agents
/// </summary>
public class Sortable : MonoBehaviour, IIEntityQuality
{
    public SortType SortType;
}
