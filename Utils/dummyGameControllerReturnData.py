import socket
import struct
import time

class RoboCupGameControlReturnData:
    def __init__(self, team_num, player_num, fallen, pose, ball_age, ball_pos):
        self.header = b'RGrt'
        self.version = 4
        self.playerNum = player_num
        self.teamNum = team_num
        self.fallen = fallen
        self.pose = pose
        self.ballAge = ball_age
        self.ball = ball_pos

def create_dummy_data():
    # Dummy data with plain English values
    player_num = 1
    team_num = 51
    fallen = 0  # 0 means that the robot can play
    pose = (0.0, 0.0, 0.0)  # x, y, theta
    ball_age = -1.0  # -1.0 means the robot hasn't seen the ball
    ball_position = (0.0, 0.0)  # x, y

    return RoboCupGameControlReturnData(team_num, player_num, fallen, pose, ball_age, ball_position)

def send_dummy_data(ip, port):
    try:
        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        sock.bind(('192.168.40.57', 0))
        while True:
            dummy_data = create_dummy_data()
            packed_data = struct.pack("4siiiffff2f",
                                    dummy_data.header,
                                    dummy_data.playerNum, 
                                    dummy_data.teamNum, 
                                    dummy_data.fallen, 
                                    *dummy_data.pose, 
                                    dummy_data.ballAge,
                                    *dummy_data.ball)
            print("Sending packet of length:", len(packed_data))
            sock.sendto(packed_data, (ip, port))
            print("Dummy data sent to {}:{}".format(ip, port))
            time.sleep(2)
    except Exception as e:
        print("Error:", e)
    finally:
        sock.close()

if __name__ == "__main__":
    # Set the IP address and port of the RoboCup GameController
    roboCupGameControllerIP = "192.168.40.57"  # Replace with the actual IP address
    roboCupGameControllerPort = 3939  # Replace with the actual port

    send_dummy_data(roboCupGameControllerIP, roboCupGameControllerPort)
