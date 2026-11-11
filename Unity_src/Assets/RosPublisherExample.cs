using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;

/// <summary>
///
/// </summary>
public class RosPublisherExample : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "pos_rot";

    // The game object
    public GameObject robot;
    // Publish the robot's position and rotation every N seconds
    public float publishMessageFrequency = 0.5f;

    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;

    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PosRotMsg>(topicName);
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            robot.transform.rotation = Random.rotation;

            PosRotMsg robotPos = new PosRotMsg(
                robot.transform.position.x,
                robot.transform.position.y,
                robot.transform.position.z,
                robot.transform.rotation.x,
                robot.transform.rotation.y,
                robot.transform.rotation.z,
                robot.transform.rotation.w
            );

            // Finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, robotPos);

            timeElapsed = 0;
        }
    }
}
