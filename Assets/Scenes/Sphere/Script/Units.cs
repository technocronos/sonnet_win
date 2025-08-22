using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//
// 全ユニットグラフィックを統括するムービー。
// …もともとステージムービーに直接ユニットを配していたのだけど、
// duplicateMovieClip() したときのZインデックスが大きくなって、
// ステージの他の要素を隠してしまうために、ワンクッション置くことにしたもの。
public class Units
{

    SphereBehaviour Sphere { get; set; }
    StageBehaviour Stage { get; set; }

    public Dictionary<string, UnitBehaviour> units { get; set; } = new Dictionary<string, UnitBehaviour>();

    private UnitBehaviour source { get; set; } = null;

    public IEnumerator Init(UnitBehaviour _source)
    {
        Sphere = SphereBehaviour.Instance;
        Stage = StageBehaviour.Instance;
        source = _source;


        Debug.Log("Units Start running..");
        // 念のため、コピー元のオリジナルムービーを非表示にしておく。
        //source.enabled = false;

        // ユニットを作成＆配置
        for (int no = 1; no <= Sphere.sphere.unitNum; no++)
        {
            if (Sphere.sphere.unit.ContainsKey(no))
            {
                this.createUnits(no);
            }
        }

        yield break;
    }

    //
    // 変数 no で指定されたユニットデータを参照して、ユニットの新規作成を行う。
    public void createUnits(int no)
    {
        // 指定されたユニットの基本情報を取得。
        jsonUnit unitinfo = Sphere.sphere.unit[no];

        // ちゃんと基本情報があるもののみを処理する。
        if (unitinfo != null)
        {

            // ユニットムービーを複製
            UnitBehaviour _unit = UnityEngine.Object.Instantiate(source, new Vector3(0, 0, 0), Quaternion.identity, Stage.transform);
            _unit.name = "unit_" + no.ToString();

            units[_unit.name] = _unit;

            // グラフィック番号をセット。
            int graphNo = int.Parse(unitinfo.Info.Split(new char[] { ' ' })[0]);
            units[_unit.name].graphNo = graphNo;

            //初期化
            units[_unit.name].Init();

            // グラフィック向きをセット。
            int graphAlign = int.Parse(unitinfo.Info.Split(new char[] { ' ' })[3]);

            //画像を反映する
            units[_unit.name].setAlign(graphAlign);

            // 表示座標を反映。
            units["unit_" + no.ToString()].setPos();

        }
        else
        {
            // 無効なユニットである場合はX座標上での位置でそれを示す
            unitinfo.X = -1;
        }
    }

    public void move(int no, int x, int y, int align)
    {
        jsonUnit unitinfo = Sphere.sphere.unit[no];

        units["unit_" + no.ToString()].setAlign(align);

        unitinfo.X = x;
        unitinfo.Y = y;
        units["unit_" + no.ToString()].setPos();

    }

    //
    // 変数 no で示されたユニットを削除する。
    public void remove(int no)
    {
        UnitBehaviour unit = units["unit_" + no.ToString()];

        GameObject.Destroy(unit.gameObject);

        // 無効なユニットである場合はX座標上での位置でそれを示す
        jsonUnit unitinfo = Sphere.sphere.unit[no];
        unitinfo.X = -1;
    }
}
