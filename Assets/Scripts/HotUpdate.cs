using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class HotUpdate : MonoBehaviour
{
    private string resInfoFile;


    void Start()
    { 
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_MAC || UNITY_EDITOR
        SceneManager.LoadScene("Main");
#else
        StartCoroutine(CheckUpdate());
#endif
    }


    IEnumerator CheckUpdate()
    {
        yield return null;
    }
}
