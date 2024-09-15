import cv2
import depthai as dai
import open3d as o3d
import numpy as np
from flask import Flask, jsonify, send_file, request
from flask_cors import CORS
import threading
import logging
import time
import sys
import io
import json
from collections import deque


logging.basicConfig(level=logging.DEBUG, format='%(asctime)s - %(levelname)s - %(message)s', stream=sys.stdout)





app = Flask(__name__)
CORS(app)

# spot control

from spot_controller import SpotController
ROBOT_IP = "192.168.80.3"#os.environ['ROBOT_IP']
SPOT_USERNAME = "admin"#os.environ['SPOT_USERNAME']
SPOT_PASSWORD = "2zqa8dgw7lor"#os.environ['SPOT_PASSWORD']



spot = SpotController(username=SPOT_USERNAME, password=SPOT_PASSWORD, robot_ip=ROBOT_IP)

def process_command(command, args=None):
    global spot
    print("command: ", command)
    if args is None:
        args = {}
    if command == 'start':
        spot.lease_control()
        spot.release_estop()
        spot.power_on_stand_up()

        time.sleep(2)
        # spot.move_head_in_points(yaws=[0.2, 0],
        #                     pitches=[0.3, 0],
        #                     rolls=[0.4, 0],
        #                     sleep_after_point_reached=1)

        # spot.move_head_in_points(yaws=[0.2, 0],
        #                     pitches=[0.3, 0],
        #                     rolls=[0.4, 0],
        #                     sleep_after_point_reached=1)

    elif command == 'velocity':

        v_x = args.get('v_x', 0)
        v_y = args.get('v_y', 0)
        v_rot = args.get('v_rot', 0)
        cmd_duration = args.get('cmd_duration', 1)
        spot.move_by_velocity_control(v_x=v_x, v_y=v_y, v_rot=v_rot, cmd_duration=cmd_duration)
        time.sleep(cmd_duration)
    elif command == 'goal':
        goal_x = args['goal_x']
        goal_y = args['goal_y']
        spot.move_to_goal(goal_x=goal_x, goal_y=goal_y)
        
    elif command == 'stop':
        spot.power_off_sit_down()
        spot.return_lease()
        spot.set_estop()

@app.route('/command', methods=['POST'])
def command():
    print("ENTER COMMAND POST")
    try:
        print(request)
        content = request.json
        if not content or 'command' not in content:
            return jsonify({'error': 'Invalid request, missing command'}), 400
        
        process_command(content['command'], content.get('args'))
        return jsonify({'message': 'execution started'}), 200
    except Exception as e:
        logging.error(f"Error processing command: {e}")
        return jsonify({'error': 'Internal Server Error'}), 500



# Global variables to store the latest point cloud data
latest_points = None
latest_colors = None
latest_frame = None
data_lock = threading.Lock()

# Configuration
MAX_DISTANCE = 5  # Maximum distance in meters
FPS_TARGET = 10  # Target frames per second
MAX_POINTS = 10000  # Maximum number of points to keep



def create_pipeline():
    pipeline = dai.Pipeline()

    # Define sources and outputs
    monoLeft = pipeline.create(dai.node.MonoCamera)
    monoRight = pipeline.create(dai.node.MonoCamera)
    stereo = pipeline.create(dai.node.StereoDepth)
    colorCam = pipeline.create(dai.node.ColorCamera)

    xoutDepth = pipeline.create(dai.node.XLinkOut)
    xoutColor = pipeline.create(dai.node.XLinkOut)

    xoutDepth.setStreamName("depth")
    xoutColor.setStreamName("color")

    # Properties
    monoLeft.setResolution(dai.MonoCameraProperties.SensorResolution.THE_400_P)
    monoLeft.setBoardSocket(dai.CameraBoardSocket.CAM_B)
    monoRight.setResolution(dai.MonoCameraProperties.SensorResolution.THE_400_P)
    monoRight.setBoardSocket(dai.CameraBoardSocket.CAM_C)

    colorCam.setResolution(dai.ColorCameraProperties.SensorResolution.THE_1080_P)
    colorCam.setInterleaved(False)
    colorCam.setColorOrder(dai.ColorCameraProperties.ColorOrder.RGB)
    colorCam.setFps(FPS_TARGET)
    colorCam.setBoardSocket(dai.CameraBoardSocket.CAM_A)

    stereo.setDefaultProfilePreset(dai.node.StereoDepth.PresetMode.HIGH_DENSITY)
    stereo.setDepthAlign(dai.CameraBoardSocket.CAM_A)

    # Linking
    monoLeft.out.link(stereo.left)
    monoRight.out.link(stereo.right)
    stereo.depth.link(xoutDepth.input)
    colorCam.video.link(xoutColor.input)

    return pipeline

class FrameSync:
    def __init__(self, max_queue_size=4):
        self.max_queue_size = max_queue_size
        self.queues = {'depth': deque(maxlen=max_queue_size), 'color': deque(maxlen=max_queue_size)}

    def add_frame(self, name, frame):
        self.queues[name].append(frame)
        return self.get_synced_pair()

    def get_synced_pair(self):
        if len(self.queues['depth']) > 0 and len(self.queues['color']) > 0:
            return self.queues['depth'].popleft(), self.queues['color'].popleft()
        return None, None

class PointCloudVisualizer:
    def __init__(self, intrinsic_matrix, width, height):
        self.pinhole_camera_intrinsic = o3d.camera.PinholeCameraIntrinsic(width,
                                                                         height,
                                                                         intrinsic_matrix[0][0],
                                                                         intrinsic_matrix[1][1],
                                                                         intrinsic_matrix[0][2],
                                                                         intrinsic_matrix[1][2])

    def rgbd_to_projection(self, depth_map, rgb, max_distance):
        rgb_o3d = o3d.geometry.Image(rgb)
        depth_o3d = o3d.geometry.Image(depth_map)

        rgbd_image = o3d.geometry.RGBDImage.create_from_color_and_depth(
            rgb_o3d, depth_o3d, convert_rgb_to_intensity=False, depth_trunc=max_distance, depth_scale=1000.0
        )

        pcd = o3d.geometry.PointCloud.create_from_rgbd_image(rgbd_image, self.pinhole_camera_intrinsic)

        # Filter points based on the max distance
        points = np.asarray(pcd.points)
        colors = np.asarray(pcd.colors)
        distances = np.linalg.norm(points, axis=1)
        valid_indices = distances <= max_distance

        filtered_points = points[valid_indices]
        filtered_colors = colors[valid_indices]

        # Downsample if we have too many points
        if len(filtered_points) > MAX_POINTS:
            indices = np.random.choice(len(filtered_points), MAX_POINTS, replace=False)
            filtered_points = filtered_points[indices]
            filtered_colors = filtered_colors[indices]

        return filtered_points, filtered_colors

def run_pipeline():
    global latest_points, latest_colors, latest_frame

    try:
        pipeline = create_pipeline()
        logging.info("Pipeline created successfully.")

        with dai.Device(pipeline) as device:
            logging.info(f"Connected to device: {device.getDeviceName()}")

            depthQueue = device.getOutputQueue(name="depth", maxSize=4, blocking=False)
            colorQueue = device.getOutputQueue(name="color", maxSize=4, blocking=False)

            calibData = device.readCalibration()
            intrinsics = calibData.getCameraIntrinsics(dai.CameraBoardSocket.CAM_A)
            w, h = int(intrinsics[0][2] * 2), int(intrinsics[1][2] * 2)

            pcl_converter = PointCloudVisualizer(intrinsics, w, h)
            frame_sync = FrameSync()

            logging.info("Starting point cloud generation...")
            frame_count = 0
            start_time = time.time()

            while True:
                in_depth = depthQueue.tryGet()
                in_color = colorQueue.tryGet()

                if in_depth is not None:
                    depth, color = frame_sync.add_frame("depth", in_depth)
                elif in_color is not None:
                    depth, color = frame_sync.add_frame("color", in_color)
                else:
                    time.sleep(0.001)  # Small sleep to prevent busy waiting
                    continue

                if depth is not None and color is not None:
                    try:
                        depth_frame = depth.getFrame()
                        color_frame = color.getCvFrame()

                        rgb = cv2.cvtColor(color_frame, cv2.COLOR_BGR2RGB)
                        points, colors = pcl_converter.rgbd_to_projection(depth_frame, rgb, MAX_DISTANCE)
                        _, buffer = cv2.imencode('.jpg', color_frame)
                        
                        with data_lock:
                            latest_points = points
                            latest_colors = colors
                            latest_frame = latest_frame = buffer.tobytes()

                        frame_count += 1
                        if frame_count % FPS_TARGET == 0:
                            elapsed_time = time.time() - start_time
                            fps = frame_count / elapsed_time
                            logging.info(f"Processed {frame_count} frames. FPS: {fps:.2f}")
                            logging.info(f"Updated point cloud with {len(latest_points)} points")
                            start_time = time.time()
                            frame_count = 0
                    except Exception as e:
                        logging.error(f"Error processing frames: {e}")
                        continue

    except Exception as e:
        logging.error(f"Error in pipeline execution: {e}")
        logging.exception("Exception details:")


@app.route('/get_point_cloud', methods=['GET'])
def get_point_cloud():
    global latest_points, latest_colors
    with data_lock:
        if latest_points is None or latest_colors is None:
            return jsonify({"status": "no data"})

        response_data = {
            "status": "success",
            "points": latest_points.tolist(),
            "colors": latest_colors.tolist()
        }

    json_response = json.dumps(response_data)
    
    logging.info(f"Point cloud data size:")
    logging.info(f"  Number of points: {len(latest_points)}")
    logging.info(f"  Total JSON response: {sys.getsizeof(json_response)} bytes")

    return json_response, 200, {'Content-Type': 'application/json'}

@app.route('/get_frame')
def get_frame():
    if latest_frame is not None:
        return send_file(
            io.BytesIO(latest_frame),
            mimetype='image/jpeg',
            as_attachment=True,
            download_name='frame.jpg'
        )
    else:
        return "No frame available", 404


if __name__ == "__main__":
    pipeline_thread = threading.Thread(target=run_pipeline)
    pipeline_thread.daemon = True
    pipeline_thread.start()

    logging.info("DepthAI pipeline started. Server is running.")
    
    app.run(host='0.0.0.0', port=5001, threaded=True)
