using System;
using System.Collections.Generic;
using Microsoft.Kinect;
using WpfKinectV2CustomButton.Models;

namespace WpfKinectV2CustomButton.Services
{
    public sealed class KinectTrackingService : IDisposable
    {
        private readonly KinectSensor sensor;
        private readonly KinectBodyView bodyView;
        private readonly List<GestureDetector> gestureDetectors = new List<GestureDetector>();
        private BodyFrameReader bodyFrameReader;
        private Body[] bodies;

        public KinectTrackingService(KinectSensor sensor)
        {
            this.sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
            bodyView = new KinectBodyView(sensor);
            bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += OnBodyFrameArrived;

            int bodyCount = sensor.BodyFrameSource.BodyCount;
            for (int i = 0; i < bodyCount; i++)
            {
                var resultView = new GestureResultView(i);
                var detector = new GestureDetector(sensor, resultView);
                gestureDetectors.Add(detector);

                if (i == 0)
                {
                    resultView.GestureStateChanged += (sender, args) =>
                    {
                        GestureStateChanged?.Invoke(this, args);
                    };
                }
            }
        }

        public event EventHandler<GestureStateChangedEventArgs> GestureStateChanged;

        public KinectBodyView BodyView
        {
            get { return bodyView; }
        }

        public void Dispose()
        {
            if (bodyFrameReader != null)
            {
                bodyFrameReader.FrameArrived -= OnBodyFrameArrived;
                bodyFrameReader.Dispose();
                bodyFrameReader = null;
            }

            foreach (GestureDetector detector in gestureDetectors)
            {
                detector.Dispose();
            }

            gestureDetectors.Clear();
        }

        private void OnBodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (bodies == null)
                    {
                        bodies = new Body[bodyFrame.BodyCount];
                    }

                    bodyFrame.GetAndRefreshBodyData(bodies);
                    dataReceived = true;
                }
            }

            if (!dataReceived || bodies == null)
            {
                return;
            }

            bodyView.UpdateBodyFrame(bodies);

            int maxBodies = sensor.BodyFrameSource.BodyCount;
            for (int i = 0; i < maxBodies; i++)
            {
                ulong trackingId = bodies[i].TrackingId;
                if (trackingId != gestureDetectors[i].TrackingId)
                {
                    gestureDetectors[i].TrackingId = trackingId;
                    gestureDetectors[i].IsPaused = trackingId == 0;
                }
            }
        }
    }
}
