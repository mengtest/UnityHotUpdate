using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;


public class ResMgr : Singleton<ResMgr>
{
    private Hashtable _resHash = null;
    public ResMgr()
    {
        _resHash = new Hashtable();
    }

    
    public T Load<T>(string path, bool cache) where T : UnityEngine.Object
    {
        if (_resHash.Contains(path))
        {
            return _resHash[path] as T;
        }

        T obj = null;

        string loadFile = Config.AssetBundlePath + path;
        if (!File.Exists(loadFile))
        {
            obj = Resources.Load<T>(path);
        }
        else
        {
            Debug.Log("Load \"" + path + "\" from AssetBundle");
            obj = AssetMgr.Inst.Load<T>(path);
        }

        if (obj == null)
        {
            Debug.LogError("Resources中找不到资源：" + path);
            return obj;
        }
        if (cache)
        {
            _resHash.Add(path, obj);
            Debug.Log("Asset对象被缓存, Resource path=" + path);
        }

        return obj;
    }

    public GameObject CreateGO(string path, bool cache = false)
    {
        GameObject assetObj = Load<GameObject>(path, cache);
        GameObject go = GameObject.Instantiate(assetObj) as GameObject;
        if (go == null)
        {
            Debug.LogWarning("从Resource创建对象失败：" + path);
        }

        return go;
    }
}
