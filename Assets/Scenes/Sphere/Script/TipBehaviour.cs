using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipBehaviour : MonoBehaviour
{
    // 特殊なマップチップの番号
    private string EMPTY { get; } = "EMPTY";     // カラ
    private string CURTAIN { get; } = "CURTAIN";   // 暗幕


    //x_y の番号
    private string tipName { get; set; }

    private SphereBehaviour Sphere { get; set; }
    private StageBehaviour Stage { get; set; }
    private Sprite[] _sprites { get; set; }
    private int _count { get; set; } = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init(string _tipName, SphereBehaviour _Sphere, StageBehaviour _Stage)
    {
        tipName = _tipName;
        Sphere = _Sphere;
        Stage = _Stage;

        transform.GetComponent<Rigidbody2D>().simulated = false;
        transform.GetComponent<Collider2D>().enabled = false;

    }

    //
    // 変数 tipName で示されたチップの描画を更新する。
    // そのチップの座標を変数 x, y で示しておくこと。
    public void draw(int x, int y, string structkind = null)
    {
        string _tipName = "";

        if (string.IsNullOrEmpty(structkind))
        {
            if (x < 0 || Sphere.sphere.structWid <= x || y < 0 || Sphere.sphere.structHei <= y)
                _tipName = EMPTY;
            else if (Sphere.sphere.mat[y].Substring(x, 1) == "0")
                _tipName = Stage.no["no" + this.tipName];
            else
                _tipName = CURTAIN;
        }
        else if (structkind == "background" || structkind == "overlayer1" || structkind == "overlayer2" || structkind == "cover")
        {
            if (Stage.no["no" + this.tipName] == "0000")
                _tipName = EMPTY;
            else
                _tipName = Stage.no["no" + this.tipName];
        }
        else
        {
            int wid = 0;
            int hei = 0;

            switch (structkind)
            {
                case "head":
                    wid = Sphere.sphere.headWid;
                    hei = Sphere.sphere.headHei;
                    break;
                case "left":
                    wid = Sphere.sphere.leftWid;
                    hei = Sphere.sphere.leftHei;
                    break;
                case "right":
                    wid = Sphere.sphere.rightWid;
                    hei = Sphere.sphere.rightHei;
                    break;
                case "foot":
                    wid = Sphere.sphere.footWid;
                    hei = Sphere.sphere.footHei;
                    break;
            }

            if (x < 0 || wid <= x || y < 0 || hei <= y)
                _tipName = EMPTY;
            else
                _tipName = Stage.no["no" + this.tipName];
        }


        // チップ画像をセット。
        Sprite _sprite = Utility.getAssetImage("Image/MapTip/" + _tipName);
        transform.GetComponent<Image>().sprite = _sprite;


        //sortingOrderを設定
        if (structkind == "background")
        {
            transform.GetComponent<Canvas>().sortingOrder = 0;
        }
        else if (structkind == "overlayer1")
        {
            transform.GetComponent<Canvas>().sortingOrder = 3;
        }
        else if (structkind == "overlayer2")
        {
            transform.GetComponent<Canvas>().sortingOrder = 4;
        }
        else if (structkind == "cover")
        {
            transform.GetComponent<Canvas>().sortingOrder = 7;
        }
    }

    public void setCost()
    {
        return;

        transform.GetComponent<Rigidbody2D>().simulated = true;
        transform.GetComponent<Collider2D>().enabled = true;
    }

    public void destroy()
    {
        GameObject.Destroy(this);
    }

    IEnumerator anim()
    {
        int _frame = _sprites.Length;

        while (true)
        {
            //0.1秒に一回
            yield return new WaitForSeconds(0.1f);

            if (_count >= _frame)
                _count = 0;

            transform.GetComponent<Image>().sprite = _sprites[_count];
            _count++;
        }
    }

    public void setPos(int posX, int posY)
    {
        // チップを該当の位置へ移動。
        transform.localPosition = new Vector3(posX, posY * -1, 0);
    }
}
