# -*- coding: utf-8 -*-
"""
Created on Thu Oct  8 18:34:28 2020

@author: markk
"""
import cv2
import numpy as np

image  = cv2.imread('C:/Users/markk/Downloads/1008/1008/detention_1.jpg', cv2.IMREAD_GRAYSCALE)

transfer_img = image.copy()

row = transfer_img.shape[0]
cols  = transfer_img.shape[1]

cols = int(cols)
row = int(row)
c = 255 / np.max(np.log10(1.0 + image))
for i in range(row):
    for j in range(cols): 
        if j <  int((cols/2)):
            transfer_img[i,j] = 255 - image[i,j]
            transfer_img[i,j]  = transfer_img[i,j].astype(np.uint8)
        if j >= int((cols/2)):
            transfer_img[i,j] = c * np.log10(1.0 + image[i,j])
            transfer_img[i,j]  = transfer_img[i,j].astype(np.uint8)
       
cv2.imshow("Output" , transfer_img)
cv2.imwrite('C:/Users/markk/Downloads/1008/1008/106502027_KuoZongYing.png' , transfer_img)
cv2.waitKey(0)
cv2.destroyAllWindows()
    