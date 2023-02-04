using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static T GetRandom<T>(this List<T> list)
    {
        var listCount = list.Count;
        var randomIdx = Random.Range(0, listCount);
        return list[randomIdx];
    }
}