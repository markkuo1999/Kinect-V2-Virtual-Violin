using Microsoft.Kinect;
using Microsoft.Kinect.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfKinectV2CustomButton.Models;
using WpfKinectV2CustomButton.Services;

namespace WpfKinectV2CustomButton
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly Dictionary<string, KinectV2CustomButton> buttonsById = new Dictionary<string, KinectV2CustomButton>();
        private readonly ButtonBrushFactory brushFactory = new ButtonBrushFactory();
        private readonly ImageSource gestureActiveImage = new BitmapImage(new Uri("Images/sound.png", UriKind.Relative));

        private MidiService midiService;
        private KinectTrackingService trackingService;
        private ViolinSessionController sessionController;
        private ImageSource imageSource;
        private string statusText;

        public MainWindow()
        {
            InitializeComponent();
            InitializeKinect();
            InitializeMidiAndSession();
            Loaded += OnLoaded;
            Closing += OnClosing;
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string StatusText
        {
            get { return statusText; }
            set
            {
                if (statusText != value)
                {
                    statusText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ImageSource ImageSource
        {
            get { return imageSource; }
            private set
            {
                if (imageSource != value)
                {
                    imageSource = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void InitializeKinect()
        {
            KinectRegion.SetKinectRegion(this, kinectRegion);
            kinectRegion.KinectSensor = KinectSensor.GetDefault();
            kinectRegion.KinectSensor.IsAvailableChanged += OnSensorAvailabilityChanged;

            StatusText = kinectRegion.KinectSensor.IsAvailable
                ? Properties.Resources.RunningStatusText
                : Properties.Resources.NoSensorStatusText;

            trackingService = new KinectTrackingService(kinectRegion.KinectSensor);
            trackingService.GestureStateChanged += OnGestureStateChanged;
        }

        private void InitializeMidiAndSession()
        {
            midiService = new MidiService();
            if (!midiService.TryOpen())
            {
                StatusText = midiService.StatusMessage;
            }

            sessionController = new ViolinSessionController(midiService);
            sessionController.GestureIndicatorChanged += (sender, isActive) => ImageSource = isActive ? gestureActiveImage : null;
            sessionController.ButtonVisualStateChanged += OnButtonVisualStateChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CreateNoteButtons();
            CreateInstrumentButtons();
        }

        private void CreateNoteButtons()
        {
            foreach (NoteButtonDefinition note in MusicCatalog.NoteButtons)
            {
                KinectV2CustomButton button = CreateButton(note.Id, note.Id, 30, new Thickness(50));
                KinectButtons.Children.Add(button);
            }
        }

        private void CreateInstrumentButtons()
        {
            foreach (InstrumentConfig instrument in MusicCatalog.Instruments)
            {
                KinectV2CustomButton button = CreateButton(instrument.Name, instrument.Name, 17, new Thickness(25));
                KinectButtons2.Children.Add(button);
            }
        }

        private KinectV2CustomButton CreateButton(string id, string content, int fontSize, Thickness margin)
        {
            var button = new KinectV2CustomButton
            {
                Name = id,
                Content = content,
                Margin = margin,
                Width = 60,
                Height = 60,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.White,
                BorderBrush = Brushes.Transparent,
                Background = brushFactory.DefaultBrush,
            };

            button.HandPointerEnter += OnButtonHandPointerEnter;
            button.HandPointerLeave += OnButtonHandPointerLeave;
            buttonsById[id] = button;
            return button;
        }

        private void OnButtonHandPointerEnter(object sender, EventArgs e)
        {
            var button = (KinectV2CustomButton)sender;
            sessionController.OnButtonEntered(button.Name, button);
        }

        private void OnButtonHandPointerLeave(object sender, EventArgs e)
        {
            var button = (KinectV2CustomButton)sender;
            sessionController.OnButtonLeft(button.Name, button);
        }

        private void OnGestureStateChanged(object sender, GestureStateChangedEventArgs e)
        {
            if (e.BodyIndex != 0)
            {
                return;
            }

            sessionController.OnGestureStateChanged(e.IsDetected);
        }

        private void OnButtonVisualStateChanged(object sender, ButtonVisualStateChangedEventArgs e)
        {
            KinectV2CustomButton button = e.ButtonElement as KinectV2CustomButton ?? FindButton(e.ButtonId);
            if (button == null)
            {
                return;
            }

            button.Background = e.IsActive ? brushFactory.ActiveBrush : brushFactory.DefaultBrush;
        }

        private KinectV2CustomButton FindButton(string buttonId)
        {
            KinectV2CustomButton button;
            return buttonsById.TryGetValue(buttonId, out button) ? button : null;
        }

        private void OnSensorAvailabilityChanged(object sender, IsAvailableChangedEventArgs e)
        {
            StatusText = kinectRegion.KinectSensor.IsAvailable
                ? Properties.Resources.RunningStatusText
                : Properties.Resources.SensorNotAvailableStatusText;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (trackingService != null)
            {
                trackingService.Dispose();
                trackingService = null;
            }

            if (midiService != null)
            {
                midiService.Dispose();
                midiService = null;
            }

            if (kinectRegion.KinectSensor != null)
            {
                kinectRegion.KinectSensor.IsAvailableChanged -= OnSensorAvailabilityChanged;
                kinectRegion.KinectSensor.Close();
                kinectRegion.KinectSensor = null;
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
