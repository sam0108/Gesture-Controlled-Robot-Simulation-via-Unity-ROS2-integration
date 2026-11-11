import rclpy
from rclpy.node import Node
from sensor_msgs.msg import LaserScan
from std_msgs.msg import Bool
from rclpy.qos import QoSProfile, ReliabilityPolicy, HistoryPolicy
import math


class EmergencyBrakeNode(Node):
    def __init__(self):
        super().__init__('emergency_brake_node')

        self.declare_parameter('front_safe_distance', 0.6)
        self.front_safe_distance = self.get_parameter('front_safe_distance').value
        self.declare_parameter('back_safe_distance', 5.0)
        self.back_safe_distance = self.get_parameter('back_safe_distance').value

        qos_profile = QoSProfile(
            reliability=ReliabilityPolicy.BEST_EFFORT,
            history=HistoryPolicy.KEEP_LAST,
            depth=10
        )

        self.scan_sub = self.create_subscription(
            LaserScan, '/scan', self.scan_callback, qos_profile)

        self.emergency_front_pub = self.create_publisher(Bool, '/emergency_stop_signal_front', 10)
        self.emergency_back_pub = self.create_publisher(Bool, '/emergency_stop_signal_back', 10)

        self.get_logger().info(
            f'Emergency brake node initialized ...')

    def scan_callback(self, msg):
        front_ranges = msg.ranges[0:30] + msg.ranges[330:359]
        valid_ranges = [r for r in front_ranges if math.isfinite(r) and r > msg.range_min]
        front_min_dist = min(valid_ranges) if valid_ranges else float('inf')

        emergency_msg_front = Bool()
        emergency_msg_front.data = front_min_dist < self.front_safe_distance
        self.emergency_front_pub.publish(emergency_msg_front)

        back_ranges = msg.ranges[150:210]
        valid_ranges = [r for r in back_ranges if math.isfinite(r) and r > msg.range_min]
        back_min_dist = min(valid_ranges) if valid_ranges else float('inf')

        emergency_msg_back = Bool()
        emergency_msg_back.data = back_min_dist < self.back_safe_distance
        self.emergency_back_pub.publish(emergency_msg_back)

        self.get_logger().info(
            f'Front min distance: {front_min_dist:.2f} m, Back min distance: {back_min_dist:.2f} m, Emergency Front: {emergency_msg_front.data}, Emergency Back: {emergency_msg_back.data}')
        
def main(args=None):
    rclpy.init(args=args)
    node = EmergencyBrakeNode()
    try:
        rclpy.spin(node)
    except KeyboardInterrupt:
        pass
    finally:
        node.destroy_node()
        rclpy.shutdown()


if __name__ == '__main__':
    main()
