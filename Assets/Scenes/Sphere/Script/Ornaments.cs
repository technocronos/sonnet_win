using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Ornaments
{
    SphereBehaviour Sphere { get; set; }
    StageBehaviour Stage { get; set; }

    public Dictionary<string, OrnamentBehaviour> orns { get; set; } = new Dictionary<string, OrnamentBehaviour>();

    private OrnamentBehaviour source { get; set; } = null;

    public void Init(OrnamentBehaviour _source)
    {
        Debug.Log("MassEffect Start running..");

        Sphere = SphereBehaviour.Instance;
        Stage = StageBehaviour.Instance;
        source = _source;

        foreach (Transform childTransform in source.transform)
        {
            //全ての子オブジェクトを非表示
            childTransform.gameObject.SetActive(false);
        }


        // 置物を作成＆配置
        foreach (KeyValuePair<int, string> ornKeyVal in Sphere.sphere.orn)
        {
            this.createOrns(ornKeyVal.Key);
        }
    }

    /// <summary>
    /// 変数 no で指定された置物データを参照して、置物の新規作成を行う。
    /// </summary>
    /// <param name="no"></param>
    public void createOrns(int no)
    {

        // 指定された置物の基本情報を取得。
        string info = Sphere.sphere.orn[no];

        // ちゃんと基本情報があるもののみを処理する。
        if (info != "")
        {

            // 置物ムービーを複製
            OrnamentBehaviour _orn = UnityEngine.Object.Instantiate(source, new Vector3(0, 0, 0), Quaternion.identity, Stage.transform);
            _orn.name = "orn_" + no;

            orns[_orn.name] = _orn;

            // グラフィック番号をセット。
            int GraphNo = int.Parse(info.Split(new char[] { ' ' })[0]);

            // 表示座標を反映。
            int x = int.Parse(info.Split(new char[] { ' ' })[1]) * Sphere.TIP_SIZE;
            int y = int.Parse(info.Split(new char[] { ' ' })[2]) * Sphere.TIP_SIZE * -1;

            //位置をセットして再生
            orns[_orn.name].setPos(x, y);
            orns[_orn.name].Play(GraphNo);

            // 暗幕の状態を見て、表示／非表示を反映する。
            this.Show(no);
        }
    }

    public void ShowAll()
    {
        // 置物を作成＆配置
        foreach (KeyValuePair<int, string> ornKeyVal in Sphere.sphere.orn)
        {
            this.Show(ornKeyVal.Key);
        }
    }
    /// <summary>
    /// 変数noで示された置物の表示／非表示を更新する。
    /// </summary>
    /// <param name="no"></param>
    void Show(int no)
    {
        string info = Sphere.sphere.orn[no];

        // ちゃんと基本情報があるもののみを処理する。
        if (info != "")
        {
            int x = int.Parse(info.Split(new char[] { ' ' })[1]);
            int y = int.Parse(info.Split(new char[] { ' ' })[2]);

            bool _visible = true;

            try
            {
                _visible = (Sphere.sphere.mat[y].Substring(x, 1) == "0");
            }
            catch (Exception e)
            {
                _visible = true;
            }

            orns["orn_" + no].gameObject.SetActive(_visible);

            if (_visible)
            {
                //アニメを再生
                int GraphNo = int.Parse(info.Split(new char[] { ' ' })[0]);
                orns["orn_" + no].Play(GraphNo);
            }
        }
    }

    /// <summary>
    /// 変数 no で指定された置物データを参照して、置物の更新を行う。
    /// </summary>
    public void refInfo(int no)
    {
        var g = Stage.transform.Find("orn_" + no);
        if (g != null)
            GameObject.Destroy(g.gameObject);

        this.createOrns(no);
    }

}
