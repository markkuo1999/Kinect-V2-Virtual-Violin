using Microsoft.Kinect;
using Microsoft.Kinect.Wpf.Controls;
using System.Windows;
using RtMidi.Core;
using RtMidi.Core.Messages;
using RtMidi.Core.Devices;
using RtMidi.Core.Enums;
using Key = RtMidi.Core.Enums.Key;
using System.Linq;
using System.Windows.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Kinect.VisualGestureBuilder;
using System.Media;
using Serilog;
using RtMidi.Core.Unmanaged.Devices;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using System.IO;
using Newtonsoft.Json;

namespace WpfKinectV2CustomButton
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        RtMidi.Core.Devices.Infos.IMidiOutputDeviceInfo devInfo = MidiDeviceManager.Default.OutputDevices.ToList()[0];
        public static IMidiOutputDevice dev = null;
        /// <summary> Active Kinect sensor </summary>
        private KinectSensor kinectSensor = null;
        /// <summary> Reader for body frames </summary>
        private BodyFrameReader bodyFrameReader = null;
        /// <summary> Current status text to display </summary>
        private string statusText = null;
        /// <summary> List of gesture detectors, there will be one detector created for each potential body (max of 6) </summary>
        private List<GestureDetector> gestureDetectorList = null;
        /// <summary> Image to show when the 'detected' property is true for a tracked body </summary>
        private readonly ImageSource seatedImage = new BitmapImage(new Uri(@"Images\sound.png", UriKind.Relative));

        /// <summary> Image to show when the 'detected' property is false for a tracked body </summary>
        private readonly ImageSource notSeatedImage = new BitmapImage(new Uri(@"Images\cross.PNG", UriKind.Relative));

        private ImageSource imageSource = null;

        //private readonly ImageSource seatedImage = new BitmapImage(new Uri(@"Images\Seated.png", UriKind.Relative));
        /// <summary> Array for the bodies (Kinect will track up to 6 people simultaneously) </summary>
        public Body[] bodies = null;

        public static int a, chord;

        public static bool playing = false;

        public static string ButtonName;

        public static object send;

        public float oldhandleftposition = 0;

        public static bool continueplaying = false;

        public static bool buttonoff = false;
        
        public static string s;

        public static object Sender;

        bool test = false;

        static List<Body> trackedBodies = new List<Body>();

        public CameraSpacePoint newhandleftposition;
       
        /// <summary> KinectBodyView object which handles drawing the Kinect bodies to a View box in the UI </summary>
        private KinectBodyView kinectBodyView = null;



        public MainWindow()
        {
            
            InitializeComponent();
            KinectRegion.SetKinectRegion(this, kinectRegion);
            kinectRegion.KinectSensor = KinectSensor.GetDefault();
            // only one sensor is currently supported
            //kinectRegion.KinectSensor = KinectSensor.GetDefault();
            // set IsAvailableChanged event notifier
            kinectRegion.KinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            //kinectRegion.KinectSensor.Open();
            // set the status text
            this.StatusText = kinectRegion.KinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // open the reader for the body frames
            this.bodyFrameReader = kinectRegion.KinectSensor.BodyFrameSource.OpenReader();

            // set the BodyFramedArrived event notifier
            this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;
            // initialize the gesture detection objects for our gestures
            this.gestureDetectorList = new List<GestureDetector>();

            // initialize the BodyViewer object for displaying tracked bodies in the UI
            kinectBodyView = new KinectBodyView(kinectRegion.KinectSensor);

            // initialize the MainWindow
            //this.InitializeComponent();

            // set our data context objects for display in UI
            this.DataContext = this;
            //this.kinectBodyViewbox.DataContext = this.kinectBodyView;


            int col0Row = 0;
            int col1Row = 0;
            int maxBodies = kinectRegion.KinectSensor.BodyFrameSource.BodyCount;

            


            a = 40;
            chord = 0;
            /*GestureResultView result = new GestureResultView(0, false, false, 0.0f, false);
            
            GestureDetector detector = new GestureDetector(kinectRegion.KinectSensor, result);
            this.gestureDetectorList.Add(detector);

            // split gesture results across the first two columns of the content grid
            ContentControl contentControl = new ContentControl();
            contentControl.Content = this.gestureDetectorList[0].GestureResultView;*/

            for (int i = 0; i < maxBodies; ++i)
            {
                GestureResultView result = new GestureResultView(i, false, false, 0.0f, false);
                GestureDetector detector = new GestureDetector(kinectRegion.KinectSensor, result);
                this.gestureDetectorList.Add(detector);

                // split gesture results across the first two columns of the content grid
                ContentControl contentControl = new ContentControl();
                contentControl.Content = this.gestureDetectorList[i].GestureResultView;

                if (i % 2 == 0)
                {
                    // Gesture results for bodies: 0, 2, 4
                    Grid.SetColumn(contentControl, 0);
                    Grid.SetRow(contentControl, col0Row);
                    ++col0Row;
                }
                else
                {
                    // Gesture results for bodies: 1, 3, 5
                    Grid.SetColumn(contentControl, 1);
                    Grid.SetRow(contentControl, col1Row);
                    ++col1Row;
                }

                //this.contentGrid.Children.Add(contentControl);
            }

            //this.ImageSource = this.notSeatedImage;
            Loaded += MainWindow_Loaded;

            dev = devInfo.CreateDevice();
            dev.Open();         
        }

        
       
        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

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
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.FrameArrived -= this.Reader_BodyFrameArrived;
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.gestureDetectorList != null)
            {
                // The GestureDetector contains disposable members (VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader)
                foreach (GestureDetector detector in this.gestureDetectorList)
                {
                    detector.Dispose();
                }

                this.gestureDetectorList.Clear();
                this.gestureDetectorList = null;
            }

            if (kinectRegion.KinectSensor != null)
            {
                kinectRegion.KinectSensor.IsAvailableChanged -= this.Sensor_IsAvailableChanged;
                kinectRegion.KinectSensor.Close();
                kinectRegion.KinectSensor = null;
            }
        }

        /// <summary>
        /// Handles the event when the sensor becomes unavailable (e.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.StatusText = kinectRegion.KinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor and updates the associated gesture detector object for each body
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;
           
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        // creates an array of 6 bodies, which is the max number of bodies that Kinect can track simultaneously
                        this.bodies = new Body[bodyFrame.BodyCount];                       
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.

                   

                    bodyFrame.GetAndRefreshBodyData(this.bodies);                  
                    dataReceived = true;
                }

               
                //trackedBodies = bodies.Where(b => b.IsTracked == true).ToList();
                //string kinectBodyDataString = JsonConvert.SerializeObject(trackedBodies);

            }

            if (dataReceived)
            {
                // visualize the new body data
                this.kinectBodyView.UpdateBodyFrame(this.bodies);

                

                // we may have lost/acquired bodies, so update the corresponding gesture detectors
                if (this.bodies != null)
                {
                   
                    // loop through all bodies to see if any of the gesture detectors need to be updated
                    int maxBodies = kinectRegion.KinectSensor.BodyFrameSource.BodyCount;
                    for (int i = 0; i < maxBodies; ++i)
                    {
                        Body body = this.bodies[i];
                        ulong trackingId = body.TrackingId;

                        // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                        if (trackingId != this.gestureDetectorList[i].TrackingId)
                        {
                            this.gestureDetectorList[i].TrackingId = trackingId;

                            // if the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                            // if the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                            this.gestureDetectorList[i].IsPaused = trackingId == 0;
                        }
                    }


                    string s, k;
                    
                    /*
                    trackedBodies = bodies.Where(b => b.IsTracked == true).ToList();
                    trackedBodies.Add(this.bodies[0]);
                    string kinectBodyDataString = JsonConvert.SerializeObject(trackedBodies);
                    //s = (trackedBodies[0].Joints[JointType.HandRight].Position.X - oldhandleftposition).ToString();
                    //k = (oldhandleftposition).ToString();
                    //textBlock.Text = s;
                    //textBlock.FontSize = 30;

                    //textBlock2.Text = k;                        
                    //textBlock2.FontSize = 30;
                    //textBlock2.Visibility = 0;
                    if ((trackedBodies[0].Joints[JointType.HandRight].Position.X - oldhandleftposition > 0.00375 || trackedBodies[0].Joints[JointType.HandRight].Position.X - oldhandleftposition < -0.00375))
                    {
                        continueplaying = true;
                        if(test == false)
                        {
                            //if(((KinectV2CustomButton)sender).Name == "E0"){
                                dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                                dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key76 + chord, 127));
                            //}                           
                            test = true;
                        }
                        

                    }
                    else
                    {
                        continueplaying = false;
                        //if(((KinectV2CustomButton)sender).Name == "E0")
                        //{
                            dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key76 + chord, 127));
                        //}                            
                        test = false;

                    }
                    */
                    if (GestureResultView.detecthandmove == true /*&& (trackedBodies[0].Joints[JointType.HandRight].Position.X - oldhandleftposition > 0.00375 || trackedBodies[0].Joints[JointType.HandRight].Position.X - oldhandleftposition < -0.00375)*/)
                    {
                        this.ImageSource = seatedImage;                     
                    }
                    else
                    {
                        this.ImageSource = null;                                             
                    }
                    //oldhandleftposition = trackedBodies[0].Joints[JointType.HandRight].Position.X;
                    //trackedBodies.Clear();
                    GestureResultView.detect = false;               
                }
            }
        }

      

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var brush = new ImageBrush();
            var brush2 = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(@"C:\Users\markk\Desktop\my_kinect_violin\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\Images\violinicon.png", UriKind.Relative));
            brush2.ImageSource = new BitmapImage(new Uri(@"C:\Users\markk\Desktop\my_kinect_violin\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\Images\violinicon2.png", UriKind.Relative));

            for (int i = 0; i < 16; i++)
            {
                KinectV2CustomButton button1 = new KinectV2CustomButton();
                if (i <= 3)
                {
                    button1.Name = "G" + i;
                    button1.Content = "G" + i;                                
                }
                else if (i >= 4 && i <= 7)
                {
                    button1.Name = "D" + (i - 4);
                    button1.Content = "D" + (i - 4);
                }
                else if (i >= 8 && i <= 11)
                {
                    button1.Name = "A" + (i - 8);
                    button1.Content = "A" + (i - 8);
                }
                else
                {
                    button1.Name = "E" + (i - 12);
                    button1.Content = "E" + (i - 12);
                }
                button1.Margin = new Thickness(50);
                button1.Width = 60;
                button1.Height = 60;
                button1.FontSize = 30;
                button1.HandPointerEnter += Button1_HandPointerEnter;
                button1.HandPointerLeave += Button1_HandPointerLeave;
                button1.Background = brush;
                button1.FontWeight = FontWeights.Bold;
                button1.FontStyle = FontStyles.Italic;
                button1.Foreground = Brushes.White;
                button1.BorderBrush = Brushes.Transparent;
                //button1
                //button1.
                
                KinectButtons.Children.Add(button1);
            }
            for (int i = 0; i < 4; i++)
            {
                KinectV2CustomButton button2 = new KinectV2CustomButton();
                if (i == 0) 
                {
                    button2.Name = "Violin";
                    button2.Content = "Violin";
                    button2.Background = brush;
                }
                if (i == 1)
                {
                    button2.Name = "Ensem";
                    button2.Content = "Ensem";
                    button2.Background = brush;
                }
                if (i == 2)
                {
                    button2.Name = "Cello";
                    button2.Content = "Cello";
                    button2.Background = brush;
                }
                if (i == 3)
                {
                    button2.Name = "Bass";
                    button2.Content = "Bass";
                    button2.Background = brush;
                }
                button2.Margin = new Thickness(25);
                button2.Width = 60;
                button2.Height = 60;
                button2.FontSize = 17;
                button2.HandPointerEnter += Button1_HandPointerEnter;
                button2.HandPointerLeave += Button1_HandPointerLeave;
                button2.BorderBrush = Brushes.Transparent;
                button2.FontStyle = FontStyles.Italic;
                button2.Foreground = Brushes.White;
                button2.FontWeight = FontWeights.Bold;
                //button2.BorderBrush = Brushes.White;
                KinectButtons2.Children.Add(button2);
            }
                  
            

        }

        public void Button1_HandPointerEnter(object sender, System.EventArgs e)
        {
            txtMessage.Text = ((KinectV2CustomButton)sender).Name;           
            txtMessage.FontSize = 1;        
            buttonoff = true;
            var brush = new ImageBrush();
            var brush2 = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(@"C:\Users\markk\Desktop\my_kinect_violin\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\Images\violinicon.png", UriKind.Relative));
            brush2.ImageSource = new BitmapImage(new Uri(@"C:\Users\markk\Desktop\my_kinect_violin\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\Images\violinicon2.png", UriKind.Relative));
            //GestureResultView ob = new GestureResultView(0, false, false, 0, false);
            //ob.UpdateGestureResult.;          
            textBlock.Text = continueplaying.ToString();
            textBlock.FontSize = 1;
            if (playing == true)
            {
               
                if (txtMessage.Text == "E0" && GestureResultView.detect == true)
                {                    
                    //if ( == true)
                    ((KinectV2CustomButton)sender).Background = brush2;
                    //else
                    // ((KinectV2CustomButton)sender).Background = brush;

                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key76+chord, 127));
                    //this.ImageSource = this.seatedImage;
                    //playing = true;
                }
                else if (txtMessage.Text == "E1" && GestureResultView.detect == true)
                {
                    ((KinectV2CustomButton)sender).Background = brush2;
                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key78 + chord, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "E2" && GestureResultView.detect == true)
                {
                    ((KinectV2CustomButton)sender).Background = brush2;
                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key80+chord, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "E3" && GestureResultView.detect == true)
                {
                    ((KinectV2CustomButton)sender).Background = brush2;
                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key81 + chord, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "A0" && GestureResultView.detect == true)
                {
                    ((KinectV2CustomButton)sender).Background = brush2;
                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key69 + chord, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "A1" && GestureResultView.detect == true)
                {
                    ((KinectV2CustomButton)sender).Background = brush2;
                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key71 + chord, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "A2" && GestureResultView.detect == true)
                {
                    ((KinectV2CustomButton)sender).Background = brush2;
                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key72 + chord, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "A3" && GestureResultView.detect == true)
                {
                    ((KinectV2CustomButton)sender).Background = brush2;
                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key74 + chord, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "D0" && GestureResultView.detect == true)
                {
                    ((KinectV2CustomButton)sender).Background = brush2;
                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key62 + chord, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "D1" && GestureResultView.detect == true)
                {
                    ((KinectV2CustomButton)sender).Background = brush2;
                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key64 + chord, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "D2" && GestureResultView.detect == true)
                {
                    ((KinectV2CustomButton)sender).Background = brush2;
                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key66 + chord, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "D3" && GestureResultView.detect == true)
                {
                    ((KinectV2CustomButton)sender).Background = brush2;
                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key67 + chord, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "G0" && GestureResultView.detect == true)
                {
                    ((KinectV2CustomButton)sender).Background = brush2;
                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key55 + chord, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "G1" && GestureResultView.detect == true)
                {
                    ((KinectV2CustomButton)sender).Background = brush2;
                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key57 + chord, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "G2" && GestureResultView.detect == true)
                {
                    ((KinectV2CustomButton)sender).Background = brush2;
                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key59 + chord, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "G3" && GestureResultView.detect == true)
                {
                    ((KinectV2CustomButton)sender).Background = brush2;
                    dev.Send(new ProgramChangeMessage(Channel.Channel1, a));
                    dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key60 + chord, 127));
                    //playing = true;
                }
                if (txtMessage.Text == "Ensem")
                {
                    //KinectButtons2
                    ((KinectV2CustomButton)sender).Background = brush2;
                    //buttonoff = true;
                    a = 49;
                    chord = 0;
                    //dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key60, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "Violin")
                {
                    
                    ((KinectV2CustomButton)sender).Background = brush2;                     
                    a = 40;
                    chord = 0;
                    //dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key60, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "Cello")
                {

                    ((KinectV2CustomButton)sender).Background = brush2;
                    a = 42;
                    chord = -20;
                    //dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key60, 127));
                    //playing = true;
                }
                else if (txtMessage.Text == "Bass")
                {

                    ((KinectV2CustomButton)sender).Background = brush2;
                    a = 43;
                    chord = -32;
                    //dev.Send(new NoteOnMessage(Channel.Channel1, Key.Key60, 127));
                    //playing = true;
                }              
            }
            
            ButtonName = txtMessage.Text;
            send = sender;
        }

        public void Button1_HandPointerLeave(object sender, System.EventArgs e)
        {
            var brush = new ImageBrush();
            var brush2 = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(@"C:\Users\markk\Desktop\my_kinect_violin\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\Images\violinicon.png", UriKind.Relative));
            brush2.ImageSource = new BitmapImage(new Uri(@"C:\Users\markk\Desktop\my_kinect_violin\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\WPFKinectV2CustomButton-master\Images\violinicon2.png", UriKind.Relative));
            //txtMessage.Text = ((KinectV2CustomButton)sender).Name;
            //txtMessage.Background = ((KinectV2CustomButton)sender).Background;
            if (txtMessage.Text == "E0")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key76 + chord, 127));
            }
            else if (txtMessage.Text == "E1")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key78 + chord, 127));
            }
            else if (txtMessage.Text == "E2")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key80+chord, 127));
            }
            else if (txtMessage.Text == "E3")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key81 + chord, 127));
            }
            else if (txtMessage.Text == "A0")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key69 + chord, 127));
            }
            else if (txtMessage.Text == "A1")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key71 + chord, 127));
            }
            else if (txtMessage.Text == "A2")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key72 + chord, 127));
            }
            else if (txtMessage.Text == "A3")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key74 + chord, 127));
            }
            else if (txtMessage.Text == "D0")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key62 + chord, 127));
            }
            else if (txtMessage.Text == "D1")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key64 + chord, 127));
            }
            else if (txtMessage.Text == "D2")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key66 + chord, 127));
            }
            else if (txtMessage.Text == "D3")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key67 + chord, 127));
            }
            else if (txtMessage.Text == "G0")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key55 + chord, 127));
            }
            else if (txtMessage.Text == "G1")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key57 + chord, 127));
            }
            else if (txtMessage.Text == "G2")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key59 + chord, 127));
            }
            else if (txtMessage.Text == "G3")
            {
                ((KinectV2CustomButton)sender).Background = brush;

                dev.Send(new NoteOffMessage(Channel.Channel1, Key.Key60 + chord, 127));
            }
            else if(buttonoff == true)
            {
                ((KinectV2CustomButton)sender).Background = brush;
            }
            playing = true;
            ButtonName = "NULL";
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
