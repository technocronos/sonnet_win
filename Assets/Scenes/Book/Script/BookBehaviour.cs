using MyScene;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BookBehaviour : BaseBehaviour
{

    public TextMeshProUGUI capturetext;
    public Image BG;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //BG.sprite = Utility.getAssetImage("Image/BG/bg_book");

        Header.Instance.SetTitle(Utility.getText("TEXT_BOOK"));

        HomeApi summary = Header.Instance.GetSummary();

        capturetext.text = Utility.getText("BOOK_CAPTURE_PER") + " " + summary.monster_capture + "/" + summary.monster_count;

        DispatchEvent(CwEvent.SCENE_READY);
    }

    public void onButton(int category)
    {
        AudioManager.Instance.PlaySE("se_btn");

        SceneController.Instance.Jump("BookDetail", (() =>
        {
            BookDetailBehaviour _scene = FindObjectOfType<BookDetailBehaviour>() as BookDetailBehaviour;
            _scene.Param = new BookDetailBehaviour.Parameter
            {
                Category = category,
            };
        }));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
