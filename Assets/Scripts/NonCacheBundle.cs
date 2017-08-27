using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;


public class NonCacheBundle : MonoBehaviour
{
    private string BundleURL;
    private string AssetName = "Cube";


    void Awake()
    {
#if UNITY_ANDROID
        BundleURL = "jar:file://" + Application.persistentDataPath + "/../AssetBundle/A/Cube";
#elif UNITY_IPHONE
        BundleURL = Application.persistentDataPath + "/../AssetBundle/A/Cube";
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_MAC || UNITY_EDITOR
        BundleURL = "file://" + Application.dataPath + "/../AssetBundle/A/Cube";
#else
        BundleURL = string.Empty;
#endif
        Debug.Log("Application.persistentDataPath: " + Application.persistentDataPath);
    }


    IEnumerator Start()
    {
        WWW www = new WWW(BundleURL);
        yield return www;
        if (www.error != null)
        {
            throw new Exception("WWW download had an error: " + www.error);
        }
        AssetBundle bundle = www.assetBundle;
        GameObject cube = Instantiate(bundle.LoadAsset(AssetName)) as GameObject;
        cube.transform.position = new Vector3(0, 0, 0);
        bundle.Unload(false);
        www.Dispose();
    }
}
