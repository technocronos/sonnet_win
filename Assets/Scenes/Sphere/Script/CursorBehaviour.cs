using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorBehaviour : MonoBehaviour
{
    private Sprite[] _sprites { get; set; }
    private Image _image { get; set; }
    private int _count { get; set; } = 0;
    private int _frame { get; set; } = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Resoucesから対象のテクスチャから生成したスプライト一覧を取得
        _sprites = Resources.LoadAll<Sprite>("Image/marker");
        _image = gameObject.GetComponent<Image>();

        _frame = _sprites.Length;

        // カーソルを動かすようにする。
        StartCoroutine("anim");
    }

    IEnumerator anim()
    {
        while (true)
        {
            //0.05秒に一回
            yield return new WaitForSeconds(0.05f);

            if (_count >= _frame)
                _count = 0;

            _image.sprite = _sprites[_count];
            _count++;
        }
    }

}
