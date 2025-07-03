using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    public bool useAStar;
    public Node currentNode;
    public List<Node> path = new List<Node>();
    public Node enemyNode;

    public MapGenerator mapGenerator;
    public NavMeshAgent agent;

    public float diffrence = 1;
    public float wanderRange;
    public float radius;
    public float foodcounter;

    private enum State
    {
        Wander,
        FindFood,
        Rest
    };

    State state;

    private bool searched;
    private bool waited;

    public bool preditor;
    private GameObject pray;

    private void Awake()
    {
        state = State.Wander;
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
            case State.Wander:
                ren.material.color = Color.white;
                if (useAStar)
                {
                    if (path.Count > 0)
                    {
                        WalkPath();
                    }
                    else if (!searched)
                    {
                        GetPath();
                        searched = true;
                    }
                    else
                    {
                        searched = false;
                        state = State.Rest;
                    }
                }
                else
                {
                    bool reachedGoal = false;
                    if (agent.isOnNavMesh)
                    {
                        if (agent.remainingDistance < 0.5f)
                        {
                            reachedGoal = true;
                        }
                    }
                    if(!agent.hasPath || ( reachedGoal && !searched))
                    {
                        GetPath();
                        searched = true;
                    }
                    else if(reachedGoal && searched)
                    {
                        searched = false; 
                        FoodCounter();
                        state = State.FindFood;
                    }
                }
                break;
            case State.FindFood:
                ren.material.color = Color.red;

                if (useAStar)
                {
                    if (path.Count > 0 && path[path.Count - 1] == enemyNode)
                    {
                        WalkPath();
                    }
                    else if (currentNode != enemyNode)
                    {
                        path = AStarManager.instance.GeneratePath(currentNode, enemyNode);
                    }
                    else
                    {
                        enemyNode.containsPlayer = false;

                        Node[] nodes = FindObjectsOfType<Node>();
                        nodes[Random.Range(0, nodes.Length)].containsPlayer = true;
                        searched = false;
                        state = State.Rest;
                    }
                }
                else
                {
                    if(!agent.hasPath)
                    {
                        FindFood();
                    }

                    if(preditor)
                    {
                        if(pray != null)
                        {
                            if(agent.remainingDistance < 0.2f)
                            {
                                agent.SetDestination(pray.transform.position);
                            }
                        }
                    }
                    else if(agent.remainingDistance < 0.05f)
                    {
                        FoodCounter();
                        FindFood();
                    }
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
        if (useAStar)
        {
            Node[] nodes = FindFirstObjectByType<MapGenerator>().nodeList.ToArray();
            while (path == null || path.Count == 0)
            {
                Node target = nodes[Random.Range(0, nodes.Length)];
                while (target == currentNode)
                {
                    target = nodes[Random.Range(0, nodes.Length)];
                }
                path = AStarManager.instance.GeneratePath(currentNode, target);
            }
        }
        else
        {
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(transform.position + new Vector3(Random.Range(-wanderRange, wanderRange), 0, Random.Range(-wanderRange, wanderRange)));
            }
        }
    }

    public void FindFood()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        GameObject closestObj = null;

        foreach (Collider col in hitColliders)
        {
            if (preditor)
            {
                if (col.transform.tag == "Animal" && col.gameObject != gameObject)
                {
                    if (closestObj == null) closestObj = col.gameObject;
                    if (Vector3.Distance(transform.position, col.transform.position) < Vector3.Distance(transform.position, closestObj.transform.position))
                    {
                        closestObj = col.gameObject;
                    }
                }
            }
            else
            {
                if (col.transform.tag == "Food")
                {
                    if (closestObj == null) closestObj = col.gameObject;
                    if (Vector3.Distance(transform.position, col.transform.position) < Vector3.Distance(transform.position, closestObj.transform.position))
                    {
                        closestObj = col.gameObject;
                    }
                }
            }
        }
        if (closestObj != null)
        {
            if(preditor)
            {
                pray = closestObj;
            }
            else
            {
                NavMeshHit hit;
                NavMesh.SamplePosition(closestObj.transform.position, out hit, 20f, NavMesh.AllAreas);
                closestObj.transform.position = hit.position;
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            state = State.Wander;
            GetPath();
            FoodCounter();
        }
    }

    public void CheckForEnemy()
    {
        foreach(Node n in currentNode.conactions)
        {
            if(n.containsPlayer)
            {
                enemyNode = n;
                state = State.FindFood;
                break;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!preditor)
        {
            if (collision.transform.tag == "Food")
            {
                foodcounter = 0;
                state = State.Rest;
                Destroy(collision.gameObject);
            }
        }

        if(preditor)
        {
            if(collision.gameObject == pray)
            {
                foodcounter = 0;
                state = State.Rest;
                Destroy(collision.gameObject);
            }
        }

        if (!agent.isOnNavMesh)
        {
            NavMeshSurface navMeshSurface = collision.gameObject.GetComponent<NavMeshSurface>();
            if (navMeshSurface != null)
            {
                navMeshSurface.BuildNavMesh();
                NavMeshHit hit;
                NavMesh.SamplePosition(agent.transform.position, out hit, 20f, NavMesh.AllAreas);
                agent.transform.position = hit.position;
                agent.Warp(agent.transform.position);
            }
        }
    }

    public void RestTimer()
    {
        StartCoroutine(restTime());
        if (waited)
        {
            state = State.Wander;
            GetPath();
            waited = false;

            StopAllCoroutines();
        }
    }

    IEnumerator restTime()
    {
        yield return new WaitForSeconds(5);
        waited = true;
    }

    public void FoodCounter()
    {
        foodcounter++;
        if(foodcounter > 20)
        {
            print("starved");
            Destroy(gameObject);
        }
    }
}
