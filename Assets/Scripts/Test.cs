using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class Test : MonoBehaviour
{
    public Text textLua;


    void Start()
    {
        Debug.Log("Config.platform: " + Config.platform);
        GameObject go = ResMgr.Inst.CreateGO("Test/Cube");
        go.transform.position = new Vector3(0, 0, 0);

        textLua.text = UtilLua.FileContent("Main");
    }
}
