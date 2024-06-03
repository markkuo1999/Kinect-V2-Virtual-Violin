# Kinect-V2-Virtual-Violin
Utilizing the motion capture capabilities of the Kinect V2, the device detects human joints. I developed a WPF interface using C# to interact with the Kinect V2. By tracking the user's left hand joint, the Kinect V2 allows selection of different violin notes, while waving the right arm simulates bow movements. Users can also choose from various instruments like string ensemble, cello, and double bass. The detection of right arm waving is achieved through the Kinect Gesture Builder, which utilizes recorded videos as a training set to recognize specific gestures. Music sounds are produced using the Rtmidi library.

![alt text](https://i.ibb.co/NNr27Zc/image.jpg)

Fig. The interface of virtual violin

User can choose 16 different notes from left part, and instrument sound can be changed by pressing the button on up right hand side of the interface.

Note: Kinect V2 device and its SDK installation is required

Kinect V2 SDK download link:https://www.microsoft.com/en-us/download/details.aspx?id=44561

Reference Code:

https://github.com/dehariapankaj/WPFKinectV2CustomButton

Source code from Kinect V2 SDK
