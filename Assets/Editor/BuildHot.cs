using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;


public class BuildHot
{
    private static string _pathHotRoot = Application.dataPath + "/../hotroot/";
    private static string _pathWwwRoot = Application.dataPath + "/../wwwroot/";
    private static string _verInfoFile = _pathWwwRoot + "version";

    private static readonly VersionInfo _versionInfo = new VersionInfo();

    private static string _platform;
    private static int _ver;
    private static string _pathPlatVerAB;


    [MenuItem("Game/AssetBundle/Build(Android)")]
    private static void BuildAssetBundlesAndroid()
    {
        _platform = "Android";
        _ver = _versionInfo.ver_android_res;
        BuildAssetBundles(BuildTarget.Android);
    }

    [MenuItem("Game/AssetBundle/Build(iOS)")]
    private static void BuildAssetBundlesIos()
    {
        _platform = "iOS";
        _ver = _versionInfo.ver_ios_res;
        BuildAssetBundles(BuildTarget.iOS);
    }

    [MenuItem("Game/AssetBundle/Build(Win)")]
    private static void BuildAssetBundlesWin()
    {
        _platform = "Win";
        _ver = _versionInfo.ver_win_res;
        BuildAssetBundles(BuildTarget.StandaloneWindows);
    }

    [MenuItem("Game/AssetBundle/Update(Android)")]
    private static void UpdateAssetBundlesAndroid()
    {
        _platform = "Android";
        _ver = _versionInfo.ver_android_res;
        UpdateAssetBundles();
    }

    [MenuItem("Game/AssetBundle/Update(iOS)")]
    private static void UpdateAssetBundlesIos()
    {
        _platform = "iOS";
        _ver = _versionInfo.ver_ios_res;
        UpdateAssetBundles();
    }

    [MenuItem("Game/AssetBundle/Update(Win)")]
    private static void UpdateAssetBundlesWin()
    {
        _platform = "Win";
        _ver = _versionInfo.ver_win_res;
        UpdateAssetBundles();
    }


    private static void BuildAssetBundles(BuildTarget buildTarget)
    {
        _pathPlatVerAB = _pathHotRoot + _platform + "/res/v" + _ver + "/";
        CreatePath(_pathPlatVerAB);

        ClearAssetBundleNames();
        SetAssetBundleName();
        AssetBundleManifest mainfest = BuildPipeline.BuildAssetBundles(_pathPlatVerAB, BuildAssetBundleOptions.UncompressedAssetBundle, buildTarget);
        BuildAssetBundleAfter(mainfest);
    }

    private static void UpdateAssetBundles()
    {
        _pathPlatVerAB = _pathHotRoot + _platform + "/res/v" + _ver + "/";
        CreatePath(_pathWwwRoot);

        ZipRes();
        WriteVerInfo();
    }


    private static void CreatePath(string path)
    {
        if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
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
        string fileResInfo = _pathPlatVerAB + "resourcesinfo";
        string fileResInfo2 = Application.dataPath + "/Resources/resourcesinfo";

        using (FileStream fs = new FileStream(fileResInfo, FileMode.Create))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
                foreach (string bundleName in bundleNames)
                {
                    //Debug.Log("bundleName: " + bundleName);
                    string cont = bundleName + ":" + mainfest.GetAssetBundleHash(bundleName).ToString();
                    sw.WriteLine(cont);
                }
            }
        }
        File.Copy(fileResInfo, fileResInfo2, true);
    }

    private static void ZipRes()
    {
        if (_ver <= 1) { return; }

        int verLast = _ver - 1;
        string pathPlatVerABLast = _pathHotRoot + _platform + "/res/v" + verLast + "/";
        string zipPath = _pathWwwRoot + _platform + "/res/";
        string zipFile = zipPath + "r" + _ver + ".zip";

        List<string> listResInfoDiff = GetFileInfoDiff(_pathPlatVerAB + "resourcesinfo", pathPlatVerABLast + "resourcesinfo");

        Dictionary<string, string> dictZipFile = new Dictionary<string, string>();
        dictZipFile.Add(_pathPlatVerAB + "v" + _ver, "AssetBundle");
        dictZipFile.Add(_pathPlatVerAB + "resourcesinfo", "resourcesinfo");
        foreach (string diffFile in listResInfoDiff)
        {
            string diffFile2 = _pathPlatVerAB + diffFile;
            dictZipFile.Add(diffFile2, diffFile);
        }

        if (!Directory.Exists(zipPath))
        {
            Directory.CreateDirectory(zipPath);
        }
        UtilZip.Zip(dictZipFile, zipFile);
    }

    private static List<string> GetFileInfoDiff(string fileInfo, string fileInfoLast)
    {
        Dictionary<string, string> dictFileInfo = GetFileInfoDict(fileInfo);
        Dictionary<string, string> dictFileInfoLast = GetFileInfoDict(fileInfoLast);

        List<string> listFileInfoDiff = new List<string>();
        foreach (var kv in dictFileInfo)
        {
            string k = kv.Key;
            string v = kv.Value;
            if (!dictFileInfoLast.ContainsKey(k) || dictFileInfoLast[k] != v)
            {
                listFileInfoDiff.Add(k);
            }
        }
        return listFileInfoDiff;
    }

    private static Dictionary<string, string> GetFileInfoDict(string file)
    {
        Dictionary<string, string> dictFileInfo = new Dictionary<string, string>();

        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] fileAndSign = Regex.Split(line, ":", RegexOptions.IgnoreCase);
                    dictFileInfo.Add(fileAndSign[0], fileAndSign[1]);
                }
            }
        }
        return dictFileInfo;
    }

    private static void WriteVerInfo()
    {
        string verInfoStr = JsonUtility.ToJson(_versionInfo);
        File.WriteAllBytes(_verInfoFile, System.Text.Encoding.Default.GetBytes(verInfoStr));
    }

    private static void ClearAssetBundleNames()
    {
        foreach (string bundleName in AssetDatabase.GetAllAssetBundleNames())
        {
            AssetDatabase.RemoveAssetBundleName(bundleName, true);
        }
    }
}
