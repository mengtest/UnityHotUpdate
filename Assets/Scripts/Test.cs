using System.Collections;
using UnityEngine;


public class Test : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Config.platform: " + Config.platform);
        GameObject go = ResMgr.Inst.CreateGO("Cube3");
        go.transform.position = new Vector3(0, 0, 0);
    }
}
