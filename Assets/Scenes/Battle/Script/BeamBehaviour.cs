using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ビームを制御するムービー
/// </summary>
public class BeamBehaviour : MonoBehaviour
{
    public Animator Anim;

    // 発射口の座標
    float BLASTER_X { get; set; }
    float BLASTER_Y { get; set; }

    // グラフィックの有効フレーム数。
    int GRAPH_FRAMES = 125;

    // 起爆タイミングとなるフレーム位置。
    int IGNITE_FRAME = 100;

    // スピードが等しい場合の、モーションにかかる秒数。
    float MOTION_SECS = 0.9f;

    // スピード差によるモーション秒数変動の幅。
    float REVISE_WIDTH = 0.6f;

    // 起動状態かどうかのフラグを初期化。
    public bool motion { get; set; } = false;

    public delegate void OnCompleteDelegate();
    public OnCompleteDelegate CompleteHandler;

    public int targetNo { get; set; }
    string side { get; set; }
    public int target { get; set; }

    float prog { get; set; } = 0.0f;
    bool igniteFlg { get; set; } = false;
    float spd { get; set; }

    public static BeamBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static BeamBehaviour instance;

    private void Start()
    {
        instance = this;
    }

    // ビームグラフィックの位置あわせ。
    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    public void SetSide(string _side)
    {
        side = _side;
    }

    private static readonly int hashBeamShot = Animator.StringToHash("BeamShot");
    private static readonly int hashBeamWait = Animator.StringToHash("BeamWait");

    IEnumerator BeamShot()
    {
        Debug.Log("BeamBehaviour BeamShot run..");
        motion = true;

        Anim.SetFloat("Speed", spd);
        Anim.Play(hashBeamShot);
        yield return null; // ステートの反映に1フレームいる。解せぬ
        yield return new WaitForAnimation(Anim, 0);

        Debug.Log("BeamBehaviour BeamShot end..");

        // 起動フラグをおろして、再び待機する。
        motion = false;
        Anim.Play(hashBeamWait);

    }

    // まだ起爆しておらずタイミングを迎えたなら起爆。
    public void fireIgnaite()
    {
        Debug.Log("BeamBehaviour fireIgnaite run..");

        // まだ起爆しておらずタイミングを迎えたなら起爆。
        if (!igniteFlg)
        {
            CompleteHandler?.Invoke();
            CompleteHandler = null;
            igniteFlg = true;
        }
    }

    // 
    // モーションを開始する。以下の変数がセットされている。
    //     side         どちらが起動したのか。"P" か "E"。
    //     targetNo     何番の発射台を狙って打ち出されたのか。
    //     offsetX      発射台集合のX座標
    //     offsetY      発射台集合のY座標
    public void fire(OnCompleteDelegate _callback)
    {
        Debug.Log("BeamBehaviour fire run.. motion = " + motion);

        CompleteHandler += _callback;

        BLASTER_X = BattleBehaviour.HIT_XP;
        BLASTER_Y = BattleBehaviour.HIT_YP;

        RevengePhaseBehaviour Revenge = RevengePhaseBehaviour.Instance;

        if (!motion)
        {
            AudioManager.Instance.PlaySE("se_beam");

            // どの発射台を狙っているのかを保持。
            target = targetNo;

            // ビーム発射口に座標をあわせる。
            transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(BLASTER_X * ((side == "P") ? 1 : -1), BLASTER_Y, 0);

            Debug.Log("BeamBehaviour fire target = " + target);

            // ビーム目標地点を取得。
            Vector3 _dest = Revenge.getCirclePos(target);

            // 発射口と目標地点から、角度を算出する。
            float mathX = _dest.x - BLASTER_X * ((side == "P") ? 1 : -1);
            float mathY = _dest.y - BLASTER_Y;

            float deg = Mathf.Atan2(mathY, mathX);
            float degree = deg * Mathf.Rad2Deg;

            Quaternion q = transform.localRotation;
            transform.localRotation = Quaternion.Euler(q.x, q.y, degree);

            // ビーム打ち出しの速度設定。
            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            // モーションに何フレームかけるのかを計算。
            float result = mtnFrms();

            // 1フレームでグラフィックがいくつ進捗するかを計算。
            spd = GRAPH_FRAMES / result;

            //Debug.Log("deg = " + deg);
            //Debug.Log("spd = " + spd);

            // 進捗度、起爆フラグをリセットして、モーション実行フレームへgoto。
            prog = 0.0f;
            igniteFlg = false;

            StartCoroutine(BeamShot());
        }

    }

    /// <summary>
    /// 変数 side で示されたほうが起動した場合に、モーションに
    /// 何フレームかけるのかを計算する。
    /// 結果は変数 result にセットする。floatなので注意。
    /// </summary>
    public float mtnFrms()
    {
        // 起動したほうの視点で、スピード差のレートを取得。
        // 起動したほうが速いなら高い値とする。(-1.0～+1.0)
        int spdRate = BattleBehaviour.Instance.battle.spdRate * (side == "P" ? +1 : -1);

        // モーションに何秒かけるかを計算。
        float secs = MOTION_SECS - (spdRate * REVISE_WIDTH);

        // 結果を計算。
        return secs * BattleBehaviour.FRAME_RATE;

    }

}
