using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ### AgentGenerator ###
// 1. エージェントごとに与えられた目的地に向かって移動させる
// 2. 登場するエージェントの頭部には、エージェントの名前を表す文字を出現させる
// 3. L2Bridge.csからHumanTracker上のデータを受け取って、エージェントを移動させる
// 
// Shogo Nishimura / niscode    Dec 2022


public class AgentController : MonoBehaviour
{
    public string pointName;        // 目的地を受け取るための変数

    private NavMeshAgent agent;     // NavMeshAgentコンポーネントを格納
    private GameObject destination; // 目的地のSphereを格納

    private LineRenderer line;      // LineRendererコンポーネントを格納
    private int count;              // 線の頂点の数を格納

    private string[] pointGoal = { "passage_to_1F", "stair_to_GF", "door_to_1F", "door_to_Outside" };


    // show UI
    private GameObject ui_Canvas;
    public Text text_ID;
  


    void Start()
    {
        // NavMeshAgentコンポーネントと目的地のオブジェクトを取得
        agent = GetComponent<NavMeshAgent>();
        destination = GameObject.Find(pointName);  // 目的地に設定した地点名を指定

        if (agent.pathStatus != NavMeshPathStatus.PathInvalid)
        {
            // 目的地を指定(目的地のオブジェクトの位置情報を渡す）
            agent.SetDestination(destination.transform.position);
        }

        line = GetComponent<LineRenderer>(); // LineRendererコンポーネントを取得


        // show id on top of agent
        ui_Canvas = transform.Find("ui_Canvas").gameObject;

        Text id = Instantiate(text_ID, transform);
        id.name = "id_" + gameObject.name;
        id.text = gameObject.name;
        id.transform.SetParent(ui_Canvas.transform);
        // そのままインスタンス化すると、{pos: 0,200,150} / {scale: 2.5,2.5,2.5} に出現するためこれを変更
        RectTransform rect = id.GetComponent<RectTransform>();
        rect.localPosition = new Vector3(0f, 25f, 15f);
        rect.localScale = new Vector3(0.25f, 0.25f, 0.25f);
    }


    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))    // 左クリックした時
        {
            text_ID.enabled = false;
        }

        if (Input.GetMouseButtonDown(1))    // 右クリックした時
        {
            text_ID.enabled = true;
        }
    }

    void FixedUpdate() // updateでもいいけど，fixedのほうが今回都合がいい
    {
        count += 1; // 頂点数を１つ増やす
        line.positionCount = count; // 頂点数の更新
        line.SetPosition(count - 1, transform.position); // オブジェクトの位置情報をセット
    }

    private void OnTriggerEnter(Collider collision)
    {
        for (int i = 0; i < pointGoal.Length; i++)
        {
            if (collision.name == pointGoal[i])
            {
                Destroy(gameObject);
                Debug.Log(gameObject.name + " は目的地に辿り着いた！");
            }
        }
    }

    private void OnDestroy()
    {
        
    }
}
