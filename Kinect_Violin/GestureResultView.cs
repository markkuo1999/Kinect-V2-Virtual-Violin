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
    using System.Windows.Media.Imaging;
    using System.Media;
    using System.Collections.Generic;
    using RtMidi.Core.Messages;
    using RtMidi.Core.Unmanaged.Devices;
    using RtMidi.Core.Devices;
    using RtMidi.Core.Enums;
    using System.Linq;
    using RtMidi.Core;
    using Key = RtMidi.Core.Enums.Key;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Wpf.Controls;
    using System.Windows;   
    //using System.Drawing; 
    using System.Collections.ObjectModel;
    using System.Windows.Controls;
    using Microsoft.Kinect.VisualGestureBuilder;   
    using Serilog;
   
    /// <summary>
    /// Stores discretediscrete gesture results for the GestureDetector.
    /// Properties are stored/updated for display in the UI.
    /// </summary>
    public sealed class GestureResultView : INotifyPropertyChanged
    {
        /// <summary> Image to show when the 'detected' property is true for a tracked body </summary>
        //private readonly ImageSource seatedImage = new BitmapImage(new Uri(@"C:\Users\markk\Desktop\my_kinect_violin\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\Images\Seated.png", UriKind.Relative));
        
        /// <summary> Image to show when the 'detected' property is false for a tracked body </summary>
        //private readonly ImageSource notSeatedImage = new BitmapImage(new Uri(@"C:\Users\markk\Desktop\my_kinect_violin\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\Images\NotSeated.png", UriKind.Relative));

        /// <summary> Image to show when the body associated with the GestureResultView object is not being tracked </summary>
        //private readonly ImageSource notTrackedImage = new BitmapImage(new Uri(@"C:\Users\markk\Desktop\my_kinect_violin\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\Images\NotTracked.png", UriKind.Relative));

        /// <summary> Array of brush colors to use for a tracked body; array position corresponds to the body colors used in the KinectBodyView class </summary>
        private readonly Brush[] trackedColors = new Brush[] { Brushes.Red, Brushes.Orange, Brushes.Green, Brushes.Blue, Brushes.Indigo, Brushes.Violet };

        /// <summary> Brush color to use as background in the UI </summary>
        private Brush bodyColor = Brushes.Silver;

        /// <summary> The body index (0-5) associated with the current gesture detector </summary>
        private int bodyIndex = 0;

        /// <summary> Current confidence value reported by the discrete gesture </summary>
        private float confidence = 0.0f;

        /// <summary> True, if the discrete gesture is curressntly being detected </summary>
        private bool detected = false;

        bool flag = false;

        public bool Detect = false;
        /// <summary> Image to display in UI which corresponds to tracking/detection state </summary>
        private ImageSource imageSource = null;

        /// <summary> True, if the body is currently being tracked </summary>
        private bool isTracked = false;

        //RtMidi.Core.Devices.Infos.IMidiOutputDeviceInfo devInfo2 = MidiDeviceManager.Default.OutputDevices.ToList()[1];
        //IMidiOutputDevice dev = null;

        /// <summary> Current status text to display </summary>
        private string statusText = null;

        // MainWindow ob = new MainWindow(false);
        //public bool Detect { get; set; }
        public static bool detecthandmove = false;

        public static bool detect = false;

        public string test;

        /// <summary>
        /// Initializes a new instance of the GestureResultView class and sets initial property values
        /// </summary>
        /// <param name="bodyIndex">Body Index associated with the current gesture detector</param>
        /// <param name="isTracked">True, if the body is currently tracked</param>
        /// <param name="detected">True, if the gesture is currently detected for the associated body</param>
        /// <param name="confidence">Confidence value for detection of the 'Seated' gesture</param>
        public GestureResultView(int bodyIndex, bool isTracked, bool detected, float confidence, bool flag)
        {           
            this.BodyIndex = bodyIndex;
            this.IsTracked = isTracked;
            this.Detected = detected;
            this.Confidence = confidence;
            //this.ImageSource = this.notSeatedImage;
            this.flag = flag;

            //Loaded += MainWindow_Loaded;
            //dev = devInfo2.CreateDevice();
            //dev.Open();
           
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> 
        /// Gets the body index associated with the current gesture detector result 
        /// </summary>
        public int BodyIndex
        {
            get
            {
                return this.bodyIndex;
            }

            private set
            {
                if (this.bodyIndex != value)
                {
                    this.bodyIndex = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets the body index associated with the current gesture detector result 
        /// </summary>
        public bool Flag
        {
            get
            {
                //MainWindow result = new MainWindow(this.Detect);
                return this.Detect;          
            }

            private set
            {
                if (this.Detect != value)
                {
                    this.Detect = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets the body color corresponding to the body index for the result
        /// </summary>
        public Brush BodyColor
        {
            get
            {
                return this.bodyColor;
            }

            private set
            {
                if (this.bodyColor != value)
                {
                    this.bodyColor = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets a value indicating whether or not the body associated with the gesture detector is currently being tracked 
        /// </summary>
        public bool IsTracked
        {
            get
            {
                return this.isTracked;
            }

            private set
            {
                if (this.IsTracked != value)
                {
                    this.isTracked = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets a value indicating whether or not the discrete gesture has been detected
        /// </summary>
        public bool Detected
        {
            get
            {
                return this.detected;
                
            }

            private set
            {
                if (this.detected != value)
                {
                    this.detected = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets a float value which indicates the detector's confidence that the gesture is occurring for the associated body 
        /// </summary>
        public float Confidence
        {
            get
            {
                return this.confidence;
            }

            private set
            {
                if (this.confidence != value)
                {
                    this.confidence = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets an image for display in the UI which represents the current gesture result for the associated body 
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;
            }

            private set
            {
                if (this.ImageSource != value)
                {
                    this.imageSource = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Updates the values associated with the discrete gesture detection result
        /// </summary>
        /// <param name="isBodyTrackingIdValid">True, if the body associated with the GestureResultView object is still being tracked</param>
        /// <param name="isGestureDetected">True, if the discrete gesture is currently detected for the associated body</param>
        /// <param name="detectionConfidence">Confidence value for detection of the discrete gesture</param>
        public void UpdateGestureResult(bool isBodyTrackingIdValid, bool isGestureDetected, float detectionConfidence)
        {
            this.IsTracked = isBodyTrackingIdValid;
            this.Confidence = 0.0f;
            var brush = new ImageBrush();
            var brush2 = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(@"C:\Users\markk\Desktop\my_kinect_violin\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\Images\violinicon.png", UriKind.Relative));
            brush2.ImageSource = new BitmapImage(new Uri(@"C:\Users\markk\Desktop\my_kinect_violin\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\Images\violinicon2.png", UriKind.Relative));
            //MainWindow ob = new MainWindow();
            if (!this.IsTracked)
            {
               
                //this.ImageSource = this.notTrackedImage;
                this.Detected = false;
                this.BodyColor = Brushes.Silver;
            }
            else
            {
                this.Detected = isGestureDetected;
                this.BodyColor = this.trackedColors[this.BodyIndex];              
                if (this.Detected)
                {
                    this.Confidence = detectionConfidence;
                    //((KinectV2CustomButton)MainWindow.send).Background = Brushes.Red;
                    //this.ImageSource = this.seatedImage;
                    detecthandmove = true;
                    detect = true;
                    if (this.flag == false)
                    {
                        
                        //this.ImageSource = this.seatedImage;
                        //if(MainWindow.playing == true)
                        //{
                        if (MainWindow.ButtonName == "E0" /*&& MainWindow.continueplaying == false*/)
                        {
                            ((KinectV2CustomButton)MainWindow.send).Background = brush;
                            MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key76 + MainWindow.chord, 127));
                        }


                        if (MainWindow.ButtonName == "E0")
                        {
                                //if ( == true)
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                //else
                                // ((KinectV2CustomButton)sender).Background = Brushes.Silver;

                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key76 + MainWindow.chord, 127));
                                
                        }
                            else if (MainWindow.ButtonName == "E1")
                            {
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key78 + MainWindow.chord, 127));
                            }
                            else if (MainWindow.ButtonName == "E2")
                            {
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key80 + MainWindow.chord, 127));
                            }
                            else if (MainWindow.ButtonName == "E3")
                            {
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key81 + MainWindow.chord, 127));
                            }
                            else if (MainWindow.ButtonName == "A0")
                            {
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key69 + MainWindow.chord, 127));
                            }
                            else if (MainWindow.ButtonName == "A1")
                            {
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key71 + MainWindow.chord, 127));
                            }
                            else if (MainWindow.ButtonName == "A2")
                            {
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key72 + MainWindow.chord, 127));
                            }
                            else if (MainWindow.ButtonName == "A3")
                            {
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key74 + MainWindow.chord, 127));
                            }
                            else if (MainWindow.ButtonName == "D0")
                            {
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key62 + MainWindow.chord, 127));
                            }
                            else if (MainWindow.ButtonName == "D1")
                            {
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key64 + MainWindow.chord, 127));
                            }
                            else if (MainWindow.ButtonName == "D2")
                            {
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key66 + MainWindow.chord, 127));
                            }
                            else if (MainWindow.ButtonName == "D3")
                            {
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key67 + MainWindow.chord, 127));
                            }
                            else if (MainWindow.ButtonName == "G0")
                            {
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key55 + MainWindow.chord, 127));
                            }
                            else if (MainWindow.ButtonName == "G1")
                            {
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key57 + MainWindow.chord, 127));
                            }
                            else if (MainWindow.ButtonName == "G2")
                            {
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key59 + MainWindow.chord, 127));
                            }
                            else if (MainWindow.ButtonName == "G3")
                            {
                                ((KinectV2CustomButton)MainWindow.send).Background = brush2;
                                MainWindow.dev.Send(new ProgramChangeMessage(Channel.Channel1, MainWindow.a));
                                MainWindow.dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key60 + MainWindow.chord, 127));

                            }                                                         
                    this.flag = true;                        
                    }
                }
                else
                {
                    detecthandmove = false;
                    detect = false;
                    //MainWindow.detect = false;
                    //this.ImageSource = this.notSeatedImage;
                    if (MainWindow.ButtonName == "E0")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key76 + MainWindow.chord, 127));
                    }
                    else if (MainWindow.ButtonName == "E1")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key78 + MainWindow.chord, 127));
                    }
                    else if (MainWindow.ButtonName == "E2")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key80+MainWindow.chord, 127));
                    }
                    else if (MainWindow.ButtonName == "E3")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key81 + MainWindow.chord, 127));
                    }
                    else if (MainWindow.ButtonName == "A0")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key69 + MainWindow.chord, 127));
                    }
                    else if (MainWindow.ButtonName == "A1")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key71 + MainWindow.chord, 127));
                    }
                    else if (MainWindow.ButtonName == "A2")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key72 + MainWindow.chord, 127));
                    }
                    else if (MainWindow.ButtonName == "A3")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key74 + MainWindow.chord, 127));
                    }
                    else if (MainWindow.ButtonName == "D0")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key62 + MainWindow.chord, 127));
                    }
                    else if (MainWindow.ButtonName == "D1")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key64 + MainWindow.chord, 127));
                    }
                    else if (MainWindow.ButtonName == "D2")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key66 + MainWindow.chord, 127));
                    }
                    else if (MainWindow.ButtonName == "D3")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key67 + MainWindow.chord, 127));
                    }
                    else if (MainWindow.ButtonName == "G0")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key55 + MainWindow.chord, 127));
                    }
                    else if (MainWindow.ButtonName == "G1")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key57 + MainWindow.chord, 127));
                    }
                    else if (MainWindow.ButtonName == "G2")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key59 + MainWindow.chord, 127));
                    }
                    else if (MainWindow.ButtonName == "G3")
                    {
                        ((KinectV2CustomButton)MainWindow.send).Background = brush;

                        MainWindow.dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key60 + MainWindow.chord, 127));
                    }
                    //MainWindow.playing = false;
                    //ob.Detect = Detect;
                    //dev.Send(new NoteOffMessage(Channel.Channel13, Key.Key69, 127));
                    
                    //MainWindow result = new MainWindow(Detect);
                    this.flag = false;          
                }
            }
        }

       

        /// <summary>
        /// Notifies UI that a property has changed
        /// </summary>
        /// <param name="propertyName">Name of property that has changed</param> 
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
