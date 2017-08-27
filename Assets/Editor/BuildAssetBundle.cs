using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;


public class BuildAssetBundle
{
    [MenuItem("Game/Build AssetBundles(Android)")]
    private static void BuildAssetBundlesAndroid()
    {
        SetAssetBundleName();
        BuildPipeline.BuildAssetBundles(Application.dataPath + "/../AssetBundle/", BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.Android);
    }

    [MenuItem("Game/Build AssetBundles(iOS)")]
    private static void BuildAssetBundlesIos()
    {
        SetAssetBundleName();
        BuildPipeline.BuildAssetBundles(Application.dataPath + "/../AssetBundle", BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.iOS);
    }


    private static void SetAssetBundleName()
    {
        string fullPath = Application.dataPath + "/Resources/";
        var relativeLen = "Assets/Resources/".Length; // Assets 长度
        if (Directory.Exists(fullPath))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(fullPath);
            FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                var fileInfo = files[i];
                if (files[i].Name.EndsWith(".meta")) { continue; }

                var basePath = fileInfo.FullName.Substring(fullPath.Length - relativeLen).Replace('\\', '/');
                //Debug.Log("basePath: " + basePath);

                string extName = fileInfo.Extension;
                string assetBundleName = Regex.Split(basePath.Substring(relativeLen), extName, RegexOptions.IgnoreCase)[0];
                //Debug.Log("assetBundleName: " + assetBundleName);

                AssetImporter importer = AssetImporter.GetAtPath(basePath);
                importer.assetBundleName = assetBundleName;
            }
        }
        else
        {
            Debug.LogError(fullPath + " not exists!");
        }
    }
}
