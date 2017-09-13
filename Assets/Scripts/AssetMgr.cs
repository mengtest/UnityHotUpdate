using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;


public class AssetMgr : Singleton<AssetMgr>
{
    private string _manifestName = "AssetBundleManifest";
    private string _bundleDirPath = Config.ResPath;
    AssetBundleManifest _bundleManifest = null;
    private Dictionary<string, AssetBundle> _bundleDict = new Dictionary<string, AssetBundle>();


    public AssetMgr()
    {
        AssetBundle _bundleMani = AssetBundle.LoadFromFile(Config.AssetBundleFile);
        if (_bundleMani != null)
        {
            _bundleManifest = _bundleMani.LoadAsset<AssetBundleManifest>(_manifestName);
            if (_bundleManifest == null)
            {
                Debug.LogError("Load Manifest: " + _manifestName + " error!");
            }
        }
        else
        {
            Debug.LogError("Load Manifest AssetBundle file: " + Config.AssetBundleFile + " error!");
        }
    }


    public AssetBundle LoadBundle(string bundleName)
    {
    	bundleName = bundleName.ToLower();

        if (!bundleName.EndsWith(Config.BundleExtension))
    	{
            bundleName += Config.BundleExtension;
    	}
        
    	LoadDependencies(bundleName);

    	AssetBundle bundle;
        if (!_bundleDict.TryGetValue(bundleName, out bundle))
    	{
    		string bundlePath = _bundleDirPath + bundleName;
    		bundle = AssetBundle.LoadFromFile(bundlePath);
            _bundleDict.Add(bundleName, bundle);
    	}
    	return bundle;
    }

    public T Load<T>(string bundleName) where T : UnityEngine.Object
    {
        if (_bundleManifest != null)
        {
            AssetBundle bundle = LoadBundle(bundleName);
            if (bundle != null)
            {
                string objName = Path2Objname(bundleName);
                return bundle.LoadAsset<T>(objName);
            }
            else
            {
                Debug.LogError("AssetBundle加载失败 bundleName: " + bundleName);
            }
        }

        return null;
    }


    private void LoadDependencies(string bundleName)
    {
        string[] depedencies = _bundleManifest.GetAllDependencies(bundleName);
    	foreach (string dep in depedencies)
    	{
    		LoadBundle(dep);
    	}
    }

    private string Path2Objname(string path)
    {
        string[] paths = Regex.Split(path, "/", RegexOptions.IgnoreCase);
        return paths[paths.Length - 1];
    }
}
