using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGate : MonoBehaviour
{
    public Test test;
    public bool selecting;
    public string code;
    public List<GameObject> inputs, outputs;
    public List<bool> inputsBools, outputsBools;
    public List<bool> prevInputsBools;
    public string gateName;
    public bool change;

    // Start is called before the first frame update
    void Start()
    {
        change=true;
        foreach(bool b in inputsBools)
            prevInputsBools.Add(b);
    }

    // Update is called once per frame
    void Update()
    {
        for(int i=0; i<inputsBools.Count; i++)
            if(inputsBools[i]!=prevInputsBools[i])
                change=true;

        if(change)
        {
            List<string> inputsStrings = new List<string>();
            List<string> outputsStrings = new List<string>();

            inputsStrings.Clear();

            foreach(bool b in inputsBools)
            {
                inputsStrings.Add((b==true)?"1":"0");
            }
            
            // foreach(string i in inputsStrings)
            //     Debug.Log(i);

            outputsStrings = test.UseGate(inputsStrings, code);
            outputsBools.Clear();

            foreach(string s in outputsStrings)
            {
                if(s!="1"&&s!="0")
                {
                    Debug.LogError("Output wrong, error calculating the output.");
                    Debug.Log(s);
                }
                outputsBools.Add((s=="1")?true:false);
            }
            prevInputsBools.Clear();
            foreach(bool b in inputsBools)
                prevInputsBools.Add(b);
            change=false;
        }

        if(!selecting)
        {
            for(int i=0; i<inputsBools.Count; i++)
            {
                this.transform.GetChild(2).transform.GetChild(i).GetComponent<SpriteRenderer>().color = inputsBools[i]==true ? Color.green : Color.red;
            }
            for(int i=0; i<outputsBools.Count; i++)
            {
                // Debug.Log(i+inputs.Count);
                this.transform.GetChild(2).transform.GetChild(i+inputsBools.Count).GetComponent<SpriteRenderer>().color = outputsBools[i]==true ? Color.green : Color.red;
            }
        }
    }
}
