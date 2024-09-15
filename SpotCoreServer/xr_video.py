import cv2
import depthai as dai
import numpy as np
from flask import Flask, send_file, request, jsonify
import threading
import io
import time  # Imported time module

app = Flask(__name__)

# Global variable to store the latest frame
latest_frame = None

from spot_controller import SpotController

ROBOT_IP = "192.168.80.3"  # os.environ['ROBOT_IP']
SPOT_USERNAME = "admin"    # os.environ['SPOT_USERNAME']
SPOT_PASSWORD = "2zqa8dgw7lor"  # os.environ['SPOT_PASSWORD']

spot = SpotController(username=SPOT_USERNAME, password=SPOT_PASSWORD, robot_ip=ROBOT_IP)

def process_command(command, args):
    global spot
    print("command: ", command)
    if command == 'start':
        spot.lease_control()
        spot.release_estop()
        spot.power_on_stand_up()

        time.sleep(2)
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
def command_route():
    try:
        content = request.json
        if not content or 'command' not in content:
            return jsonify({'error': 'Invalid request, missing command'}), 400

        process_command(content['command'], content.get('args'))
        return jsonify({'message': 'Execution started'}), 200
    except Exception as e:
        # Log the exception details for debugging
        app.logger.error(f"Error in /command route: {e}")
        return jsonify({'error': 'Internal Server Error'}), 500

def create_pipeline():
    # Create pipeline
    pipeline = dai.Pipeline()

    # Define source and output
    camRgb = pipeline.create(dai.node.ColorCamera)
    xoutRgb = pipeline.create(dai.node.XLinkOut)

    xoutRgb.setStreamName("rgb")

    # Properties
    camRgb.setPreviewSize(416, 416)
    camRgb.setInterleaved(False)
    camRgb.setColorOrder(dai.ColorCameraProperties.ColorOrder.BGR)

    # Linking
    camRgb.preview.link(xoutRgb.input)

    return pipeline

def capture_frames():
    global latest_frame

    # Connect to device and start pipeline
    with dai.Device(create_pipeline()) as device:
        # Output queue will be used to get the rgb frames from the output defined above
        qRgb = device.getOutputQueue(name="rgb", maxSize=4, blocking=False)

        while True:
            inRgb = qRgb.get()  # blocking call, will wait until new data arrives
            frame = inRgb.getCvFrame()
            
            # Encode frame to JPEG
            success, buffer = cv2.imencode('.jpg', frame)
            if success:
                latest_frame = buffer.tobytes()
            else:
                app.logger.error("Failed to encode frame to JPEG.")

@app.route('/get_frame', methods=['GET'])
def get_frame():
    if latest_frame is not None:
        return send_file(
            io.BytesIO(latest_frame),
            mimetype='image/jpeg',
            as_attachment=True,
            download_name='frame.jpg'
        )
    else:
        return jsonify({"error": "No frame available"}), 404

if __name__ == '__main__':
    # Start frame capture in a separate thread
    frame_thread = threading.Thread(target=capture_frames, daemon=True)
    frame_thread.start()
    
    # Run Flask app
    app.run(host='0.0.0.0', port=5002)
