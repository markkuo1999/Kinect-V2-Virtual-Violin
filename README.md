# Kinect-V2-Virtual-Violin
With motion capture technique of Kinect V2, the device can detect human joint. I made an interface with WPF using C# to interact with Kinect V2. Kinect V2 traces user's left hand joint so as to choose different notes of violin, and by waving right arm, Kinect V2 can detect the movement like playing the bow of violin. Users can also choose different instrument like string ensemble, cello and double bass. Right arm waving detection is made by Kinect Gesture Builder, using recorded videos as training set to train specific gestures. Music sound are from Rtmidi library.

![alt text](https://i.ibb.co/NNr27Zc/image.jpg)

Fig. The interface of virtual violin

User can choose 16 different notes from left part, and instrument sound can be changed by pressing the button on up right hand side of the interface.

Note: Kinect V2 device and its SDK installation is required

Kinect V2 SDK download link:https://www.microsoft.com/en-us/download/details.aspx?id=44561

Reference Code:

https://github.com/dehariapankaj/WPFKinectV2CustomButton

Source code from Kinect V2 SDK
