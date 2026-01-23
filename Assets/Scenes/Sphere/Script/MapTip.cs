using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//
// 背景を統括するムービー。
// 最初はすべてのチップを作成して並べておいたのだけど、
// メモリオーバーしてしまったため、スクリーン内に収まる部分だけを
// 作成して、スクロール時はフレーム内に収まるように移動し続けるという手法をとっている。
public class MapTip
{
    SphereBehaviour Sphere;
    StageBehaviour Stage;

    Dictionary<string, int> scrX = new Dictionary<string, int>();
    Dictionary<string, int> scrY = new Dictionary<string, int>();

    public Dictionary<string, TipBehaviour> tips { get; set; } = new Dictionary<string, TipBehaviour>();

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("MapTip Start running..");

    }

    Image tip_source;
    string[] structkinds = { "background", "stage", "overlayer1", "overlayer2", "cover", "head", "left", "right", "foot" };

    public IEnumerator Init(Image _tip_source)
    {
        tip_source = _tip_source;

        Sphere = SphereBehaviour.Instance;
        Stage = StageBehaviour.Instance;

        /*
        *---------------------------------------------------------------------------------------------
        *チップムービーの作成
        *---------------------------------------------------------------------------------------------
        */

        // スクリーンに入ると考える幅・高さをチップ単位で。
        // 横幅はギリギリだとチラつくので1チップ分マージンをとる
        //float BG_WID = Mathf.Ceil(Sphere.STAGE_WID / Sphere.TIP_SIZE) + 1;
        //float BG_HEI = Mathf.Ceil(Sphere.STAGE_HEI / Sphere.TIP_SIZE);

        /*
        *---------------------------------------------------------------------------------------------
        * チップムービーの配置
        * backfround 背景　コストは換算されず常に通れない
        * struct 本体
        * overlayer1 上書きレイヤ1　コストは上書きされる
        * overlayer2 上書きレイヤ2　コストは上書きされる
        * cover カバー　主に隠し通路のカバーに使う　コストは換算されず常に通れる
        * head left right foot ヘッダー、フッター、左右オビ。　余白隠しのため
        *---------------------------------------------------------------------------------------------
        */


        foreach (string structkind in structkinds)
        {
            int[] array = new int[2];
            array = getWidHei(convertStructkind(structkind));

            int BG_WID = array[0];
            int BG_HEI = array[1];

            // ステージの原点から見て、スクリーンに入ると思われる左上のマスがどこかを
            // 表す変数を初期化。
            scrX.Add(structkind, 0);
            scrY.Add(structkind, 0);

            // 初期配置。
            int posX = scrX[structkind] * Sphere.TIP_SIZE;

            // チップを該当の位置へ移動。
            switch (structkind)
            {
                case "background":
                case "head":
                case "left":
                case "foot":
                    posX -= (Sphere.TIP_SIZE * Sphere.sphere.leftWid);
                    break;
                case "right":
                    posX += (Sphere.TIP_SIZE * Sphere.sphere.structWid);
                    break;
            }


            for (int x = scrX[structkind]; x < scrX[structkind] + BG_WID; x++)
            {
                int posY = scrY[structkind] * Sphere.TIP_SIZE;

                // チップを該当の位置へ移動。
                switch (structkind)
                {
                    case "background":
                    case "head":
                        posY -= (Sphere.TIP_SIZE * Sphere.sphere.headHei);

                        break;
                    case "foot":
                        posY += (Sphere.TIP_SIZE * Sphere.sphere.structHei);
                        break;
                }


                for (int y = scrY[structkind]; y < scrY[structkind] + BG_HEI; y++)
                {

                    if (Stage.no.ContainsKey("no" + convertStructkind(structkind) + x + "_" + y))
                    {
                        // チップムービーの作成。
                        Image tip = UnityEngine.Object.Instantiate(tip_source, new Vector3(0, 0, 0), Quaternion.identity, Stage.transform);
                        tip.name = getTipName(convertStructkind(structkind), x, y);
                        tip.transform.localPosition = new Vector3(x, y, 0);

                        TipBehaviour objtip = tip.transform.GetComponent<TipBehaviour>();
                        objtip.Init(convertStructkind(structkind) + x + "_" + y, Sphere, Stage);

                        tips[tip.name] = objtip;

                        // 示された場所に配置するチップの名前を取得する。
                        string tipName = getTipName(convertStructkind(structkind), x, y);

                        // 画像をセット
                        tips[tipName].draw(x, y, convertStructkind(structkind));

                        // チップを該当の位置へ移動。
                        tips[tipName].setPos(posX, posY);

                        //はみ出た所は非表示
                        if(posX > (int)Sphere.STAGE_WID || posY > (int)Sphere.STAGE_HEI) { 
                            tips[tipName].gameObject.SetActive(false);
                        }
                        
                        //背景がある場合・・
                        if (structkind == "background" && Sphere.sphere.backgroundWid > 0 && Sphere.sphere.backgroundHei > 0)
                        {
                            //背景のオーバーレイ効果があるなら表示する。今の所「clowd」のみ
                            if (Sphere.sphere.sphere_bg != null)
                            {
                                Transform bg = Sphere.sphere_bg.transform.Find(Sphere.sphere.sphere_bg);
                                if (bg != null)
                                {
                                    Vector3 size = new Vector3(Sphere.sphere.backgroundWid * Sphere.TIP_SIZE, Sphere.sphere.backgroundHei * Sphere.TIP_SIZE, 0);
                                    Sphere.sphere_bg.GetComponent<RectTransform>().sizeDelta = size;

                                    Vector3 pos = new Vector3(Sphere.TIP_SIZE * Sphere.sphere.leftWid * -1, Sphere.TIP_SIZE * Sphere.sphere.headHei, 0);
                                    Sphere.sphere_bg.GetComponent<RectTransform>().anchoredPosition = pos;

                                    Sphere.sphere_bg.SetActive(true);
                                    bg.gameObject.SetActive(true);
                                }
                            }
                        }
                    }

                    posY += Sphere.TIP_SIZE;
                }
                posX += Sphere.TIP_SIZE;
            }
        }

        yield break;
    }

    public void setCost()
    {
        foreach(KeyValuePair<string, int> costpair in Stage.cost)
        {
            var coststr = costpair.Key.Replace("cost", "");

            foreach (KeyValuePair<string, TipBehaviour> tipspair in tips)
            {
                var tipsstr = tipspair.Key.Replace("tip", "");
                if(coststr == tipsstr && costpair.Value == 9999)
                {
                    tipspair.Value.setCost();
                }
            }

        }
    }

    //
    // 変数 y で示された行のチップを更新する。
    public void line(int y)
    {
        //ここはステージのみ更新
        string structkind = "stage";

        // その行のチップを張り替える。
        for (int x = 0; x < Sphere.sphere.structWid; x++)
        {
            string tipName = getTipName(convertStructkind(structkind), x, y);

            if (tips.ContainsKey(tipName))
                tips[tipName].draw(x, y, convertStructkind(structkind));
        }
    }

    public void change(string structkind, int x, int y)
    {
        // チップムービーの名前のYの部分を取得。
        string tipName = getTipName(convertStructkind(structkind), x,y);

        if (tips.ContainsKey(tipName))
            tips[tipName].draw(x, y, convertStructkind(structkind));
    }

    private string getTipName(string structkind, int x , int y)
    {
        string tipName = "tip" + structkind + x + "_" + y;

        return tipName;
    }

    int[] getWidHei(string structkind)
    {
        int[] array = new int[2];

        switch (structkind)
        {
            case "background":
                array[0] = Sphere.sphere.backgroundWid;
                array[1] = Sphere.sphere.backgroundHei;
                break;
            case "overlayer1":
                array[0] = Sphere.sphere.overlayer1Wid;
                array[1] = Sphere.sphere.overlayer1Hei;
                break;
            case "overlayer2":
                array[0] = Sphere.sphere.overlayer2Wid;
                array[1] = Sphere.sphere.overlayer2Hei;
                break;
            case "cover":
                array[0] = Sphere.sphere.coverWid;
                array[1] = Sphere.sphere.coverHei;
                break;
            case "head":
                array[0] = Sphere.sphere.headWid;
                array[1] = Sphere.sphere.headHei;
                break;
            case "left":
                array[0] = Sphere.sphere.leftWid;
                array[1] = Sphere.sphere.leftHei;
                break;
            case "right":
                array[0] = Sphere.sphere.rightWid;
                array[1] = Sphere.sphere.rightHei;
                break;
            case "foot":
                array[0] = Sphere.sphere.footWid;
                array[1] = Sphere.sphere.footHei;
                break;
            default:
                array[0] = Sphere.sphere.structWid;
                array[1] = Sphere.sphere.structHei;
                break;

        }

        return array;
    } 

    string convertStructkind(string structkind)
    {
        return structkind !="stage" ? structkind : String.Empty;
    }

    int oldX = 0;
    int oldY = 0;

    //
    // ステージの座標を参照して、スクリーンから外れているチップを、
    // 新たにスクリーンに入ることになる位置に再配置する。
    public IEnumerator refinfo()
    {
        while (true)
        {
            yield return new WaitForSeconds(Main.Instance.getParFrame());
            Vector3 _stage = Stage.transform.GetComponent<RectTransform>().anchoredPosition;

            // ステージの座標を参照して、scrX, scrY の新たな値を取得。
            int newX = (int)(_stage.x / Sphere.TIP_SIZE);
            int newY = (int)(-1 * (_stage.y / Sphere.TIP_SIZE));

            foreach (string structkind in structkinds)
            {
                if (scrX[structkind] != newX || scrY[structkind] != newY)
                {
                    int[] array = new int[2];
                    array = getWidHei(convertStructkind(structkind));

                    int BG_WID = array[0];
                    int BG_HEI = array[1];

                    oldX = scrX[structkind];
                    oldY = scrY[structkind];

                    // ステージ座標更新
                    scrX[structkind] = newX;
                    scrY[structkind] = newY;

                    // 初期配置。
                    int posX = Sphere.TIP_SIZE;

                    // チップを該当の位置へ移動。
                    switch (structkind)
                    {
                        case "background":
                        case "head":
                        case "left":
                        case "foot":
                            posX -= (Sphere.TIP_SIZE * Sphere.sphere.leftWid);
                            break;
                        case "right":
                            posX += (Sphere.TIP_SIZE * Sphere.sphere.structWid);
                            break;
                    }


                    for (int x = 0; x < BG_WID; x++)
                    {
                        int posY = Sphere.TIP_SIZE;

                        // チップを該当の位置へ移動。
                        switch (structkind)
                        {
                            case "background":
                            case "head":
                                posY -= (Sphere.TIP_SIZE * Sphere.sphere.headHei);

                                break;
                            case "foot":
                                posY += (Sphere.TIP_SIZE * Sphere.sphere.structHei);
                                break;
                        }


                        for (int y = 0; y < BG_HEI; y++)
                        {
                            if (Stage.no.ContainsKey("no" + convertStructkind(structkind) + x + "_" + y))
                            {
                                // 示された場所に配置するチップの名前を取得する。
                                string tipName = getTipName(convertStructkind(structkind), x, y);

                                //はみ出た所は非表示
                                if (((scrX[structkind] + 1) * Sphere.TIP_SIZE) + posX <= 0 || ((scrX[structkind] - 2) * Sphere.TIP_SIZE) + posX >= (int)Sphere.STAGE_WID)
                                {
                                    tips[tipName].gameObject.SetActive(false);
                                }
                                else if(((scrY[structkind] + 1) * Sphere.TIP_SIZE) + posY <= 0 || ((scrY[structkind] - 2) * Sphere.TIP_SIZE) + posY >= (int)Sphere.STAGE_HEI)
                                {
                                    tips[tipName].gameObject.SetActive(false);
                                }
                                else
                                {
                                    tips[tipName].gameObject.SetActive(true);
                                }
                            }

                            posY += Sphere.TIP_SIZE;
                        }
                        posX += Sphere.TIP_SIZE;
                    }
                }
            }
        }

    }

}
