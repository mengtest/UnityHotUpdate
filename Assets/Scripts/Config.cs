using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VersionInfo
{
    public int ver_android_res = 1;
    public int ver_android_lua = 1;

    public int ver_ios_res = 1;
    public int ver_ios_lua = 1;

    public int ver_win_res = 1;
    public int ver_win_lua = 1;
}

public enum Platform
{
    Android,
    iOS,
    Win,
}

public class Config
{
    public static string ApiUrl = "http://192.168.0.102:81/";
    public static string ApiVersion = ApiUrl + "version";

#if UNITY_ANDROID
    public static Platform platform = Platform.Android;
#elif UNITY_IOS
    public static Platform platform = Platform.iOS;
#else
    public static Platform platform = Platform.Win;
#endif

    public static string ResPath = Application.persistentDataPath + "/AssetBundle/";
    public static string AssetBundleFile = ResPath + "AssetBundle";

    public static string VerKeyRes = "ver_res";

    //更新任务数量, 暂时AssetBundle, 后面会增加lua
    public static int TaskUpdateNum = 1;
}
