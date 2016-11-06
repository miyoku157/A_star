using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

public class LoadTerrain : MonoBehaviour
{
    List<List<GameObject>> Terrain;

    GameObject player;
    KeyValuePair<GameObject, List<KeyValuePair<int, int>>> ennemy;
    int width;
    int height;
    // Use this for initialization
    void Start()
    {
        int i = 0;
        int j = 0;
        TextAsset terr = Resources.Load("terrain") as TextAsset;
        string text = terr.text;
        string[] lines = Regex.Split(text, "\n");
        for (int n = 0; n < lines.GetLength(0); n++)
        {
            lines[n] = lines[n].Trim();
        }
        Terrain = new List<List<GameObject>>();

        foreach (string line in lines)
        {
            string[] values = Regex.Split(line, ";");
            Terrain.Add(new List<GameObject>());
            j = 0;
            foreach (string value in values)
            {

                Terrain[i].Add(GameObject.CreatePrimitive(PrimitiveType.Plane));
                Terrain[i][j].transform.position = new Vector3(i, 0, j);
                Terrain[i][j].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                if (int.Parse(value) == 1)
                {
                    Terrain[i][j].GetComponent<MeshRenderer>().material.color = Color.red;
                }
                else if (int.Parse(value) == 2)
                {
                    Terrain[i][j].GetComponent<MeshRenderer>().material.color = Color.green;
                    player = Terrain[i][j];
                }
                else if (int.Parse(value) == 3)
                {
                    Terrain[i][j].GetComponent<MeshRenderer>().material.color = Color.black;
                    ennemy = new KeyValuePair<GameObject, List<KeyValuePair<int, int>>>(Terrain[i][j], new List<KeyValuePair<int, int>>());
                }
                else
                {
                    Terrain[i][j].GetComponent<MeshRenderer>().material.color = Color.white;

                }
                j++;
            }
            i++;
        }
        width = i;
        height = j;
        A_star();

        StartCoroutine("Move");
    }
    void MovePlayer(char entry)
    {
        Vector3 deplacement;
        if (entry == 'h')
        {
            deplacement = new Vector3(-1, 0, -0);

        }
        else if (entry == 'b')
        {
            deplacement = new Vector3(1, 0, 0);

        }
        else if (entry == 'd')
        {
            deplacement = new Vector3(0, 0, 1);

        }
        else
        {

            deplacement = new Vector3(0, 0, -1);

        }
        int first_index = Terrain.FindIndex(x => x.Exists(u => u == player));
        int second_index = Terrain[first_index].FindIndex(x => x == player);
        if ((first_index + (int)deplacement.x) >= 0 && (first_index + (int)deplacement.x) < width && (second_index + (int)deplacement.z) >= 0 && (second_index + (int)deplacement.z < height))
        {
            if (Terrain[first_index + (int)deplacement.x][second_index + (int)deplacement.z].GetComponent<MeshRenderer>().material.color != Color.red
                && Terrain[first_index + (int)deplacement.x][second_index + (int)deplacement.z].GetComponent<MeshRenderer>().material.color != Color.black)
            {
                Terrain[first_index][second_index].GetComponent<MeshRenderer>().material.color = Color.white;
                Terrain[first_index + (int)deplacement.x][second_index + (int)deplacement.z].GetComponent<MeshRenderer>().material.color = Color.green;
                player = Terrain[first_index + (int)deplacement.x][second_index + (int)deplacement.z];
            }
        }
    }
    void A_star()
    {
        List<KeyValuePair<KeyValuePair<GameObject, int>, GameObject>> Open;
        List<KeyValuePair<GameObject, GameObject>> Close;
        Open = new List<KeyValuePair<KeyValuePair<GameObject, int>, GameObject>>();
        Close = new List<KeyValuePair<GameObject, GameObject>>();
        KeyValuePair<GameObject, int> temp = new KeyValuePair<GameObject, int>(ennemy.Key, 0);
        Open.Add(new KeyValuePair<KeyValuePair<GameObject, int>, GameObject>(temp, null));
        bool isFinished = false;
        KeyValuePair<KeyValuePair<GameObject, int>, GameObject> remove = new KeyValuePair<KeyValuePair<GameObject, int>, GameObject>();
        ennemy.Value.Clear();
        while (Open.Count != 0 && !isFinished)
        {


            remove = Open.Find(x => x.Key.Value == Open.Min(u => u.Key.Value));
            if (remove.Key.Key.transform.position.Equals(player.transform.position))
            {
                isFinished = true;
            }

            Close.Add(new KeyValuePair<GameObject, GameObject>(remove.Key.Key, remove.Value));
            Open.Remove(remove);

            int coordx = (int)remove.Key.Key.transform.position.x;
            int coordy = (int)remove.Key.Key.transform.position.z;
            for (int m = -1; m < 2; m++)
            {
                for (int t = -1; t < 2; t++)
                {
                    if (ValidCoordinates(coordx + m, coordy + t))
                    {
                        if (Terrain[coordx + m][coordy + t].GetComponent<MeshRenderer>().material.color != Color.red)
                        {
                            if (!Open.Contains(Open.Find(x => x.Key.Key.Equals(Terrain[coordx + m][coordy + t])))
                            && !Close.Contains(Close.Find(x => x.Key == Terrain[coordx + m][coordy + t])))
                            {
                                KeyValuePair<GameObject, int> k = new KeyValuePair<GameObject, int>(Terrain[coordx + m][coordy + t], getDistance(Terrain[coordx + m][coordy + t], player));
                                Open.Add(new KeyValuePair<KeyValuePair<GameObject, int>, GameObject>(k, remove.Key.Key));
                            }
                        }
                    }
                }
            }


        }
        int p = Close.FindIndex(x => x.Key == remove.Value);
        while (Close[p].Key != ennemy.Key)
        {
            ennemy.Value.Add(new KeyValuePair<int, int>((int)Close[p].Key.transform.position.x, (int)Close[p].Key.transform.position.z));
            p = Close.FindIndex(x => x.Key == Close[p].Value);
        }
        ennemy.Value.Add(new KeyValuePair<int, int>((int)ennemy.Key.transform.position.x, (int)ennemy.Key.transform.position.z));
        ennemy.Value.Reverse();
    }
    IEnumerator Move()
    {

        for (;;)
        {

            Terrain[(int)ennemy.Value[0].Key][(int)ennemy.Value[0].Value].GetComponent<MeshRenderer>().material.color = Color.white;
            if (ennemy.Value.Count > 1)
            {
                Terrain[(int)ennemy.Value[1].Key][(int)ennemy.Value[1].Value].GetComponent<MeshRenderer>().material.color = Color.black;
                ennemy = new KeyValuePair<GameObject, List<KeyValuePair<int, int>>>(Terrain[(int)ennemy.Value[1].Key][(int)ennemy.Value[1].Value], ennemy.Value);
            }
            yield return new WaitForSeconds(0.6f);

            A_star();
        }
    }
    int getDistance(GameObject start, GameObject to)
    {
        int dx = (int)start.transform.position.x - (int)to.transform.position.x;
        int dz = (int)start.transform.position.z - (int)to.transform.position.z;

        return (dx * dx) + (dz * dz);
    }
    private bool ValidCoordinates(float x, float y)
    {
        if (x < 0)
        {
            return false;
        }
        if (y < 0)
        {
            return false;
        }
        if (x >= width)
        {
            return false;
        }
        if (y >= height)
        {
            return false;
        }
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            MovePlayer('h');
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            MovePlayer('d');
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            MovePlayer('g');
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            MovePlayer('b');
        }
    }
}
