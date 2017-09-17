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
        text.text += "\n当前资源版本：" + GetLocalResVersion();
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
#if UNITY_EDITOR
        SceneManager.LoadScene("Main");
#else
        StartCoroutine(DoUpdate());
#endif
    }


    IEnumerator DoUpdate()
    {
        WWW www = new WWW(Config.ApiVersion);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError("request version error: " + www.error);
            yield break;
        }

        VersionInfo versionInfo = JsonUtility.FromJson<VersionInfo>(www.text);

        int verResLocal = GetLocalResVersion();
        int verResServer = GetServerResVersion(versionInfo);
        Debug.Log("verResLocal: " + verResLocal + " verResServer: " + verResServer);

        StartCoroutine(DoResUpdate(verResLocal, verResServer));
    }

    IEnumerator DoResUpdate(int verResLocal, int verResServer)
    {
        if (verResLocal >= verResServer)
        {
            _taskUpdateNum--;
        }
        else
        {
            verResLocal++;
            string resFile = Config.ApiUrl + "res/" + Config.platform + "/" + verResLocal + ".zip";

            WWW wwwRes = new WWW(resFile);
            yield return wwwRes;
            if (!string.IsNullOrEmpty(wwwRes.error))
            {
                Debug.LogError("request " + resFile + " error: " + wwwRes.error);
                yield break;
            }

            string localResFile = Config.ResPath + "res.zip";
            File.WriteAllBytes(localResFile, wwwRes.bytes);

            UtilZip.UnZip(localResFile, Config.ResPath);
            
            File.Delete(localResFile);
            SetLocalResVersion(verResLocal);
            StartCoroutine(DoResUpdate(verResLocal, verResServer));
        }
    }

    private int GetServerResVersion(VersionInfo versionInfo)
    {
        if (Config.platform == Platform.Android)
        {
            return versionInfo.ver_res_android;
        }
        else if (Config.platform == Platform.iOS)
        {
            return versionInfo.ver_res_ios;
        }
        else
        {
            return versionInfo.ver_res_win;
        }
    }

    private int GetLocalResVersion()
    {
        if (Config.platform == Platform.Android)
        {
            return PlayerPrefs.GetInt(Config.VerKeyRes, _verInfo.ver_res_android);
        }
        else if (Config.platform == Platform.iOS)
        {
            return PlayerPrefs.GetInt(Config.VerKeyRes, _verInfo.ver_res_ios);
        }
        else
        {
            return PlayerPrefs.GetInt(Config.VerKeyRes, _verInfo.ver_res_win);
        }
    }

    private void SetLocalResVersion(int ver)
    {
        PlayerPrefs.SetInt(Config.VerKeyRes, ver);
    }
}
