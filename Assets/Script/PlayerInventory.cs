using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private Dictionary<string, int> keys = new Dictionary<string, int>();

    public void AddKey(string keyID)
    {
        if (keys.ContainsKey(keyID))
        {
            keys[keyID] += 1;
        }
        else
        {
            keys[keyID] = 1;
        }
    }

    public bool HasKey(string keyID, int amountRequied)
    {
        return keys.ContainsKey(keyID) && keys[keyID] >= amountRequied;
    }
}
