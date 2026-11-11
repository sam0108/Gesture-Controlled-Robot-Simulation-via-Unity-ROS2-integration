using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor; // 記得先在 Message Browser Build 過 sensor_msgs

public class LidarPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/scan";

    [Header("Lidar Settings")]
    public int numSamples = 360;      // 每一圈採樣幾點
    public float rangeMin = 0.01f;    // 最小偵測距離
    public float rangeMax = 3.5f;     // Waffle 雷達標準最大距離
    public float scanFrequency = 5f;  // 每秒掃描幾次

    private float nextScanTime;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<LaserScanMsg>(topicName);
    }

    void Update()
    {
        if (Time.time >= nextScanTime)
        {
            PublishLaserScan();
            nextScanTime = Time.time + (1f / scanFrequency);
        }
    }

    void PublishLaserScan()
    {
        var msg = new LaserScanMsg();
        msg.header.frame_id = "base_scan";
        msg.angle_min = 0;
        msg.angle_max = 2 * Mathf.PI;
        msg.angle_increment = (2 * Mathf.PI) / numSamples;
        msg.range_min = rangeMin;
        msg.range_max = rangeMax;

        float[] ranges = new float[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            float angle = i * msg.angle_increment;
            // 計算射線方向 (假設 Unity Y 軸為向上)
            Vector3 direction = Quaternion.Euler(0, -angle * Mathf.Rad2Deg, 0) * transform.forward;
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, rangeMax, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                ranges[i] = hit.distance >= rangeMin ? hit.distance : float.PositiveInfinity;
            }
            else
            {
                ranges[i] = float.PositiveInfinity; // 代表沒打到東西
            }
        }
        Debug.Log("Front distance: " + ranges[0] + " meters, Back distance: " + ranges[numSamples / 2] + " meters");
        msg.ranges = ranges;
        ros.Publish(topicName, msg);
    }
}