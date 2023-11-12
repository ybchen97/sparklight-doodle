#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using System;
using UnityEngine.Networking;

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

    List<Vector3> _drawPoints = new List<Vector3>();

    // Vectors in the following coordinate (relative to the center of the palm) :
    // (0): Palm Center to Wrist
    // (1): Cross (0), (2)
    // (2): Palm Normal
    public List<Vector3> DrawPoints { get => _drawPoints; }

    // Max (x, y)
    Vector2 topLeft = new Vector2(float.MinValue, float.MinValue);
    // Min (x, y)
    Vector2 bottomRight = new Vector2(float.MaxValue, float.MaxValue);

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
                ClassifyDrawing();
                _state = DrawState.CLASSIFYING;
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
        DrawState tentativeState = DrawState.EMPTY;
        if (IsFingerTouchingPalm(rightHand.Fingers[1], leftHand))
        {
            //print("overlapped");
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
            //print("overlapped: " + count);
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
        Vector3 relativeIntersection = intersection - palmPosition;
        float x = Vector3.Dot(relativeIntersection, palmToWrist);
        float y = Vector3.Dot(relativeIntersection, transversePositiveAxis);
        float z = Vector3.Dot(relativeIntersection, palmNormal);
        if (_drawPoints.Count == 0 ||
            _drawPoints.Count > 0 && Vector3.Distance(new Vector3(x, y, z), _drawPoints[_drawPoints.Count - 1]) >= 0.001)
        {
            _drawPoints.Add(new Vector3(x, y, z));
            float distance = Vector3.Distance(palmPosition, hand.WristPosition);
            topLeft.x = Mathf.Max(topLeft.x, distance);
            topLeft.y = Mathf.Max(topLeft.y, hand.PalmWidth);
            bottomRight.x = Mathf.Min(bottomRight.x, -hand.PalmWidth);
            bottomRight.y = Mathf.Min(bottomRight.y, -hand.PalmWidth);
        }
    }

    private void Send()
    {
        print("sent!");
        _drawPoints.Clear();
        debugCube.transform.position = new Vector3(-1f, -1f, -1f);
    }

    private void ProcessDrawStateChange(DrawState tentativeState, Hand rightHand, Hand hand)
    {
        DrawState resultingState = _state;
        if (tentativeState == DrawState.EMPTY && _state == DrawState.DRAWING)
        {
            ClassifyDrawing();
            resultingState = DrawState.CLASSIFYING;
        }
        if (tentativeState == DrawState.EMPTY && _state == DrawState.SENDING)
        {
            Send();
            resultingState = DrawState.COOLING_DOWN;
        }
        if (tentativeState == DrawState.DRAWING && _state == DrawState.EMPTY)
        {
            resultingState = DrawState.DRAWING;
        }
        if (tentativeState == DrawState.SENDING && _state == DrawState.CLASSIFY_FINISHED)
        {
            resultingState = DrawState.SENDING;
        }
        _state = resultingState;
        if (_state == DrawState.DRAWING)
        {
            AddDrawingPoint(rightHand.Fingers[1], hand);
        }
    }

    private void ClassifyDrawing()
    {
        List<Vector2> coords = GetDrawnImageCoordinates();
        StartCoroutine(GetRemoteClassification(coords));
    }

    private IEnumerator GetRemoteClassification(List<Vector2> coords)
    {
        string stringCoords = "";
        for (int i = 0; i < coords.Count; i++)
        {
            Vector2 coord = coords[i];
            stringCoords += coord.x + " " + coord.y;
            if (i < coords.Count - 1)
            {
                stringCoords += ",";
            }
        }
        string uri = "http://localhost:4000";
        print(stringCoords);
        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, "{ \"coords\": \"" + stringCoords + "\" }", "application/json"))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    print(webRequest.downloadHandler.text);
                    _state = DrawState.CLASSIFY_FINISHED;
                    break;
            }
        }
    }

    private void OnInvalidDraw()
    {

    }

    public List<Vector2> GetDrawnImageCoordinates()
    {
        //int[,] image = new int[80, 60];
        List<Vector2> result = new List<Vector2>();
        float xUnit = (topLeft.x - bottomRight.x) / 79;
        float yUnit = (topLeft.y - bottomRight.y) / 79;
        //int[,] deltas = new int[,] {
        //    { 1, 1 }, { 1, 0 }, { 1, -1 },
        //    { 0, 1 }, { 0, 0 }, { 0, -1 },
        //    { -1, 1 }, { -1, 0 }, { -1, -1 }
        //};
        foreach (Vector3 point in _drawPoints)
        {
            int x = Mathf.RoundToInt((topLeft.x - point.x) / xUnit);
            int y = Mathf.RoundToInt((topLeft.y - point.y) / yUnit);
            result.Add(new Vector2(x, y));
            //image[x, y] = 255;
            //for (int i = 0; i < deltas.Length / 2; i++)
            //{
            //    int deltaX = x + deltas[i, 0];
            //    int deltaY = y + deltas[i, 1];
            //    if (deltaX >= 0 && deltaX < 80 && deltaY >= 0 && deltaY < 60)
            //    {
            //        image[deltaX, deltaY] = 255;
            //    }
            //}
        }
        return result;
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
}
