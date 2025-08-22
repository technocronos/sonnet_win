using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Firebase;
using Firebase.Auth;
using System.Web;

public class FireBaseManager : EventDispatcher
{
    [System.NonSerialized]
    public Firebase.Auth.FirebaseUser userInfo = null;
    private static FireBaseManager mInstance;
    private static Firebase.Auth.FirebaseAuth auth;

    //コルーチンを途中で完全に停止させるため変数にいれる
    private IEnumerator _routine;
    private string _eventName = FireBaseEvent.NONE;

    public static FireBaseManager Instance
    {
        get
        {
            if (mInstance == null)
            {

                GameObject go = new GameObject("FireBaseManager");
                mInstance = go.AddComponent<FireBaseManager>();
                auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                DontDestroyOnLoad(go);
            }
            return mInstance;
        }
    }
    private void Update()
    {
        //Debug.Log("_eventName");
        if (_eventName != FireBaseEvent.NONE)
        {
            string eventName = _eventName;
            _eventName = FireBaseEvent.NONE;
            DispatchEvent(eventName);
        }
    }

    public string GetUserId()
    {
        return userInfo.UserId;
    }

    // 匿名ログインにメール情報を結びつけて匿名アカウントをメールアカウントで永久アカウントに昇格させる
    public void EmailAccount(string loginEmail, string loginPass)
    {
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        Firebase.Auth.Credential credential = Firebase.Auth.EmailAuthProvider.GetCredential(loginEmail, loginPass);
        auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("LinkWithCredentialAsync was canceled.");
            }
            if (task.IsFaulted)
            {
                Debug.LogError("LinkWithCredentialAsync encountered an error: " + task.Exception);
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("Credentials successfully linked to Firebase user: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });
    }

    public void Login()
    {
        Debug.Log("FireBaseManagerでFireBaseログインを開始します");
        if (userInfo == null)
        {
            //　匿名認証
            Debug.Log("匿名認証開始します");
            auth.SignInAnonymouslyAsync().ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInAnonymouslyAsync was canceled.");

                    //DispatchEvent(FireBaseEvent.FAILD_FIREBASE);
                    //ここは別メソッドらしく、Macでios設定だとDispatchEvent､コルーチン、Invokeもthisさえとれない
                    _eventName = FireBaseEvent.FAILD_FIREBASE;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);

                    //DispatchEvent(FireBaseEvent.FAILD_FIREBASE);
                    //.Net4SDKだと、ここは別スレッドらしく、DispatchEvent､コルーチン、Invokeもthisさえとれない
                    _eventName = FireBaseEvent.FAILD_FIREBASE;
                }

                userInfo = task.Result;
                //Debug.LogFormat("User signed in successfully: {0} ({1})",UserInfo.DisplayName, UserInfo.UserId);
                //通信キーの取得
                //DispatchEvent(FireBaseEvent.LOGIN_END);
                //.Net4SDKだと、ここは別スレッドらしく、DispatchEvent､コルーチン、Invokeもthisさえとれない
                _eventName = FireBaseEvent.LOGIN_END;
            });
        }
        else
        {
            Debug.Log("Firebaseログインしました");
            _eventName = FireBaseEvent.LOGIN_END;
        }
    }

    //ここは永久に実行される事はないはず・・
    private void OnDestroy()
    {
        DestroyListener();
    }
}

//ユーザーデータ
public class UserData
{
    public string userData;
}
