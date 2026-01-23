using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;

public class HpBehaviour : MonoBehaviour
{
    public Transform gauge;
    public TextMeshProUGUI TextHP;
    public TextMeshProUGUI TextHPMAX;

    int gauge_width = 292;

    // 行動ptを表示するムービー。
    public void show(int unitNo)
    {
        jsonUnit unitinfo = SphereBehaviour.Instance.sphere.unit[unitNo];

        int _hp = unitinfo.Status.hp;
        int _hp_max = unitinfo.Status.maxhp;

        TextHP.text = _hp.ToString();
        TextHPMAX.text = _hp_max.ToString();

        float hp_val = Mathf.Min(_hp, _hp_max);

        // hpゲージを更新。
        int posx = (int)(((hp_val * 1.0f) / _hp_max) * gauge_width);
        gauge.transform.localPosition = new Vector3(posx - gauge_width, 0, 0);

        transform.gameObject.SetActive(true);
    }
}
