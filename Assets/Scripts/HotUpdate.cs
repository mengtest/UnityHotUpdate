using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;


public class VersionInfo
{
    public int ver_android_res = 1;
    public int ver_ios_res = 1;

    public int ver_android_lua = 1;
    public int ver_ios_lua = 1;

    public int ver_win_res = 2;
    public int ver_win_lua = 1;
}


public class HotUpdate : MonoBehaviour
{
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
        Config.RealInfoFile = File.Exists(Config.HotResInfoFile) ? Config.HotResInfoFile : Config.ResInfoFile;
    }

    IEnumerator DoUpdate()
    {
        if (!Directory.Exists(Config.AssetBundlePath)) Directory.CreateDirectory(Config.AssetBundlePath);
        GetResInfoFile();
        if (!File.Exists(Config.RealInfoFile))
        {
            Debug.LogError("resourcesinfo file:" + Config.RealInfoFile + " not exist");
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
            string resFile = Config.ApiUrl + Config.platform + "/res/r" + verLocal + ".zip";

            WWW wwwRes = new WWW(resFile);
            yield return wwwRes;
            if (!string.IsNullOrEmpty(wwwRes.error))
            {
                Debug.LogError("request " + resFile + " error: " + wwwRes.error);
                yield break;
            }

            string localResFile = Config.AssetBundlePath + "res.zip";
            File.WriteAllBytes(localResFile, wwwRes.bytes);

            UtilZip.UnZip(localResFile, Config.AssetBundlePath);
            File.Delete(localResFile);

            SetLocalVersion(verLocal);

            Config.RealInfoFile = Config.HotResInfoFile;
            StartCoroutine(DoUpdate(verLocal, verServer));
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
