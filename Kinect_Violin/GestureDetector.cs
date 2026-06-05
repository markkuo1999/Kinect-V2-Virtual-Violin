//------------------------------------------------------------------------------
// <copyright file="GestureDetector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace WpfKinectV2CustomButton
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Kinect;
    using Microsoft.Kinect.VisualGestureBuilder;

    /// <summary>
    /// Listens for VisualGestureBuilderFrame events and updates the associated result view.
    /// </summary>
    public class GestureDetector : IDisposable
    {
        private readonly string gestureDatabase = @"Database\PlayString.gba";
        private readonly string gestureName = "PlayString";
        private VisualGestureBuilderFrameSource vgbFrameSource;
        private VisualGestureBuilderFrameReader vgbFrameReader;

        public GestureDetector(KinectSensor kinectSensor, GestureResultView gestureResultView)
        {
            if (kinectSensor == null)
            {
                throw new ArgumentNullException(nameof(kinectSensor));
            }

            if (gestureResultView == null)
            {
                throw new ArgumentNullException(nameof(gestureResultView));
            }

            GestureResultView = gestureResultView;
            vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);
            vgbFrameSource.TrackingIdLost += OnTrackingIdLost;
            vgbFrameReader = vgbFrameSource.OpenReader();

            if (vgbFrameReader != null)
            {
                vgbFrameReader.IsPaused = true;
                vgbFrameReader.FrameArrived += OnGestureFrameArrived;
            }

            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(gestureDatabase))
            {
                foreach (Gesture gesture in database.AvailableGestures)
                {
                    if (gesture.Name.Equals(gestureName))
                    {
                        vgbFrameSource.AddGesture(gesture);
                    }
                }
            }
        }

        public GestureResultView GestureResultView { get; private set; }

        public ulong TrackingId
        {
            get { return vgbFrameSource.TrackingId; }
            set
            {
                if (vgbFrameSource.TrackingId != value)
                {
                    vgbFrameSource.TrackingId = value;
                }
            }
        }

        public bool IsPaused
        {
            get { return vgbFrameReader.IsPaused; }
            set
            {
                if (vgbFrameReader.IsPaused != value)
                {
                    vgbFrameReader.IsPaused = value;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (vgbFrameReader != null)
            {
                vgbFrameReader.FrameArrived -= OnGestureFrameArrived;
                vgbFrameReader.Dispose();
                vgbFrameReader = null;
            }

            if (vgbFrameSource != null)
            {
                vgbFrameSource.TrackingIdLost -= OnTrackingIdLost;
                vgbFrameSource.Dispose();
                vgbFrameSource = null;
            }
        }

        private void OnGestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            using (VisualGestureBuilderFrame frame = e.FrameReference.AcquireFrame())
            {
                if (frame == null)
                {
                    return;
                }

                IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;
                if (discreteResults == null)
                {
                    return;
                }

                foreach (Gesture gesture in vgbFrameSource.Gestures)
                {
                    if (!gesture.Name.Equals(gestureName) || gesture.GestureType != GestureType.Discrete)
                    {
                        continue;
                    }

                    if (discreteResults.TryGetValue(gesture, out DiscreteGestureResult result) && result != null)
                    {
                        GestureResultView.UpdateGestureResult(true, result.Detected, result.Confidence);
                    }
                }
            }
        }

        private void OnTrackingIdLost(object sender, TrackingIdLostEventArgs e)
        {
            GestureResultView.UpdateGestureResult(false, false, 0.0f);
        }
    }
}
