# This file helps to generate the dataset for the image classifier.
# At the beginning, it will ask for the name of the class.
# Then, it will pop up a cv2 or matplotlib completely white window to create the image from user input.
# The window should have a size of 80x60. When the user presses the 's' key, it takes whatever the user draws on the window and saves it as a .jpg file inside the
# "dataset/{class_name}" folder. User's drawing should have a stroke of 3 pixels.
# After saving the image, the window should not close, and the user should be able to draw another image.

import os
import cv2
import numpy as np

def draw(event, x, y, flags, param):
    global drawing
    if event == cv2.EVENT_LBUTTONDOWN:
        drawing = True
    elif event == cv2.EVENT_MOUSEMOVE:
        if drawing:
            cv2.circle(img, (x, y), 15, (0, 0, 0), -1)
    elif event == cv2.EVENT_LBUTTONUP:
        drawing = False
        cv2.circle(img, (x, y), 15, (0, 0, 0), -1)

def save_image(class_name, img, count):
    img = cv2.resize(img, (80, 80), interpolation=cv2.INTER_AREA)
    folder_path = os.path.join("dataset", class_name)
    if not os.path.exists(folder_path):
        os.makedirs(folder_path)
    file_name = os.path.join(folder_path, f"{count}.jpg")
    cv2.imwrite(file_name, img)

# Ask for class name
class_name = input("Enter the class name: ")

# Initialize drawing window
drawing = False
img = np.ones((800, 800, 3), np.uint8) * 255
image_count = 1  # Counter for the image index

cv2.namedWindow("Image")
cv2.setMouseCallback("Image", draw)

while True:
    cv2.imshow("Image", img)
    window_title = f"Image - Drawing {image_count}"
    cv2.setWindowTitle("Image", window_title)
    key = cv2.waitKey(1)

    if key == ord('s'):
        save_image(class_name, img, image_count)
        img = np.ones((800, 800, 3), np.uint8) * 255  # Reset canvas
        image_count += 1  # Increment the image count

    if key == ord('z'):
        img = np.ones((800, 800, 3), np.uint8) * 255  # Reset canvas

    if key == 27:  # ESC key to exit
        break

cv2.destroyAllWindows()

