using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void DisposeDelegate();
public delegate void StopJobDelegate();
public class Disposer : MonoBehaviour
{

    public event DisposeDelegate CallDispose;
    public event StopJobDelegate CallStopJob;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    void OnApplicationQuit()
    {

        StopJob();
        Dispose();
    }
    void Dispose()
    {

        CallDispose?.Invoke();
    }
    void StopJob()
    {

        CallStopJob?.Invoke();
    }
}
