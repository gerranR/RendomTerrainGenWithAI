using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarManager : MonoBehaviour
{
    public static AStarManager instance;

    private void Awake()
    {
        instance = this;

        //Node[] nodes = FindFirstObjectByType<MapGenerator>().nodeList.ToArray();
        //nodes[Random.Range(0, nodes.Length)].containsPlayer = true;
    }

    public List<Node> GeneratePath(Node start, Node end)
    {
        List<Node> openSet = new List<Node>();
        
        foreach(Node n in FindFirstObjectByType<MapGenerator>().nodeList)
        {
            n.g = float.MaxValue;
        }

        start.g = 0;
        start.h = Vector3.Distance(start.transform.position, end.transform.position);
        openSet.Add(start);

        while(openSet.Count > 0)
        {
            int lowestF = default;

            for(int i = 0; i < openSet.Count; i++)
            {
                if (openSet[i].f() < openSet[lowestF].f())
                {
                    lowestF = i;
                }
            }

            Node currentNode = openSet[lowestF];

            openSet.Remove(currentNode);

            if(currentNode == end)
            {
                List<Node> path = new List<Node>();

                path.Insert(0, end);
                while(currentNode != start)
                {
                    currentNode = currentNode.cameFrom;
                    path.Add(currentNode);
                }

                path.Reverse();
                return path;
            }

            foreach(Node connection in currentNode.conactions)
            {
                float heldG = currentNode.g + Vector3.Distance(currentNode.transform.position, connection.transform.position);

                if(heldG < connection.g)
                {
                    connection.cameFrom = currentNode;
                    connection.g = heldG;
                    connection.h = Vector3.Distance(connection.transform.position, end.transform.position);

                    if(!openSet.Contains(connection))
                    {
                        openSet.Add(connection);
                    }
                }
            }
        }

        return null;
    }
}
