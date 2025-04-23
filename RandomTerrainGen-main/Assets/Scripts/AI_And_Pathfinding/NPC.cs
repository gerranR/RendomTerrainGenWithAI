using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public Node currentNode;
    public List<Node> path = new List<Node>();
    public Node enemyNode;

    public MapGenerator mapGenerator;

    public float diffrence = 1;

    private enum State
    {
        Patrol,
        Attack,
        Search,
        Rest
    };

    State state;

    private bool searched;
    private bool waited;

    private void Awake()
    {
        state = State.Patrol;
    }

    public Renderer ren;

    // Update is called once per frame
    void Update()
    {
        StateCheck();

        if(transform.position.y <= -5f)
        {
            Destroy(gameObject);
        }

    }

    public void StateCheck()
    {
        switch (state)
        {
            case State.Patrol:
                ren.material.color = Color.white;

                if (path.Count > 0)
                {
                    WalkPath();
                }
                else if(!searched)
                {
                    GetPath();
                    searched = true;
                }
                else
                {
                    searched = false;
                    state = State.Rest;
                }
                break;
            case State.Attack:
                ren.material.color = Color.red;

                if (path.Count > 0 && path[path.Count - 1] == enemyNode)
                {
                    WalkPath();
                }
                else if(currentNode != enemyNode)
                {
                    path = AStarManager.instance.GeneratePath(currentNode, enemyNode);
                }
                else
                {
                    enemyNode.containsPlayer = false;

                    Node[] nodes = FindObjectsOfType<Node>();
                    nodes[Random.Range(0, nodes.Length)].containsPlayer = true;
                    searched = false;
                    state = State.Search;
                }

                break;
            case State.Search:
                ren.material.color = Color.blue;

                if (path.Count > 0)
                {
                    WalkPath();
                }
                else if(searched == false)
                {
                    searched = true;
                    path = AStarManager.instance.GeneratePath(currentNode, enemyNode.conactions[Random.Range(0, enemyNode.conactions.Count)]);
                }
                else
                {
                    searched = false;
                    state = State.Rest;
                }
                break;
            case State.Rest:
                ren.material.color = Color.black;

                RestTimer();
                break;
        }
    }

    public void WalkPath()
    {
        int x = 0;

        transform.position = Vector3.MoveTowards(transform.position, path[x].transform.position, 3 * Time.deltaTime);

        if(Vector3.Distance(transform.position, path[x].transform.position) < diffrence)
        {
            currentNode = path[x];
            CheckForEnemy();
            path.RemoveAt(x);
        }
    }

    public void GetPath()
    {
        Node[] nodes = FindFirstObjectByType<MapGenerator>().nodeList.ToArray();
        while (path == null || path.Count == 0)
        {
            Node target = nodes[Random.Range(0, nodes.Length)];
            while(target == currentNode)
            {
                target = nodes[Random.Range(0, nodes.Length)];
            }
            path = AStarManager.instance.GeneratePath(currentNode, target);
        }
    }

    public void CheckForEnemy()
    {
        foreach(Node n in currentNode.conactions)
        {
            if(n.containsPlayer)
            {
                enemyNode = n;
                state = State.Attack;
                break;
            }
        }
    }


    public void RestTimer()
    {
        StartCoroutine(restTime());
        if (waited)
        {
            state = State.Patrol;
            waited = false;

            StopAllCoroutines();
        }
    }

    IEnumerator restTime()
    {
        yield return new WaitForSeconds(5);
        waited = true;
    }
}
