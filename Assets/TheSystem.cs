using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheSystem : MonoBehaviour
{
    GameObject obj;


    //For simulation
    public int NumberOfPathing = 0;
    public List<float> timeList = new List<float>();
    public int NumberOfRePathing = 0;
    public int NumberOfAbandon = 0;

    //For generating NPCs
    public GameObject npc;
    public int numOfNPC;
    int[,] NpcStartPanel = new int[85, 50];

    //For generating obstacles
    public GameObject obs;
    private int[,] panel = new int[10, 11];
    private float[] posX = new float[10];
    private float[] posY = new float[11];

    //For generating the grid
    public Node[,] grid1 = new Node[50, 50]; //1st floor
    public Node[,] grid2 = new Node[35, 50]; //2nd floor
    public LayerMask obstacleMask;

    // Start is called before the first frame update
    void Start()
    {
        //Initialize grids and obstacles
        setArrays();
        GenerateObstacles();
        Physics.SyncTransforms();
        GenerateGrid();
        numOfNPC = 32;
       
        foreach(Node n in grid1)
        {
            n.parent = new Node[numOfNPC];
            n.h = new float[numOfNPC];
            n.g = new float[numOfNPC];
        }
        foreach (Node n in grid2)
        {
            n.parent = new Node[numOfNPC];
            n.h = new float[numOfNPC];
            n.g = new float[numOfNPC];
        }

        //Initialize NPCs
        for (int i=0; i<numOfNPC; i++){

            Vector3 v1 = getRandomPos();          
            obj = Instantiate(npc, v1, Quaternion.identity);
            obj.GetComponent<NPC>().index = i;

            GameObject head = obj.transform.GetChild(0).gameObject;
            // Color c = Random.ColorHSV(0f, 1f, 0.1f, 0.5f, 0.5f, 1f);
            Color c = Color.white;
            obj.GetComponent<Renderer>().material.color = c;
            head.GetComponent<Renderer>().material.color = c;
        }

        InvokeRepeating("logNumbers", 20f, 20f);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //For simulating
    void logNumbers()
    {
        UnityEngine.Debug.Log("Number of Paths:" + NumberOfPathing);
        float avg;
        float sum = 0;
        for (int i = 0; i<timeList.Count; i++)
        {
            sum += timeList[i];
        }
        avg = sum / timeList.Count;
        UnityEngine.Debug.Log("Average time:" + avg);
        UnityEngine.Debug.Log("Number of Repathing:" + NumberOfRePathing);
        UnityEngine.Debug.Log("Number of abandoned:" + NumberOfAbandon);
    }

    public void GenerateObstacles()
    {
        int number = 0;
        float h;
        while (number < 10)
        {
            int xx = Random.Range(0, 10);
            int yy = Random.Range(0, 11);

            if (panel[xx, yy] != 1)
            {
                if (yy <= 7)
                {
                    h = 2;
                }
                else
                {
                    h = 32;
                }
                float scaleX = Random.Range(0f, 10f);
                float scaleZ = Random.Range(0f, 10f);
                float rotate = Random.Range(0f, 1f);
                var o = Instantiate(obs, new Vector3(posY[yy], h, posX[xx]), Quaternion.identity) as GameObject;
                o.transform.localScale += new Vector3(scaleX, 0, scaleZ);
                o.transform.Rotate(0, rotate * 180, 0);
                
                number++;
                panel[xx, yy] = 1;
            }

        }

    }

   
    public void GenerateGrid()
    {
        int i, j;

        //floor 1
        for(int x=0; x<50; x++)
        {
            for (int y=0; y<50; y++)
            {
                i = -25 + 2 * x;
                j = -50 + 2 * y;
                Vector3 pos = new Vector3(i + 1f, 0, j + 1f);
                Node n = new Node(pos, 1, x, y);
                if((i<=-7&&i>=-11&&j<=-34&&j>=-40) || (i<=-7&&i>=-11&&j<=36&&j>=30))
                {
                    n.transporter = true;
                }
                if((i < 45 && i >=25 && j<-1) || (i < 45 && i >= 25 && j > 1))
                {
                    n.obstacle = true;
                }
                else if (Physics.CheckSphere(pos, 1.5f, obstacleMask))
                {
                    n.obstacle = true;
                }              
                grid1[x, y] = n;
            }
        }

        //floor 2
        for (int x = 0; x < 35; x++)
        {
            for (int y = 0; y < 50; y++)
            {
                i = -25 + 2 * x;
                j = -50 + 2 * y;
                Vector3 pos = new Vector3(i + 1f, 29.5f, j + 1f);
                Node n = new Node(pos, 2, x, y);
                if ((i<=-7 && i>=-11 && j<=-34 && j>=-40) || (i<=-7 && i>=-11 && j<=36 && j>=30))
                {
                    n.transporter = true;
                }
                if ((i<45&&i>=5&&j<-31) || (i<45&&i>=5&&j>29) || (i<45&&i>=5&&j>-29&&j<28))
                {
                    n.obstacle = true;
                }
                else if (Physics.CheckSphere(pos, 1.5f, obstacleMask))
                {
                    n.obstacle = true;
                }
                if (i < 45 && i >= 5 && !n.obstacle)
                {
                    n.pos.y = (35 - x) * 1.475f;
                }
                grid2[x, y] = n;
            }
        }
    }


    //Random positions for NPC initialization
    Vector3 getRandomPos()
    {
        Node n;
        while (true)
        {
            int xx = Random.Range(0, 85);
            int yy = Random.Range(0, 50);
            if (xx <= 49)
            {
                n = grid1[xx, yy];
            }
            else
            {
                n = grid2[xx - 50, yy];
            }
            if (!n.obstacle && !n.transporter && NpcStartPanel[xx,yy] !=1)
            {
                NpcStartPanel[xx, yy] = 1;
                return new Vector3(n.pos.x, n.pos.y + 3.2f, n.pos.z);
            }
        }

    }

    //Some arrays to help set random locations for obstacles and NPCs
    //To avoid overlapings, waiting areas, ...
    public void setArrays()
    {
        float x1 = -45;
        for (int i = 0; i < 10; i++)
        {
            posX[i] = x1;
            x1 += 10;
        }
        float y1 = -20;
        for (int i = 0; i < 5; i++)
        {
            posY[i] = y1;
            y1 += 10;
        }
        y1 += 20;
        for (int i = 5; i < 8; i++)
        {
            posY[i] = y1;
            y1 += 10;
        }
        y1 = -20;
        for (int i = 8; i < 11; i++)
        {
            posY[i] = y1;
            y1 += 10;
        }

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 11; j++)
            {
                panel[i, j] = 0;
            }
        }
        for (int i = 0; i < 10; i++)
        {
            panel[i, 4] = 1;
            panel[i, 5] = 1;
            panel[i, 10] = 1;
        }
        panel[1, 1] = 1;
        panel[8, 1] = 1;
        panel[1, 9] = 1;
        panel[8, 9] = 1;

        for (int i = 0; i < 85; i++)
        {
            for (int j = 0; j < 50; j++)
            {
                NpcStartPanel[i, j] = 0;
            }
        }
    }

    

}
