using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerMoveCheck : MonoBehaviour
{
    public Transform leftController;
    public Transform rightController;
    public Transform mainCamera;

    public float movementThreshold = 0.02f;  // 컨트롤러가 실제로 움직였다고 판단할 최소 거리
    public float speedMultiplier = 15f;  // 속도 배율
    public float maxSpeed = 7f;  // 최대 이동 속도
    private Quaternion previousCameraRotation;

    private Vector3[] previousPosition = new Vector3[2];
    private float previousCameraX;
    private float previousCameraY;
    private float currentSpeed = 0f;  // 현재 속도 저장

    void Start()
    {
        previousPosition[0] = leftController.position;
        previousPosition[1] = rightController.position;

        previousCameraRotation = mainCamera.rotation;

        previousCameraX = mainCamera.eulerAngles.x;
        previousCameraY = mainCamera.eulerAngles.y;
    }

    void Update()
    {
        // 현재 컨트롤러 위치 가져오기
        Vector3[] currentPosition = new Vector3[2] { leftController.position, rightController.position };

        // 컨트롤러 이동 거리 계산
        float leftMoveDistance = (currentPosition[0] - previousPosition[0]).magnitude;
        float rightMoveDistance = (currentPosition[1] - previousPosition[1]).magnitude;

        // 이동한 거리의 평균 구하기
        float totalMoveDistance = (leftMoveDistance + rightMoveDistance) / 2f;
        Debug.Log(totalMoveDistance);
        // 이동 거리가 threshold(노이즈 제거 기준)보다 크면 speed 계산
        if (totalMoveDistance > movementThreshold && totalMoveDistance < 0.4)
        {
            currentSpeed = Mathf.Clamp(totalMoveDistance * speedMultiplier, 0.5f, maxSpeed);
        }
        else
        {
            Debug.Log("컨트롤러 이동 멈춤");
            // 컨트롤러가 안 움직이면 속도 0
            currentSpeed = 0f;
        }

        // 속도가 0이 아닐 때만 이동
        if (currentSpeed > 0)
        {
            Vector3 moveDirection = mainCamera.forward + mainCamera.up;
            moveDirection.z = 0;
            moveDirection.Normalize();
            Debug.Log(moveDirection);
            transform.position += moveDirection * currentSpeed * Time.deltaTime;
        }

        //// 카메라 회전 변화량만큼만 회전 적용
        //float cameraYDeltaX = mainCamera.eulerAngles.x - previousCameraX;
        //float cameraYDeltaY = mainCamera.eulerAngles.y - previousCameraY;
        //transform.Rotate(cameraYDeltaX, cameraYDeltaY, 0, Space.World);
        //previousCameraX = mainCamera.eulerAngles.x;
        //previousCameraY = mainCamera.eulerAngles.y;

        // 카메라 회전 변화량 적용 (흔들림 보정)
        Quaternion cameraRotationDelta = mainCamera.rotation * Quaternion.Inverse(previousCameraRotation);
        transform.rotation = cameraRotationDelta * transform.rotation;
        previousCameraRotation = mainCamera.rotation;

        // 현재 위치를 이전 위치로 저장
        previousPosition[0] = currentPosition[0];
        previousPosition[1] = currentPosition[1];
    }
}
