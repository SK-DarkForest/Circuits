using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public Main main;
    public bool selecting;

    public enum GATE_TYPE 
    {
        OR,
        AND,
        NOT,
        YES
    }
    public GATE_TYPE gateType;

    public bool input_1, input_2, output;

    public List<GameObject> inputs, outputs;

    // Start is called before the first frame update
    void Start()
    {
        main = GameObject.Find("Main").GetComponent<Main>();
    }

    // Update is called once per frame
    void Update()
    {
        if(gateType == GATE_TYPE.OR)
            output = main.OR(input_1, input_2);
        else if(gateType == GATE_TYPE.AND)
            output = main.AND(input_1, input_2);
        else if(gateType == GATE_TYPE.NOT)
            output = main.NOT(input_1);
        else if(gateType == GATE_TYPE.YES)
            output = input_1;

        if(!selecting)
        {
            this.transform.GetChild(1).GetComponent<SpriteRenderer>().color = input_1 ? Color.green : Color.red;
            this.transform.GetChild(2).GetComponent<SpriteRenderer>().color = input_2 ? Color.green : Color.red;
            this.transform.GetChild(3).GetComponent<SpriteRenderer>().color = output ? Color.green : Color.red;
        }
    }
}
