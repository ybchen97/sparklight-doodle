import cv2
import glob
import os
import numpy as np

class_name = input("Enter the class name: ")

images = sorted(glob.glob(f'dataset/{class_name}/*.jpg'))
count = 0
folder_path = 'dataset/' + class_name + '_square'
for image in images:
    count += 1
    img = cv2.imread(image)[0:60, 0:60,:]
    if np.count_nonzero(255 - img) > 1000:
        img = cv2.resize(img, (80, 80), interpolation=cv2.INTER_LINEAR)
        if not os.path.exists(folder_path):
            os.makedirs(folder_path)
        cv2.imwrite(f'{folder_path}/{str(count)}.jpg', img)