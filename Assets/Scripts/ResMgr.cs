using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


public class ResMgr
{
    private static ResMgr mInst;
    public static ResMgr Inst
    {
        get
        {
            if (mInst == null)
            {
                mInst = new ResMgr();
            }

            return mInst;
        }
    }


    /// <summary> 资源缓存容器 </summary>
    private Hashtable res;
    private Dictionary<string, AssetBundle> mAssetBundles;
    private ResMgr()
    {
        res = new Hashtable();
        mAssetBundles = new Dictionary<string, AssetBundle>();
    }

    
    /// <summary>
    /// Load 资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径</param>
    /// <param name="cacheAsset">是否要缓存资源</param>
    /// <returns></returns>
    public T Load<T>(string path, bool cache) where T : UnityEngine.Object
    {
        if (res.Contains(path))
        {
            return res[path] as T;
        }
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_MAC || UNITY_EDITOR
        //Debug.Log("ResMgr load in Resources path: " + path);
        //T assetObj = Resources.Load<T>(path);
        T assetObj = LoadAssetBundle<T>(path, cache);
#else
        T assetObj = LoadAssetBundle<T>(path, cache);
#endif
        if (assetObj == null)
        {
            Debug.LogWarning("Resources中找不到资源：" + path);
            return assetObj;
        }
        if (cache)
        {
            res.Add(path, assetObj);
            Debug.Log("Asset对象被缓存, Resource path=" + path);
        }

        return assetObj;
    }

    public T LoadAssetBundle<T>(string path, bool cache) where T : UnityEngine.Object
    {
        string loadFile = Config.AssetBundlePath + path;
        if (!File.Exists(loadFile))
        {
            T assetObj = Resources.Load<T>(path);
            return assetObj;
        }
        if (!File.Exists(Config.AssetBundleFile))
        {
            T assetObj = Resources.Load<T>(path);
            return assetObj;
        }

        AssetBundle mainfestBundle = AssetBundle.LoadFromFile(Config.AssetBundleFile);
        if (mainfestBundle != null)
        {
            AssetBundleManifest mainfest = (AssetBundleManifest)mainfestBundle.LoadAsset("AssetBundleManifest");
            string[] depends = mainfest.GetAllDependencies(path);
            AssetBundle[] dependsAssetBundle = new AssetBundle[depends.Length];

            for (int i = 0; i < depends.Length; i++)
            {
                string fileDepend = Config.AssetBundlePath + depends[i];
                dependsAssetBundle[i] = AssetBundle.LoadFromFile(fileDepend);
            }

            string[] paths = Regex.Split(path, "/", RegexOptions.IgnoreCase);
            string objName = paths[paths.Length - 1];
            path = Config.AssetBundlePath + path;
            AssetBundle bundle = AssetBundle.LoadFromFile(path);
            T assetObj = bundle.LoadAsset(objName) as T;
            return assetObj;
        }

        return null;
    }

    /// <summary>
    /// 创建Resource中GameObject对象
    /// </summary>
    /// <param name="path"资源路径</param>
    /// <param name="cache">是否缓存</param>
    /// <returns></returns>
    public GameObject CreateGO(string path, bool cache = false)
    {
        GameObject assetObj = Load<GameObject>(path, cache);
        GameObject go = GameObject.Instantiate(assetObj) as GameObject;
        if (go == null) { Debug.LogWarning("从Resource创建对象失败：" + path); }

        return go;
    }
}
