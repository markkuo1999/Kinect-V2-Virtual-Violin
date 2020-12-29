### imoprt library ###
import numpy as np
import cv2

### Capture from camera or Read an video ###
cap = cv2.VideoCapture('C:/Users/markk/Downloads/109_Lab1/Assignment/minion_video.avi')
### Display the frame ###
while(cap.isOpened()):
    ret, frame = cap.read()
    cv2.imwrite('C:/Users/markk/Downloads/109_Lab1/test.png', frame)
    img = cv2.imread('C:/Users/markk/Downloads/109_Lab1/test.png')
    row, cols, channels = img.shape
    b, g, r = cv2.split(img)
    red = 1*r
    green = 1*g
    blue = 1*b
    gray = 0.114 * b + 0.587 * g + 0.299 * r
    gray = gray.astype(np.uint8)
    red = red.astype(np.uint8)
    green = green.astype(np.uint8)
    blue = blue.astype(np.uint8)
    ### Do the processing (convert RGB to grayscale)###
    #gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    cv2.imshow('106502027_KuoZongYing.png', gray)
    z = np.zeros([row ,cols], dtype=np.uint8)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break
    
    if cv2.waitKey(1) & 0xFF == ord('r'):
        #cv2.imwrite('C:/Users/markk/Downloads/109_Lab1/test1.png', frame)
        img = cv2.imread('C:/Users/markk/Downloads/109_Lab1/test1.png')
        cv2.imshow('106502027_KuoZongYing_Capture_r.png', cv2.merge([z, z, r]))
        cv2.imwrite('C:/Users/markk/Downloads/109_Lab1/106502027_KuoZongYing_Capture_r.png', cv2.merge([z, z, r]))
    if cv2.waitKey(1) & 0xFF == ord('g'):
        #cv2.imwrite('C:/Users/markk/Downloads/109_Lab1/test2.png', frame)
        img = cv2.imread('C:/Users/markk/Downloads/109_Lab1/test1.png')
        cv2.imshow('106502027_KuoZongYing_Captubre_g.png',cv2.merge([z, g, z]))
        cv2.imwrite('C:/Users/markk/Downloads/109_Lab1/106502027_KuoZongYing_Capture_g.png', cv2.merge([z, g, z]))
    if cv2.waitKey(1) & 0xFF == ord('b'):
        #cv2.imwrite('C:/Users/markk/Downloads/109_Lab1/test3.png', frame)
        img = cv2.imread('C:/Users/markk/Downloads/109_Lab1/test1.png')
        cv2.imshow('106502027_KuoZongYing_Capture_b.png',cv2.merge([b, z, z]))
        cv2.imwrite('C:/Users/markk/Downloads/109_Lab1/106502027_KuoZongYing_Capture_b.png', cv2.merge([b, z, z]))
    
    

### Close and Exit ###
cap.release()
cv2.destroyAllWindows()


