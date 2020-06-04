using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoroutineManager : MonoBehaviour
{
    private static CoroutineManager instance;

    public static CoroutineManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
        runningCoroutine = new Dictionary<string, Coroutine>();
    }

    [HideInInspector] public Dictionary<string, Coroutine> runningCoroutine { get; private set;}

    public void Run(string name, IEnumerator coroutine )
    {
       Stop(name);
       runningCoroutine.Add(name, StartCoroutine(coroutine));
    }

    public void Stop()
    {
        if (runningCoroutine != null)
        {
            StopAllCoroutines();
            runningCoroutine.Clear();
        }
    }

    private void Stop(string name)
    {
        if (runningCoroutine != null)
        {
            if (runningCoroutine.ContainsKey(name))
            {
                StopCoroutine(runningCoroutine[name]);
                runningCoroutine[name] = null;
                runningCoroutine.Remove(name);
            }
        }
    }
}
