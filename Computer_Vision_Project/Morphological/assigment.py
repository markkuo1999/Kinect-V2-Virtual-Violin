import numpy as np
import cv2

def img_equal(src1, src2):
    return not (np.bitwise_xor(src1, src2).any())
def dilate_(src, kernel, iterations=1):
    # IndexList
    colIndexList = np.array([[-1, -1, -1], [0, 0, 0], [1, 1, 1]])
    rowIndexList = np.array([[-1, 0, 1], [-1, 0, 1], [-1, 0, 1]])
    #
    img = src.copy()
    for iters in range(iterations):
        ###################################################################################################complete zone
        img = np.pad(img, (1, 1), "constant")  # padding ,pad 0
        dilation_output = np.zeros(img.shape, dtype=np.uint8)  # Blank space
        for center_col in range(1, img.shape[0] - 1):
            for center_row in range(1, img.shape[1] - 1):
                #! Get mul : mul = patch * kernel (elementwise-mutiply)
                mul = (img[colIndexList+center_col, rowIndexList+center_row])*(kernel)
                #print(mul)
                #! Condition : if mul have any 255,than center = 255
                if (mul==255).any():
                    dilation_output[center_col][center_row] = 255
        img = dilation_output[1:-1, 1:-1]
        #--------------------------------------------------------------------------------------------------complete zone
    return img


def erode_(src, kernel, iterations=1):
    # IndexList
    colIndexList = np.array([[-1, -1, -1], [0, 0, 0], [1, 1, 1]])
    rowIndexList = np.array([[-1, 0, 1], [-1, 0, 1], [-1, 0, 1]])
    #
    img = src.copy()
    for iters in range(iterations):
        ###################################################################################################complete zone
        img = np.pad(img, (1, 1), "constant")  # padding,pad 0
        erosion_output = np.zeros(img.shape, dtype=np.uint8)  # Blank space
        for center_col in range(1, img.shape[0] - 1):
            for center_row in range(1, img.shape[1] - 1):
                #! Get mul : mul = patch * kernel (elementwise-mutiply)
                mul = (img[colIndexList+center_col, rowIndexList+center_row])*(kernel)
                #print(mul)
                #! Condition : if all elemnet mul == kernel * 255,than center = 255
                if (mul==kernel*255).all():
                    erosion_output[center_col][center_row] = 255
        img = erosion_output[1:-1, 1:-1]
        #--------------------------------------------------------------------------------------------------complete zone
    return img

def open_(src, kernel, iterations=1):
    ###################################################################################################complete zone
    erosion = erode_(src, kernel, iterations=2)
    dilation = dilate_(erosion, kernel, iterations=2)
    
    return dilation
    #--------------------------------------------------------------------------------------------------complete zone


def close_(src, kernel, iterations=1):
    ###################################################################################################complete zone
    dilation = dilate_(src, kernel, iterations=2)
    erosion = erode_(dilation, kernel, iterations=2)
    
    return erosion
    #--------------------------------------------------------------------------------------------------complete zone

if __name__ == '__main__':
    ## Read Image
    source_img = cv2.imread("j.png", 0)
    open_img = cv2.imread("opening_j.png", 0)
    close_img = cv2.imread("closing_j.png", 0)

    ##　Set Kernel
    kernel = np.array([
        [0, 1, 0],
        [1, 1, 1],
        [0, 1, 0]
    ], dtype=np.uint8)

    ## Your Version
    dilation = dilate_(source_img, kernel, iterations=1)
    erosion = erode_(source_img, kernel, iterations=1)
    opening = open_(open_img, kernel, iterations=2)
    closing = close_(close_img, kernel, iterations=2)


    ## Opencv Version
    dilation_ans =  cv2.dilate(source_img, kernel, iterations=1)
    erosion_ans = cv2.erode(source_img, kernel, iterations=1)
    opening_ans = cv2.morphologyEx(open_img, cv2.MORPH_OPEN, kernel, iterations=2)
    closing_ans = cv2.morphologyEx(close_img, cv2.MORPH_CLOSE, kernel, iterations=2)

    ## Display
    cv2.imshow("dilate_answer", dilation)
    cv2.imshow("erode_answer", erosion)
    cv2.imshow("opening_answer", opening)
    cv2.imshow("closing_answer", closing)
    #
    cv2.imshow("answer_dilation", dilation_ans)
    cv2.imshow("answer_erosion", erosion_ans)
    cv2.imshow("answer_opening", opening_ans)
    cv2.imshow("answer_closing", closing_ans)
    ## Check
    print("dilate Check:\t",img_equal(dilation, dilation_ans))
    print("erosion Check:\t",img_equal(erosion, erosion_ans))
    print("opening Check:\t",img_equal(opening, opening_ans))
    print("closing Check:\t",img_equal(closing, closing_ans))

    cv2.waitKey(0)
