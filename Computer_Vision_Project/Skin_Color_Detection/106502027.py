import numpy as np
import cv2
from PIL import Image


cap = cv2.VideoCapture("Lab4.mp4")
while(cap.isOpened()):
    ret, frame = cap.read()
    if frame is None:
        break
    '''
    Your Code
    '''
    dst = cv2.GaussianBlur(frame, (3,3), 2)
    dst = cv2.cvtColor(frame,cv2.COLOR_BGR2YCR_CB)
    Y, Cr, Cb = cv2.split(dst)
    mask = (Cr >= 133) & (Cr <= 177) & (Cb >= 98) & (Cb <= 122)
    #cv2.imshow('test', dst)
    dst = cv2.erode(mask.astype(np.uint8), np.ones((9,9)).astype(np.uint8))
    dst = cv2.dilate(dst.astype(np.uint8), np.ones((9,9)).astype(np.uint8))
    #cv2.imshow("test", dst)
    contours, hierarchy = cv2.findContours(dst.astype(np.uint8),cv2.RETR_EXTERNAL,cv2.CHAIN_APPROX_SIMPLE)
    #cv2.imshow('test', dst)
    cv2.drawContours(frame,contours,-1,(0,255,0),1)
    
    
    frame2 = frame.copy()
    #print(mask)
    
    
    frame2[dst == False] = 0
    #cv2.imshow('test', frame)
    cv2.imshow('test', frame2)

    if cv2.waitKey(0) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()