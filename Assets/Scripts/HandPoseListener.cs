#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class HandPoseListener : MonoBehaviour
{
    public LeapProvider leapProvider;
    public float DistanceThreshold = 0.025f;
    public float CoolDownTime = 0.5f;

    DrawState _state = DrawState.EMPTY;
    public DrawState State { get => _state; }

    readonly DetectedHand _leftHand = new DetectedHand();
    readonly DetectedHand _rightHand = new DetectedHand();

    float _coolDownTime;
    GameObject debugCube;
    // Get an encapsulation class of:
    // 1. LastDetected: the Hand object last detected (only null before the first detection);
    // 2. Current: the Hand object currently detected (could be null when no hand is detected currently);
    public DetectedHand GetDetectedHand(Chirality chirality)
    {
        if (chirality == Chirality.Left)
        {
            return _leftHand;
        } else
        {
            return _rightHand;
        }
    }
    private void Start()
    {
        _coolDownTime = CoolDownTime;
        debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugCube.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
    }
    private void Update()
    {
        if (_state == DrawState.COOLING_DOWN)
        {
            _coolDownTime = Mathf.Max(0f, _coolDownTime - Time.deltaTime);
            if (_coolDownTime == 0f)
            {
                _coolDownTime = CoolDownTime;
                _state = DrawState.EMPTY;
            }
        }
    }

    private void OnUpdateFrame(Frame frame)
    {
        Hand leftHand = frame.GetHand(Chirality.Left);
        Hand rightHand = frame.GetHand(Chirality.Right);
        _leftHand.Current = leftHand;
        _rightHand.Current = rightHand;
        if (leftHand != null)
        {
            _leftHand.LastDetected = leftHand;
        }
        if (rightHand != null)
        {
            _rightHand.LastDetected = rightHand;
        }
        ProcessHands(leftHand, rightHand);
    }

    private void ProcessHands(Hand? leftHand, Hand? rightHand)
    {
        if (leftHand == null || rightHand == null)
        {
            if (_state == DrawState.DRAWING)
            {
                _state = DrawState.DRAWING_FINISHED;
            }
            if (_state == DrawState.SENDING)
            {
                Send();
                _state = DrawState.COOLING_DOWN;
            }
            debugCube.SetActive(false);
            return;
        }
        debugCube.SetActive(true);
        debugCube.transform.position = leftHand.WristPosition;
        DrawState tentativeState = DrawState.DRAWING_FINISHED;
        if (IsFingerTouchingPalm(rightHand.Fingers[1], leftHand))
        {
            print("overlapped");
            tentativeState = DrawState.DRAWING;
        }
        int count = 0;
        foreach (Finger finger in rightHand.Fingers)
        {
            if (IsFingerTouchingPalm(finger, leftHand))
            {
                count++;
            }
        }
        if (count >= 2)
        {
            print("overlapped: " + count);
            tentativeState = DrawState.SENDING;
        }
        ProcessDrawStateChange(tentativeState, rightHand, leftHand);
    }

    private void AddDrawingPoint(Finger finger, Hand hand)
    {
        Pose palmPose = hand.GetPalmPose();
        Vector3 transversePositiveAxis = Vector3.Normalize(Vector3.Cross(hand.WristPosition - hand.PalmPosition, hand.PalmNormal));
        Vector3 palmNormal = Vector3.Normalize(hand.PalmNormal);
        Vector3 palmToWrist = Vector3.Normalize(Vector3.Cross(palmNormal, transversePositiveAxis));
        //Vector3 palmNormal = Vector3.Normalize(hand.PalmNormal);
        Vector3 palmPosition = hand.PalmPosition;
        Vector3 palmDirection = hand.Direction;
        Vector3 rayBase = finger.bones[(int)Bone.BoneType.TYPE_DISTAL].PrevJoint;
        Vector3 rayDirection = finger.TipPosition - rayBase;
        float a = Vector3.Dot((palmPosition - rayBase), palmNormal);
        float b = Vector3.Dot(rayDirection, palmNormal);
        if (b == 0)
        {
            return;
        }
        Vector3 intersection = a / b * rayDirection + rayBase;
        debugCube.transform.position = intersection;
    }

    private void Send()
    {
        print("sent!");
        debugCube.transform.position = new Vector3(-1f, -1f, -1f);
    }

    private void ProcessDrawStateChange(DrawState tentativeState, Hand rightHand, Hand hand)
    {
        DrawState resultingState = _state;
        if (tentativeState == DrawState.DRAWING_FINISHED && _state == DrawState.DRAWING)
        {
            resultingState = DrawState.DRAWING_FINISHED;
        }
        if (tentativeState == DrawState.DRAWING_FINISHED && _state == DrawState.SENDING)
        {
            Send();
            resultingState = DrawState.COOLING_DOWN;
        }
        if (tentativeState == DrawState.DRAWING && _state == DrawState.EMPTY)
        {
            resultingState = DrawState.DRAWING;
        }
        if (tentativeState == DrawState.SENDING && _state == DrawState.DRAWING_FINISHED)
        {
            resultingState = DrawState.SENDING;
        }
        _state = resultingState;
        if (_state == DrawState.DRAWING)
        {
            AddDrawingPoint(rightHand.Fingers[1], hand);
        }
    }

    // [WristDistance (+: toward the wrist/ -: away from the wrist),
    // TransverseDistance (always +),
    // VerticalDistance (always +),
    // ]
    private float[] GetDistancesBetweenFingerTipAndPalm(Finger finger, Hand hand)
    {
        Vector3 palmPosition = hand.PalmPosition;
        Vector3 transversePositiveAxis = Vector3.Normalize(Vector3.Cross(hand.WristPosition - hand.PalmPosition, hand.PalmNormal));
        Vector3 palmNormal = Vector3.Normalize(hand.PalmNormal);
        Vector3 palmToWrist = Vector3.Normalize(Vector3.Cross(palmNormal, transversePositiveAxis));
        Vector3 fingerTip = finger.TipPosition;
        float verticalDistance = Vector3.Dot(fingerTip - palmPosition, palmNormal);
        //Vector3 horizontalPalmVector = Vector3.Normalize(Vector3.Cross(palmNormal, Vector3.Cross(fingerTip - palmPosition, palmNormal)));
        //float horizontalDistance = Vector3.Dot(fingerTip - palmPosition, horizontalPalmVector);
        //return new float[] { Mathf.Abs(verticalDistance), Mathf.Abs(horizontalDistance) };
        float wristDistance = Vector3.Dot(fingerTip - palmPosition, palmToWrist);
        float transverseDistance = Vector3.Dot(fingerTip - palmPosition, transversePositiveAxis);
        return new float[] { wristDistance, Mathf.Abs(transverseDistance), Mathf.Abs(verticalDistance) };
    }

    private bool IsFingerTouchingPalm(Finger finger, Hand hand)
    {
        float[] distances = GetDistancesBetweenFingerTipAndPalm(finger, hand);
        return distances[2] <= DistanceThreshold &&
            distances[1] <= hand.PalmWidth &&
            distances[0] >= -hand.PalmWidth && distances[0] <= Vector3.Distance(hand.WristPosition, hand.PalmPosition);
    }

    // Check if any fingers are folded into the hand.
    public bool isHandCoveredByFingers(Hand hand)
    {
        Vector3 palmPosition = hand.PalmPosition;
        foreach (Finger leftFinger in hand.Fingers)
        {
            Vector3 root = leftFinger.bones[(int)Bone.BoneType.TYPE_METACARPAL].PrevJoint;
            Vector3 tip = leftFinger.TipPosition;
            if (Vector3.Dot(tip - root, root - palmPosition) < 0f)
            {
                // The finger is covering the palm;
                return true;
            }
        }
        return false;
    }

    private void OnEnable()
    {
        if (leapProvider != null)
        {
            leapProvider.OnUpdateFrame += OnUpdateFrame;
        }
    }

    private void OnDisable()
    {
        if (leapProvider != null)
        {
            leapProvider.OnUpdateFrame -= OnUpdateFrame;
        }
    }

    public Vector3 GetPalmPose(){
        
        if(_leftHand.LastDetected!= null){
            return _leftHand.LastDetected.PalmPosition;
        }
        else{
            return new Vector3(0,0,0);
        }
    }
    
    public Vector3 GetHandDirection(){
        
        if(_leftHand.LastDetected!= null){
            return (_leftHand.LastDetected.PalmPosition - _leftHand.LastDetected.WristPosition).normalized;
        }
        else{
            return new Vector3(0,0,0);
        }
    }
}
