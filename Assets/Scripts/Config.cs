using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VersionInfo
{
    public int ver_res_android = 1;
    public int ver_res_ios = 1;
    public int ver_res_win = 1;
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

    public static string BundleExtension = ".unity";

#if UNITY_ANDROID
    public static Platform platform = Platform.Android;
#elif UNITY_IOS
    public static Platform platform = Platform.iOS;
#else
    public static Platform platform = Platform.Win;
#endif

    public static string HotRootPath = Path.GetFullPath(Application.dataPath + "/../hotroot/");
    public static string WwwRootPath = Path.GetFullPath(Application.dataPath + "/../wwwroot/");

    public static string ResPath = Application.persistentDataPath + "/AssetBundle/";
    public static string AssetBundleFile = ResPath + "AssetBundle";

    public static string LuaPath = Path.GetFullPath(Application.dataPath + "/../Lua/");
    public static string LuaPathRes = Application.dataPath + "/Resources/Lua/";

    public static string VerKeyRes = "ver_res";

    //更新任务数量AssetBundle
    public static int TaskUpdateNum = 1;
}
