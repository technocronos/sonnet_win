using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//
// 可能範囲を示すマーカを統括するムービー。
public class RangeMarker
{

    SphereBehaviour Sphere;
    StageBehaviour Stage;

    public Dictionary<string, int> marks { get; set; } = new Dictionary<string, int>();
    public Dictionary<int, int> x_arr { get; set; } = new Dictionary<int, int>();
    public Dictionary<int, int> y_arr { get; set; } = new Dictionary<int, int>();
    public Dictionary<int, int> mPow_arr { get; set; } = new Dictionary<int, int>();
    Dictionary<string, MarkerBehaviour> markObj { get; set; } = new Dictionary<string, MarkerBehaviour>();

    public int actNo { get; set; }
    public int stack { get; set; } = 1;
    public int mType { get; set; }
    public int union { get; set; }
    public string color { get; set; }

    private Image source { get; set; } = null;

    // Start is called before the first frame update
    void Start()
    {

    }

    public IEnumerator Init(Image _source)
    {

        Sphere = SphereBehaviour.Instance;
        Stage = StageBehaviour.Instance;
        source = _source;

        // ルーム内各マスに関する情報を取得。
        // 以下の変数がセットされる
        //     markX_Y  そのマスのマーク値(X, Yは可変。マスの座標)。
        for (int x = 0; x < Sphere.sphere.structWid; x++)
        {
            for (int y = 0; y < Sphere.sphere.structHei; y++)
            {
                marks["mark" + x + "_" + y] = -1;
            }
        }

        // マーカーをセットするときに使う値を初期化。
        actNo = -1;

        yield break;

    }


    //
    // 指定されたポイントから指定された範囲のチップにマーカをセットする。
    // 引数)
    //     stack    固定で1。必ずセットすること
    //     x1, y1   範囲元になるポイント
    //     mPow1    移動力
    //     mType    移動タイプ。
    //                  0   移動コスト、ZOC無視
    //                  1   通常
    //     union    移動タイプ:通常 の場合に、移動者の所属番号(2桁)
    //     color    マーカの色。"move", "target", "damag", "recov" のいずれか。
    public void mark()
    {
        // このcallで必要なローカル変数もどきを設定。
        int x = x_arr[stack];
        int y = y_arr[stack];
        int mPow = mPow_arr[stack];

        // 指定されているマスの位置にマークを作成する。
        string suffix = x + "_" + y;
        marks["mark" + suffix] = mPow;
        this.setMarker(x, y);

        // 左のマスに対して再帰チェック。
        if (x > 0)
        {
            int argX = x - 1;
            int argY = y;
            this.recMarker(argX, argY, mPow);
        }

        // 上
        if (y > 0)
        {
            int argX = x;
            int argY = y - 1;
            this.recMarker(argX, argY, mPow);
        }

        // 右
        if (x < Sphere.sphere.structWid - 1)
        {
            int argX = x + 1;
            int argY = y;
            this.recMarker(argX, argY, mPow);
        }

        // 下
        if (y < Sphere.sphere.structHei - 1)
        {
            int argX = x;
            int argY = y + 1;
            this.recMarker(argX, argY, mPow);
        }

        // 終了処理。スタックカウントを減じるとともに、ローカル変数もどきを戻す。
        stack--;
        if (stack > 0)
        {
            x = x_arr[stack];
            y = y_arr[stack];
            mPow = mPow_arr[stack];
        }

        // カーソル最前面に移動
        Image _cursor = Stage.transform.Find("cursor").GetComponent<Image>();
        _cursor.GetComponent<RectTransform>().SetAsLastSibling();

        // color == "move" の場合、すべての再帰が完了した時（stack == 0）にすべてのマーカーの画像を更新
        if (color == "move" && stack == 0)
        {
            //UpdateMarkerSprites();
        }
    }

    // すべてのマーカーの周囲を確認して、適切な画像を設定する
    private void UpdateMarkerSprites()
    {
        foreach (KeyValuePair<string, MarkerBehaviour> keyValue in markObj)
        {
            if (keyValue.Value == null || !keyValue.Value.gameObject.activeSelf) continue;

            Vector3 _mkpos = keyValue.Value.transform.localPosition;
            int x = (int)(_mkpos.x / Sphere.TIP_SIZE);
            int y = (int)((_mkpos.y / Sphere.TIP_SIZE) * -1);

            // 周囲のマーカーの存在を確認
            bool hasLeft = x > 0 && marks["mark" + (x - 1) + "_" + y] >= 0;
            bool hasRight = x < Sphere.sphere.structWid - 1 && marks["mark" + (x + 1) + "_" + y] >= 0;
            bool hasTop = y > 0 && marks["mark" + x + "_" + (y - 1)] >= 0;
            bool hasBottom = y < Sphere.sphere.structHei - 1 && marks["mark" + x + "_" + (y + 1)] >= 0;

            int adjacentCount = (hasLeft ? 1 : 0) + (hasRight ? 1 : 0) + (hasTop ? 1 : 0) + (hasBottom ? 1 : 0);

            string markerType = "";

            // 1. 上下左右にマーカーがある場合
            if (hasLeft && hasRight && hasTop && hasBottom)
            {
                markerType = "2";
            }
            // 2. 角である場合（2つが隣接している）
            else if (adjacentCount == 2)
            {
                if (hasTop && hasLeft) // このマーカーの左上にマーカーがある = このマーカーは右下の角
                {
                    markerType = "br";
                }
                else if (hasTop && hasRight) // このマーカーの右上にマーカーがある = このマーカーは左下の角
                {
                    markerType = "bl";
                }
                else if (hasBottom && hasLeft) // このマーカーの左下にマーカーがある = このマーカーは右上の角
                {
                    markerType = "ur";
                }
                else if (hasBottom && hasRight) // このマーカーの右下にマーカーがある = このマーカーは左上の角
                {
                    markerType = "ul";
                }
                else if (hasBottom && hasTop) 
                {
                    markerType = "lr";
                }
                else if (hasLeft && hasRight)
                {
                    markerType = "ub";
                }
            }
            // 3方向に接している場合（1方向だけ接していない）
            else if (adjacentCount == 3)
            {
                if (!hasTop) // 上だけ無い（左、右、下に接している）
                {
                    markerType = "u";
                }
                else if (!hasBottom) // 下だけ無い（左、右、上に接している）
                {
                    markerType = "b";
                }
                else if (!hasLeft) // 左だけ無い（右、上、下に接している）
                {
                    markerType = "l";
                }
                else if (!hasRight) // 右だけ無い（左、上、下に接している）
                {
                    markerType = "r";
                }
            }
            // 4. 上に何もないようなケース（上方向だけに接している場合はrangemarker_b_uを使用）
            // 左右下だけに接している場合も回転させて使用
            // 5. どこか一個だけ接している場合はrangemarker_b_3を使用
            // （ただし、rangemarker_b_3は右だけあいているので、上下左の場合は回転）
            else if (adjacentCount == 1)
            {
                if (hasTop) // 上だけ - 条件4: rangemarker_b_uを使用
                {
                    markerType = "3_bottom";
                }
                else if (hasRight) // 右だけ - 条件5: rangemarker_b_3を使用
                {
                    markerType = "3_left";
                }
                else if (hasBottom) // 下だけ - 条件5: rangemarker_b_3を使用（回転）
                {
                    markerType = "3_top";
                }
                else if (hasLeft) // 左だけ - 条件5: rangemarker_b_3を使用（回転）
                {
                    markerType = "3_right";
                }
            }
            // 6. その他は何もしない

            if (!string.IsNullOrEmpty(markerType))
            {
                keyValue.Value.SetMarkerType(markerType);
            }
        }
    }

    // 
    // セットしたマーカをすべてクリアする。
    public void clearMarker()
    {
        // アクティブなマーカを一つずつ処理する。
        for (int i = 0; i <= actNo; i++)
        {
            string markerName = "no" + i;

            // 変数 markX_Y につけている値をリセット。
            Vector3 _mkpos = markObj[markerName].transform.localPosition;
            marks["mark" + (int)(_mkpos.x / Sphere.TIP_SIZE) + "_" + (int)((_mkpos.y / Sphere.TIP_SIZE) * -1)] = -1;

            // マーカムービを破棄。
            GameObject.Destroy(markObj[markerName].gameObject);
        }

        // アクティブなマーカのナンバーをリセット。
        actNo = -1;
    }

    public void clearOneMarker(string markerName)
    {
        //存在しない場合はリターン
        if (!isExists(markerName)) return;

        // 変数 markX_Y につけている値をリセット。
        Vector3 _mkpos = markObj[markerName].transform.localPosition;
        marks["mark" + (int)(_mkpos.x / Sphere.TIP_SIZE) + "_" + (int)((_mkpos.y / Sphere.TIP_SIZE) * -1)] = -1;

        // マーカムービを破棄。
        GameObject.Destroy(markObj[markerName].gameObject);

    }


    public bool isExists(string markerName)
    {
        //存在しない場合はリターン
        if (!markObj.ContainsKey(markerName)) return false;
        if (markObj[markerName] == null) return false;

        return true;
    }

    public string isExists(int x, int y)
    {
        foreach (KeyValuePair<string, MarkerBehaviour> keyValue in markObj)
        {
            if (keyValue.Value != null)
            {
                Vector3 _mkpos = keyValue.Value.transform.localPosition;

                //同じ位置にすでにある場合はreturn
                if (_mkpos.x == (x * Sphere.TIP_SIZE) && _mkpos.y == (y * Sphere.TIP_SIZE * -1))
                {
                    return keyValue.Key;
                }
            }
        }
        return string.Empty;
    }

    //
    // 変数 x, y で示された場所にマーカーを置く。
    private void setMarker(int x, int y)
    {
        // 新たに置くマーカーの名前を取得。
        string markerName = "no" + ++actNo;

        foreach (KeyValuePair<string, MarkerBehaviour> keyValue in markObj)
        {
            if (keyValue.Value != null)
            {
                Vector3 _mkpos = keyValue.Value.transform.localPosition;

                //同じ位置にすでにある場合は非表示
                if (_mkpos.x == (x * Sphere.TIP_SIZE) && _mkpos.y == (y * Sphere.TIP_SIZE * -1))
                {
                    keyValue.Value.gameObject.SetActive(false);
                }
            }
        }

        // マーカを動的に確保する。
        Image _marker = UnityEngine.Object.Instantiate(source, new Vector3(0, 0, 0), Quaternion.identity, Stage.transform);
        _marker.name = markerName;

        MarkerBehaviour objmark = _marker.transform.GetComponent<MarkerBehaviour>();
        objmark.Init(color);

        // 示された位置にセット。
        objmark.setPos(x * Sphere.TIP_SIZE, y * Sphere.TIP_SIZE);

        markObj[markerName] = objmark;

    }

    //
    // 変数 x, y で示された場所にマーカーを置く。個別用
    public void setOneMarker(string markerName, int x, int y, string color)
    {
        // マーカを動的に確保する。
        Image _marker = UnityEngine.Object.Instantiate(source, new Vector3(0, 0, 0), Quaternion.identity, Stage.transform);
        _marker.name = markerName;

        MarkerBehaviour objmark = _marker.transform.GetComponent<MarkerBehaviour>();
        objmark.Init(color);

        // 示された位置にセット。
        objmark.setPos(x * Sphere.TIP_SIZE, y * Sphere.TIP_SIZE);

        markObj[markerName] = objmark;

    }

    //
    // markラベルのサブルーチン。
    // 変数argX, argYで示されたマスに進入する場合の移動力残余等を計算して、
    // 進入できるなら mark ラベルを再帰コールする。
    void recMarker(int argX, int argY, int mPow)
    {
        int cost = 0;

        // 示されたマスの移動コストを取得。
        string suffix = argX + "_" + argY;
        if (mType == 0)
            cost = 1;
        else
            cost = Stage.cost["cost" + suffix];

        // 移動した場合の移動力残余を変数 remain に求める。
        int remain = mPow - cost;

        // 移動可能、かつ、前回調査よりも高い残余で踏み込めるなら...
        if (remain >= 0 && remain > marks["mark" + suffix])
        {

            // 他勢力のZOCでないか調べる。
            bool zoc = false;

            // 移動タイプがZOC無視でないなら調べる。
            if (mType != 0)
            {

                // ユニットがいるかどうかチェック。
                int unitNo = Sphere.FindUnit(argX, argY);

                // ユニットがいて・・
                if (unitNo > 0)
                {
                    //それが他勢力の所属なら、そこは他勢力のZOC。
                    int u = int.Parse(Sphere.sphere.unit[unitNo].Info.Split(new char[] { ' ' })[1]);
                    if (union != u)
                        zoc = true;
                }
            }

            // ZOCでないなら踏み込める。
            if (!zoc)
            {

                // 再帰callする。スタックカウントを上げて引数をセット。
                stack++;
                x_arr[stack] = argX;
                y_arr[stack] = argY;
                mPow_arr[stack] = remain;

                this.mark();
            }
        }
    }
}
