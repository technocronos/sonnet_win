using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CreateWave;
using Scenes.Common.Scripts;

public class AvatarRecordBehaviour : BaseBehaviour
{

    public AvatarBehaviour Avatar;
    public GameObject Chara;
    string[] formationE;

    List<Dictionary<string, string[]>> monsterlist = new List<Dictionary<string, string[]>>();
    int i = 0;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        Debug.Log("AvatarRecordBehaviour start..");
        setSafearea("AvatarRecordCanvas");

        Dictionary<string, string[]> dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] { "11001", "12001", "13001", "14001" });
        monsterlist.Add(dic_p);

        dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] {"11002", "12002", "13002", "14002"});
        monsterlist.Add(dic_p);

        dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] { "11010", "12003", "13002", "14002" });
        monsterlist.Add(dic_p);


        dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] {"11003", "12004", "13003", "14003" });
        monsterlist.Add(dic_p);

        dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] {"11032", "12032", "13032", "14032" });
        monsterlist.Add(dic_p);

        dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] {"11005", "12005", "13005", "14004" });
        monsterlist.Add(dic_p);

        dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] { "11006", "12005", "13005", "14005" });
        monsterlist.Add(dic_p);

        dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] { "11007", "12005", "13005", "14006" });
        monsterlist.Add(dic_p);

        dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] {"11008", "12007", "13006", "14008" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11011", "12011", "13011", "14011" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11004", "12006", "13004", "14007" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11009", "12008", "13007", "14009" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11014", "12014", "13014", "14014" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11012", "12012", "13012", "14012" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11013", "12013", "13013", "14013" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11015", "12015", "13015", "14015" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11016", "12016", "13016", "14016" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11021", "12021", "13021", "14021" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11018", "12018", "13018", "14018" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11020", "12020", "13020", "14020" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11017", "12017", "13017", "14017" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11022", "12022", "13022", "14022" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11023", "12023", "13023", "14023" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11025", "12025", "13025", "14025" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11026", "12026", "13026", "14026" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11028", "12028", "13028", "14028" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11027", "12027", "13027", "14027" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11024", "12024", "13024", "14024" });
        monsterlist.Add(dic_p);
        dic_p = new Dictionary<string, string[]>();

        dic_p.Add("PLA", new string[] {"11019", "12019", "13019", "14019" });
        monsterlist.Add(dic_p);

        dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] {"11029", "12029", "13029", "14029" });
        monsterlist.Add(dic_p);

        dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] {"11030", "12030", "13030", "14030" });
        monsterlist.Add(dic_p);

        dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] {"11031", "12031", "13031", "14031" });
        monsterlist.Add(dic_p);

        dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] { "11122", "12122", "13122", "14122" });
        monsterlist.Add(dic_p);

        dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] { "11123", "12123", "13123", "14123" });
        monsterlist.Add(dic_p);

        dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] { "11125", "12125", "13125", "14125" });
        monsterlist.Add(dic_p);

        dic_p = new Dictionary<string, string[]>();
        dic_p.Add("PLA", new string[] { "11127", "12127", "13127", "14127" });
        monsterlist.Add(dic_p);

        //monster
        string[] arr= { "1100", "1200", "1300", "1400", "2100", "2200", "2300", "3100", "3200", "4200", "5100", "6001", "9101", "9102", "9103", "9104", "9105", "9106", "9107", "9110", "9111", "9112", "9113", "9114", "9115", "9116", "9117", "9118", "9119", "9120", "9121", "9122", "9123", "9124", "9125", "9126", "9127", "9128", "9129", "9130", "9131", "9132", "9133", "9134", "9135", "9136", "9137", "9138", "9139", "9140", "9141", "9142", "9143", "9144", "9145", "9146", "9147", "9148", "9149", "9150", "9151", "9152", "9153", "9154", "9155", "9156", "9157", "9158", "9159", "9160", "9161", "9162", "9902", "9903", "9904", "9905", "10001", "10002", "10003", "10004", "10005", "10006", "10007", "10008", "10009", "10010", "10011", "10012", "10013", "10014", "10015", "10016", "10017", "10018", "10019", "10020", "10021", "10022", "10023", "10024", "10025", "10026", "10027", "10028", "10029", "10030", "10031", "10032", "10033", "10034", "10035", "10036", "10037", "10038", "10039", "10040", "10041", "10042", "10043", "10044", "10045", "10046", "10047", "10048", "10049", "10050", "10051", "10052", "10053", "10054", "10055", "10056", "10057", "10058", "10059", "10060", "10061", "10062", "10063", "10064", "10065", "10066", "10067", "10068", "10070", "10071", "10072", "10073", "10074", "10075", "10077", "10078", "10079", "10080", "10081", "10082", "10083", "10084", "10085", "10086", "10087", "10088", "10089", "10090", "10091", "10092", "10093", "10094", "10095", "10096", "10097", "10098", "10099", "10100", "10101", "10103", "10104", "10105", "10106", "10107", "10069", "10109", "10076", "10111", "10112", "10113", "10114", "10102", "10116", "10117", "10118", "10119", "10120", "10121", "10122", "10123", "10124", "10125", "20100", "20102", "20103", "20104", "20105", "20106" };
        foreach(string str in arr)
        {
            string[] s = { str };
            Dictionary<string, string[]> dic = new Dictionary<string, string[]>();
            dic.Add("MOB", s);
            monsterlist.Add(dic);
        }



        StartCoroutine(show());



        DispatchEvent(CwEvent.SCENE_READY);
    }

    IEnumerator show()
    {
        while (true) { 
            Debug.Log("AvatarRecordBehaviour show..");

            foreach (string[] arr in monsterlist[i].Values) {
                if (monsterlist[i].ContainsKey("MOB"))
                    formationE = new string[] { "MOB", arr[0] };
                else
                    formationE = new string[] { "PLA", arr[0] , arr[1] , arr[2] , arr[3] };

            }

            this.makeCharaAnim(formationE, Chara);

            StartCoroutine(Avatar.PlayAnim("AvatarWait"));

            yield return new WaitForSeconds(2.0f);

            i++;

            if (monsterlist.Count <= i)
            {
                i = 0;
            }

        }

    }

}
