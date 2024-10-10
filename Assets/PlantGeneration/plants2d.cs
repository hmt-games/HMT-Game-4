using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Plants2d : MonoBehaviour
{
    public string startAxiom = "F";
    public int iterCount = 0;
    public float ang = 25f;
    public float segLength = 0.5f;
   

    public Sprite[] branchSprites;
    public Sprite[] fruitSprites;


    private Dictionary<char, string> ruleSet = new Dictionary<char, string>();
    private string curString;
    public int numOfRules = 5;

    private List<string> rules = new List<string>();

    //example rules:
    //"F[+F]F[-F]F",
    //"FF-[+F+F]F[--F][+F]",
    //"F[+F]F[-F][F]",
    //"F[-F]F[+F][--F]",
    //"FF[+F][-F]F"


    private List<string> ruleSymbols = new List<string> { "F", "+", "-" };



    private List<GameObject> sp = new List<GameObject>();
    private float randFruitScale;
    private float randBranchScale;
    private Color randFruitColor;
    private Color randBranchColor;
    private Sprite randBranchSprite;
    private Sprite randFruitSprite;

    public int maxIter = 3;
    private bool fruitStage = false;

    void Start()
    {
        GenerateRules(numOfRules);
        SetRule(0);
        GenerateTree();
    }

    void GenerateRules(int numOfRules)
    {
        rules.Clear();
        for (int r = 0; r < numOfRules; r++)
        {
            int numOfSymbols = Random.Range(5, 10);

            //all start with a branch F
            string rule = startAxiom;

            for (int s = 0; s < numOfSymbols; s++)
            {
                bool goBack = Random.Range(0, 2) == 1;

                //go back to the saved pos
                if (goBack)
                {
                    rule += "[";
                    int numOfSymbolsInBracket = Random.Range(1, 3);
                    for (int ns = 0; ns < numOfSymbolsInBracket; ns++)
                    {
                        int randSymbolIndex = Random.Range(0, ruleSymbols.Count);
                        rule += ruleSymbols[randSymbolIndex];
                    }
                    rule += "]";
                }
                else
                {
                    int numOfSymbolsNoBracket = Random.Range(1, 3);
                    for (int ns = 0; ns < numOfSymbolsNoBracket; ns++)
                    {
                        int randSymbolIndex = Random.Range(0, ruleSymbols.Count);
                        rule += ruleSymbols[randSymbolIndex];
                    }
                }
            }
            rules.Add(rule);
        }
    }

    void SetRule(int i)
    {
        ruleSet.Clear();
        Debug.Log("This is rule " + i);
        Debug.Log(rules[i]);

        ruleSet.Add('F', rules[i]);



        RandomizeScales();
        RandomizeColors();
        RandomizeSprites();
        
    }

    void RandomizeScales()
    {
        randFruitScale = Random.Range(0.008f, 0.02f);
        randBranchScale = Random.Range(0.002f, 0.05f);
    }

    void RandomizeColors()
    {
        randFruitColor = new Color(Random.value, Random.value, Random.value, 1.0f);
        randBranchColor = new Color(Random.value, Random.value, Random.value, 1.0f);
    }

    void RandomizeSprites()
    {
        randBranchSprite = branchSprites[Random.Range(0, branchSprites.Length)];
        randFruitSprite = fruitSprites[Random.Range(0, fruitSprites.Length)];
    }

    private string GenerateString(string inputString, Dictionary<char, string> ruleSet, int iterations)
    {
        string curString = inputString;

        for (int i = 0; i < iterations; i++)
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

        return curString;
    }

    void GenerateTree()
    {
        curString = GenerateString(startAxiom, ruleSet, iterCount);
        DrawTree();
    }

    void DrawTree()
    {
        Debug.Log("stage" + iterCount);
        foreach (GameObject obj in sp)
        {
            Destroy(obj);
        }
        sp.Clear();

        Stack<transformInfo> transformStack = new Stack<transformInfo>();
        HashSet<Vector3> endpoints = new HashSet<Vector3>();

        // Initialize position and rotation with the empty object's position and rotation
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        foreach (char c in curString)
        {
            //draw a branch
            if (c == 'F')
            {
                Vector3 startPos = pos;
                pos += rot * Vector3.up * segLength;

                CreateBranch(startPos, pos);

                endpoints.Add(pos);
            }
            //rotate clockwise
            else if (c == '+')
            {
                rot *= Quaternion.Euler(0, 0, ang);
            }
            //counterclockwise
            else if (c == '-')
            {
                rot *= Quaternion.Euler(0, 0, -ang);
            }
            //push the curr pos
            else if (c == '[')
            {
                transformStack.Push(new transformInfo(pos, rot));
            }
            //pop the curr pos
            else if (c == ']')
            {
                transformInfo ti = transformStack.Pop();
                pos = ti.position;
                rot = ti.rotation;

                endpoints.Remove(pos);
            }
        }

        if (fruitStage)
        {
            foreach (var endpoint in endpoints)
            {
                CreateFruit(endpoint);
            }
        }
    }



    Color MutedColor(Color originalColor)
    {
        float gray = (originalColor.r + originalColor.g + originalColor.b) / 3f;
        float desaturationAmount = 0.8f; // 100% grey
        Color mutedColor = Color.Lerp(originalColor, new Color(gray, gray, gray), desaturationAmount);

        return mutedColor;
    }


    void CreateBranch(Vector3 start, Vector3 end)
    {
        GameObject branch = new GameObject("Branch");
        branch.transform.position = (start + end) / 2;
        branch.transform.parent = transform;

        SpriteRenderer sr = branch.AddComponent<SpriteRenderer>();
        sr.sprite = randBranchSprite;
        sr.sortingOrder = 0;

        Vector3 direction = end - start;
        float length = direction.magnitude;
        float ang = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        branch.transform.rotation = Quaternion.Euler(0, 0, ang);
        float spriteWidth = sr.sprite.bounds.size.x;
        branch.transform.localScale = new Vector3(length / spriteWidth, randBranchScale, 1);

        sr.color = randBranchColor;

        //sr.color = MutedColor(randFruitColor);

        sp.Add(branch);
    }

    void CreateFruit(Vector3 pos)
    {
        GameObject fruit = new GameObject("Fruit");
        fruit.transform.position = pos;
        fruit.transform.localScale = new Vector3(randFruitScale, randFruitScale, 1f);
        fruit.transform.parent = transform;

        SpriteRenderer sr = fruit.AddComponent<SpriteRenderer>();
        sr.sprite = randFruitSprite;
        sr.sortingOrder = 1; //fruit before branches

        sr.color = randFruitColor;

        //sr.color = MutedColor(randFruitColor);

        sp.Add(fruit);
    }

    public void IncreaseIterations()
    {
        if (iterCount < maxIter)
        {
            iterCount++;
            GenerateTree();
        }
        else if (iterCount == maxIter && !fruitStage)
        {
            fruitStage = true; //enter the fruit stage
            GenerateTree();    
        }
    }

    public void DecreaseIterations()
    {
        if (fruitStage)
        {
            fruitStage = false;
            GenerateTree();
        }
        else if (iterCount > 0)
        {
            iterCount--;
            GenerateTree();
        }
    }

    public void OnButtonPress()
    {
        int i = Random.Range(0, rules.Count);
        SetRule(i);
        iterCount = 0; //reset iter
        fruitStage = false; //reset fruit stage
        GenerateTree();
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
}
