using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpPiece : MonoBehaviour
{
    // Start is called before the first frame update
    private const float distance = -0.2f;//2.0f;
    private int add_exp = 1;
    public int GetExpAmount()
    {
        return add_exp;
    }

    private UnitBehaviour unit;

    public void Setup(UnitBehaviour unit, Vector3 position)
    {
        this.unit = unit;
        gameObject.SetActive(true);
        transform.SetLocalPositionAndRotation(position, Quaternion.identity);
        transform.Rotate(Vector3.right, -30f);

        this.StartCoroutine(this.Play());
    }

    private IEnumerator Play()
    {
        {
            var dir = Vector3.up;//(target.transform.position - this.transform.position).normalized;
            var pos1 = this.transform.position;
            var pos2 = this.transform.position - dir * distance;
            var elapsed = 0.0f;
            while (elapsed <= 1.0f)
            {
                this.transform.position = Vector3.Lerp(pos1 - 0.1f * Vector3.up, pos2, elapsed);
                elapsed += Time.deltaTime * 16;
                yield return null;
            }
            while (elapsed <= 2.0f)
            {
                this.transform.position = Vector3.Lerp(pos2, pos1, elapsed - 1f);
                elapsed += Time.deltaTime * 8;
                yield return null;
            }
        }
        // 経験値が落ちている位置をマップ座標に変換（初期位置を保存）
        var sphere = SphereBehaviour.Instance;
        float expX = transform.localPosition.x / sphere.TIP_SIZE;
        float expY = (transform.localPosition.y * -1) / sphere.TIP_SIZE;
        
        // その位置のコストをチェック
        int checkX = (int) Mathf.Floor(expX);
        int checkY = (int) Mathf.Floor(expY);
        int maxCost = GetCost(checkX, checkY);
        
        bool isCost9999 = (maxCost >= 9990);
        
        while (true)
        {
            yield return null;
            
            if (isCost9999 && unit != null)
            {
                // コスト9999の場所に落ちている場合、プレイヤーが前後左右1チップ分以内に近づいたらDespawnExp
                Vector3 playerPos = unit.transform.localPosition;
                
                float diffX = Mathf.Abs(transform.localPosition.x - playerPos.x);
                float diffY = Mathf.Abs(transform.localPosition.y - playerPos.y);
                
                if (diffX <= sphere.TIP_SIZE && diffY <= sphere.TIP_SIZE)
                {
                   OnComplete();
                    yield break;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        StartCoroutine(CatchRoutine(collision.gameObject));
    }

    private IEnumerator CatchRoutine(GameObject collision)
    {
        if(unit != null)
        {
            if (collision.transform.name == unit.name)
            {
                var pos1 = this.transform.position;
                var elapsed = 0.0f;
                while (elapsed <= 1.0f)
                {
                    if (unit == null) yield break;

                    this.transform.position = Vector3.Lerp(pos1, unit.transform.position, elapsed);
                    elapsed += Time.deltaTime * 4;
                    yield return null;
                }
                this.OnComplete();
            }
        }
    }

    private void OnComplete()
    {
        AudioManager.Instance.PlaySE("se_coin");
        unit.AddExp(add_exp);
        unit.DespawnExp(this);
    }

    private int GetCost(int x, int y)
    {
        string costKey = "cost" + x + "_" + y;
        var stage = StageBehaviour.Instance;
        if (stage != null && stage.cost.ContainsKey(costKey))
            return (int)stage.cost[costKey];
        return 9999; // 到達不能
    }

}
