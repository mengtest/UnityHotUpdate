﻿using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class HotUpdate : MonoBehaviour
{
    private VersionInfo _verInfo = new VersionInfo();
    private int _taskUpdateNum = 0;

    public Text text;


    void Awake()
    {
        Screen.SetResolution(1920, 1080, true);
    }

    void Start()
    {
        _taskUpdateNum = Config.TaskUpdateNum;

        text.text = "当前平台：" + Config.platform;
        text.text += "\nConfig.ApiVersion：" + Config.ApiVersion;
        text.text += "\nConfig.ResPath：" + Config.ResPath;
        if (!Directory.Exists(Config.ResPath)) Directory.CreateDirectory(Config.ResPath);
    }

    void Update()
    {
        if (_taskUpdateNum <= 0)
        {
            SceneManager.LoadScene("Main");
        }
    }


    public void OnButtonClick()
    {
        Debug.Log("点击了按钮");
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_MAC || UNITY_EDITOR
        SceneManager.LoadScene("Main");
#else
        StartCoroutine(DoResUpdate());
#endif
    }


    IEnumerator DoResUpdate()
    {
        WWW www = new WWW(Config.ApiVersion);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            text.text += "\nrequest version error：" + www.error;
            Debug.Log("Config.ApiVersion: " + Config.ApiVersion);
            Debug.LogError("request version error: " + www.error);
            yield break;
        }

        VersionInfo versionInfo = JsonUtility.FromJson<VersionInfo>(www.text);

        int verResLocal = GetLocalResVersion();
        int verResServer = GetServerResVersion(versionInfo);
        text.text += "\nverResLocal: " + verResLocal + " verResServer: " + verResServer;
        Debug.Log("verResLocal: " + verResLocal + " verResServer: " + verResServer);
        StartCoroutine(DoResUpdate(verResLocal, verResServer));
    }

    IEnumerator DoResUpdate(int verLocal, int verServer)
    {
        text.text += "\nverResLocal: " + verLocal + " verResServer: " + verServer + " _taskUpdateNum: " + _taskUpdateNum;
        if (verLocal >= verServer)
        {
            _taskUpdateNum--;
        }
        else
        {
            verLocal++;
            string resFile = Config.ApiUrl + Config.platform + "/res/r" + verLocal + ".zip";
            text.text += "\nresFile: " + resFile;

            WWW wwwRes = new WWW(resFile);
            yield return wwwRes;
            if (!string.IsNullOrEmpty(wwwRes.error))
            {
                text.text += "\nrequest " + resFile + " error: " + wwwRes.error;
                Debug.LogError("request " + resFile + " error: " + wwwRes.error);
                yield break;
            }

            string localResFile = Config.ResPath + "res.zip";
            text.text += "\nlocalResFile: " + localResFile;
            File.WriteAllBytes(localResFile, wwwRes.bytes);
            text.text += "\nlocalResFile: " + localResFile;

            UtilZip.UnZip(localResFile, Config.ResPath);
            text.text += "\nlocalResFile: " + localResFile;
            File.Delete(localResFile);
            text.text += "\nlocalResFile: " + localResFile;
            SetLocalResVersion(verLocal);
            text.text += "\nlocalResFile: " + localResFile;
            StartCoroutine(DoResUpdate(verLocal, verServer));
        }
    }

    private int GetServerResVersion(VersionInfo versionInfo)
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

    private int GetLocalResVersion()
    {
        if (Config.platform == Platform.Android)
        {
            return PlayerPrefs.GetInt(Config.VerKeyRes, _verInfo.ver_android_res);
        }
        else if (Config.platform == Platform.iOS)
        {
            return PlayerPrefs.GetInt(Config.VerKeyRes, _verInfo.ver_ios_res);
        }
        else
        {
            return PlayerPrefs.GetInt(Config.VerKeyRes, _verInfo.ver_win_res);
        }
    }

    private void SetLocalResVersion(int ver)
    {
        PlayerPrefs.SetInt(Config.VerKeyRes, ver);
    }
}
