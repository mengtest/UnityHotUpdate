using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AssetMgr : Singleton<AssetMgr>
{
    //private string _bundleExtension = ".unity";
    private string _manifestName = "AssetBundleManifest";
    private string _bundleDirPath = Config.AssetBundlePath;
    AssetBundleManifest _bundleManifest = null;
    private Dictionary<string, AssetBundle> _bundleDict = new Dictionary<string, AssetBundle>();


    public AssetMgr()
    {
        AssetBundle _bundleMani = AssetBundle.LoadFromFile(Config.AssetBundleFile);
        if (_bundleManifest != null)
        {
            _bundleManifest = _bundleMani.LoadAsset<AssetBundleManifest>(_manifestName);
            if (_bundleManifest == null)
            {
                Debug.Log("Load Manifest: " + _manifestName + " error!");
            }
        }
        else
        {
            Debug.Log("Load Manifest AssetBundle file: " + Config.AssetBundleFile + " error!");
        }
    }


    public AssetBundle LoadBundle(string bundleName)
    {
    	bundleName = bundleName.ToLower();
        /*
    	if (!bundleName.EndsWith(bundleExtension))
    	{
    		bundleName += bundleExtension;
    	}
         * */
    	LoadDependencies(bundleName);

    	AssetBundle bundle;
        if (!_bundleDict.TryGetValue(bundleName, out bundle))
    	{
    		Debug.Log("Load bundle from file: " + bundleName);
    		string bundlePath = _bundleDirPath + bundleName;
    		bundle = AssetBundle.LoadFromFile(bundlePath);
            _bundleDict.Add(bundleName, bundle);
    	}
    	return bundle;
    }


    protected void LoadDependencies(string bundleName)
    {
        string[] depedencies = _bundleManifest.GetAllDependencies(bundleName);
    	foreach (string dep in depedencies)
    	{
    		LoadBundle(dep);
    	}
    }
}
