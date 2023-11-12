#nullable enable
using UnityEngine;
using Leap;
using Leap.Unity;

public class DetectedHand
{
    Hand? _lastDetected;
    Hand? _current;
    public Hand? LastDetected { get => _lastDetected; set => _lastDetected = value; }
    public Hand? Current { get => _current; set => _current = value; }
}

public enum DrawState
{
    EMPTY,
    DRAWING,
    DRAWING_FINISHED,
    SENDING,
    COOLING_DOWN,
}

