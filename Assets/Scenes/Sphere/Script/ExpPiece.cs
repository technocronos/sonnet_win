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
        while (true)
        {
            yield return null;
            var pos = Camera.main.WorldToViewportPoint(transform.position);
            if (pos.x > 1.5f || pos.x < -0.5f || pos.y > 1.5f || pos.y < -0.5f) unit.DespawnExp(this);
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

}
