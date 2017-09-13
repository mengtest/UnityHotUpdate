using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using UnityEngine;


public class UtilLua
{
    public static string FileContent(string fileName)
    {
        if (!fileName.EndsWith(".lua"))
        {
            fileName += ".lua";
        }
        TextAsset textAsset = ResMgr.Inst.Load<TextAsset>("Lua/" + fileName);
        if (textAsset != null)
        {
            return textAsset.text;
        }
        return "";
    }

    public static byte[] Require(string fileName)
    {
        if (!fileName.EndsWith(".lua"))
        {
            fileName += ".lua";
        }
        TextAsset textAsset = ResMgr.Inst.Load<TextAsset>("Lua/" + fileName);
        if (textAsset != null)
        {
            return textAsset.bytes;
        }
        return null;
    }
}
