using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

public class UnityWebRequestAwaiter : INotifyCompletion
{
    private UnityWebRequestAsyncOperation asyncOp;
    private Action continuation;

    public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
    {
        //Debug.Log("UnityWebRequestAwaiter run..");

        this.asyncOp = asyncOp;
        asyncOp.completed += OnRequestCompleted;
    }

    public bool IsCompleted { get { return asyncOp.isDone; } }

    public void GetResult()
    {

        //Debug.Log("UnityWebRequestAwaiter GetResult run..");

    }

    public void OnCompleted(Action continuation)
    {
        //Debug.Log("UnityWebRequestAwaiter OnCompleted run..");
        this.continuation = continuation;
    }

    private void OnRequestCompleted(AsyncOperation obj)
    {
        //Debug.Log("UnityWebRequestAwaiter OnRequestCompleted run..");
        continuation();
    }

    public UnityWebRequestAwaiter GetAwaiter()
    {
        return this;
    }
}

public static class ExtensionMethods
{
    public static UnityWebRequestAwaiter ConfigureAwait(this UnityWebRequestAsyncOperation asyncOperation, IProgress<float> progress)
    {
        var progressNotifier = new WebRequestProgressNotifier(asyncOperation, progress);
        LoadingAnime.Instance.AddItem(progressNotifier);

        return new UnityWebRequestAwaiter(asyncOperation);
    }

    public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
    {
        return new UnityWebRequestAwaiter(asyncOp);
    }
}
