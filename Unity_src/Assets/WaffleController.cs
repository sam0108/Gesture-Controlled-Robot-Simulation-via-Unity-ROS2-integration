using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry; // 確保你已經 Build 過 geometry_msgs
using RosMessageTypes.Std;  // 注意首字母大寫

public class WaffleController : MonoBehaviour
{
    private ROSConnection ros;
    public string twistTopicName = "/cmd_vel"; // 對應 ROS 2 的 topic
    public string brakeTopicName = "/emergency_stop_signal";

    private float linearVelocity;
    private float angularVelocity;

    public ArticulationBody leftWheel;  // 在 Inspector 拖入左輪物件
    public ArticulationBody rightWheel; // 在 Inspector 拖入右輪物件

    public float wheelRadius = 0.033f;  // Waffle 輪胎半徑 (約 3.3cm)
    public float trackWidth = 0.287f;   // 兩輪間距 (約 28.7cm)


    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        // 訂閱來自視覺控制節點的指令
        ros.Subscribe<TwistStampedMsg>(twistTopicName, ReceiveMove);
        // 訂閱來自緊急停止節點的訊號
        ros.Subscribe<BoolMsg>(brakeTopicName, ReceiveBrake);
    }

    // void ReceiveMove(TwistMsg msg)
    // {
    //     linearVelocity = (float)msg.linear.x;
    //     angularVelocity = (float)msg.angular.z;
    // }
    
    void ReceiveMove(TwistStampedMsg msg)
    {   
        print("Received TwistStampedMsg: " + msg);
        linearVelocity = (float)msg.twist.linear.x * 5; // 調整速度倍率
        angularVelocity = (float)msg.twist.angular.z;
    }

    void ReceiveBrake(BoolMsg msg)
    {
        if (msg.data)
        {
            linearVelocity = 0f;
            angularVelocity = 0f;
        }
    }

    void FixedUpdate()
    {
        // 1. 計算左右輪的切線速度 (Linear Velocity of each wheel)
        float vLeft = linearVelocity - (angularVelocity * trackWidth / 2f);
        float vRight = linearVelocity + (angularVelocity * trackWidth / 2f);

        // 2. 轉換為角速度 (Angular Velocity in Degrees/s)
        // 公式：ω = v / r (弧度)，再轉為角度
        float omegaLeft = (vLeft / wheelRadius) * Mathf.Rad2Deg;
        float omegaRight = (vRight / wheelRadius) * Mathf.Rad2Deg;

        // 3. 調用 Articulation Body 的 Drive 屬性
        var leftDrive = leftWheel.xDrive;
        leftDrive.targetVelocity = omegaLeft;
        leftWheel.xDrive = leftDrive;

        var rightDrive = rightWheel.xDrive;
        rightDrive.targetVelocity = omegaRight;
        rightWheel.xDrive = rightDrive;
    }

    void Update()
    {
        // // 基礎移動邏輯 (簡單版)
        // // 注意：Unity 的 Forward 是 Z 軸，ROS 的 Forward 是 X 軸
        // transform.Translate(Vector3.forward * linearVelocity * Time.deltaTime * 10f); // 調整速度倍率
        // transform.Rotate(Vector3.up * -angularVelocity * Mathf.Rad2Deg * Time.deltaTime);
    }
}