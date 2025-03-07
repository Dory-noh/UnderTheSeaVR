using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerMove : MonoBehaviour
{
    public InputActionProperty leftHandPositionAction; // 왼쪽 손 위치
    public InputActionProperty rightHandPositionAction; // 오른쪽 손 위치
    public Transform mainCamera; // 카메라

    public float movementThreshold = 0.02f; // 컨트롤러가 실제로 움직였다고 판단할 최소 거리
    public float speedMultiplier = 15f; // 속도 배율
    public float maxSpeed = 7f; // 최대 이동 속도
    public float smoothSpeed = 5f; // 부드러운 이동 속도
    public float acceleration = 2f; // 가속도
    public float deceleration = 5f; // 감속도
    private Quaternion previousCameraRotation;

    private Vector3[] previousPosition = new Vector3[2]; // 왼쪽, 오른쪽 컨트롤러 위치
    private float currentSpeed = 0f; // 현재 속도 저장
    private Vector3 targetPosition; // 목표 위치

    void Start()
    {
        // InputActionProperty로 왼쪽, 오른쪽 손 위치 읽기
        previousPosition[0] = leftHandPositionAction.action.ReadValue<Vector3>();
        previousPosition[1] = rightHandPositionAction.action.ReadValue<Vector3>();

        previousCameraRotation = mainCamera.rotation;
        targetPosition = transform.position; // 초기 목표 위치 설정
    }

    void Update()
    {
        // 현재 손 위치 가져오기
        Vector3[] currentPosition = new Vector3[2] { leftHandPositionAction.action.ReadValue<Vector3>(),
                                                        rightHandPositionAction.action.ReadValue<Vector3>() };

        // 컨트롤러 이동 거리 계산
        float leftMoveDistance = (currentPosition[0] - previousPosition[0]).magnitude;
        float rightMoveDistance = (currentPosition[1] - previousPosition[1]).magnitude;

        // 이동한 거리의 평균 구하기
        float totalMoveDistance = (leftMoveDistance + rightMoveDistance) / 2f;

        // 이동 거리가 threshold(노이즈 제거 기준)보다 크면 speed 계산
        if (totalMoveDistance > movementThreshold && totalMoveDistance < 0.4f)
        {
            currentSpeed = Mathf.Clamp(currentSpeed + acceleration * Time.deltaTime, 0f, maxSpeed); // 가속도 적용
        }
        else
        {
            currentSpeed = Mathf.Clamp(currentSpeed - deceleration * Time.deltaTime, 0f, maxSpeed); // 감속도 적용
        }

        // 속도가 0이 아닐 때만 이동
        if (currentSpeed > 0)
        {
            // 카메라의 전방 방향과 상하 방향을 함께 고려하여 이동 방향 설정
            Vector3 moveDirection = mainCamera.forward; // 카메라의 전방
            moveDirection = moveDirection.normalized; // 정규화하여 일정한 속도로 이동

            // 목표 위치 설정
            targetPosition += moveDirection * currentSpeed * Time.deltaTime;
            Debug.Log(targetPosition);
        }

        // 부드러운 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        // 카메라 회전 변화량 적용 (흔들림 보정)
        Quaternion cameraRotationDelta = mainCamera.rotation * Quaternion.Inverse(previousCameraRotation);
        cameraRotationDelta.x = 0f;
        cameraRotationDelta.z = 0f;
        transform.rotation = cameraRotationDelta * transform.rotation;
        Debug.Log("회전:"+transform.rotation);
        previousCameraRotation = mainCamera.rotation;

        // 현재 위치를 이전 위치로 저장
        previousPosition[0] = currentPosition[0];
        previousPosition[1] = currentPosition[1];
    }
}