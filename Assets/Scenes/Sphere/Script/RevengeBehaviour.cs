using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.UI;

public class RevengeBehaviour : MonoBehaviour
{
    private UnitBehaviour unit;
    SphereBehaviour Sphere { get; set; }
    private int card_type;
    private int index;      // このリベンジオブジェクトのインデックス（0から開始）
    private int totalCount; // リベンジオブジェクトの総数

    public void init(UnitBehaviour _unit, int _card_type, int _index = 0, int _totalCount = 1)
    {
        Sphere = SphereBehaviour.Instance;

        this.unit = _unit;
        this.card_type = _card_type;
        this.index = _index;
        this.totalCount = _totalCount;

        // Resoucesから対象のテクスチャから生成したスプライト一覧を取得
        var _sprite = Resources.Load<Sprite>("Image/Icon/bullet_" + card_type);
        var _image = gameObject.GetComponent<Image>();

        _image.sprite = _sprite;

        StartCoroutine(anim());

    }

    IEnumerator anim()
    {
        if (unit == null)
            yield break;

        // 回転半径（マップチップサイズの半分程度）
        float radius = Sphere.TIP_SIZE * 0.8f;

        // 回転速度（1秒で360度回転）
        float rotationSpeed = 360f;

        // 初期角度を計算（複数のリベンジオブジェクトが均等に分散するように）
        // 360度を総数で割って、インデックス分の角度を加算
        float initialAngleOffset = (360f / totalCount) * index;
        float angle = initialAngleOffset;

        // ユニットの中心位置を計算（左上基準の座標から中心位置へ）
        // UNIT_SIZEの半分を加算して中心位置を取得
        float unitCenterOffsetX = Sphere.UNIT_SIZE * 0.5f;
        float unitCenterOffsetY = Sphere.UNIT_SIZE * 0.5f;

        // ユニットの初期位置を取得（左上基準）
        Vector3 unitTopLeft = unit.transform.localPosition;
        // 中心位置を計算（Y軸が反転しているため、Yは減算）
        Vector3 unitCenter = new Vector3(
            unitTopLeft.x + unitCenterOffsetX,
            unitTopLeft.y - unitCenterOffsetY,
            unitTopLeft.z
        );

        // リベンジオブジェクトの初期位置を設定（初期角度から開始）
        float initialRadian = initialAngleOffset * Mathf.Deg2Rad;
        Vector3 initialOffset = new Vector3(
            Mathf.Cos(initialRadian) * radius,
            Mathf.Sin(initialRadian) * radius,
            0
        );
        transform.localPosition = unitCenter + initialOffset;

        while (true)
        {
            if (Sphere.gamestate.is_gameover || unit == null)
            {
                GameObject.Destroy(transform.gameObject);
                yield break;
            }

            // 角度を更新
            angle += rotationSpeed * Time.deltaTime;
            if (angle >= 360f)
                angle -= 360f;

            // ユニットの現在位置を取得（左上基準）
            Vector3 currentUnitTopLeft = unit.transform.localPosition;
            // 中心位置を計算
            Vector3 currentUnitCenter = new Vector3(
                currentUnitTopLeft.x + unitCenterOffsetX,
                currentUnitTopLeft.y - unitCenterOffsetY,
                currentUnitTopLeft.z
            );

            // 円形に回転する位置を計算
            float radian = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(radian) * radius,
                Mathf.Sin(radian) * radius,
                0
            );

            // リベンジオブジェクトの位置を更新
            transform.localPosition = currentUnitCenter + offset;

            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Sphere.gamestate.is_gameover)
            return;

        if(collision.transform.name.Contains("unit_"))
            StartCoroutine(CatchRoutine(collision.gameObject));
    }

    private IEnumerator CatchRoutine(GameObject collision)
    {
        if (unit != null)
        {
            if (collision.transform.name != unit.name)
            {
                jsonUnit attacker = unit.unitinfo;
                jsonUnit defencer = collision.transform.GetComponent<UnitBehaviour>().unitinfo;

                // スピードバランスを取得。
                double speedBalance = unit.getSpeedBalance(attacker, defencer);

                var damage = unit.calcRevengeDamage(attacker, defencer, 1, speedBalance, card_type);

                var collunit = collision.transform.GetComponent<UnitBehaviour>();

                if (!collunit.death)
                {
                    GameObject.Destroy(transform.gameObject);
                    yield return StartCoroutine(collunit.TakeDamage(damage, attacker));
                }

                yield return null;
            }
        }
    }

}
