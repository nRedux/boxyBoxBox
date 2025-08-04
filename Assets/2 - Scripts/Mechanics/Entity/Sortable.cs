using UnityEngine;


public enum SortType { Red, Blue}

public class Sortable : MonoBehaviour, IIEntityQuality
{
    public SortType SortType;
}
