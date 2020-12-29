import numpy as np
import cv2
import matplotlib.pyplot as plt
import numpy
global img

def opencv_main():
    hist, _ = np.histogram(img.flatten(), 256, [0, 256])
    save_histPlot(img,hist, fn="hist")
    #
    normed_img = cv2.equalizeHist(img)
    cv2.imshow("Histogram Equalization", normed_img)
    #
    hist2, _ = np.histogram(normed_img.flatten(), 256, [0, 256])
    save_histPlot(normed_img,hist2, fn="hist2")
    cv2.waitKey(0)
def save_histPlot(img,hist,fn = "hist"):
    plt.clf()
    cdf =  hist.cumsum()
    cdf_normalized = float(hist.max()) * (cdf / cdf.max())  # sacle hist.max * normalized_cdf
    plt.plot(cdf_normalized, color='b')
    plt.hist(img.flatten(), 256, [0, 256], color='r')
    plt.xlim([0, 256])
    plt.legend(('cdf', 'histogram'), loc='upper left')
    plt.savefig('{}.png'.format(fn))
###########################################################################################################
# please complete this function
###########################################################################################################
def cal_hist(img):
    #!git pixel count |  tip: use np.unique
    counts = np.zeros(256)
    pixValue = np.unique(img)
    rows = len(img)
    columns = len(img[0])
    #print(pixValue)
    for i in range(rows):
        for j in range(columns):
            counts[img[i][j]] += 1
                  
    # convert to dict
    countDict = dict(zip(pixValue.tolist(), counts.tolist()))
    #!fill count zero as none exist pixel value, do on countDict. countDict need to contain keys include [0,255]
    
    


    # hist sort by key(pixel value) and convert to np.array
    hist = list(dict(sorted(countDict.items())).values())
    return np.array(hist)


#######################
# main
#######################
def code_main():
    ###################
    # As : hist, bins = np.histogram(img.flatten(), 256, [0, 256])
    ###################
    hist=cal_hist(img)
    ###################
    # use hist(Pdf) -> cdf ,do Histograms Equalization
    ###################
    save_histPlot(img,hist, fn="hist") # plot hist

    cdf = hist.cumsum()
    cdf_m = np.ma.masked_equal(cdf,0)
    ###########################################################################################################
    #!complete this line!, ref formula in ptt
    #!cdf_m contain none zero value in cdf, tip: use .min() .max()  on cdf_m
    cdf_m = ((cdf_m - cdf_m.min())/(cdf_m.max() - cdf_m.min())) * 255
    ###########################################################################################################
    cdf = np.ma.filled(cdf_m,0).astype('uint8')
    # now ,cdf as transform table
    normed_img = cdf[img]
    #####################
    # Display
    #####################
    hist2, _ = np.histogram(normed_img.flatten(), 256, [0, 256])
    save_histPlot(normed_img,hist2, fn="hist2")
    #
    cv2.imshow("Histogram Equalization",normed_img)

    cv2.waitKey(0)


if __name__ == '__main__':
    img=cv2.imread("./test.jpg",0)
    cv2.imshow("source", img)

    # opencv_main() # thie line is the opencv way, just for ref
    code_main()