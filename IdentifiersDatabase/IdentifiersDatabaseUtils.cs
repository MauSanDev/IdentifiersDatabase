using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class IdentifiersDatabaseUtils
{
    public static string GenerateHash<T>(int digits, List<T> siblings, string parentGuid = "") where T : IIdentifiable
    {
        int maxDigits = Mathf.RoundToInt(Mathf.Pow(16, digits));

        if (maxDigits == siblings.Count)
        {
            throw new Exception($"Cannot generate Hash because the Database is full for {maxDigits} elements.");
        }
        
        int random = Random.Range(0, maxDigits);
        string hash = random.ToString("x" + digits);

        while (HashExists(hash, siblings, parentGuid))
        {
            random++;
            random = Mathf.RoundToInt(Mathf.Repeat(random, maxDigits + 1));
            hash = random.ToString("x" + digits);
        }

        return parentGuid + hash;
    }

    private static bool HashExists<T>(string hash, List<T> siblings, string parentGuid) where T : IIdentifiable
    {
        if(!string.IsNullOrEmpty(parentGuid))
        {
            hash = hash.Replace(parentGuid, string.Empty);
        }

        return siblings.Find(x => x.GUID == hash) != null;
    }
}
