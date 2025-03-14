using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPath : MonoBehaviour
{
    public Color lineCor;
    public List<Transform> nodeList;
    private void Start()
    {

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = lineCor;
        Transform[] PathTr = GetComponentsInChildren<Transform>();
        nodeList = new List<Transform>();
        for (int i = 0; i < PathTr.Length; i++)
        {
            if (PathTr[i] != this.transform)
            {
                nodeList.Add(PathTr[i]);
            }
        }
        for (int i = 0; i < nodeList.Count; i++) 
        { 
            Vector3 currentNode = nodeList[i].position;
            Vector3 previousNode = Vector3.zero;
            if (i > 0)
            {
                previousNode = nodeList[i - 1].position;
            }
            else if (i == 0 && nodeList.Count > 1) 
            {
                previousNode = nodeList[nodeList.Count - 1].position;
            }
            Gizmos.DrawLine(previousNode, currentNode);
            Gizmos.DrawSphere(currentNode, 0.25f);
        }

    }
}
