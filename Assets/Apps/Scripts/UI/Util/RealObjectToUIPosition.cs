using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RealObjectToUIPosition : MonoBehaviour
{
    [SerializeField] string TagName;
    [SerializeField] string TargetName;
    GameObject target;

    private void Start()
    {
        FindTarget();
        SnapToTarget();
    }

    public void FindTarget()
    {
        var goes = GameObject.FindGameObjectsWithTag(TagName).ToArray();
        target = goes.Where(x => x.name == TargetName).FirstOrDefault();
    }


    public void SnapToTarget()
    {
        if (target == default) return;
        var point = this.transform.parent.InverseTransformPoint(target.transform.position);
        point.z = 0;
        this.transform.localPosition = point;
    }
}
