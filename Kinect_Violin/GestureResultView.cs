//------------------------------------------------------------------------------
// <copyright file="GestureResultView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace WpfKinectV2CustomButton
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Media;
    using WpfKinectV2CustomButton.Models;

    /// <summary>
    /// Stores discrete gesture results for display and raises state-change events.
    /// </summary>
    public sealed class GestureResultView : INotifyPropertyChanged
    {
        private static readonly Brush[] TrackedColors =
        {
            Brushes.Red,
            Brushes.Orange,
            Brushes.Green,
            Brushes.Blue,
            Brushes.Indigo,
            Brushes.Violet,
        };

        private readonly int bodyIndex;
        private Brush bodyColor = Brushes.Silver;
        private float confidence;
        private bool detected;
        private bool isTracked;
        private bool wasDetected;

        public GestureResultView(int bodyIndex)
        {
            this.bodyIndex = bodyIndex;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<GestureStateChangedEventArgs> GestureStateChanged;

        public int BodyIndex
        {
            get { return bodyIndex; }
        }

        public Brush BodyColor
        {
            get { return bodyColor; }
            private set
            {
                if (bodyColor != value)
                {
                    bodyColor = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool IsTracked
        {
            get { return isTracked; }
            private set
            {
                if (isTracked != value)
                {
                    isTracked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool Detected
        {
            get { return detected; }
            private set
            {
                if (detected != value)
                {
                    detected = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public float Confidence
        {
            get { return confidence; }
            private set
            {
                if (Math.Abs(confidence - value) > float.Epsilon)
                {
                    confidence = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public void UpdateGestureResult(bool isBodyTrackingIdValid, bool isGestureDetected, float detectionConfidence)
        {
            IsTracked = isBodyTrackingIdValid;

            if (!IsTracked)
            {
                Detected = false;
                Confidence = 0.0f;
                BodyColor = Brushes.Silver;
                RaiseGestureStateIfChanged(false, 0.0f);
                return;
            }

            Detected = isGestureDetected;
            BodyColor = TrackedColors[BodyIndex];
            Confidence = isGestureDetected ? detectionConfidence : 0.0f;
            RaiseGestureStateIfChanged(isGestureDetected, Confidence);
        }

        private void RaiseGestureStateIfChanged(bool isGestureDetected, float detectionConfidence)
        {
            if (wasDetected == isGestureDetected)
            {
                return;
            }

            wasDetected = isGestureDetected;
            GestureStateChanged?.Invoke(
                this,
                new GestureStateChangedEventArgs(BodyIndex, IsTracked, isGestureDetected, detectionConfidence));
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
