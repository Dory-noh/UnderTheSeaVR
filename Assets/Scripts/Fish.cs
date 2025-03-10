using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// ������� �ൿ�� �����ϴ� ��ũ��Ʈ.
/// ���ġ�� �ִϸ��̼�, ��ֹ� ȸ��, ���� �̵�(��Ȳ) ���� ����� ����.
/// </summary>
public class Fish : MonoBehaviour
{
    private int level;
    public int Level {  get { return level; } set { level = value; } }
    /// <summary>
    /// ������ �߽� ��ġ.  
    /// ��ֹ� ȸ�� �� ���������� ����.
    /// </summary>
    public Transform tankCenterGoal;

    /// <summary>
    /// ��ֹ� ���� �Ÿ� (����: ����).  
    /// �� �Ÿ� �ȿ� ��ֹ��� ������ ȸ�� �ൿ�� ������.
    /// </summary>
    public float obstacleSensingDistance = 0.8f;

    /// <summary>
    /// ������� �ּ� �̵� �ӵ� (m/s).
    /// </summary>
    public float swimSpeedMin = 0.2f;

    /// <summary>
    /// ������� �ִ� �̵� �ӵ� (m/s).
    /// </summary>
    public float swimSpeedMax = 0.6f;

    /// <summary>
    /// ������� �ִ� ȸ�� �ӵ� (Y�� ����).
    /// </summary>
    public float maxTurnRateY = 5f;

    /// <summary>
    /// ��Ȳ(Wandering) �� ���� ��ȯ ������ �ִ� ����.
    /// </summary>
    public float maxWanderAngle = 45f;

    /// <summary>
    /// ��Ȳ �ֱ� (����: ��).  
    /// ���� �ð����� ����Ⱑ ������ ������ �� ����.
    /// ���� ���� Ȯ���� <tt>wanderProbability</tt>�� ���� ������.
    /// </summary>
    public float wanderPeriodDuration = 0.8f;

    /// <summary>
    /// ����Ⱑ ��Ȳ�� �� ������ ������ Ȯ�� (0~1).
    /// </summary>
    public float wanderProbability = 0.15f;

    /// <summary>
    /// ���� ������� �̵� �ӵ� (m/s).
    /// </summary>
    [HideInInspector]
    public float swimSpeed;

    /// <summary>
    /// ������� ���� �̵� ����.
    /// </summary>
    private Vector3 swimDirection
    {
        get { return transform.TransformDirection(Vector3.forward); }
    }

    // ��ֹ��� �����ߴ��� ����
    private bool obstacleDetected = false;

    // ���� ��Ȳ �ֱ��� ���� �ð�
    private float wanderPeriodStartTime;

    // ����Ⱑ ȸ���� ��ǥ ���� (Quaternion)
    private Quaternion goalLookRotation;

    // ����� ��ü�� Transform ĳ��
    private Transform bodyTransform;

    // ����⸶�� �����ϰ� �����Ǵ� �� (�ൿ ���̸� ����� ����)
    private float randomOffset;

    // ����׿� ���� (������ ��ǥ ������ �׸��� ���� ���)
    private Vector3 hitPoint;
    private Vector3 goalPoint;


    /* ----- MonoBehaviour �⺻ �޼��� ----- */


    void Start()
    {
        tankCenterGoal = GameObject.Find("Water").transform;
        // tankCenterGoal�� �������� �ʾ����� ��� �޽��� ���
        if (tankCenterGoal == null)
        {
            Debug.LogError("[" + name + "] tankCenterGoal ���� �ʿ������� �������� ����.");
            UnityEditor.EditorApplication.isPlaying = false;
        }

        bodyTransform = transform.Find("Body");
        randomOffset = Random.value;
    }


    private void Update()
    {
        if (GameManager.Instance.IsOver == true) return;
        Wiggle();          // ���ġ�� �ִϸ��̼� ó��
        Wander();          // ���� �̵� ó��
        AvoidObstacles();  // ��ֹ� ȸ�� ó��

        DrawDebugAids();   // ����׿� �ð��� ��� �׸���
        UpdatePosition();  // ��ġ ������Ʈ
    }


    private void OnDrawGizmos()
    {
        DrawDebugAids(); // �����⿡�� ����׿� �ð��� ��� ǥ��
    }


    /* ----- ����� �ൿ ���� �޼��� ----- */


    /// <summary>
    /// ������� ���ġ�� �ִϸ��̼��� ������Ʈ.
    /// </summary>
    void Wiggle()
    {
        // ���� �ӵ��� �������� ��鸲 �ӵ� ���
        float speedPercent = swimSpeed / swimSpeedMax;
        float minWiggleSpeed = 12f;
        float maxWiggleSpeed = minWiggleSpeed + 1f;
        float wiggleSpeed = Mathf.Lerp(minWiggleSpeed, maxWiggleSpeed, speedPercent);

        // ���� ��� �̿��� �¿�� ��鸮�� �ִϸ��̼� ����
        float angle = Mathf.Sin(Time.time * wiggleSpeed) * 5f;
        var wiggleRotation = Quaternion.AngleAxis(angle, Vector3.up);
        bodyTransform.localRotation = wiggleRotation;
    }


    /// <summary>
    /// ������� ���� �̵�(��Ȳ) ����.
    /// </summary>
    void Wander()
    {
        // Perlin Noise�� ����Ͽ� �ӵ��� �����ϰ� ��ȭ
        float noiseScale = .5f;
        float speedPercent = Mathf.PerlinNoise(Time.time * noiseScale + randomOffset, randomOffset);
        speedPercent = Mathf.Pow(speedPercent, 2);
        swimSpeed = Mathf.Lerp(swimSpeedMin, swimSpeedMax, speedPercent);

        if (obstacleDetected) return;

        if (Time.time > wanderPeriodStartTime + wanderPeriodDuration)
        {
            wanderPeriodStartTime = Time.time;

            if (Random.value < wanderProbability)
            {
                // ���� ���� ����
                var randomAngle = Random.Range(-maxWanderAngle, maxWanderAngle);
                var relativeWanderRotation = Quaternion.AngleAxis(randomAngle, Vector3.up);
                goalLookRotation = transform.rotation * relativeWanderRotation;
            }
        }

        // ��ǥ ȸ�� �������� �ε巴�� ȸ��
        transform.rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.deltaTime / 2f);
    }


    /// <summary>
    /// ��ֹ� ���� �� ȸ�� ����.
    /// </summary>
    void AvoidObstacles()
    {
        RaycastHit hit;
        obstacleDetected = Physics.Raycast(transform.position, swimDirection, out hit, obstacleSensingDistance);

        if (obstacleDetected)
        {
            hitPoint = hit.point;

            Vector3 reflectionVector = Vector3.Reflect(swimDirection, hit.normal);
            float goalPointMinDistanceFromHit = 1f;
            Vector3 reflectedPoint = hit.point + reflectionVector * Mathf.Max(hit.distance, goalPointMinDistanceFromHit);

            goalPoint = (reflectedPoint + tankCenterGoal.position) / 2f;
            Vector3 goalDirection = goalPoint - transform.position;
            goalLookRotation = Quaternion.LookRotation(goalDirection);

            float dangerLevel = Mathf.Pow(1 - (hit.distance / obstacleSensingDistance), 4f);
            dangerLevel = Mathf.Max(0.01f, dangerLevel);

            float turnRate = maxTurnRateY * dangerLevel;
            Quaternion rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.deltaTime * turnRate);
            transform.rotation = rotation;
        }
    }


    /// <summary>
    /// ����׿� �ð��� ��� (����) �׸���.
    /// </summary>
    void DrawDebugAids()
    {
        Color rayColor = obstacleDetected ? Color.red : Color.cyan;
        Debug.DrawRay(transform.position, swimDirection * obstacleSensingDistance, rayColor);

        if (obstacleDetected)
        {
            Debug.DrawLine(hitPoint, goalPoint, Color.green);
        }
    }


    /// <summary>
    /// ������� ��ġ�� ������Ʈ.
    /// </summary>
    private void UpdatePosition()
    {
        Vector3 position = transform.position + swimDirection * swimSpeed * Time.fixedDeltaTime;
        transform.position = position;
    }
}