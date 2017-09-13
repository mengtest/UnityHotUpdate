using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using UnityEngine;


public class UtilIO
{
    public static void CopyDir(string dirFrom, string dirTo, string ext = "")
    { 
        if (!Directory.Exists(dirFrom))
        {
            return;
        }
        CreateDir(dirTo);

        foreach (string fileFromName in Directory.GetFiles(dirFrom))
        {
            string fileFrom = Path.GetFileName(fileFromName);
            string fileTo = Path.Combine(dirTo, fileFrom);
            File.Copy(fileFromName, fileTo + ext, true);
        }

        foreach (string dirFromName in Directory.GetDirectories(dirFrom))
        {
            string dirFrom2 = Path.GetFileName(dirFromName);
            string dirTo2 = Path.Combine(dirTo, dirFrom2);
            CopyDir(dirFromName, dirTo2, ext);
        }
    }

    public static void CreateDir(string dir)
    {
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    public static string FileMd5(string fileName)
    {
        using (FileStream fs = new FileStream(fileName, FileMode.Open))
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }

    public static string FileContent(string fileName)
    {
        if (!File.Exists(fileName))
        {
            Debug.LogError("fileName: " + fileName + " not exist!");
            return "";
        }
        return File.ReadAllText(fileName);
    }

    public static byte[] FileBytes(string fileName)
    {
        if (!File.Exists(fileName))
        {
            Debug.LogError("fileName: " + fileName + " not exist!");
            return null;
        }
        return Encoding.UTF8.GetBytes(fileName);
    }
}
