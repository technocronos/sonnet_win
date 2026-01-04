using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;//シーン用
using UnityEngine.EventSystems; //必須 (旧SendMessge)RecieveDestroyInterface のメッセージシステムに必要
using System;

namespace MyScene
{
    public class SceneController : EventDispatcher
    {
        public delegate void EventCallback();
        public EventCallback eventCallback = null;
        public EventCallback ParamSetEventCallback = null;

        [System.NonSerialized]
        public GameObject ClickGurd;

        [System.NonSerialized]
        public string SceneName = "none";

        [System.NonSerialized]
        public GameObject SecenRootObj;

        [System.NonSerialized]
        public GameObject PopUpRootObj;

        [System.NonSerialized]
        public bool deckEdit = false;

        private List<string> preScene = new List<string>();
        private float colorAlpfa;
        private static SceneController mInstance;

        private List<string> _popUpNames = new List<string>();

        public static SceneController Instance
        {
            get
            {
                if (mInstance == null)
                {

                    GameObject go = new GameObject("SceneController");
                    mInstance = go.AddComponent<SceneController>();
                    DontDestroyOnLoad(go);
                }
                return mInstance;
            }
        }

        public void Init(string nextsceneName, EventCallback SceneReadyCallback = null, GameObject _clickGurd = null)
        {
            this.eventCallback = SceneReadyCallback;

            this.ClickGurd = _clickGurd;
            if (this.ClickGurd != null)
            {
                this.ClickGurd.SetActive(false);
            }

            if (this.SceneName != "none")
                preScene.Add(this.SceneName);

            this.SceneName = nextsceneName;

            StartCoroutine("LoadSceneInit");
        }

        IEnumerator LoadSceneInit()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);

            //シーン名を指定する
            Scene scene = SceneManager.GetSceneByName(SceneName);

            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (var obj in rootObjects)
            {
                // Debug.LogFormat("RootObject = {0}", obj.name);
                if (obj.name == SceneName)
                {
                    SecenRootObj = obj;
                    break;
                }
            }

            //指定したシーン名をアクティブにする
            SceneManager.SetActiveScene(scene);

            if (this.eventCallback != null)
            {
                this.eventCallback?.Invoke();
            }

            this.eventCallback = null;
        }

        public void Jump(string nextsceneName, EventCallback _paramSetCallback = null, EventCallback SceneReadyCallback = null, bool isSound = true)
        {
            this.eventCallback = SceneReadyCallback;
            this.ParamSetEventCallback = _paramSetCallback;

            if (this.SceneName != "none")
                preScene.Add(this.SceneName);

            this.SceneName = nextsceneName;

            //header,footer下げる
            Header.Instance.SetOutPosition();
            Footer.Instance.SetOutPosition();

            //loadingが回っている場合
            if (ClickGurd != null)
            {
                //シーン移動前にクリックやタップイベントを無効化する
                ClickGurd.SetActive(true);
                Image image = ClickGurd.GetComponent<Image>();
                if (image.color.a > 0f)
                {
                    //すでにフェードアウトして黒画面が想定
                    StartCoroutine(LoadSceneCoroutine());
                }
                else
                {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, 0.1f);

                    Hashtable hash = new Hashtable(){
                        {"from", 0f},
                        {"to", 1f},
                        {"time", 0.5f},
                        {"easeType",iTween.EaseType.easeOutQuad},
                        {"loopType",iTween.LoopType.none},
                        {"onupdate", "setFade"},
                        {"oncomplete", "fadeEnd"},
                        {"onupdatetarget", gameObject}
                    };

                    iTween.ValueTo(gameObject, hash);
                }
            }
            else
            {
                StartCoroutine(LoadSceneCoroutine());
            }
        }

        private void fadeEnd()
        {
            // iTweenで呼ばれたら、受け取った値をImageのアルファ値にセット
            Image image = ClickGurd.GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);

            StartCoroutine(LoadSceneCoroutine());
        }

        private void setFade(float alpha)
        {
            // iTweenで呼ばれたら、受け取った値をImageのアルファ値にセット
            Image image = ClickGurd.GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
        }

        IEnumerator LoadSceneCoroutine()
        {
            ClosePopUpAll();
            if (preScene.Count > 0)
            {
                foreach (string s in preScene)
                {
                    SceneManager.UnloadSceneAsync(s);
                }

                preScene.Clear();
            }
            yield return new WaitForSeconds(0.1f);
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);

            //パラメータ引き渡し関数をコールする
            if (this.ParamSetEventCallback != null)
            {
                this.ParamSetEventCallback.Invoke();
                this.ParamSetEventCallback = null;
            }

            //シーン名を指定する
            Scene scene = SceneManager.GetSceneByName(SceneName);

            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (var obj in rootObjects)
            {
                if (obj.name == SceneName)
                {
                    SecenRootObj = obj;
                    break;
                }
            }
            BaseBehaviour baseBehaviour = SecenRootObj.GetComponent(typeof(BaseBehaviour)) as BaseBehaviour;
            baseBehaviour.AddEventListener(CwEvent.SCENE_READY, SceneReady);

            //指定したシーン名をアクティブにする
            SceneManager.SetActiveScene(scene);
        }

        private void SceneReady(GameObject eventObject, string str)
        {
            BaseBehaviour baseBehaviour = SecenRootObj.GetComponent(typeof(BaseBehaviour)) as BaseBehaviour;

            if (baseBehaviour.show_header)
            {
                Header.Instance.gameObject.SetActive(true);
                Footer.Instance.gameObject.SetActive(true);
                Header.Instance.SetPosition();
                Footer.Instance.SetPosition();
            }
            else
            {
                Header.Instance.gameObject.SetActive(false);
                Footer.Instance.gameObject.SetActive(false);
            }

            if (ClickGurd != null)
            {
                Hashtable hash = new Hashtable(){
                    {"from", 1f},
                    {"to", 0f},
                    {"time", 1f},
                    {"easeType",iTween.EaseType.easeOutQuad},
                    {"loopType",iTween.LoopType.none},
                    {"onupdate", "setFade"},
                    {"onupdatetarget", gameObject},
                    {"oncomplete", "fadeOutEnd"},
                    {"oncompletetarget", gameObject},
                    {"oncompleteparams", false}
                };
                iTween.ValueTo(gameObject, hash);
            }
            else
            {
                if (this.eventCallback != null)
                {
                    this.eventCallback?.Invoke();
                }

                this.eventCallback = null;
            }
        }

        private void fadeOutEnd()
        {
            Image image = ClickGurd.GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
            if (this.eventCallback != null)
            {
                this.eventCallback?.Invoke();
            }

            this.eventCallback = null;

            ClickGurd.SetActive(false);

        }

        //PopUp関係
        public void PopUp(string popUpName, EventCallback _ParamSetEventCallback = null)
        {
            if (_popUpNames.IndexOf(popUpName) >= 0)
            {
                return;
            }

            this.ParamSetEventCallback = _ParamSetEventCallback;
            this._popUpNames.Add(popUpName);

            StartCoroutine(LoadPopUpCoroutine());
        }
        IEnumerator LoadPopUpCoroutine()
        {
            string popUpName = _popUpNames[_popUpNames.Count - 1];
            yield return SceneManager.LoadSceneAsync(popUpName, LoadSceneMode.Additive);

            //パラメータ引き渡し関数をコールする
            if (this.ParamSetEventCallback != null)
            {
                this.ParamSetEventCallback.Invoke();
                this.ParamSetEventCallback = null;
            }

            //シーン名を指定する
            Scene scene = SceneManager.GetSceneByName(popUpName);
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (var obj in rootObjects)
            {
                if (obj.name == popUpName)
                {
                    PopUpRootObj = obj;
                    break;
                }
            }
            //指定したシーン名をアクティブにする
            SceneManager.SetActiveScene(scene);
            DispatchEvent(CwEvent.LOAD_COMP);
        }

        public void ClosePopUp()
        {
            if (_popUpNames.Count <= 0)
            {
                return;
            }
            string popUpName = _popUpNames[_popUpNames.Count - 1];
            SceneManager.UnloadSceneAsync(popUpName);
            _popUpNames.RemoveAt(_popUpNames.Count - 1);

            Scene scene;
            if (_popUpNames.Count > 0)
            {
                //他のPopUpがある
                popUpName = _popUpNames[_popUpNames.Count - 1];
                scene = SceneManager.GetSceneByName(popUpName);
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (var obj in rootObjects)
                {
                    if (obj.name == popUpName)
                    {
                        PopUpRootObj = obj;
                        break;
                    }
                }
            }
            else
            {
                //PopUpが全部なくなった
                PopUpRootObj = null;
                scene = SceneManager.GetSceneByName(SceneName);
            }
            SceneManager.SetActiveScene(scene);
            System.GC.Collect();
        }

        public void ClosePopUpName(string popUpName)
        {
            if (_popUpNames.Count <= 0)
            {
                return;
            }

            int index = _popUpNames.IndexOf(popUpName);
            if (index < 0)
            {
                return;
            }

            SceneManager.UnloadSceneAsync(popUpName);
            _popUpNames.RemoveAt(index);

            Scene scene;
            if (_popUpNames.Count > 0)
            {
                //他のPopUpがある
                popUpName = _popUpNames[_popUpNames.Count - 1];
                scene = SceneManager.GetSceneByName(popUpName);
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (var obj in rootObjects)
                {
                    if (obj.name == popUpName)
                    {
                        PopUpRootObj = obj;
                        break;
                    }
                }
            }
            else
            {
                //PopUpが全部なくなった
                PopUpRootObj = null;
                scene = SceneManager.GetSceneByName(SceneName);
            }

            if (SceneName != "none")
            {
                SceneManager.SetActiveScene(scene);
            }
            System.GC.Collect();
        }

        public void ClosePopUpAll()
        {
            if (_popUpNames.Count <= 0)
            {
                return;
            }

            for (int i = _popUpNames.Count - 1; i >= 0; i--)
            {
                string popUpName = _popUpNames[i];
                if (popUpName != "PopUpAlert")
                {
                    SceneManager.UnloadSceneAsync(popUpName);
                    _popUpNames.RemoveAt(i);
                }
            }
            //_popUpNames.Clear();
            PopUpRootObj = null;
            // Scene scene = SceneManager.GetSceneByName(SceneName);
            // SceneManager.SetActiveScene(scene);
            System.GC.Collect();
        }

        private void destroyChildren(GameObject gameObject)
        {
            foreach (Transform obj in gameObject.transform)
            {
                destroyChildren(obj.gameObject);
            }
            Destroy(gameObject);
        }

        //functorに設定するメソッド
        private void destroyMethod(RecieveDestroyInterface reciever, BaseEventData eventData)
        {
            reciever.DestroyScene();
        }

    }

}
