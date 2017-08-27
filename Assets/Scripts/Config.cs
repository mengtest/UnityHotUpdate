using System.Collections;
using System.Collections.Generic;


public class Config
{
    public static int VerAndroidRes = 1;
    public static int VerIosRes = 1;
    public static int VerAndroidLua = 1;
    public static int VerIosLua = 1;

#if UNITY_ANDROID
    public static string AssetBundlePath = "jar:file://" + Application.persistentDataPath + "/../AssetBundle/";
    public static string AssetBundleFile = AssetBundlePath + "AssetBundle";
#elif UNITY_IOS
    public static string AssetBundlePath = Application.persistentDataPath + "/../AssetBundle/";
    public static string AssetBundleFile = AssetBundlePath + "AssetBundle";
#endif
}
