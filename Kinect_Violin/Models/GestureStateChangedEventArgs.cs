using System;

namespace WpfKinectV2CustomButton.Models
{
    public sealed class GestureStateChangedEventArgs : EventArgs
    {
        public GestureStateChangedEventArgs(int bodyIndex, bool isTracked, bool isDetected, float confidence)
        {
            BodyIndex = bodyIndex;
            IsTracked = isTracked;
            IsDetected = isDetected;
            Confidence = confidence;
        }

        public int BodyIndex { get; }

        public bool IsTracked { get; }

        public bool IsDetected { get; }

        public float Confidence { get; }
    }
}
