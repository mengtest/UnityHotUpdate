using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;


public class BuildHot
{
    private static string _verInfoFile = Config.WwwRootPath + "version";

    private static readonly VersionInfo _versionInfo = new VersionInfo();

    private static Platform _platform;
    private static int _verRes;
    private static string _pathPlatVer;


    [MenuItem("Game/AssetBundle/Build(Android)")]
    private static void BuildAssetBundlesAndroid()
    {
        _platform = Platform.Android;
        _verRes = _versionInfo.ver_res_android;
        BuildAssetBundles(BuildTarget.Android);
    }

    [MenuItem("Game/AssetBundle/Build(iOS)")]
    private static void BuildAssetBundlesIos()
    {
        _platform = Platform.iOS;
        _verRes = _versionInfo.ver_res_ios;
        BuildAssetBundles(BuildTarget.iOS);
    }

    [MenuItem("Game/AssetBundle/Build(Win)")]
    private static void BuildAssetBundlesWin()
    {
        _platform = Platform.Win;
        _verRes = _versionInfo.ver_res_win;
        BuildAssetBundles(BuildTarget.StandaloneWindows);
    }

    [MenuItem("Game/AssetBundle/Update(Android)")]
    private static void UpdateAssetBundlesAndroid()
    {
        _platform = Platform.Android;
        _verRes = _versionInfo.ver_res_android;
        UpdateAssetBundles();
    }

    [MenuItem("Game/AssetBundle/Update(iOS)")]
    private static void UpdateAssetBundlesIos()
    {
        _platform = Platform.iOS;
        _verRes = _versionInfo.ver_res_ios;
        UpdateAssetBundles();
    }

    [MenuItem("Game/AssetBundle/Update(Win)")]
    private static void UpdateAssetBundlesWin()
    {
        _platform = Platform.Win;
        _verRes = _versionInfo.ver_res_win;
        UpdateAssetBundles();
    }


    private static void BuildAssetBundles(BuildTarget buildTarget)
    {
        _pathPlatVer = VerPath(_verRes);
        UtilIO.CreateDir(_pathPlatVer);

        CopyLua();
        AssetDatabase.Refresh();
        ClearAssetBundleNames();
        SetAssetBundleName();
        AssetBundleManifest mainfest = BuildPipeline.BuildAssetBundles(_pathPlatVer, BuildAssetBundleOptions.UncompressedAssetBundle, buildTarget);
        BuildAssetBundleAfter(mainfest);
    }

    private static void UpdateAssetBundles()
    {
        _pathPlatVer = VerPath(_verRes);
        UtilIO.CreateDir(Config.WwwRootPath);

        ZipRes();
        WriteVerInfo();
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
                importer.assetBundleName = assetBundleName + Config.BundleExtension;
            }
        }
        else
        {
            Debug.LogError(fullPath + " not exists!");
        }
    }

    private static void BuildAssetBundleAfter(AssetBundleManifest mainfest)
    {
        string fileResInfo = _pathPlatVer + "resourcesinfo";

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
    }

    private static void ZipRes()
    {
        if (_verRes <= 1) { return; }

        string pathPlatVerLast = VerPath(_verRes - 1);
        string zipPath = Config.WwwRootPath + "res/" + _platform + "/";
        string zipFile = zipPath + _verRes + ".zip";

        List<string> listResInfoDiff = GetFileInfoDiff(_pathPlatVer + "resourcesinfo", pathPlatVerLast + "resourcesinfo");

        Dictionary<string, string> dictZipFile = new Dictionary<string, string>();
        dictZipFile.Add(_pathPlatVer + "v" + _verRes, "AssetBundle");
        foreach (string diffFile in listResInfoDiff)
        {
            string diffFile2 = _pathPlatVer + diffFile;
            dictZipFile.Add(diffFile2, diffFile);
        }

        UtilIO.CreateDir(zipPath);
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

    private static void CopyLua()
    {
        UtilIO.CopyDir(Config.LuaPath, Config.LuaPathRes, ".txt");
    }

    private static void ClearAssetBundleNames()
    {
        foreach (string bundleName in AssetDatabase.GetAllAssetBundleNames())
        {
            AssetDatabase.RemoveAssetBundleName(bundleName, true);
        }
    }

    private static string VerPath(int ver)
    {
        return Config.HotRootPath + "res/" + _platform + "/v" + ver + "/";
    }
}
