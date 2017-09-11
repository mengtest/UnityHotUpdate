using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;


public class BuildAssetBundle
{
    private static string _pathAssetBundle = Application.dataPath + "/../hotroot/";
    private static string _pathWwwRoot = Application.dataPath + "/../wwwroot/";
    private static string _verInfoFile = _pathWwwRoot + "version";
    private static readonly VersionInfo _versionInfo = new VersionInfo();
    private static string _platform;
    private static int _ver;
    private static string _pathPlatVerAB;


    [MenuItem("Game/Build AssetBundles(Android)")]
    private static void BuildAssetBundlesAndroid()
    {
        _platform = "Android";
        _ver = _versionInfo.ver_android_res;
        BuildAssetBundles(BuildTarget.Android);
    }

    [MenuItem("Game/Build AssetBundles(iOS)")]
    private static void BuildAssetBundlesIos()
    {
        _platform = "iOS";
        _ver = _versionInfo.ver_ios_res;
        BuildAssetBundles(BuildTarget.iOS);
    }

    [MenuItem("Game/Build AssetBundles(Win)")]
    private static void BuildAssetBundlesWin()
    {
        _platform = "Win";
        _ver = _versionInfo.ver_win_res;
        BuildAssetBundles(BuildTarget.StandaloneWindows);
    }


    private static void BuildAssetBundles(BuildTarget buildTarget)
    {
        _pathPlatVerAB = _pathAssetBundle + _platform + "/res/v" + _ver + "/";

        ClearAssetBundleNames();
        CreateAssetBundlePath();
        SetAssetBundleName();
        AssetBundleManifest mainfest = BuildPipeline.BuildAssetBundles(_pathPlatVerAB, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.iOS);
        BuildAssetBundleAfter(mainfest);
        CheckDiffFile();
        SetVerInfo();
    }

    private static void CreateAssetBundlePath()
    {
        if (!Directory.Exists(_pathPlatVerAB))
        {
            Directory.CreateDirectory(_pathPlatVerAB);
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
        string fileResInfo = _pathPlatVerAB + "resourcesinfo";
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

    private static void CheckDiffFile()
    {
        if (_ver <= 1)
        {
            return;
        }

        int verLast = _ver - 1;
        string pathPlatVerABLast = _pathAssetBundle + _platform + "/res/v" + verLast + "/";
        string zipPath = _pathWwwRoot + _platform + "/res/";
        string zipFile = zipPath + "r" + _ver + ".zip";

        List<string> listResInfoDiff = GetResInfoDiff(_pathPlatVerAB + "resourcesinfo", pathPlatVerABLast + "resourcesinfo");

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

    private static List<string> GetResInfoDiff(string resInfo, string resInfoLast)
    {
        Dictionary<string, string> dictResInfo = GetResInfoDict(resInfo);
        Dictionary<string, string> dictResInfoLast = GetResInfoDict(resInfoLast);

        List<string> listResInfoDiff = new List<string>();
        foreach (var kv in dictResInfo)
        {
            string k = kv.Key;
            string v = kv.Value;
            if (!dictResInfoLast.ContainsKey(k) || dictResInfoLast[k] != v)
            {
                listResInfoDiff.Add(k);
            }
        }
        return listResInfoDiff;
    }

    private static Dictionary<string, string> GetResInfoDict(string file)
    {
        Dictionary<string, string> dictResInfo = new Dictionary<string, string>();

        using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
        using (StreamReader sr = new StreamReader(fs))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] fileAndSign = Regex.Split(line, ":", RegexOptions.IgnoreCase);
                dictResInfo.Add(fileAndSign[0], fileAndSign[1]);
            }
        }
        return dictResInfo;
    }

    private static void SetVerInfo()
    {
        string verStr = JsonUtility.ToJson(_versionInfo);
        File.WriteAllBytes(_verInfoFile, System.Text.Encoding.Default.GetBytes(verStr));
    }

    private static void ClearAssetBundleNames()
    {
        /*
        int length = AssetDatabase.GetAllAssetBundleNames().Length;
        string[] oldAssetBundleNames = new string[length];
        for (int i = 0; i < length; i++)
        {
            oldAssetBundleNames[i] = AssetDatabase.GetAllAssetBundleNames()[i];
        }
         * */
        foreach (string bundleName in AssetDatabase.GetAllAssetBundleNames())
        {
            AssetDatabase.RemoveAssetBundleName(bundleName, true);
        }
    }
}
