using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;


public class BuildAssetBundle
{
    private static readonly string pathAssetBundle = Application.dataPath + "/../AssetBundle/";


    [MenuItem("Game/Build AssetBundles(Android)")]
    private static void BuildAssetBundlesAndroid()
    {
        BuildAssetBundles(BuildTarget.Android);
    }

    [MenuItem("Game/Build AssetBundles(iOS)")]
    private static void BuildAssetBundlesIos()
    {
        BuildAssetBundles(BuildTarget.iOS);
    }


    private static void BuildAssetBundles(BuildTarget buildTarget)
    {
        CreateAssetBundlePath();
        SetAssetBundleName();
        AssetBundleManifest mainfest = BuildPipeline.BuildAssetBundles(Application.dataPath + "/../AssetBundle", BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.iOS);
        BuildAssetBundleAfter(mainfest);
    }

    private static void CreateAssetBundlePath()
    {
        if (!Directory.Exists(pathAssetBundle))
        {
            Directory.CreateDirectory(pathAssetBundle);
        }
    }

    private static void SetAssetBundleName()
    {
        string fullPath = Application.dataPath + "/Resources/";
        var relativeLen = "Assets/Resources/".Length;
        string fileResInfo = Application.dataPath + "/Resources/resourcesinfo";
        if (Directory.Exists(fullPath))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(fullPath);
            FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                var fileInfo = files[i];
                if (fileInfo.Name.EndsWith(".meta") || fileInfo.Name == fileResInfo) { continue; }

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

    private static void BuildAssetBundleAfter(AssetBundleManifest mainfest)
    {
        string fileResInfo = Application.dataPath + "/../AssetBundle/resourcesinfo";
        string fileResInfo2 = Application.dataPath + "/Resources/resourcesinfo";

        FileStream fs = new FileStream(fileResInfo, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);

        string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
        foreach (string bundleName in bundleNames)
        {
            //Debug.Log("bundleName: " + bundleName);
            string cont = bundleName + ":" + mainfest.GetAssetBundleHash(bundleName).ToString();
            sw.WriteLine(cont);
        }

        sw.Flush();
        sw.Close();
        fs.Close();

        File.Copy(fileResInfo, fileResInfo2, true);
    }
}
