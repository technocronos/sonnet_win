using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassEffect
{
    SphereBehaviour Sphere { get; set; }
    StageBehaviour Stage { get; set; }

    private EffectsBehaviour source { get; set; } = null;

    public int num { set; get; } = 0;
    public Dictionary<int, int> posX = new Dictionary<int, int>();
    public Dictionary<int, int> posY = new Dictionary<int, int>();

    public bool effect_play { get; set; } = true;

    public void Init(EffectsBehaviour _source)
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
    }

    //
    // マスエフェクトの再生を開始する。複数の座標に同時にセットできる。
    // 
    // 引数)
    //     type     エフェクトの種類
    //     posNX    再生するマスの座標X。Nは0から始まる連番。
    //     posNY    同Y。
    //     num      座標をセットした数
    //
    // 変数 no で指定されたユニットデータを参照して、ユニットの新規作成を行う。
    public void go(string type)
    {
        effect_play = true;

        // セットされた座標を一つずつ処理していく。
        for (int i = 0; i < num; i++)
        {
            // エフェクトの名前を決定。
            string name = "efc_" + i;

            // ユニットムービーを複製
            EffectsBehaviour _massEffect = UnityEngine.Object.Instantiate(source, new Vector3(0, 0, 0), Quaternion.identity, Stage.transform);
            _massEffect.name = name;
            _massEffect.CompleteHandler += endCallback;

            int _x = posX[i] * Sphere.TIP_SIZE + (Sphere.TIP_SIZE / 2);
            int _y = (posY[i] * Sphere.TIP_SIZE * -1) - (Sphere.TIP_SIZE / 2);

            // 指定されたエフェクトを再生させる。
            _massEffect.playEffects(type, _x, _y);
        }

        //安全のため
        if (num == 0)
            effect_play = false;
    }

    /// <summary>
    /// アニメ終了イベントハンドラ
    /// Sphereがwatch_flgで監視しているので通知
    /// </summary>
    /// <param name="result"></param>
    void endCallback(string result)
    {
        Debug.Log("MassEffect endCallback run..");
        effect_play = false;
    }

}
