using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMoveFish : MonoBehaviour
{
    public Transform tr;
    public float moveSpeed = 3.0f;
    public float rotationSpeed = 3.0f;
    public Vector3 targetPos;
    public float range = 10f;
    public LayerMask obstacleMask;

    void Start()
    {
        tr = transform;
        SetRandomTarget();
    }

    void Update()
    {
        MoveToTarget();
    }

    void MoveToTarget()
    {
        Vector3 movePos = targetPos - tr.position;

        if (movePos.magnitude < 0.5f)
        {
            SetRandomTarget();
            return;
        }

        // 장애물이 있으면 새로운 목표를 설정
        if (Physics.Raycast(tr.position, movePos.normalized, 1.5f, obstacleMask))
        {
            SetRandomTarget();
            return;
        }

        tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(movePos), Time.deltaTime * rotationSpeed);
        tr.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    void SetRandomTarget()
    {
        for (int i = 0; i < 10; i++) // 최대 10번 시도하여 장애물이 없는 위치 찾기
        {
            Vector3 newTarget = new Vector3(
                Random.Range(-range, range),
                tr.position.y, // y축 고정 (지형 고려)
                Random.Range(-range, range)
            );

            if (!Physics.Raycast(tr.position, (newTarget - tr.position).normalized, Vector3.Distance(tr.position, newTarget), obstacleMask))
            {
                targetPos = newTarget;
                return;
            }
        }
    }
}
