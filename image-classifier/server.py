from http.server import HTTPServer, BaseHTTPRequestHandler
import json
import threading
import time
import cv2
import numpy as np
import os
from keras.models import load_model

global count
global model
global class_names
count = 0

# Load the model
model = load_model("model/keras_Model.h5", compile=False)

# Load the labels
class_names = open("model/labels.txt", "r").readlines()

class RequestHandler(BaseHTTPRequestHandler):
    def __init__(self, request, client_address, server) -> None:
        super().__init__(request, client_address, server)

    def do_POST(self):
        content_length = int(self.headers['Content-Length'])
        post_data = self.rfile.read(content_length)
        # print(str(post_data))
        post_data = json.loads(post_data.decode('utf-8'))
        string_coords = post_data['coords']
        image = self.process_image(string_coords)
        prediction = model.predict(image)
        index = np.argmax(prediction)
        class_name = class_names[index][2:-1]
        confidence_score = np.round(prediction[0][index] * 100)
        self.send_response(200)
        self.send_header('content-type', 'application/json')
        self.end_headers()
        self.wfile.write(str(json.dumps({"status": "ok", "class_name": class_name, "confidence_score": confidence_score})).encode())

    def process_image(self, string_coords):
        global count
        count += 1
        img = np.ones((800, 800, 3), np.uint8) * 255
        for coord in string_coords.split(','):
            string_pixels = coord.split(' ')
            x = int(string_pixels[0])
            y = int(string_pixels[1])
            cv2.circle(img, (5 + x * 10, 5 + y * 10), 15, (0, 0, 0), -1)
        img = cv2.resize(img, (224, 224), interpolation=cv2.INTER_AREA)
        folder_path = os.path.join("dataset", "test")
        if not os.path.exists(folder_path):
            os.makedirs(folder_path)
        file_name = os.path.join(folder_path, f"{count}.jpg")
        cv2.imwrite(file_name, img)
        img = np.asarray(img, dtype=np.float32).reshape(1, 224, 224, 3)
        return img

def run_server_thread():
    PORT = 4000
    server_address = ('localhost', PORT)
    server = HTTPServer(server_address, RequestHandler)
    print('Server running on port %s' % PORT)
    server.serve_forever()

def run_server():
    server_thread = threading.Thread(target=run_server_thread)
    server_thread.setDaemon(True)
    server_thread.start()

if __name__ == '__main__':
    run_server()
    while True:
        time.sleep(1)