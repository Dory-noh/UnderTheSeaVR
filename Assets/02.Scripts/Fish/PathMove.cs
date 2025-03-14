using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMove: MonoBehaviour
{
    [SerializeField ] List<Transform> PathList = new List<Transform>();
    public int currentPath = 0;
    Transform tr;
    // Start is called before the first frame update
    void Start()
    {
        tr = transform;
        var ptr = GameObject.Find("AiPath").transform;
        if (ptr != null)
            ptr.GetComponentsInChildren<Transform>(PathList);
        PathList.RemoveAt(0);
    }

    // Update is called once per frame
    void Update()
    {
        WayPointMove();
        CheckDist();
    }
    void WayPointMove() 
    { 
        Vector3 movePos = PathList[currentPath].position-tr.position;
        tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(movePos), Time.deltaTime * 0.5f);
        tr.Translate(Vector3.forward*10.0f*Time.deltaTime);
    }
    void CheckDist() 
    {
        if (Vector3.Distance(transform.position, PathList[currentPath].position) < 4.5f)
        {
            if (currentPath == PathList.Count - 1)
                currentPath = 0;
            else
                currentPath++;
        }
    }
}
