using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class NPC : MonoBehaviour
{
    public int index;
    public bool ifJPS;

    //For path finding
    public Node targetNode;
    public Node currNode;
    public List<Node> currPath = new List<Node>();

    //For going along a path
    public Node prevNode;
    public Node nextDestNode;
    Node blockNode;

    //Manage pausing, walking, and waitng at transporters
    public bool pathFound;
    bool pause;
    int pauseTime;
    public bool waiting;

    //Other scripts
    public TheSystem GroundScript;
    public Transporter TranScript1;
    public Transporter TranScript2;

    // head of the NPC, for setting color
    GameObject head;



    // Start is called before the first frame update
    void Start()
    {

        GameObject ground = GameObject.Find("TheSystem");
        GroundScript = ground.GetComponent<TheSystem>();
        GameObject tran1 = GameObject.Find("Transporter1");
        TranScript1 = tran1.GetComponent<Transporter>();
        GameObject tran2 = GameObject.Find("Transporter2");
        TranScript2 = tran2.GetComponent<Transporter>();

        head = transform.GetChild(0).gameObject;

        ifJPS = false;

        pathFound = false;
        blockNode = null;
        pause = false;
        waiting = false;
        getCurrNode();
        currNode.NPC_onThis = true;
        getRandomTargetNode();
        FindPath();

    }

    // Update is called once per frame
    void Update()
    {
        // Set colors to NPCs using transporters
        // Blue: at 2nd floor, red: at 1st floor
        // Black: moving with transporters
        if (TranScript1.NPC_Moving.Contains(this.gameObject) || TranScript2.NPC_Moving.Contains(this.gameObject))
        {
            GetComponent<Renderer>().material.color = Color.black;
            head.GetComponent<Renderer>().material.color = Color.black;
        }
        else if (TranScript1.NPC_Up.Contains(this.gameObject) || TranScript2.NPC_Up.Contains(this.gameObject))
        {
            GetComponent<Renderer>().material.color = Color.red;
            head.GetComponent<Renderer>().material.color = Color.red;
        }
        else if (TranScript1.NPC_Down.Contains(this.gameObject) || TranScript2.NPC_Down.Contains(this.gameObject))
        {
            GetComponent<Renderer>().material.color = Color.blue;
            head.GetComponent<Renderer>().material.color = Color.blue;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.white;
            head.GetComponent<Renderer>().material.color = Color.white;
        }


        if (pause)                             // Pause after finishing a path
        {
            pauseTime++;
            if (pauseTime >= 200)
            {
                pause = false;
                pauseTime = 0;
            }
        }
        else
        {
            if (pathFound)
            {
                walk();                        // Walk along a path
            }
            else
            {
                getCurrNode();                 // Cannot find a path to current target.
                currNode.NPC_onThis = true;    // Change Target.
                getRandomTargetNode();
                FindPath();
                GroundScript.NumberOfAbandon++;
            }
        }

    }
    //--------------------------NPC Movmement Controlling ------------------------------------------------//
    void FindPath()
    {
        clearValues();
        if (ifJPS)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            RunJPS(currNode, targetNode, blockNode);
            sw.Stop();
            GroundScript.timeList.Add(sw.Elapsed.Milliseconds);

        }
        else
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            A_Star(currNode, targetNode);
            sw.Stop();
            GroundScript.timeList.Add(sw.Elapsed.Milliseconds);
        }
    }
    void walk()
    {
        prevNode = currNode;
        getCurrNode();
        prevNode.NPC_onThis = false;
        currNode.NPC_onThis = true;
        if (waiting)  //Check if NPC is Waiting for transporter
        {
            return;
        }
        if (currNode != targetNode)
        {
            // Update next node on the path
            if (currPath.Contains(currNode))
            {
                int i = currPath.IndexOf(currNode);
                nextDestNode = currPath[i + 1];
            }
            //Repath to target when next node of path is blocked
            if (nextDestNode.NPC_onThis)
            {
                blockNode = nextDestNode;
                FindPath();
                blockNode = null;
                GroundScript.NumberOfRePathing++;
            }
            //Operations when NPC wants to use transporter
            else if (currNode.transporter && nextDestNode.floor != currNode.floor)
            {
                waiting = true;
                if (currNode.pos.z < 0)
                {
                    TranScript1.AddWaitingNPC(this.gameObject, currNode.floor);
                }
                else
                {
                    TranScript2.AddWaitingNPC(this.gameObject, currNode.floor);
                }
            }
            // Translate to next node on the path
            else
            {
                Vector3 d = new Vector3(nextDestNode.pos.x, nextDestNode.pos.y + 3.2f, nextDestNode.pos.z);
                transform.position = Vector3.MoveTowards(transform.position, d, 8 * Time.deltaTime);
            }
        }

        //If we arrive the target,
        //Pause and find new target and path.
        else
        {
            pause = true;
            pauseTime = 0;
            pathFound = false;
            getRandomTargetNode();
            FindPath();
        }
    }
    void getRandomTargetNode()
    {
        Node n;
        bool ok = false;
        while (!ok)
        {
            int xx = Random.Range(0, 84);
            int yy = Random.Range(0, 50);
            if (xx <= 49)
            {
                n = GroundScript.grid1[xx, yy];
            }
            else
            {
                n = GroundScript.grid2[xx - 50, yy];
            }
            if (!n.obstacle && !n.transporter)
            {
                targetNode = n;
                ok = true;
            }
        }

    }
    public void getCurrNode()
    {
        float posX = transform.position.x;
        int xx = (int)((posX - (-25)) / 2);
        float posZ = transform.position.z;
        int zz = (int)((posZ - (-50)) / 2);
        float posY = transform.position.y;
        if (posY > 3.2f && xx <= 34)
        {
            currNode = GroundScript.grid2[xx, zz];
        }
        else
        {
            currNode = GroundScript.grid1[xx, zz];
        }
    }

    void clearValues()
    {
        foreach (Node n in GroundScript.grid1)
        {
            n.parent[index] = null;
            n.h[index] = 0;
            n.g[index] = 0;
        }
        foreach (Node n in GroundScript.grid2)
        {
            n.parent[index] = null;
            n.h[index] = 0;
            n.g[index] = 0;
        }
    }


    //--------------------------A* PATH FINDING ------------------------------------------------//
    public void A_Star(Node start, Node target)
    {

        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();

        open.Add(start);
        while (open.Count > 0)
        {
            Node n = open[0];
            for (int i = 1; i < open.Count; i++)
            {
                // f= g+h
                if (open[i].g[index] + open[i].h[index] <= n.g[index] + n.h[index] && open[i].h[index] < n.h[index])
                {
                    n = open[i];
                }
            }
            open.Remove(n);
            if (!closed.Contains(n))
            {
                closed.Add(n);
            }

            if (n == target)
            {
                currPath = new List<Node>();
                Node pathNode = target;
                while (pathNode != start)
                {
                    currPath.Add(pathNode);
                    pathNode = pathNode.parent[index];
                }
                pathFound = true;
                currPath.Reverse();
                nextDestNode = currPath[0];
                GroundScript.NumberOfPathing++;
                return;
            }

            List<Node> neighbourList = getNeighbourList(n);
            foreach (Node neighbour in neighbourList)
            {
                if (!neighbour.obstacle && !closed.Contains(neighbour) && neighbour != blockNode)
                {
                    float new_g = n.g[index] + distance(n, neighbour);
                    if (new_g < neighbour.g[index] || !open.Contains(neighbour))
                    {
                        neighbour.g[index] = new_g;
                        neighbour.h[index] = distance(neighbour, target);
                        neighbour.parent[index] = n;

                        if (!open.Contains(neighbour))
                        {
                            open.Add(neighbour);
                        }
                    }
                }
            }
        }
    }

    //Neighbour List For A*
    public List<Node> getNeighbourList(Node n)
    {
        List<Node> neighbourList = new List<Node>();

        //Transporters
        if (n.transporter)
        {
            if (n.floor == 1)
            {
                neighbourList.Add(GroundScript.grid2[n.indexX, n.indexY]);
            }
            else
            {
                neighbourList.Add(GroundScript.grid1[n.indexX, n.indexY]);
            }
        }
        //Bridges
        if (n.floor == 1 && n.indexX == 35 && n.indexY == 10)
        {
            neighbourList.Add(GroundScript.grid2[34, 10]);
        }
        else if (n.floor == 1 && n.indexX == 35 && n.indexY == 39)
        {
            neighbourList.Add(GroundScript.grid2[34, 39]);
        }
        else if (n.floor == 2 && n.indexX == 34 && n.indexY == 10)
        {
            neighbourList.Add(GroundScript.grid1[35, 10]);
        }
        else if (n.floor == 2 && n.indexX == 34 && n.indexY == 39)
        {
            neighbourList.Add(GroundScript.grid1[35, 39]);
        }
        //Neighbours at same floor
        //Using 8-ways
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (i != 1 || j != 1)
                {
                    int neighbourX = n.indexX - 1 + i;
                    int neighbourY = n.indexY - 1 + j;
                    if (n.floor == 1 && neighbourX >= 0 && neighbourX < 50 && neighbourY >= 0 && neighbourY < 50)
                    {
                        neighbourList.Add(GroundScript.grid1[neighbourX, neighbourY]);
                    }
                    else if (n.floor == 2 && neighbourX >= 0 && neighbourX < 35 && neighbourY >= 0 && neighbourY < 50)
                    {
                        neighbourList.Add(GroundScript.grid2[neighbourX, neighbourY]);
                    }
                }
            }
        }
        return neighbourList;
    }

    //--------------------------------DISTANCE FUNCTION------------------------------------------------//
    //Using Octile Distance   
    float distance(Node node, Node goal)
    {
        int dx = Mathf.Abs(node.indexX - goal.indexX);
        int dy = Mathf.Abs(node.indexY - goal.indexY);
        float dst = (dx + dy) + (1.4f - 2f) * Mathf.Min(dx, dy);
        return dst;
    }


    //--------------------------------JPS PATH FINDING ------------------------------------------------//
    public bool RunJPS(Node start, Node target, Node block)
    {
        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();
        open.Add(start);
        while (open.Count > 0)
        {
            Node n = open[0];
            for (int i = 1; i < open.Count; i++)
            {
                // f= g+h
                if (open[i].g[index] + open[i].h[index] <= n.g[index] + n.h[index] && open[i].h[index] < n.h[index])
                {
                    n = open[i];
                }
            }
            open.Remove(n);
            if (n == target)
            {
                currPath = new List<Node>();
                Node pathNode = target;
                while (pathNode != start)
                {
                    currPath.Add(pathNode);
                    pathNode = pathNode.parent[index];
                }
                pathFound = true;
                currPath.Reverse();
                nextDestNode = currPath[0];
                GroundScript.NumberOfPathing++;
                return true;
            }
            if (!closed.Contains(n))
            {
                closed.Add(n);
            }
            List<Node> successors = GetSuccessors(n, target);
            foreach (Node node in successors)
            {
                if (!closed.Contains(node) && node != block)
                {
                    float new_g = n.g[index] + distance(n, node);
                    if (new_g < node.g[index] || !open.Contains(node))
                    {
                        node.g[index] = new_g;
                        node.h[index] = distance(node, target);
                        node.parent[index] = n;

                        if (!open.Contains(node))
                        {
                            open.Add(node);
                        }
                    }
                }
            }
        }
        return false;

    }

    private List<Node> GetSuccessors(Node n, Node target)
    {
        Node jumpPoint;
        List<Node> s = new List<Node>();
        List<Node> floorNeighbours = GetFloorNeighbours_JPS(n);
        List<Node> neighbours = GetNeighboursList_JPS(n);
        foreach (Node neigh in floorNeighbours)
        {
            s.Add(neigh);
        }
        foreach (Node neighbour in neighbours)
        {
            jumpPoint = Jump(neighbour, n, target);
            if (jumpPoint != null)
            {
                s.Add(jumpPoint);
            }
        }
        return s;
    }

    private Node Jump(Node n, Node parent, Node target)
    {
        int f;
        Node[,] grid;
        if (n == null)
        {
            return null;
        }

        if (n.floor == 1)
        {
            f = 1;
            grid = GroundScript.grid1;
        }
        else
        {
            f = 2;
            grid = GroundScript.grid2;
        }

        int x = n.indexX;
        int y = n.indexY;
        int dx = n.indexX - parent.indexX;
        int dy = n.indexY - parent.indexY;

        if (!walkable(n.indexX, n.indexY, f))
        {
            return null;
        }
        if (target == n)
        {
            return n;
        }
        // Diagonal
        if (dx != 0 && dy != 0)
        {
            if ((walkable(x - dx, y + dy, f) && !walkable(x - dx, y, f)) ||
                (walkable(x + dx, y - dy, f) && !walkable(x, y - dy, f)))
            {
                return n;
            }
            if (Jump(onGrid(x + dx, y, f), n, target) != null ||
                Jump(onGrid(x, y + dy, f), n, target) != null)
            {
                return n;
            }
        }
        else
        {
            if (dx != 0)
            {   // Horizontal
                if ((walkable(x + dx, y + 1, f) && !walkable(x, y + 1, f)) ||
                    (walkable(x + dx, y - 1, f) && !walkable(x, y - 1, f)))
                {
                    return n;
                }
            }
            else
            {   // Vertical
                if ((walkable(x + 1, y + dy, f) && !walkable(x + 1, y, f)) ||
                    (walkable(x - 1, y + dy, f) && !walkable(x - 1, y, f)))
                {
                    return n;
                }
            }
        }
        if (walkable(x + dx, y, f) || walkable(x, y + dy, f))
        {
            return Jump(onGrid(x + dx, y + dy, f), n, target);
        }
        return null;
    }

    // Helpers for JPS
    public bool walkable(int x, int y, int floor)
    {
        if (floor == 1)
        {
            return (x >= 0 && x < 50) && (y >= 0 && y < 50) && (!GroundScript.grid1[x, y].obstacle);
        }
        else
        {
            return (x >= 0 && x < 35) && (y >= 0 && y < 50) && (!GroundScript.grid2[x, y].obstacle);
        }

    }
    public Node onGrid(int x, int y, int floor)
    {
        if (floor == 1)
        {
            if ((x >= 0 && x < 50) && (y >= 0 && y < 50))
            {
                return GroundScript.grid1[x, y];
            }
            return null;
        }
        else
        {
            if ((x >= 0 && x < 35) && (y >= 0 && y < 50))
            {
                return GroundScript.grid2[x, y];
            }
            return null;
        }

    }

    // Get Neighbours for JPS
    public List<Node> GetFloorNeighbours_JPS(Node n)
    {
        List<Node> neighbourList = new List<Node>();
        //Transporters
        if (n.transporter)
        {
            if (n.floor == 1)
            {
                neighbourList.Add(GroundScript.grid2[n.indexX, n.indexY]);
            }
            else
            {
                neighbourList.Add(GroundScript.grid1[n.indexX, n.indexY]);
            }
        }
        //Bridges
        if (n.floor == 1 && n.indexX == 35 && n.indexY == 10)
        {
            neighbourList.Add(GroundScript.grid2[34, 10]);
        }
        else if (n.floor == 1 && n.indexX == 35 && n.indexY == 39)
        {
            neighbourList.Add(GroundScript.grid2[34, 39]);
        }
        else if (n.floor == 2 && n.indexX == 34 && n.indexY == 10)
        {
            neighbourList.Add(GroundScript.grid1[35, 10]);
        }
        else if (n.floor == 2 && n.indexX == 34 && n.indexY == 39)
        {
            neighbourList.Add(GroundScript.grid1[35, 39]);
        }

        return neighbourList;
    }
    public List<Node> GetNeighboursList_JPS(Node n)
    {
        List<Node> neighbourList = new List<Node>();

        int f;
        Node[,] grid;
        if (n == null)
        {
            return null;
        }
        if (n.floor == 1)
        {
            f = 1;
            grid = GroundScript.grid1;
        }
        else
        {
            f = 2;
            grid = GroundScript.grid2;
        }

        Node parent = n.parent[index];

        if (parent == null || (n.indexX == parent.indexX && n.indexY == parent.indexY))
        {
            //Neighbours at same floor
            //Using 8-ways
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (i != 1 || j != 1)
                    {
                        int neighbourX = n.indexX - 1 + i;
                        int neighbourY = n.indexY - 1 + j;
                        if (n.floor == 1 && neighbourX >= 0 && neighbourX < 50 && neighbourY >= 0 && neighbourY < 50)
                        {
                            neighbourList.Add(GroundScript.grid1[neighbourX, neighbourY]);
                        }
                        else if (n.floor == 2 && neighbourX >= 0 && neighbourX < 35 && neighbourY >= 0 && neighbourY < 50)
                        {
                            neighbourList.Add(GroundScript.grid2[neighbourX, neighbourY]);
                        }
                    }
                }
            }
        }
        else
        {
            int x = n.indexX;
            int y = n.indexY;
            int dx = n.indexX - parent.indexX;
            int dy = n.indexY - parent.indexY;
            if (dx != 0 && dy != 0)
            {
                bool left = walkable(x - dx, y, f);
                bool right = walkable(x + dx, y, f);
                bool up = walkable(x, y + dy, f);
                bool down = walkable(x, y - dy, f);
                if (up)
                {
                    neighbourList.Add(grid[x, y + dy]);
                }
                if (right)
                {
                    neighbourList.Add(grid[x + dx, y]);
                }
                if ((up || right) && walkable(x + dx, y + dy, f))
                {
                    neighbourList.Add(grid[x + dx, y + dy]);
                }
                if (!left && up && walkable(x - dx, y + dy, f))
                {
                    neighbourList.Add(grid[x - dx, y + dy]);
                }
                if (!down && right && walkable(x + dx, y - dy, f))
                {
                    neighbourList.Add(grid[x + dx, y - dy]);
                }
            }
            else
            {
                if (dx != 0)
                {
                    if (walkable(x + dx, y, f))
                    {
                        neighbourList.Add(grid[x + dx, y]);
                        if (!walkable(x, y + 1, f))
                        {
                            neighbourList.Add(grid[x + dx, y + 1]);
                        }
                        if (!walkable(x, y - 1, f))
                        {
                            neighbourList.Add(grid[x + dx, y - 1]);
                        }
                    }
                }
                else
                {
                    if (walkable(x, y + dy, f))
                    {
                        if (dy != 0) neighbourList.Add(grid[x, y + dy]);
                        if (!walkable(x + 1, y, f) && walkable(x + 1, y + dy, f))
                        {
                            neighbourList.Add(grid[x + 1, y + dy]);
                        }
                        if (!walkable(x - 1, y, f) && walkable(x - 1, y + dy, f))
                        {
                            neighbourList.Add(grid[x - 1, y + dy]);
                        }
                    }
                }
            }
        }
        return neighbourList;
    }


}
