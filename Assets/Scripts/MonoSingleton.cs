using UnityEngine;


public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    private static object _lock = new object();


    public static T Inst
    {
    	get
    	{
    		if (_appIsQuiting) { return null; }

    		lock (_lock)
    		{
    			if (_instance == null)
    			{
    				_instance = (T)FindObjectOfType(typeof(T));
    				if (FindObjectsOfType(typeof(T)).Length > 1)
    				{
    					return _instance;
    				}

    				if (_instance == null)
    				{
    					GameObject go = new GameObject();
    					_instance = go.AddComponent<T>();
    					go.name = typeof(T).ToString();

    					DontDestroyOnLoad(go);
    				}
    			}
    		}

    		return _instance;
    	}
    }


    private static bool _appIsQuiting = false;
    public void OnDestroy()
    {
        Debug.Log("MonoSingleton.OnDestroy()");
    	_appIsQuiting = true;

    	OnDestroyDone();
    }


    protected abstract void OnDestroyDone();
}
