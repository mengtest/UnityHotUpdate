using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Platform
{ 
    None,
    Android,
    iOS,
}

public class Config
{
    public static string ApiUrl = "http://192.168.0.104:81/";

    public static int VerInit = 1;
    public static int VerAndroidRes = 1;
    public static int VerIosRes = 1;
    public static int VerAndroidLua = 1;
    public static int VerIosLua = 1;

    public static string ResInfoFile = Application.dataPath + "/Resources/resourcesinfo";
#if UNITY_ANDROID
    public static Platform platform = Platform.Android;
    public static string AssetBundlePath = "jar:file://" + Application.persistentDataPath + "/../AssetBundle/";
    public static string AssetBundleFile = AssetBundlePath + "AssetBundle";
    public static string HotResInfoFile = AssetBundlePath + "resourcesinfo";
#elif UNITY_IOS
    public static Platform platform = Platform.iOS;
    public static string AssetBundlePath = Application.persistentDataPath + "/../AssetBundle/";
    public static string AssetBundleFile = AssetBundlePath + "AssetBundle";
    public static string HotResInfoFile = AssetBundlePath + "resourcesinfo";
#else
    public static Platform platform = Platform.None;
    public static string AssetBundlePath = "";
    public static string AssetBundleFile = "";
    public static string HotResInfoFile = "";
#endif

    public static string ApiVersion = ApiUrl + platform + "/version";
}
