using System.Collections;
using UnityEngine;


public class Test : MonoBehaviour
{
    void Start()
    {
        GameObject go = ResMgr.Inst.CreateGO("B/Capsule");
        go.transform.position = new Vector3(0, 0, 0);
    }
}
