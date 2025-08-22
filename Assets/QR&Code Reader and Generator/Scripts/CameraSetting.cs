using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AOT;
using UnityEngine.Android;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public sealed class CameraSetting 
{
    public static Task<int> RequestCameraPermissions()
    {
        // Request
        var permissionTask = new TaskCompletionSource<int>();
        if (Application.platform == RuntimePlatform.Android)
        {
            var requester = new GameObject("MediaDeviceQuery Permissions Helper").AddComponent<CameraPermissionHelper>();
            requester.StartCoroutine(RequestAndroid(requester));
        }
        return permissionTask.Task;
        // Define Android request
        IEnumerator RequestAndroid(CameraPermissionHelper requester)
        {
            var permission = Permission.Camera;
            if (!Permission.HasUserAuthorizedPermission(permission))
                Permission.RequestUserPermission(permission);
            yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(permission));
            permissionTask.SetResult(1);
            MonoBehaviour.Destroy(requester.gameObject);
        }
    }
    
    private sealed class CameraPermissionHelper : MonoBehaviour
    {
        void Awake() => DontDestroyOnLoad(this.gameObject);
    }

}
