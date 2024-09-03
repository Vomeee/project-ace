using System.Collections.Generic;
using UnityEngine;

public class TargettingSystem : MonoBehaviour
{
    public Transform playerTransform;
    public Transform currentTarget;
    public float coneAngle = 150f;
    public float coneRadius = 300f;

    [SerializeField]
    private List<Transform> potentialTargetTransforms = new List<Transform>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) // ���� ��� Tab Ű�� Ÿ�� ��ȯ Ű�� ���
        {
            SwitchTarget();
        }

        if (currentTarget != null)
        {
            if (IsInCone(currentTarget))
            {
                LockOnTarget(currentTarget);
            }
            else
            {
                UnlockTarget(currentTarget);
            }
        }
    }

    private bool IsInCone(Transform target)
    {
        Vector3 directionToTarget = target.position - playerTransform.position;
        float distanceToTarget = directionToTarget.magnitude;

        // ���� ���� ���� �ִ��� Ȯ�� (Ÿ���� ������ ����)
        if (distanceToTarget > coneRadius)
            return false;

        // ���� ���� ���� �ִ��� Ȯ��
        float angleToTarget = Vector3.Angle(playerTransform.forward, directionToTarget);

        // ���� ������ ���� �̳��� �ִ��� Ȯ��
        return angleToTarget <= coneAngle;
    }

    private void LockOnTarget(Transform target)
    {
        EnemyAI enemy = target.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            enemy.OnLockedOn();
        }
    }

    private void UnlockTarget(Transform target)
    {
        EnemyAI enemy = target.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            enemy.OnLockedOff();
        }
    }

    public void AddTarget(Transform target)
    {
        if (!potentialTargetTransforms.Contains(target))
        {
            potentialTargetTransforms.Add(target);
            Debug.Log("Target added: " + target.name);
        }
    }

    public void RemoveTarget(Transform target)
    {
        if (potentialTargetTransforms.Contains(target))
        {
            potentialTargetTransforms.Remove(target);
            Debug.Log("Target removed: " + target.name);

            // Ÿ���� ���ŵǾ��� �� ���� Ÿ���� ��� ó��
            if (currentTarget == target)
            {
                UnlockTarget(target);
                SwitchTarget(); // �ٸ� Ÿ������ ��ȯ
            }
        }
    }

    public void SwitchTarget()
    {
        if (potentialTargetTransforms.Count == 0) return;

        Transform bestTarget = null;
        float bestDistance = Mathf.Infinity;
        bool isInConePriority = false;

        foreach (Transform target in potentialTargetTransforms)
        {
            // ���� Ÿ���� ��ŵ
            if (target == currentTarget)
                continue;

            bool isInCone = IsInCone(target);
            float distanceToTarget = Vector3.Distance(playerTransform.position, target.position);

            // ���� ���� �� Ÿ�� �켱 ����
            if (isInCone)
            {
                if (!isInConePriority || distanceToTarget < bestDistance)
                {
                    bestDistance = distanceToTarget;
                    bestTarget = target;
                    isInConePriority = true;
                }
            }
            else if (!isInConePriority && distanceToTarget < bestDistance)
            {
                // ���� ���� �� Ÿ�� ���� (���� ���� �� Ÿ���� ���� ���)
                bestDistance = distanceToTarget;
                bestTarget = target;
            }
        }

        // ���� Ÿ���� ���� ������Ʈ
        if (currentTarget != null)
        {
            EnemyAI previousTarget = currentTarget.GetComponent<EnemyAI>();
            if (previousTarget != null)
            {
                previousTarget.OnLockedOff();
                previousTarget.OnUntargeted();
            }
        }

        // ���� ������ Ÿ������ ��ȯ
        currentTarget = bestTarget;

        // ���ο� Ÿ���� ���� ������Ʈ
        if (currentTarget != null)
        {
            EnemyAI newTarget = currentTarget.GetComponent<EnemyAI>();
            if (newTarget != null)
            {
                newTarget.OnTargeted();
            }

            Debug.Log(newTarget.name);
        }
    }

}