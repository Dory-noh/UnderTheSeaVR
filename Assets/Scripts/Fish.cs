using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// 물고기의 행동을 제어하는 스크립트.
/// 헤엄치는 애니메이션, 장애물 회피, 랜덤 이동(방황) 등의 기능을 포함.
/// </summary>
public class Fish : MonoBehaviour
{
    private int level;
    public int Level {  get { return level; } set { level = value; } }
    /// <summary>
    /// 수조의 중심 위치.  
    /// 장애물 회피 시 기준점으로 사용됨.
    /// </summary>
    public Transform tankCenterGoal;

    /// <summary>
    /// 장애물 감지 거리 (단위: 미터).  
    /// 이 거리 안에 장애물이 있으면 회피 행동을 시작함.
    /// </summary>
    public float obstacleSensingDistance = 0.8f;

    /// <summary>
    /// 물고기의 최소 이동 속도 (m/s).
    /// </summary>
    public float swimSpeedMin = 0.2f;

    /// <summary>
    /// 물고기의 최대 이동 속도 (m/s).
    /// </summary>
    public float swimSpeedMax = 0.6f;

    /// <summary>
    /// 물고기의 최대 회전 속도 (Y축 기준).
    /// </summary>
    public float maxTurnRateY = 5f;

    /// <summary>
    /// 방황(Wandering) 시 방향 전환 가능한 최대 각도.
    /// </summary>
    public float maxWanderAngle = 45f;

    /// <summary>
    /// 방황 주기 (단위: 초).  
    /// 일정 시간마다 물고기가 방향을 변경할 수 있음.
    /// 방향 변경 확률은 <tt>wanderProbability</tt>에 의해 결정됨.
    /// </summary>
    public float wanderPeriodDuration = 0.8f;

    /// <summary>
    /// 물고기가 방황할 때 방향을 변경할 확률 (0~1).
    /// </summary>
    public float wanderProbability = 0.15f;

    /// <summary>
    /// 현재 물고기의 이동 속도 (m/s).
    /// </summary>
    [HideInInspector]
    public float swimSpeed;

    /// <summary>
    /// 물고기의 현재 이동 방향.
    /// </summary>
    private Vector3 swimDirection
    {
        get { return transform.TransformDirection(Vector3.forward); }
    }

    // 장애물을 감지했는지 여부
    private bool obstacleDetected = false;

    // 현재 방황 주기의 시작 시간
    private float wanderPeriodStartTime;

    // 물고기가 회전할 목표 방향 (Quaternion)
    private Quaternion goalLookRotation;

    // 물고기 몸체의 Transform 캐싱
    private Transform bodyTransform;

    // 물고기마다 랜덤하게 설정되는 값 (행동 차이를 만들기 위함)
    private float randomOffset;

    // 디버그용 변수 (광선과 목표 지점을 그리기 위해 사용)
    private Vector3 hitPoint;
    private Vector3 goalPoint;


    /* ----- MonoBehaviour 기본 메서드 ----- */


    void Start()
    {
        tankCenterGoal = GameObject.Find("Water").transform;
        // tankCenterGoal이 설정되지 않았으면 경고 메시지 출력
        if (tankCenterGoal == null)
        {
            Debug.LogError("[" + name + "] tankCenterGoal 값이 필요하지만 설정되지 않음.");
            UnityEditor.EditorApplication.isPlaying = false;
        }

        bodyTransform = transform.Find("Body");
        randomOffset = Random.value;
    }


    private void Update()
    {
        if (GameManager.Instance.IsOver == true) return;
        Wiggle();          // 헤엄치는 애니메이션 처리
        Wander();          // 랜덤 이동 처리
        AvoidObstacles();  // 장애물 회피 처리

        DrawDebugAids();   // 디버그용 시각적 요소 그리기
        UpdatePosition();  // 위치 업데이트
    }


    private void OnDrawGizmos()
    {
        DrawDebugAids(); // 편집기에서 디버그용 시각적 요소 표시
    }


    /* ----- 물고기 행동 관련 메서드 ----- */


    /// <summary>
    /// 물고기의 헤엄치는 애니메이션을 업데이트.
    /// </summary>
    void Wiggle()
    {
        // 현재 속도를 기준으로 흔들림 속도 계산
        float speedPercent = swimSpeed / swimSpeedMax;
        float minWiggleSpeed = 12f;
        float maxWiggleSpeed = minWiggleSpeed + 1f;
        float wiggleSpeed = Mathf.Lerp(minWiggleSpeed, maxWiggleSpeed, speedPercent);

        // 사인 곡선을 이용해 좌우로 흔들리는 애니메이션 적용
        float angle = Mathf.Sin(Time.time * wiggleSpeed) * 5f;
        var wiggleRotation = Quaternion.AngleAxis(angle, Vector3.up);
        bodyTransform.localRotation = wiggleRotation;
    }


    /// <summary>
    /// 물고기의 랜덤 이동(방황) 로직.
    /// </summary>
    void Wander()
    {
        // Perlin Noise를 사용하여 속도를 랜덤하게 변화
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
                // 랜덤 방향 선택
                var randomAngle = Random.Range(-maxWanderAngle, maxWanderAngle);
                var relativeWanderRotation = Quaternion.AngleAxis(randomAngle, Vector3.up);
                goalLookRotation = transform.rotation * relativeWanderRotation;
            }
        }

        // 목표 회전 방향으로 부드럽게 회전
        transform.rotation = Quaternion.Slerp(transform.rotation, goalLookRotation, Time.deltaTime / 2f);
    }


    /// <summary>
    /// 장애물 감지 및 회피 로직.
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
    /// 디버그용 시각적 요소 (광선) 그리기.
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
    /// 물고기의 위치를 업데이트.
    /// </summary>
    private void UpdatePosition()
    {
        Vector3 position = transform.position + swimDirection * swimSpeed * Time.fixedDeltaTime;
        transform.position = position;
    }
}