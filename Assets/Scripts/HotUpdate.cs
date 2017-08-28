using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;


public class VersionInfo
{
    public int ver_android_res;
    public int ver_ios_res;
    public int ver_android_lua;
    public int ver_ios_lua;
    public int ver_win_res;
    public int ver_win_lua;
}


public class HotUpdate : MonoBehaviour
{
    private string resInfoFile;


    void Start()
    { 
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_MAC || UNITY_EDITOR
        //SceneManager.LoadScene("Main");
        StartCoroutine(DoUpdate());
#else
        StartCoroutine(DoUpdate());
#endif
    }



    private void GetResInfoFile()
    {
        if (File.Exists(Config.HotResInfoFile))
        {
            resInfoFile = Config.HotResInfoFile;
        }
        else
        {
            resInfoFile = Config.ResInfoFile;
        }
        Config.ResInfoFile = resInfoFile;
    }

    IEnumerator DoUpdate()
    {
        Debug.Log("AssetBundlePath: " + Config.AssetBundlePath);
        if (!Directory.Exists(Config.AssetBundlePath)) Directory.CreateDirectory(Config.AssetBundlePath);
        Debug.Log("AssetBundlePath: " + Config.AssetBundlePath);
        GetResInfoFile();
        if (!File.Exists(resInfoFile))
        {
            Debug.LogError("resourcesinfo file:" + resInfoFile + " not exist");
            yield break;
        }

        WWW www = new WWW(Config.ApiVersion);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log("Config.ApiVersion: " + Config.ApiVersion);
            Debug.LogError("request version error: " + www.error);
            yield break;
        }

        VersionInfo versionInfo = JsonUtility.FromJson<VersionInfo>(www.text);

        int verLocal = GetLocalVersion();
        int verServer = GetServerVersion(versionInfo);
        Debug.Log("verLocal: " + verLocal + " verServer: " + verServer);
        StartCoroutine(DoUpdate(verLocal, verServer));
    }

    IEnumerator DoUpdate(int verLocal, int verServer)
    {
        if (verLocal >= verServer)
        {
            SceneManager.LoadScene("Main");
        }
        else
        {
            verLocal++;
            string verInfoFile = Config.ApiUrl + "AssetBundle/" + Config.platform + "/v" + verLocal + "/resourcesinfo";
            WWW wwwResInfo = new WWW(verInfoFile);
            yield return wwwResInfo;
            if (!string.IsNullOrEmpty(wwwResInfo.error))
            {
                Debug.LogError("request " + verInfoFile + " error: " + wwwResInfo.error);
                yield break;
            }

            DoLocalResInfoSign();

            string[] fileLines = Regex.Split(wwwResInfo.text, "\n", RegexOptions.IgnoreCase);
            foreach (string fileLine in fileLines)
            {
                if (fileLine.Trim() == "") continue;
                string[] fileAndSign = Regex.Split(fileLine, ":", RegexOptions.IgnoreCase);
                string file = fileAndSign[0];

                bool needDown = false;

                if (mLocalResInfoSing.ContainsKey(file))
                {
                    string oldSign = mLocalResInfoSing[file];
                    string newSign = fileAndSign[1];
                    needDown = (oldSign == newSign) ? true : false;
                }
                else
                {
                    needDown = true;
                }
                if (needDown)
                {
                    string downFile = Config.ApiUrl + "AssetBundle/" + Config.platform + "/v" + verLocal + "/" + file;
                    Debug.Log("downFile: " + downFile);
                    WWW wwwDownFile = new WWW(downFile);
                    yield return wwwDownFile;
                    if (!string.IsNullOrEmpty(wwwDownFile.error))
                    {
                        Debug.LogError("request " + downFile + " error: " + wwwDownFile.error);
                        yield break;
                    }
                    Save2LocalFile(file, wwwDownFile.bytes, wwwDownFile.bytes.Length);
                }
            }
            //后续工作
            Save2LocalFile("resourcesinfo", wwwResInfo.bytes, wwwResInfo.bytes.Length);

            string verABFile = Config.ApiUrl + "AssetBundle/" + Config.platform + "/v" + verLocal + "/AssetBundle";
            WWW wwwAB = new WWW(verABFile);
            yield return wwwAB;
            if (!string.IsNullOrEmpty(wwwAB.error))
            {
                Debug.LogError("request " + verABFile + " error: " + wwwAB.error);
                yield break;
            }
            Save2LocalFile("AssetBundle", wwwAB.bytes, wwwAB.bytes.Length);
            SetLocalVersion(verLocal);

            StartCoroutine(DoUpdate(verLocal, verServer));
        }
    }

    private Dictionary<string, string> mLocalResInfoSing = new Dictionary<string, string>();
    private void DoLocalResInfoSign()
    { 
        using(FileStream fs = new FileStream(resInfoFile, FileMode.Open, FileAccess.Read))
        using (StreamReader sr = new StreamReader(fs))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] fileAndSign = Regex.Split(line, ":", RegexOptions.IgnoreCase);
                mLocalResInfoSing.Add(fileAndSign[0], fileAndSign[1]);
            }
        }
    }

    private int GetServerVersion(VersionInfo versionInfo)
    {
        if (Config.platform == Platform.Android)
        {
            return versionInfo.ver_android_res;
        }
        else if (Config.platform == Platform.iOS)
        {
            return versionInfo.ver_ios_res;
        }
        else
        {
            return versionInfo.ver_win_res;
        }
    }

    private int GetLocalVersion()
    {
        if (Config.platform == Platform.Android)
        {
            return PlayerPrefs.GetInt("ver_android_res", Config.VerInit);
        }
        else if (Config.platform == Platform.iOS)
        {
            return PlayerPrefs.GetInt("ver_ios_res", Config.VerInit);
        }
        else
        {
            return PlayerPrefs.GetInt("ver_win_res", Config.VerInit);
        }
    }

    private void SetLocalVersion(int version)
    {
        if (Config.platform == Platform.Android)
        {
            PlayerPrefs.SetInt("ver_android_res", version);
        }
        else if (Config.platform == Platform.iOS)
        {
            PlayerPrefs.SetInt("ver_ios_res", version);
        }
        else
        {
            PlayerPrefs.SetInt("ver_win_res", version);
        }
    }

    private void Save2LocalFile(string file, byte[] data, int dataLen)
    {
        string[] filePaths = Regex.Split(file, "/", RegexOptions.IgnoreCase);
        if (filePaths.Length > 1)
        {
            string[] filePathsTmp = new string[filePaths.Length - 1];
            System.Array.Copy(filePaths, 0, filePathsTmp, 0, filePaths.Length - 1);
            string filePath = string.Join("/", filePathsTmp);
            filePath = Config.AssetBundlePath + filePath;
            if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
        }

        file = Config.AssetBundlePath + file;

        FileInfo fileInfo = new FileInfo(file);
        if (fileInfo.Exists) fileInfo.Delete();

        Stream sw = fileInfo.Create();
        sw.Write(data, 0, dataLen);
        sw.Flush();
        sw.Close();
    }
}
