using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerMove : MonoBehaviour
{
    public InputActionProperty leftHandPositionAction; // ���� �� ��ġ
    public InputActionProperty rightHandPositionAction; // ������ �� ��ġ
    public Transform mainCamera; // ī�޶�

    public float movementThreshold = 0.02f; // ��Ʈ�ѷ��� ������ �������ٰ� �Ǵ��� �ּ� �Ÿ�
    public float speedMultiplier = 15f; // �ӵ� ����
    public float maxSpeed = 7f; // �ִ� �̵� �ӵ�
    public float smoothSpeed = 5f; // �ε巯�� �̵� �ӵ�
    public float acceleration = 2f; // ���ӵ�
    public float deceleration = 5f; // ���ӵ�
    private Quaternion previousCameraRotation;

    private Vector3[] previousPosition = new Vector3[2]; // ����, ������ ��Ʈ�ѷ� ��ġ
    private float currentSpeed = 0f; // ���� �ӵ� ����
    private Vector3 targetPosition; // ��ǥ ��ġ

    void Start()
    {
        // InputActionProperty�� ����, ������ �� ��ġ �б�
        previousPosition[0] = leftHandPositionAction.action.ReadValue<Vector3>();
        previousPosition[1] = rightHandPositionAction.action.ReadValue<Vector3>();

        previousCameraRotation = mainCamera.rotation;
        targetPosition = transform.position; // �ʱ� ��ǥ ��ġ ����
    }

    void Update()
    {
        // ���� �� ��ġ ��������
        Vector3[] currentPosition = new Vector3[2] { leftHandPositionAction.action.ReadValue<Vector3>(),
                                                        rightHandPositionAction.action.ReadValue<Vector3>() };

        // ��Ʈ�ѷ� �̵� �Ÿ� ���
        float leftMoveDistance = (currentPosition[0] - previousPosition[0]).magnitude;
        float rightMoveDistance = (currentPosition[1] - previousPosition[1]).magnitude;

        // �̵��� �Ÿ��� ��� ���ϱ�
        float totalMoveDistance = (leftMoveDistance + rightMoveDistance) / 2f;

        // �̵� �Ÿ��� threshold(������ ���� ����)���� ũ�� speed ���
        if (totalMoveDistance > movementThreshold && totalMoveDistance < 0.4f)
        {
            currentSpeed = Mathf.Clamp(currentSpeed + acceleration * Time.deltaTime, 0f, maxSpeed); // ���ӵ� ����
        }
        else
        {
            currentSpeed = Mathf.Clamp(currentSpeed - deceleration * Time.deltaTime, 0f, maxSpeed); // ���ӵ� ����
        }

        // �ӵ��� 0�� �ƴ� ���� �̵�
        if (currentSpeed > 0)
        {
            // ī�޶��� ���� ����� ���� ������ �Բ� ����Ͽ� �̵� ���� ����
            Vector3 moveDirection = mainCamera.forward; // ī�޶��� ����
            moveDirection = moveDirection.normalized; // ����ȭ�Ͽ� ������ �ӵ��� �̵�

            // ��ǥ ��ġ ����
            targetPosition += moveDirection * currentSpeed * Time.deltaTime;
            Debug.Log(targetPosition);
        }

        // �ε巯�� �̵�
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        // ī�޶� ȸ�� ��ȭ�� ���� (��鸲 ����)
        Quaternion cameraRotationDelta = mainCamera.rotation * Quaternion.Inverse(previousCameraRotation);
        cameraRotationDelta.x = 0f;
        cameraRotationDelta.z = 0f;
        transform.rotation = cameraRotationDelta * transform.rotation;
        Debug.Log("ȸ��:"+transform.rotation);
        previousCameraRotation = mainCamera.rotation;

        // ���� ��ġ�� ���� ��ġ�� ����
        previousPosition[0] = currentPosition[0];
        previousPosition[1] = currentPosition[1];
    }
}