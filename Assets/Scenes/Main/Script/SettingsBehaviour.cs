using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsBehaviour : MonoBehaviour
{

    public GameObject FrameBTC;
    public GameObject FlameEth;

    private jsonConstants constants;

    // Start is called before the first frame update
    void Start()
    {
        constants = APIConnectManager.Instance.login.constants;
        if (!constants.VCOIN_RELEASE_FLG)
        {
            FrameBTC.SetActive(false);
        }
        if (!constants.ETH_ADDR_OPEN)
        {
            FlameEth.SetActive(false);
        }

        if (Main.Instance.in_apply)
        {
            //apple申請中の場合は非表示
            FrameBTC.SetActive(false);
            FlameEth.SetActive(false);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
