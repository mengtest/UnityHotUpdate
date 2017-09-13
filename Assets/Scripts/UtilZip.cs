using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using ICSharpCode.SharpZipLib.Zip;


public class UtilZip
{
    /// <summary>
    /// 压缩多个文件
    /// </summary>
    /// <param name="inFiles">压缩文件字典(文件名-标识名)</param>
    /// <param name="outFile">压缩后的文件名</param>
    public static void Zip(Dictionary<string, string> inFiles, string zipFile)
    {
        using (ZipFile zip = ZipFile.Create(zipFile))
        {
            zip.BeginUpdate();

            foreach (var kv in inFiles)
            {
                zip.Add(kv.Key, kv.Value);
            }

            zip.CommitUpdate();
        }
    }

    /// <summary>
    /// 压缩多个文件
    /// </summary>
    /// <param name="inFiles">压缩文件列表</param>
    /// <param name="outFile">压缩后文件名</param>
    public static void Zip(List<string> inFiles, string zipFile)
    {
        using (ZipFile zip = ZipFile.Create(zipFile))
        {
            zip.BeginUpdate();

            foreach (string inFile in inFiles)
            {
                zip.Add(inFile);
            }

            zip.CommitUpdate();
        }
    }

    /// <summary>
    /// 解压文件(这个不好用, 在Android下有目录的情况下不能覆盖)
    /// </summary>
    /// <param name="zipFile">压缩文件</param>
    /// <param name="outPath">解压目录</param>
    public static void UnZip2(string zipFile, string outPath)
    {
        (new FastZip()).ExtractZip(zipFile, outPath, "");
    }

    /// <summary>
    /// 解压文件
    /// </summary>
    /// <param name="zipFile">压缩文件</param>
    /// <param name="outPath">解压目录</param>
    public static void UnZip(string sourceFile, string targetPath)
    {
        if (!File.Exists(sourceFile))
        {
            Debug.LogError("sourceFile: " + sourceFile + " not exist");
            return;
        }
        UtilIO.CreateDir(targetPath);

        using (var s = new ZipInputStream(File.OpenRead(sourceFile)))
        {
            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                if (theEntry.IsDirectory) continue;

                string directorName = Path.Combine(targetPath, Path.GetDirectoryName(theEntry.Name));
                string fileName = Path.Combine(directorName, Path.GetFileName(theEntry.Name));

                UtilIO.CreateDir(directorName);

                if (!String.IsNullOrEmpty(fileName))
                {
                    using (FileStream streamWriter = File.Create(fileName))
                    {
                        int size = 4096;
                        byte[] data = new byte[size];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
