import socket
import os
import json
import pprint
import time
import sys
from spot_controller import SpotController

socket_path = '/merklebot.socket'

spot = SpotController()


def send_request(action, content=None):
    client = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
    client.connect(os.path.realpath(socket_path))
    message = {"action": action}
    if content:
        message["content"] = content
    client.sendall(json.dumps(message).encode())
    response = client.recv(1024).decode()
    client.close()
    return json.loads(response)

def subscribe_messages():
    with SpotController() as sc:
        with socket.socket(socket.AF_UNIX, socket.SOCK_STREAM) as client:
            client.connect(os.path.realpath(socket_path))
            print("Subscribed to messages")
            client.sendall(json.dumps({"action": "/subscribe_messages"}).encode())
            while True:
                time.sleep(0.05)
                response = client.recv(1024).decode()
                if response:
                    print(response)
                    cmd = response
                    if cmd=='u':
                        sc.lease_control()
                        sc.release_estop()
                        sc.power_on_stand_up()
                        time.sleep(3)
                        print("powered on")
                    if cmd=='p':
                        sc.power_off_sit_down()
                        sc.return_lease()
                        sc.set_estop()
                    if cmd=='w':
                        sc.move_by_velocity_control(v_x=1, v_y=0, v_rot=0)
                    if cmd=='s':
                        sc.move_by_velocity_control(v_x=-1, v_y=0, v_rot=0)
                    if cmd=='a':
                        sc.move_by_velocity_control(v_x=0, v_y=1, v_rot=0)
                    if cmd=='d':
                        sc.move_by_velocity_control(v_x=0, v_y=-1, v_rot=0)
                    if cmd=='q':
                        sc.move_by_velocity_control(v_x=0, v_y=0, v_rot=1)
                    if cmd=='e':
                        sc.move_by_velocity_control(v_x=0, v_y=0, v_rot=-1)
                    if cmd =='b':
                        sc.bow(0.6)
                        sc.bow(0)
                    if cmd=='n':
                        sc.make_stance(0.3, 0.3)
                    if cmd=='m':
                        sc.dust_off([0, 0.3], [0, -0.6], [0, 0])

def main():
    pass
    #robot = send_request("/me")
    #print('=== /me ===')
    #pprint.pprint(robot)
    #local_devices = send_request("/local_robots")
    #print('=== /local_robots ===')
    #pprint.pprint(local_devices)
    #subscribe_messages()
    #while True:
    #    inp = sys.stdin.read(1)
    #    print(inp)
    #    #res = send_request("/send_message", "w")
    #    #print(res)
def press(key):
    print(key)
    res =send_request("/send_message", key)
    print(res)

if __name__=='__main__':
    subscribe_messages()

