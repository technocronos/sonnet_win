using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StarStoreBehaviour : MonoBehaviour
{
    public Animator StarAnim;

    public TextMeshProUGUI TextVal;

    public static StarStoreBehaviour Instance
    {
        get
        {
            return instance;
        }
    }

    private static StarStoreBehaviour instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    public Dictionary<int, int> starTypes = new Dictionary<int, int>();

    // スターのカウンタ変数
    public int value { get; set; } = 0;
    public int popType { get; set; } = 0;

    //
    // 指定されたタイプのスターを、指定された数だけ追加するcall用ラベル。
    //
    // 引数)
    //      pushType    スターのタイプ
    //      num         追加する数
    public void Push(int pushType, int num)
    {
        AudioManager.Instance.PlaySE("se_getprice");

        for (int i = 0; i < num; i++)
        {
            // スターのカウンタ変数アップ。
            value++;
            setText(value);

            // 疑似配列 starTypes にpushTypeで指定されている属性を保持。
            starTypes[value] = pushType;
        }

        // カウントアップ用のモーションを再生する。
        StartCoroutine(PlayAnim("StarStoreCharge" + pushType));

    }

    //
    // スターを一つ取り出すcall用ラベル。
    // call後、変数 popType を参照することによって受け取る。
    // もうスターがない場合、popType は 0 になる。
    public void Pop()
    {

        if (value <= 0)
        {
            // もうないなら 0。
            popType = 0;
        }
        else
        {
            // まだあるなら、疑似配列 starTypes から取得。
            // そのあと value をカウントダウンする。
            popType = starTypes[value--];
            setText(value);
        }

    }

    /// <summary>
    // リベンジを発動するタイミングで呼ばれる。
    // 保持しているスターを倍増する。
    /// </summary>
    public void Amp()
    {
        int count = value;
        for (int i = 0; i < count; i++)
        {
            value++;
            transform.Find("TextVal").GetComponent<TextMeshProUGUI>().text = (value + 1).ToString();

            starTypes[value] = starTypes[i + 1];
        }

    }

    public IEnumerator PlayAnim(string anim)
    {
        Debug.Log("StarStoreBehaviour PlayAnim start.. anim = " + anim);

        int hashAnim = Animator.StringToHash(anim);
        StarAnim.Play(hashAnim);

        yield return null;
        yield return new WaitForAnimation(StarAnim, 0);

        Debug.Log("StarStoreBehaviour PlayAnim end..");

        int hashIdle = Animator.StringToHash("StarStoreNorm");
        StarAnim.Play(hashIdle);
    }

    /// <summary>
    /// スター数を書き換える
    /// 
    /// </summary>
    /// <param name="val"></param>
    public void setText(int val)
    {
        TextVal.text = val.ToString();
    }

}
