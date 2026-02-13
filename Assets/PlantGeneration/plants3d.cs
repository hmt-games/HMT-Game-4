using System.Collections.Generic;
using UnityEngine;

public class Plants3d : MonoBehaviour
{
    public string startAxiom = "F";
    public int iterCount = 0;
    public float ang = 25f;
    public float segLength = 0.5f;
    public GameObject branchPrefab;
    private Dictionary<char, string> ruleSet = new Dictionary<char, string>();
    private string curString;

    private List<string> myRules = new List<string>
    {
        "F[+F][-F][&F][^F][\\F][/F]",
        "F[+F]F[-F]F[&F]^F",
        "FF-[-F+F+F]&[++F^F]F",
        "F[+F[+F]F][-F[-F]F][&F[^F]F]",
        "F[++F]F[--F]F[&F]/F"
    };




    private List<GameObject> sp = new List<GameObject>();

    void Start()
    {
        SetRule(0);
        GenerateL();
    }

    void SetRule(int i)
    {
        ruleSet.Clear();
        Debug.Log("this is rule " + i);
        ruleSet.Add('F', myRules[i]);
    }

    void GenerateL()
    {
        curString = startAxiom;

        for (int i = 0; i < iterCount; i++)
        {
            string res = "";
            foreach (char c in curString)
            {
                if (ruleSet.ContainsKey(c))
                {
                    res += ruleSet[c];
                }
                else
                {
                    res += c.ToString();
                }
            }
            curString = res;
        }
        DrawL();
    }

    void DrawL()
    {
        //destroy previous
        foreach (GameObject branch in sp)
        {
            Destroy(branch);
        }
        sp.Clear();

        Stack<transformInfo> transformStack = new Stack<transformInfo>();
        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        foreach (char c in curString)
        {
            if (c == 'F')
            {
                Vector3 startPos = pos;
                pos += rot * Vector3.up * segLength;

                GameObject branch = Instantiate(branchPrefab);
                branch.transform.position = (startPos + pos) / 2f;
                branch.transform.up = pos - startPos;
                branch.transform.localScale = new Vector3(0.1f, (pos - startPos).magnitude / 2f, 0.1f);
                sp.Add(branch);
            }
            //rotate x
            else if (c == '+')
            {
                rot *= Quaternion.Euler(ang, 0, 0);
            }
            else if (c == '-')
            {
                rot *= Quaternion.Euler(-ang, 0, 0);
            }
            //rotate y
            else if (c == '&')
            {
                rot *= Quaternion.Euler(0, ang, 0);
            }
            else if (c == '^')
            {
                rot *= Quaternion.Euler(0, -ang, 0);
            }
            //rotate z
            else if (c == '\\')
            {
                rot *= Quaternion.Euler(0, 0, ang);
            }
            else if (c == '/')
            {
                rot *= Quaternion.Euler(0, 0, -ang);
            }
            //pos push to stack
            else if (c == '[')
            {
                transformStack.Push(new transformInfo(pos, rot));
            }
            //pos pop from stack
            else if (c == ']')
            {
                transformInfo ti = transformStack.Pop();
                pos = ti.position;
                rot = ti.rotation;
            }
        }
    }




    struct transformInfo
    {
        public Vector3 position;
        public Quaternion rotation;
        public transformInfo(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }


    public void OnButtonPress()
    {
        int i = Random.Range(0, myRules.Count);
        SetRule(i);
        GenerateL();
    }
}
