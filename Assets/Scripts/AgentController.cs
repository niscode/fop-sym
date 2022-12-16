using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 1. エージェントごとに与えられた目的地に向かって移動させる
// 2. 登場するエージェントの頭部には、エージェントの名前を表す文字を出現させる
// 3. L2Bridge.csからHumanTracker上のデータを受け取って、エージェントを移動させる
// 
// Shogo Nishimura / niscode    Dec 2022


public class AgentController : MonoBehaviour
{
    public GameObject agent;  // AgentのPrefabを格納
    public GameObject[] pointsArray = new GameObject[9];  // 目的地を格納

    private string[] pointsNameArray = new string[9];  // 目的地を格納
    private int agent_num = 20; // エージェントを生成する数
    private int points_num = 9;  // 目的地の数


    // Start is called before the first frame update
    void Start()
    {
        // 目的地となるゲームオブジェクトを取得し、名前を抽出する
        for (int i = 0; i < points_num; i++)
        {
            pointsNameArray[i] = pointsArray[i].name;
        }

        // 生成するエージェントごとにpointNameにランダムで選ばれた目的地を代入していく
        for (int i=0; i < agent_num; i++)
        {
            int rand = UnityEngine.Random.Range(0, points_num);
            string human_id = pointsNameArray[rand];
            agent.GetComponent<AgentController>().pointName = human_id;  // 目的地の情報をAgentControllerに渡す
            GameObject obj = Instantiate(agent, new Vector3(0f, 0.6f, 0f), new Quaternion(0f, 0f, 0f, 0f));
            obj.name = "agent_" + i;
        }

    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
