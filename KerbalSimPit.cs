using System;

using UnityEngine;

[KSPAddon(KSPAddon.Startup.Instantly, true)]
public class KerbalSimPit : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(this);
        Debug.Log("KerbalSimPit: Started.");
    }
}
