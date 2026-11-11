import rclpy
from rclpy.node import Node
from sensor_msgs.msg import Image
from std_msgs.msg import String
from cv_bridge import CvBridge
import cv2
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision

class GestureRecognitionNode(Node):
    def __init__(self):
        super().__init__('gesture_recognition_node')
        self.bridge = CvBridge()
        
        # 1. 訂閱來自 Unity 的影像
        self.image_sub = self.create_subscription(Image, '/raw_image', self.image_callback, 10)
        
        # 2. 發布手勢辨識結果
        self.gesture_pub = self.create_publisher(String, '/gesture_signal', 10)
        
        # 3. 初始化 MediaPipe Tasks
        model_path = '/home/esmmam/robotic_final/Test/robotics-final/youting_src/yt_ws/src/final_package/final_package/gesture_recognizer.task'
        base_options = python.BaseOptions(model_asset_path=model_path)
        options = vision.GestureRecognizerOptions(
            base_options=base_options,
            running_mode=vision.RunningMode.IMAGE,
            num_hands=2,
            min_hand_detection_confidence=0.7,
            min_tracking_confidence=0.5
        )
        self.recognizer = vision.GestureRecognizer.create_from_options(options)
        self.get_logger().info("Gesture Recognition Node has started.")

    def image_callback(self, msg):
        # 將 ROS Image 轉換為 OpenCV 格式
        cv_image = self.bridge.imgmsg_to_cv2(msg, "rgb8")
        cv_image = cv2.flip(cv_image, 0)
        cv_image = cv2.flip(cv_image, 1)
        show_image = cv_image.copy()
        # 3. 顯示影像視窗
        # cv2.cvtColor(show_image, cv2.COLOR_RGB2BGR)
        # cv2.imshow("Unity Webcam Stream", show_image)
        
        # 4. 必須加上 waitKey 否則視窗會卡死或無法更新
        cv2.waitKey(1)
        # 轉換為 MediaPipe 影像格式
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=cv_image)
        
        # 執行辨識
        recognition_result = self.recognizer.recognize(mp_image)
        
        if recognition_result.gestures:
            # 取得信心度最高的手勢標籤
            top_gesture = recognition_result.gestures[0][0].category_name
            
            # 發布手勢訊號
            gesture_msg = String()
            gesture_msg.data = top_gesture
            self.gesture_pub.publish(gesture_msg)
            
            # 顯示於終端機以便 Debug
            # self.get_logger().info(f"Detected Gesture: {top_gesture}")
        else:
            # 沒有辨識到手勢，發布 None
            gesture_msg = String()
            gesture_msg.data = 'None'
            self.gesture_pub.publish(gesture_msg)
            # self.get_logger().info("No hand detected, published 'None'.")

def main(args=None):
    rclpy.init(args=args)
    node = GestureRecognitionNode()
    try:
        rclpy.spin(node)
    except KeyboardInterrupt:
        pass
    finally:
        node.destroy_node()
        rclpy.shutdown()

if __name__ == '__main__':
    main()