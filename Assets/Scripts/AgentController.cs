using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    public string pointName;        // 目的地を受け取るための変数

    private NavMeshAgent agent;     // NavMeshAgentコンポーネントを格納
    private GameObject destination; // 目的地のSphereを格納

    private LineRenderer line;      // LineRendererコンポーネントを格納
    private int count;              // 線の頂点の数を格納

    private string[] pointGoal = { "passage_to_1F", "stair_to_GF", "door_to_1F", "door_to_Outside" };

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

    }


    // Update is called once per frame
    void Update()
    {
        
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
