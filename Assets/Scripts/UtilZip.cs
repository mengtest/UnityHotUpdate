using System.Collections;
using System.Collections.Generic;

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
    /// 解压文件
    /// </summary>
    /// <param name="zipFile">压缩文件</param>
    /// <param name="outPath">解压目录</param>
    public static void UnZip(string zipFile, string outPath)
    {
        (new FastZip()).ExtractZip(zipFile, outPath, "");
    }
}
