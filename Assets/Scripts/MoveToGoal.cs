using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using static UnityEditor.PlayerSettings;
using Grpc.Core;


public class MoveToGoal : Agent
{
    Rigidbody m_AgentRb;
    [SerializeField] public GameObject target;
    private SpawnController spawnCheese;
    private MazeArea getAreaBound;
    float xFirst, zFirst, xLast, zLast;
    private int Episode = 0;
    private int Success = 0;
    private int Fail = 0;
    private int TimeStep = 0;

    public override void Initialize()
    {
        m_AgentRb = GetComponent<Rigidbody>();
        spawnCheese = GetComponent<SpawnController>();
        getAreaBound = GetComponent<MazeArea>();
    }

    /// <summary>
    /// Get the coordinate of first and last maze cell
    /// </summary>
    void Start()
    {
        var FirstItemCoordinate = getAreaBound.GetAreaBoundsFirst();
        xFirst = FirstItemCoordinate.Item1;
        zFirst = FirstItemCoordinate.Item2;

        var LastItemCoordinate = getAreaBound.GetAreaBoundsLast();
        xLast = LastItemCoordinate.Item1;
        zLast = LastItemCoordinate.Item2;
    }

    /// <summary>
    /// Spawn cheese at new location
    /// </summary>
    void ResetCheese()
    {
        spawnCheese.SpawnCheese(target);
    }

    void AgentReset()
    {
        Vector3 pos = this.transform.localPosition;
        float x = this.transform.position.x;
        float z = this.transform.position.z;

        if (x < xFirst)
        {
            x = xFirst;
        }
        else if (x > xLast)
        {
            x = xLast;
        }
        else if (z < zFirst)
        {
            z = zFirst;
        }
        else if (z > zLast)
        {
            z = zLast;
        }
        else
        {
            pos = new Vector3(x, 0.5f, z);
        }
    }

    /// <summary>
    /// Begin new episode
    /// </summary>
    public override void OnEpisodeBegin()
    {
        Episode += 1;
        m_AgentRb.velocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360), 0f));
        DebugText();
    }

    /// <summary>
    /// Debugging to monitor success rate
    /// </summary>
    void DebugText()
    {
        float SuccessPercent = (Success / (float)(Success + Fail)) * 100;
        Debug.Log("Episode= " + Episode + " || " + "Success= " + Success + " || " + "Fail= " + Fail + " || " + SuccessPercent + "%" + " || " + "Time: " + TimeStep);
    }

    /// <summary>
    /// Set observation for agent
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 currentPosForward = transform.TransformDirection(Vector3.forward);
        Vector3 dirToCheese = (target.transform.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(transform.InverseTransformDirection(m_AgentRb.velocity));
        sensor.AddObservation(dirToCheese);
    }

    /// <summary>
    /// Set agent movement controls
    /// </summary>
    /// <param name="act"></param>
    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var action = act[0];
        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
        }

        transform.Rotate(rotateDir, Time.deltaTime * 150f);
        m_AgentRb.AddForce(dirToGo * 3f, ForceMode.VelocityChange);
        
        if (Vector3.Distance(target.transform.position, transform.position) < 6.0f)
        {
            AddReward(0.05f);
            Success += 1;
        }

        Vector3 targetDir = target.transform.position - transform.position;
        float angleBetween = Vector3.Angle(transform.forward, targetDir);

        if (angleBetween > 40)
        {
            AddReward(-0.01f);
            Fail += 1;
        }
    }

    /// <summary>
    /// Debugging in real-time
    /// </summary>
    void FixedUpdate()
    {
        Vector3 targetDir = target.transform.position - transform.position;
        Debug.DrawRay(this.transform.position, targetDir, Color.red, 5);
        Debug.DrawRay(this.transform.position, transform.TransformDirection(Vector3.forward) * 5.0f, Color.yellow);
        TimeStep += 1;
    }

    /// <summary>
    /// Collect Trajectories
    /// </summary>
    /// <param name="actionBuffers"></param>
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //AddReward(-0.05f / MaxStep);
        MoveAgent(actionBuffers.DiscreteActions);
    }

    /// <summary>
    /// Self-play movement controls
    /// </summary>
    /// <param name="actionsOut"></param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
    }

    /// <summary>
    /// Event to detect ml-agent collision
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Goal>(out Goal goal))
        {
            Success += 1;
            SetReward(1f);
            goal.gameObject.SetActive(false);
            ResetCheese();
            EndEpisode();
        }
        if (other.TryGetComponent<Walls>(out Walls wall))
        {
            Fail += 1;
            SetReward(-1f);
            EndEpisode();
        }
    }
}
