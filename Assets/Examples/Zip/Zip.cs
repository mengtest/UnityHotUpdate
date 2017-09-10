using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Zip : MonoBehaviour
{
    private string _zipSrcPath = "";
    private string _zipSrcFile = "";
    private string _zipDestPath = "";


	void Start()
    {
        _zipSrcPath = Application.dataPath + "/Examples/Zip/Src/";
        _zipSrcFile = Application.dataPath + "/Examples/Zip/Src.zip";
        _zipDestPath = Application.dataPath + "/Examples/Zip/Dest/";

        Dictionary<string, string> dictFile = new Dictionary<string, string>();

        DirectoryInfo dirInfo = new DirectoryInfo(_zipSrcPath);
        FileInfo[] filesInfo = dirInfo.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < filesInfo.Length; i++)
        {
            var fileInfo = filesInfo[i];
            if (fileInfo.Name.EndsWith(".meta")) { continue; }
            Debug.Log("fileInfo.FullName: " + fileInfo.FullName);
            string relativeName = fileInfo.FullName.Substring(_zipSrcPath.Length);
            Debug.Log("relativeName:" + relativeName);
            dictFile.Add(fileInfo.FullName, relativeName);
        }

        UtilZip.Zip(dictFile, _zipSrcFile);

        UtilZip.UnZip(_zipSrcFile, _zipDestPath);
	}
}
