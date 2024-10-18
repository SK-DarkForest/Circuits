using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class Test : MonoBehaviour
{
    public bool stop;
    public string mainGateCode;
    // public GameObject object_1, object_2, line;
    bool holdingObject, gatesChildrenFull, varsnvalsFull;
    GameObject heldObject, selectedObject;
    public List<GameObject> ends;
    public List<GameObject> connections;
    Vector3 prevPos;
    int gateCount;
    public List<GameObject> gates, circuitOutputs, circuitInputs;
    List<string> alphabet = new List<string>(){"A","B","C","D","E","F","G","H"};
    public Main main;
    public Sprite squareSprite;
    public GameObject board;
    public GameObject orGatePrefab, andGatePrefab, notGatePrefab, yesGatePrefab, customGatePrefab, ioPrefab;
    // public List<string> inputs, outputs;
    int loop;
    public List<GameObject> customGates;
    public string _gateName;
    public TMP_InputField gate_name_input;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Select();
        Connect();
        Navigate();
        if(Input.GetKeyDown(KeyCode.Space))
        {
            // as tapping space while using the input field adds a space
            gate_name_input.text = gate_name_input.text.Trim();
            _gateName = gate_name_input.text;
            gate_name_input.text="";

            GetConnections();
            if(stop)
                return;
            CreateGateObject(mainGateCode);
            ClearStuff();
        }
    }

    void ClearStuff()
    {
        circuitInputs.Clear();
        circuitOutputs.Clear();
        foreach(GameObject conn in connections)
            if(conn.name.Contains("line"))
                Destroy(conn);
        connections.Clear();
        foreach(GameObject gate in gates)
            Destroy(gate);
        gates.Clear();
        mainGateCode="";
    }

    void CreateGateObject(string _code)
    {
        GameObject newGate = Instantiate(customGatePrefab);
        // newGate.name = newGate.GetComponent<CustomGate>().gateType.ToString() + " GATE " + gateCount;
        newGate.GetComponent<CustomGate>().code = _code;
        newGate.GetComponent<CustomGate>().gateName = _gateName;
        newGate.GetComponent<CustomGate>().test = GameObject.Find("Main").GetComponent<Test>();

        for(int i=0; i<circuitInputs.Count; i++)
        {
            // newGate.GetComponent<CustomGate>().inputs.Add(circuitInputs[i].name);
            newGate.GetComponent<CustomGate>().inputsBools.Add(false);
            GameObject input = Instantiate(ioPrefab);
            input.name = $"INPUT_{i+1}";
            input.transform.SetParent(newGate.transform.GetChild(2).transform);

            if(circuitInputs.Count%2!=0)
            {
                input.transform.position = new Vector3(
                    newGate.transform.GetChild(2).transform.position.x, 
                    ((((float)(circuitInputs.Count-1)*1.5f)/2f)-(float)i*1.5f),
                    0f);
            }
            else if(circuitInputs.Count%2==0)
            {
                input.transform.position = new Vector3(
                    newGate.transform.GetChild(2).transform.position.x, 
                    (((float)(circuitInputs.Count-1)*(1.5f/2))-(float)i*1.5f),
                    0f);
            }
        }
        
        for(int i=0; i<circuitOutputs.Count; i++)
        {
            // newGate.GetComponent<CustomGate>().outputs.Add(circuitOutputs[i].name);
            newGate.GetComponent<CustomGate>().outputsBools.Add(false);
            GameObject output = Instantiate(ioPrefab);
            output.name = $"OUTPUT_{i+1}";
            output.transform.SetParent(newGate.transform.GetChild(2).transform);

            if(circuitOutputs.Count%2!=0)
            {
                output.transform.position = new Vector3(
                    -newGate.transform.GetChild(2).transform.position.x, 
                    ((((float)(circuitOutputs.Count-1)*1.5f)/2f)-(float)i*1.5f),
                    0f);
            }
            else if(circuitOutputs.Count%2==0)
            {
                output.transform.position = new Vector3(
                    -newGate.transform.GetChild(2).transform.position.x, 
                    (((float)(circuitOutputs.Count-1)*(1.5f/2))-(float)i*1.5f),
                    0f);
            }
        }

        newGate.name = "CUSTOM GATE " + gateCount;
        newGate.transform.SetParent(board.transform);
        gateCount++;
        // gates.Add(newGate);
        customGates.Add(newGate);
    }

    public List<string> UseGate(List<string> _inputs, string _mainGateCode)
    {
        List<string> _outputs = new List<string>();
        RecreateGate(_inputs, ref _outputs, _mainGateCode);
        return _outputs;
    }

    void ParseCode(List<string> _inputs, ref List<string> vars, ref List<string> vals)
    {
        // Debug.Log(loop);
        // loop++;
        // replace the first inputs

        for(int i=0; i<vals.Count; i++)
        {
            if(alphabet.Contains(vals[i]))
            {
                string _a = vals[i];
                int _b = alphabet.IndexOf(_a);
                // int _b = Array.IndexOf(alphabet.ToArray(), _a);
                vals[i]=_inputs[_b];
            }
        }

        // use the available vars

        for(int i=0; i<vals.Count; i++)
        {
            if(vals[i]=="0"||vals[i]=="1")
            {
                for(int j=0; j<vals.Count; j++)
                {
                    if(i!=j)
                    {
                        vals[j] = vals[j].Replace(vars[i], vals[i]);
                    }
                }
            }
        }

        // calculate using the gates
        for(int i=0; i<vals.Count; i++)
        {
            bool customGateContains = false;

            string func = vals[i];
            if(vals[i].Contains("("))
                func = vals[i].Remove(vals[i].IndexOf("("));

            foreach(GameObject s in customGates)
            {
                if(func == s.GetComponent<CustomGate>().gateName)
                    customGateContains=true;
            }

            if(vals[i].Contains("NOT")||vals[i].Contains("AND")||vals[i].Contains("OR")||vals[i].Contains("YES")||customGateContains)
            {
                string var = vals[i].Substring(vals[i].IndexOf("(")+1).Replace(")", "");
                if(!var.Contains("GATE"))
                {
                    // Debug.Log(vals[i]);
                    // Debug.Log("clear");
                    if(func == "NOT")
                    {
                        vals[i] = (main.NOT(Convert.ToBoolean(var.ToString()=="0"?"False":"True")).ToString())=="False"?"0":"1";
                    }
                    if(func == "YES")
                    {
                        vals[i] = var;
                    }
                    if(func == "AND")
                    {
                        vals[i] = (main.AND
                        (
                            Convert.ToBoolean(var.Split(new string[] { ", " }, StringSplitOptions.None)[0].ToString()=="0"?"False":"True"),
                            Convert.ToBoolean(var.Split(new string[] { ", " }, StringSplitOptions.None)[1].ToString()=="0"?"False":"True")
                        
                        ).ToString())=="False"?"0":"1";
                    }
                    if(func == "OR")
                    {
                        vals[i] = (main.OR
                        (
                            Convert.ToBoolean(var.Split(new string[] { ", " }, StringSplitOptions.None)[0].ToString()=="0"?"False":"True"),
                            Convert.ToBoolean(var.Split(new string[] { ", " }, StringSplitOptions.None)[1].ToString()=="0"?"False":"True")
                        
                        ).ToString())=="False"?"0":"1";
                    }
                    if(customGateContains)
                    {
                        foreach(GameObject gate in customGates)
                        {
                            if(func == gate.GetComponent<CustomGate>().gateName)
                            {
                                int inputsCount = var.Split(new string[] { ", " }, StringSplitOptions.None).Length;
                                List<string> inputs = new List<string>();
                                for(int j=0; j<inputsCount; j++)
                                {
                                    inputs.Add(var.Split(new string[] { ", " }, StringSplitOptions.None)[j].ToString());
                                    // Debug.Log(vals[i]);
                                    // Debug.Log(var);
                                    // Debug.Log(var.Split(new string[] { ", " }, StringSplitOptions.None)[j].ToString());
                                }
                                // vals[i] = (main.OR
                                // (
                                //     Convert.ToBoolean(var.Split(new string[] { ", " }, StringSplitOptions.None)[0].ToString()=="0"?"False":"True"),
                                //     Convert.ToBoolean(var.Split(new string[] { ", " }, StringSplitOptions.None)[1].ToString()=="0"?"False":"True")
                                
                                // ).ToString())=="False"?"0":"1";
                                
                                // supposing the custom gate only returns one output
                                // List<string> outputs = new List<string>();
                                // outputs = UseGate(inputs, gate.GetComponent<CustomGate>().code);

                                // which output to choose
                                int output_index = 0;
                                // Debug.Log(vals[i]);
                                // Debug.Log(vars[i]);
                                if(vars[i].Contains(">"))
                                {
                                    output_index=Convert.ToInt32(vars[i].Split('>')[1]);
                                }
                                // Debug.Log(output_index);
                                List<string> outputs = UseGate(inputs, gate.GetComponent<CustomGate>().code);
                                for(int o=0;o<inputs.Count;o++)
                                    Debug.Log(inputs[o]);
                                for(int o=0;o<outputs.Count;o++)
                                    Debug.Log(outputs[o]);
                                Debug.Log("______________");
                                vals[i] = outputs[output_index];
                            }
                        }

                    }
                }
                else
                {
                    for(int k=0; k<vals.Count; k++)
                    {
                        if(vals[k]=="0"||vals[k]=="1")
                        {
                            for(int j=0; j<vals.Count; j++)
                            {
                                if(k!=j)
                                {
                                    vals[j] = vals[j].Replace(vars[k], vals[k]);
                                }
                            }
                        }
                    }
                }
            }
        }

        // if there is still gates to calculate
        for(int k=0; k<vals.Count; k++)
        {
            if(vals[k].Contains("(")&&loop<100)
            {
                // Debug.Log(vals[k]);  
                ParseCode(_inputs, ref vars, ref vals);
            }
        }
    }

    void RecreateGate(List<string> _inputs, ref List<string> _outputs, string _mainGateCode)
    {
        List<string> vars = new List<string>();
        List<string> vals = new List<string>();

        // Debug.Log(_mainGateCode);
        // foreach(string i in _inputs)
        //     Debug.Log(i);

        // return;

        for(int i=0; i<_mainGateCode.Split('#').Length; i++)
        {
            string[] varnval = _mainGateCode.Split('#')[i].Split(new string[] { " = " }, StringSplitOptions.None);
            vars.Add(varnval[0]);
            vals.Add(varnval[1]);
        }

        ParseCode(_inputs, ref vars, ref vals);

        _outputs.Clear();

        // a bug fix to "output" the outputs starting from O0 til 0n 

            int outputs_count=0;

            for(int i=0; i<vals.Count; i++)
            {
                if(vars[i][0]=='O'&&!vars[i].Contains("GATE"))
                {
                    outputs_count++;
                }
            }

            int min = 0;

            while(_outputs.Count<outputs_count)
            {
                for(int i=0; i<vals.Count; i++)
                {
                    if(vars[i][0]=='O'&&!vars[i].Contains("GATE")&&Convert.ToInt32(vars[i].Replace("O", ""))==min)
                    {
                        // Debug.Log(vars[i]);
                        _outputs.Add(vals[i]);
                        min++;
                        break;
                    }
                }
            }

    }

    void FindChildren(List<string> statements)
    {
        List<string> _statements = new List<string>();

        foreach(string statement in statements)
        {
            // Debug.Log(statement);
            string yo = statement.Substring(statement.IndexOf("(")+1).Replace(")", "");
            // Debug.Log(yo);
            string output_index = "";
            if(yo.Contains(">"))
            {
                output_index = yo.Substring(yo.IndexOf(">"));
                yo = yo.Remove(yo.IndexOf(">"));
            }
            // Debug.Log(output_index);

            string innerInput = "";

            for(int i=0; i<yo.Split(new string[] { ", " }, StringSplitOptions.None).Length; i++)
            {
                innerInput = yo.Split(new string[] { ", " }, StringSplitOptions.None)[i];
                
                // Debug.Log(innerInput);

                string _inputs = "";
                GameObject innerInputGO = GameObject.Find(innerInput);
                if(!innerInputGO.name.Contains("CUSTOM"))
                {
                    foreach(GameObject _input in innerInputGO.GetComponent<Gate>().inputs)
                    {
                        GameObject _properInput = _input;

                        if(!_input.transform.parent.name.Contains(",")&&!_input.transform.parent.name.Contains("GATE"))
                            _properInput=_input.transform.parent.gameObject;

                        _inputs+=_properInput.transform.parent.name
                        // +"."+_properInput.name
                        +", ";
                        // _inputCount++;
                    }
                    if(innerInputGO.GetComponent<Gate>().inputs.Count==0)
                    {
                        continue;
                    }

                    _inputs = _inputs.Remove(_inputs.Length-2);
                    string _statement = $"{innerInput} = {innerInputGO.GetComponent<Gate>().gateType.ToString()}({_inputs})";
                    _statements.Add(_statement);
                    // Debug.Log(_statement);
                    mainGateCode+=_statement+"\n";
                }
                else
                {
                    foreach(GameObject _input in innerInputGO.GetComponent<CustomGate>().inputs)
                    {
                        GameObject _properInput = _input;

                        if(!_input.transform.parent.name.Contains(",")&&!_input.transform.parent.name.Contains("GATE"))
                            _properInput=_input.transform.parent.gameObject;

                        _inputs+=_properInput.transform.parent.name
                        // +"."+_properInput.name
                        +", ";
                        // _inputCount++;
                    }
                    if(innerInputGO.GetComponent<CustomGate>().inputs.Count==0)
                    {
                        continue;
                    }

                    _inputs = _inputs.Remove(_inputs.Length-2);
                    // Debug.Log(innerInputGO.name);
                    // int output_index = innerInputGO.GetComponent<CustomGate>().outputs
                    // string _statement = $"{innerInput} = {innerInputGO.GetComponent<CustomGate>().gateName}({_inputs})>{output_index}";

                    innerInput+=output_index;

                    string _statement = $"{innerInput} = {innerInputGO.GetComponent<CustomGate>().gateName}({_inputs})";
                    _statements.Add(_statement);
                    // Debug.Log(_statement);
                    mainGateCode+=_statement+"\n";
                }
            }
        }

        if(statements.Count>0) // else the recursion will go infinitely
        {
            FindChildren(_statements);
        }
    }

    void GetConnections()
    {
        mainGateCode="";
        gatesChildrenFull=false;

        // fill gates' inputs and outputs with GOs
        if(!gatesChildrenFull)
        {
            for(int i=0; i<connections.Count/3; i++)
            {
                GameObject conn1 = connections[i*3];
                GameObject conn2 = connections[(i*3)+1];

                GameObject _output = conn1.name.Contains("OUTPUT")?conn1:conn2;
                GameObject _input = _output==conn1?conn2:conn1;
                // string connOutputGate = _output.transform.parent.name;
                // string connInputGate = _input.transform.parent.name;

                // Debug.Log($"{connOutputGate}.OUTPUT = {connInputGate}.{_input.name}");

                if(_output.transform.parent.name.Contains("GATE"))
                {
                    _output.transform.parent.GetComponent<Gate>().outputs.Add(_input);

                    if(_input.transform.parent.name.Contains("GATE"))
                    {
                        _input.transform.parent.GetComponent<Gate>().inputs.Add(_output);
                    }
                    else
                    {
                        _input.transform.parent.transform.parent.GetComponent<CustomGate>().inputs.Add(_output);
                    }
                }
                else
                {
                    _output.transform.parent.transform.parent.GetComponent<CustomGate>().outputs.Add(_input);

                    if(_input.transform.parent.name.Contains("GATE"))
                    {
                        _input.transform.parent.GetComponent<Gate>().inputs.Add(_output);
                    }
                    else
                    {
                        _input.transform.parent.transform.parent.GetComponent<CustomGate>().inputs.Add(_output);
                    }
                }
            }

            // get final outputs
            // dont include custom gates as anyways the inputs and outputs should be yes ates and maybe make it always that way as its the safest
            foreach(GameObject gate in gates)
            {
                if(gate.GetComponent<Gate>().outputs.Count==0)
                {
                    circuitOutputs.Add(gate);
                }
                if(gate.GetComponent<Gate>().inputs.Count==0)
                {
                    circuitInputs.Add(gate);
                }
            }


            // rearranging inputs and outputs as they seem before creating the new custom gate

            List<GameObject> tempCircuitInputs = new List<GameObject>();

            List<float> ysi = new List<float>();
            for(int i=0; i<circuitInputs.Count; i++)
            {
                ysi.Add(circuitInputs[i].transform.position.y);
            }
            ysi.Sort();
            ysi.Reverse();
            for(int i=0; i<circuitInputs.Count; i++)
            {
                if(ysi.Count>0)
                {
                    if(circuitInputs[i].transform.position.y==ysi[0])
                    {
                        tempCircuitInputs.Add(circuitInputs[i]);
                        ysi.Remove(ysi[0]);
                        i=-1;
                    }
                }
            }

            circuitInputs.Clear();

            foreach(GameObject a in tempCircuitInputs)
            {
                circuitInputs.Add(a);
            }

            // ************************************

            List<GameObject> tempCircuitOutputs = new List<GameObject>();

            List<float> ys = new List<float>();
            for(int i=0; i<circuitOutputs.Count; i++)
            {
                ys.Add(circuitOutputs[i].transform.position.y);
            }
            ys.Sort();
            // ys.Reverse();
            for(int i=0; i<circuitOutputs.Count; i++)
            {
                if(ys.Count>0)
                {
                    if(circuitOutputs[i].transform.position.y==ys[0])
                    {
                        tempCircuitOutputs.Add(circuitOutputs[i]);
                        ys.Remove(ys[0]);
                        i=-1;
                    }
                }
            }

            circuitOutputs.Clear();

            foreach(GameObject a in tempCircuitOutputs)
            {
                circuitOutputs.Add(a);
            }

            // reversing the outputs cause they'll get reversed again so that they stay in the same order lol, just trust the process, tbh i jst gambled and this worked lolz
            circuitOutputs.Reverse();

            gatesChildrenFull=true;
        }

        if(stop)
            return;

        // get connections as strings
        int _outputCount = 0;
        // int _inputCount = 0;
        List<string> statements = new List<string>();

        // get outputs
        foreach(GameObject _out in circuitOutputs)
        {
            string _inputs = "";
            foreach(GameObject _input in _out.GetComponent<Gate>().inputs)
            {
                string specific_output = "";
                if(_input.transform.parent.name.Contains("GATE"))
                {
                    _inputs+=_input.transform.parent.name
                    // +"."+_input.name
                    +", ";
                    // _inputCount++;
                }
                else // custom gate (probably)
                {
                    if(_input.transform.parent.transform.parent.gameObject.GetComponent<CustomGate>().outputsBools.Count>1) // it matters when the custom gate has many outputs
                    {
                        // Debug.Log("------");
                        // Debug.Log(circuitInputs.Count);
                        // Debug.Log(_outputCount);
                        // Debug.Log("--------");

                        int customGateOutputIndex = Convert.ToInt32(_input.name.Replace("OUTPUT_", ""))-1;
                        // specific_output=">"+_outputCount.ToString();
                        specific_output=">"+customGateOutputIndex.ToString();
                        Debug.Log(_out.name+" - "+customGateOutputIndex.ToString());
                    }
                    _inputs+=_input.transform.parent.transform.parent.name+specific_output+", ";
                }
            }
            _inputs = _inputs.Remove(_inputs.Length-2);
            string statement = $"O{_outputCount} = {_out.GetComponent<Gate>().gateType.ToString()}({_inputs})";
            // Debug.Log(statement);
            // Debug.Log($"O{_outputCount}");
            // Debug.Log(_inputs);
            mainGateCode+=statement+"\n";
            statements.Add(statement);
            // this increases the output count obviously but when outputing the same custom gate output to many other gates it is written in the maingatecode as if the custom gate (the custom gate inside the custom gate) has more outputs and corrupts shite
            _outputCount++;
        }

        // get other connections as strings
        FindChildren(statements);

        // save main gate
        for(int i=0; i<circuitInputs.Count; i++)
        {
            // mainGateCode+=$"{input.name} = {(input.GetComponent<Gate>().input_1==false?0:1).ToString()}\n";
            mainGateCode+=$"{circuitInputs[i].name} = {alphabet[i]}\n";
        }

        string temp = "";
        for(int i=0; i<mainGateCode.Split('\n').Length; i++)
        {
            temp += mainGateCode.Split('\n')[mainGateCode.Split('\n').Length-i-1]+"#";
        }
        mainGateCode = temp;
        mainGateCode=mainGateCode.Remove(0,1);
        mainGateCode=mainGateCode.Remove(mainGateCode.Length-1,1);
        // Debug.Log(mainGateCode);
    }

    void Navigate()
    {
        float zoomFactor = 2f;
        Camera.main.transform.position += new Vector3(0f, 0f, Input.mouseScrollDelta.y*zoomFactor);
        Vector3 mwp = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
        if(Input.GetMouseButtonDown(0))
        {
            prevPos=mwp-board.transform.position;
        }
        if(Input.GetMouseButton(0)&&!holdingObject&&selectedObject==null)
        {
            board.transform.position = mwp-prevPos;
        }
    }

    public void CreateORGate()
    {
        GameObject newGate = Instantiate(orGatePrefab);
        newGate.name = newGate.GetComponent<Gate>().gateType.ToString() + " GATE " + gateCount;
        newGate.transform.SetParent(board.transform);
        gateCount++;
        gates.Add(newGate);
    }
    public void CreateANDGate()
    {
        GameObject newGate = Instantiate(andGatePrefab);
        newGate.name = newGate.GetComponent<Gate>().gateType.ToString() + " GATE " + gateCount;
        newGate.transform.SetParent(board.transform);
        gateCount++;
        gates.Add(newGate);
    }
    public void CreateNOTGate()
    {
        GameObject newGate = Instantiate(notGatePrefab);
        newGate.name = newGate.GetComponent<Gate>().gateType.ToString() + " GATE " + gateCount;
        newGate.transform.SetParent(board.transform);
        gateCount++;
        gates.Add(newGate);
    }
    public void CreateYESGate()
    {
        GameObject newGate = Instantiate(yesGatePrefab);
        newGate.name = newGate.GetComponent<Gate>().gateType.ToString() + " GATE " + gateCount;
        newGate.transform.SetParent(board.transform);
        gateCount++;
        gates.Add(newGate);
    }

    void Connect()
    {
        for(int i=0; i<connections.Count/3; i++)
        {
            GameObject _line = connections[(i*3)+2];
            GameObject end1 = connections[(i*3)];
            GameObject end2 = connections[(i*3)+1];
            Wire(end1, end2, ref _line);
            
            // electrify
            GameObject _output = end1.name.Contains("OUTPUT")?end1:end2;
            GameObject _input = _output==end1?end2:end1;
            if(_output.GetComponent<SpriteRenderer>().color==Color.green)
            {
                _line.GetComponent<SpriteRenderer>().color = Color.green;

                if(_input.transform.parent.name.Contains("GATE"))
                {
                    if(_input.name.Contains("1"))
                    {
                        _input.transform.parent.GetComponent<Gate>().input_1=true;
                    }
                    else if(_input.name.Contains("2"))
                    {
                        _input.transform.parent.GetComponent<Gate>().input_2=true;
                    }
                }
                else
                {
                    int _index = Convert.ToInt32(_input.name.Replace("INPUT_", ""))-1;
                    _input.transform.parent.transform.parent.GetComponent<CustomGate>().inputsBools[_index]=true;
                }
            }
            else if(_output.GetComponent<SpriteRenderer>().color==Color.red)
            {
                _line.GetComponent<SpriteRenderer>().color = Color.red;
                
                if(_input.transform.parent.name.Contains("GATE"))
                {
                    if(_input.name.Contains("1"))
                    {
                        _input.transform.parent.GetComponent<Gate>().input_1=false;
                    }
                    else if(_input.name.Contains("2"))
                    {
                        _input.transform.parent.GetComponent<Gate>().input_2=false;
                    }
                }
                else
                {
                    int _index = Convert.ToInt32(_input.name.Replace("INPUT_", ""))-1;
                    _input.transform.parent.transform.parent.GetComponent<CustomGate>().inputsBools[_index]=false;
                }
            }
        }
    }

    void Select()
    {
        // mouseWorldPosition
        // for orthographic projection
        // Vector3 mwp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // for perspective projection
        Vector3 mwp = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

        // only selects ends
        if(selectedObject!=null&&!Input.GetMouseButton(1))
        {
            if(selectedObject.name.Contains("INPUT")||selectedObject.name.Contains("OUTPUT"))
            {
                if(selectedObject.transform.parent.name.Contains("GATE"))
                {
                    if(selectedObject.name.Contains("1"))
                    {
                        bool a = selectedObject.transform.parent.GetComponent<Gate>().input_1;
                        selectedObject.transform.parent.GetComponent<Gate>().input_1=!a;
                    }
                    else if(selectedObject.name.Contains("2"))
                    {
                        bool a = selectedObject.transform.parent.GetComponent<Gate>().input_2;
                        selectedObject.transform.parent.GetComponent<Gate>().input_2=!a;
                    }
                }
                else // a custom gate
                {
                    if(selectedObject.name.Contains("INPUT"))
                    {
                        int _index = Convert.ToInt32(selectedObject.name.Replace("INPUT_", ""))-1;
                        bool a = selectedObject.transform.parent.transform.parent.GetComponent<CustomGate>().inputsBools[_index];
                        selectedObject.transform.parent.transform.parent.GetComponent<CustomGate>().inputsBools[_index]=!a;
                    }
                    else if(selectedObject.name.Contains("OUTPUT"))
                    {
                        int _index = Convert.ToInt32(selectedObject.name.Replace("OUTPUT_", ""))-1;
                        bool a = selectedObject.transform.parent.transform.parent.GetComponent<CustomGate>().inputsBools[_index];
                        selectedObject.transform.parent.transform.parent.GetComponent<CustomGate>().outputsBools[_index]=!a;
                    }
                }
            }
            selectedObject=null;
        }

        // only holds gates
        if(holdingObject)
        {
            if(heldObject.name.Contains("GATE"))
            {
                heldObject.transform.position = new Vector3(mwp.x, mwp.y, heldObject.transform.position.z);
            }
            else if(heldObject.name.Contains("INPUT")||heldObject.name.Contains("OUTPUT"))
            {
                if(heldObject.transform.parent.name.Contains("GATE"))
                {
                    heldObject.transform.parent.GetComponent<Gate>().selecting = true;
                }
                else
                {
                    heldObject.transform.parent.transform.parent.GetComponent<CustomGate>().selecting = true;
                }

                heldObject.transform.GetComponent<SpriteRenderer>().color = Color.yellow;

                if(!ends.Contains(heldObject)&&ends.Count<=2)
                {
                    ends.Add(heldObject);
                }
            }

            // unselect
            if(Input.GetMouseButtonUp(0))
            {
                holdingObject=false;
                // two ends are being linked
                if(ends.Count==2)
                {
                    foreach(GameObject end in ends)
                    {
                        if(end.transform.parent.name.Contains("GATE"))
                        {
                            end.transform.parent.GetComponent<Gate>().selecting=false;
                        }
                        else
                        {
                            end.transform.parent.transform.parent.GetComponent<CustomGate>().selecting=false;
                        }
                    }

                    GameObject line = new GameObject("line");
                    line.AddComponent<SpriteRenderer>().sprite = squareSprite;
                    line.GetComponent<SpriteRenderer>().color = Color.red;

                    connections.Add(ends[0]);
                    connections.Add(ends[1]);
                    connections.Add(line);

                    ends.Clear();
                }
            }
            return;
        }

        // after linking the ends
        if(Input.GetMouseButtonUp(0))
        {
            foreach(GameObject end in ends)
            {
                if(end.transform.parent.name.Contains("GATE"))
                {
                    end.transform.parent.GetComponent<Gate>().selecting=false;
                }
                else
                {
                    end.transform.parent.transform.parent.GetComponent<CustomGate>().selecting=false;
                }
            }
            ends.Clear();
        }

        // actual selecting mechanism
        RaycastHit2D hit = Physics2D.Raycast(mwp, transform.TransformDirection(Vector3.forward), 20f);
        if(hit.collider!=null)
        {
            // Debug.DrawRay(mwp, transform.TransformDirection(Vector3.forward) * 20f, Color.yellow);
            if(Input.GetMouseButton(0))
            {
                heldObject = hit.transform.gameObject;
                holdingObject=true;
            }
            if(Input.GetMouseButton(1))
            {
                selectedObject = hit.transform.gameObject;
            }
        }
        // else
        // {
        //     Debug.DrawRay(mwp, transform.TransformDirection(Vector3.forward) * 20f, Color.white);
        // }
    }

    void Wire(GameObject _object_2, GameObject _object_1, ref GameObject _line)
    {
        float xDist = _object_2.transform.position.x-_object_1.transform.position.x;
        float yDist = _object_2.transform.position.y-_object_1.transform.position.y;

        float dist = Mathf.Sqrt(Mathf.Pow(xDist, 2)+Mathf.Pow(yDist, 2));

        float angle = Mathf.Acos((xDist/dist))*(180/Mathf.PI);
        if(_object_2.transform.position.y<_object_1.transform.position.y)
        {
            angle=360-angle;
        }

        var rot = _line.transform.localRotation.eulerAngles;
        rot.Set(0f, 0f, angle);
        _line.transform.localRotation = Quaternion.Euler(rot);

        _line.transform.localScale = new Vector3(dist, 0.5f, 1f);

        _line.transform.position = new Vector3(
            (_object_2.transform.position.x+_object_1.transform.position.x)/2,
            (_object_2.transform.position.y+_object_1.transform.position.y)/2,
            0f);
    }
}
