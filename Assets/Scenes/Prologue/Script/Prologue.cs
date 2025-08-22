using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreateWave;
using System;

public class Prologue : BaseBehaviour
{

    string next_scene = null;
    int dramaId = 0;
    int TutorialStep = 0;

    protected override void Start()
    {
        base.Start();

        Debug.Log("Prologue Start");
        DispatchEvent(CwEvent.SCENE_READY);
    }

    /*
     * 登録完了時イベントハンドラ
     */
    public void gotoNext()
    {
        jsonRegist regist = APIConnectManager.Instance.regist;

        this.TutorialStep = regist.tutorial_step;
        this.next_scene = regist.nextscene;
        if (regist.dramaId > 0)
            this.dramaId = regist.dramaId;

        SceneController.Instance.Jump(this.next_scene, (() =>
        {
            TutorialBehaviour tutorial = FindObjectOfType<TutorialBehaviour>() as TutorialBehaviour;
            tutorial.Param = new TutorialBehaviour.Parameter { TutorialStep = this.TutorialStep };
        }));
    }

}
