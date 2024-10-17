using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Plants2d : MonoBehaviour
{
    
    public int iterCount = 0;
    public float ang = 25f;
    public float segLength = 0.5f;
   

    public Sprite[] branchSprites;
    public Sprite[] fruitSprites;


    private Dictionary<char, string> ruleSet = new Dictionary<char, string>();
    private string curString;
    public int numOfRules = 5;

    public string startAxiom;

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

        bool goBack = true; //second sequence

        //valid sequences inside brackets(go back)
        List<string> validSequences = new List<string> { "F", "+F", "-F", "++F", "+F+F", "--F", "-F-F" };

        //valid sequences outside brackets
        List<string> validOutside = new List<string> { "F", "FF"};

        //valid options for the last sequence, all inside brackets
        List<string> validLastSequences = new List<string> { "[F]", "[+F]", "[-F]" };

        string lastselected = "";
       
        bool lastDoubleSymbol = false;
        int doubleSymbolCount = 0;
        int ffCount = 0;

        List<string> firstBranch = new List<string> {"F"};


        for (int r = 0; r < numOfRules; r++)
        {
            int numOfSymbols = Random.Range(1, 5);

            //all start with a branch F, FF makes a plant taller, could be added to the first branch list if necessary
            string startAxiom = firstBranch[Random.Range(0, firstBranch.Count)];

            string rule = startAxiom;

            for (int s = 0; s < numOfSymbols; s++)
            {
                string selected = "";

                if (s == numOfSymbols - 1)
                {
                    int randIndex = Random.Range(0, validLastSequences.Count);
                    rule += validLastSequences[randIndex];
                    continue;
                }

                //nno consecutive sequences
                bool found = false;

                while (!found)
                {

                    if (goBack)
                    {
                        selected = validSequences[Random.Range(0, validSequences.Count)];
                    }

                    //only allow F or FF or F- or F+
                    else
                    {
                        selected = validOutside[Random.Range(0, validOutside.Count)];
                    }

                    //use of double symbol sequences twice max
                    if ((selected == "++F" || selected == "+F+F" || selected == "--F" || selected == "-F-F") && doubleSymbolCount >= 2)
                    {
                        continue;
                    }

                    //no consecutive double symbol sequences
                    if ((selected == "++F" || selected == "+F+F" || selected == "--F" || selected == "-F-F") && lastDoubleSymbol)
                    {
                        continue;
                    }

                    //double symbols inside brackets only
                    if ((selected == "++F" || selected == "+F+F" || selected == "--F" || selected == "-F-F") && !goBack)
                    {
                        continue; //skip if we're outside brackets and a double symbol is selected
                    }

                    //FF only once per rule
                    if (selected == "FF" && ffCount >= 1)
                    {
                        continue;
                    }

                    //no consecutive -
                    if (lastselected.EndsWith("-F") && selected == "F")
                    {
                        selected = "+F";
                    }

                    //no consecutive +
                    if (lastselected.EndsWith("+F") && selected == "F")
                    {
                        selected = "-F";
                    }

                    //double symbol follows by F
                    if (lastDoubleSymbol)
                    {
                        selected = "F";
                        lastDoubleSymbol = false;
                    }

                    found = true;
                }

                //double symbol used
                if (selected == "++F" || selected == "+F+F" || selected == "--F" || selected == "-F-F")
                {
                    doubleSymbolCount++;
                    lastDoubleSymbol = true;
                }

                //usage of FF
                if (selected == "FF")
                {
                    ffCount++;
                }

                //go back to the saved pos
                if (goBack)
                {
                   
                    rule += "[" + selected + "]";

                    goBack = false;
                }

                else
                {
                    rule += selected;
                    goBack = true;
                }

                lastselected = selected;
            }

            //reset
            goBack = true;
            doubleSymbolCount = 0;
            ffCount = 0;

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
        
        randBranchScale = Random.Range(0.002f, 0.05f);

        float minFruitScale = Mathf.Max(randBranchScale/3, 0.02f);

        randFruitScale = Random.Range(minFruitScale, randBranchScale/2);
    }

    void RandomizeColors()
    {
        randBranchColor = new Color(Random.value, Random.value, Random.value, 1.0f);
        //opposite color of branch
        randFruitColor = new Color(1.0f - randBranchColor.r, 1.0f - randBranchColor.g, 1.0f - randBranchColor.b, 1.0f);
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
        List<Vector3> endpoints = new List<Vector3>();


        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        foreach (char c in curString)
        {
            //float angleRandom = Random.Range(0, 25);
            //ang = angleRandom;
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

        //randomized fuit num and position 
        if (fruitStage)
        {
            bool fruitCreated = false;
            int fruitNum = 0;
            for (int i = endpoints.Count - 1; i >= 0; i--)
            {

                //randomized whether there is a fruit or not at this end point
                int fruitNumMax = endpoints.Count / 3;
                if (Random.Range(0f, 1f) > 0.5f && fruitNum < fruitNumMax)
                {
                    CreateFruit(endpoints[i]);
                    fruitCreated = true;
                    fruitNum++;
                }
            }

            if (!fruitCreated)
            {
                Vector3 randomEnd = endpoints[Random.Range(0, endpoints.Count)];
                CreateFruit(randomEnd);
            }
        }


        //us this code if don't want random fruit number and position
        //if (fruitStage)
        //{
        //    bool fruitCreated = false;
        //    int fruitNum = 0;
        //    for (int i = endpoints.Count - 1; i >= 0; i--)
        //    {
        //        CreateFruit(endpoints[i]);
        //    }
        //}
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
        fruit.transform.localScale = new Vector3(randFruitScale, randFruitScale, 1);
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
