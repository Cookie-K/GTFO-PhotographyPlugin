using UnityEngine;

namespace CinematographyPlugin.Util;

public class IndependentDeltaTimeManager : MonoBehaviour
{
    private static float _independentDeltaTime;
    private static float _lastInterval;
    
    private void Update()
    {
        var now = Time.realtimeSinceStartup;
        _independentDeltaTime = now - _lastInterval;
        _lastInterval = now;
    }

    public static float GetDeltaTime()
    {
        return _independentDeltaTime;
    }
}