#import library
import cv2
import numpy as np
from scipy.ndimage import convolve
import math
#read an RGB image to Gray level
image = cv2.imread('lenna.png', cv2.IMREAD_GRAYSCALE)
### create two masks, including Horizontal edges and Vertical edges of Sobel ###
#hint:np.array, dtype = np.float
hori = np.array([[-1,0,1],[-2,0,2],[-1,0,1]], dtype=np.float)
ver = np.array([[-1,-2,-1],[0,0,0],[1,2,1]], dtype=np.float)
### Calculate image_sobelx and image_sobely using masks ###
#hint:dst=cv.filter2D(src, ddepth, kernel)
#cv.filter2D: Convolves an image with the kernel
#when ddepth=-1, the output image will have the same depth as the source
image_sobelx=cv2.filter2D(image, -1, hori)
image_sobely=cv2.filter2D(image, -1, ver)
#Calculate an approximation of the gradient:　　　　　G=sqrt(Gx*Gx+Gy*Gy)
#image_sobelx, image_sobely -> dtype = np.float
#then, transform to np.uint8
Gx = np.array(image_sobelx, dtype=np.float)
Gy = np.array(image_sobely, dtype=np.float)
G= np.sqrt(Gx*Gx+Gy*Gy)
sobelGrad = G.astype(np.uint8)
#Although sometimes the following simpler equation:  G=|Gx|+|Gy|
#hint:using cv2.bitwise_or to combine image_sobelx and image_sobely
sobelCombined = cv2.bitwise_or(image_sobelx, image_sobely)

#show image #
cv2.imshow('image_gray', image)
cv2.imshow('image_sobelx', image_sobelx)
cv2.imshow('image_sobely',image_sobely)
cv2.imshow('sobelGrad',sobelGrad)
cv2.imshow('image_sobelCombined',sobelCombined)
print("equal: ",not(np.bitwise_xor(sobelGrad,sobelCombined).any()))
cv2.waitKey(0)
cv2.destroyAllWindows()