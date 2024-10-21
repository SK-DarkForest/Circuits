using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public static class circuitUtils{
    public static void drawRectangle(Vector2 position, Vector2 size, Color color){
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        GUI.DrawTexture(new Rect(position.x, position.y, size.x, size.y), tex);
    }
}

public class Check{
    public Action Callback;
    public List<Pin> checkablePins;
    public float[] filters = new float[0];
    public bool solved = false;
    public void checkIfSolved(){
        if(Callback != null&&checkablePins!=null&&filters!=null){
            int i = 0;
            foreach (Pin pin in checkablePins)
            {
                if(pin.value<filters[i]){
                    return;
                }
                i++;
            }
            if(Callback != null){
                solved = true;
                Callback();
            }
        }
    }
    public Check(List<Pin> pins, float[] filters, Action callback){
        checkablePins = pins;
        this.filters = filters;
        Callback = callback;
    }
}

public class Circuit{
    public List<Check> checks = new List<Check>();
    public Circuit(){
        Components = new List<Component>();
        mainTexture = new Texture2D(1, 1);
        mainTexture.SetPixel(0, 0, Color.white);
        mainTexture.Apply();
        pinTexture = new Texture2D(1, 1);
        pinTexture.SetPixel(0, 0, Color.red);
        pinTexture.Apply();
    }
    public List<Component> Components;
    public List<Spawner> spawners = new List<Spawner>();
    public Texture2D mainTexture;
    public Texture2D pinTexture;
    public Vector2 offset;
    public Component draggedComponent;
    public Pin draggedPin;
    public bool isDraggingPin = false; 
    public void add(Component component){
        Components.Add(component);
    }
    public void addSpawner(Spawner component){
        spawners.Add(component);
        component.circuit = this;
    }
    public void delete(Component component){
        foreach (Pin pin in component.pins)
        {
            component.pins.Remove(pin);
        }
        Components.Remove(component);
        Debug.Log("Removed: "+component);
    }
    public void display(MouseInfo mouseInfo){
        foreach(Component component in Components){
            component.display(offset, mouseInfo);
        }
        foreach (Spawner spawner in spawners)
        {
            spawner.display();
        }
        if (isDraggingPin && draggedPin != null) {
            Drawing.DrawLine(
                draggedPin.position + draggedPin.baseComponent.position + offset, 
                mouseInfo.position, 
                Color.green, 3, false
            );
        }
    }
    public void update(MouseInfo mouseInfo){
        foreach(Component component in Components){
            component.update(this.offset,mouseInfo);
        }
        foreach (Check check in checks)
        {
            if(check.solved){continue;}
            check.checkIfSolved();
        }
        foreach (Spawner spawner in this.spawners)
        {
            spawner.update(mouseInfo,offset);
        }
        if(mouseInfo.up){
            this.draggedComponent = null;
        }
        handlePinDrag(mouseInfo);
    }
    private void handlePinDrag(MouseInfo mouseInfo) {
        if (mouseInfo.down&&!isDraggingPin) {//if mouseInfo.down it always returns if mouseOverPin and does not drag
            // Start dragging a pin if the mouse is over a pin
            foreach (Component component in Components) {
                foreach (Pin pin in component.pins) {
                    if (isMouseOverPin(pin, mouseInfo)) {
                        Debug.Log("Dragging pin");
                        draggedPin = pin;
                        isDraggingPin = true;
                        return;
                    }
                }
            }
        }

        if (mouseInfo.up && isDraggingPin) {
            //Debug.Log("Released pin");
            // On mouse release, try to connect to another pin
            foreach (Component component in Components) {
                foreach (Pin targetPin in component.pins) {
                    if (isMouseOverPin(targetPin, mouseInfo) && draggedPin != null) {
                        //Debug.Log("Connecting pins: "+draggedPin.connections+" -> "+targetPin.connections);
                        // Ensure valid connection (e.g., output to input)
                        if(draggedPin.connections.Contains(targetPin)){
                            Debug.Log("Removing connection: "+draggedPin.connections.Count);
                            draggedPin.connections.Remove(targetPin);
                            Debug.Log("Removed connection: "+draggedPin.connections.Count);
                            break;
                        }
                        if(targetPin.connections.Contains(draggedPin)){
                            Debug.Log("Removing connection: "+targetPin.connections.Count);
                            targetPin.connections.Remove(draggedPin);
                            Debug.Log("Removed connection: "+targetPin.connections.Count);
                            break;
                        }
                        if (draggedPin.type == Type.Output && targetPin.type == Type.Input) {
                            draggedPin.connect(targetPin);
                        }else if(draggedPin.type == Type.Input && targetPin.type == Type.Output){
                            targetPin.connect(draggedPin);
                        }
                        break;
                    }
                }
            }
            // Reset after dropping
            draggedPin = null;
            isDraggingPin = false;
        }
        //mouseInfo.up = false;
    }

    private bool isMouseOverPin(Pin pin, MouseInfo mouseInfo) {
        // Check if the mouse is hovering over a pin (considering the pin's size)
        Vector2 pinWorldPosition = pin.position + pin.baseComponent.position + offset;
        return (mouseInfo.position.x > pinWorldPosition.x - 10 && mouseInfo.position.x < pinWorldPosition.x + 10) &&
               (mouseInfo.position.y > pinWorldPosition.y - 10 && mouseInfo.position.y < pinWorldPosition.y + 10);
    }
    public bool mouseOverPinsOrComponent(MouseInfo mouseInfo){
        foreach (Component component in Components) {
            if((mouseInfo.position.x>component.position.x+offset.x)&&(mouseInfo.position.x<component.position.x+offset.x+50)&&(mouseInfo.position.y>component.position.y+offset.y)&&(mouseInfo.position.y<component.position.y+offset.y+50)){
                return true;
            }
            foreach (Pin targetPin in component.pins) {
                if (isMouseOverPin(targetPin, mouseInfo)) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool moveComponent(Vector2 position){
        /*if(this.draggedPin != null){
            Debug.Log("dragging pin");
            Drawing.DrawLine(this.draggedPin.position+this.draggedPin.baseComponent.position+this.offset, position, (this.draggedPin.value>=.5)?Color.red:Color.blue, 5, false);
            return true;
        }*/
        if(this.draggedComponent == null){
            foreach(Component component in Components){
                if(component.position.x+offset.x < position.x && component.position.x+offset.x+50 > position.x && component.position.y+offset.y < position.y && component.position.y+offset.y+50 > position.y){
                    component.position.x += Event.current.delta.x;
                    component.position.y += Event.current.delta.y;
                    this.draggedComponent = component;
                    return true;
                }
            }
            return false;
        }else{
            this.draggedComponent.position.x += Event.current.delta.x;
            this.draggedComponent.position.y += Event.current.delta.y;
            return true;
        }
    }
    public bool loadCircuit(Circuit circuit){
        if(circuit.Components.Count == 0){
            return false;
        }
        foreach(Component component in circuit.Components){
            add(component);
        }
        return true;
    }
    public bool addSolvedCallback(List<Pin> pins, float[] filters, Action callback){
        Check check = new Check(pins, filters, callback);
        if(check.Callback!=null&&check.checkablePins!=null&&check.filters!=null){
            checks.Add(check);
            return true;
        }
        return false;
    }
    public bool removeSolvedCallback(List<Pin> pins, float[] filters, Action callback){
        foreach (Check check in checks)
        {
            if(check.Callback == callback && check.checkablePins == pins && check.filters == filters){
                checks.Remove(check);
                return true;
            }
        }
        return false;
    }
}

public class Spawner{
    public Component component;
    public Vector2 position;
    public Circuit circuit;
    public int count;

    public Spawner(Component component, int count){
        this.component = component;
        this.position = component.position;
        this.count = count;
    }
    public void display(){
        this.component.display(new Vector2(0, 0), new MouseInfo());
        GUI.Label(new Rect(this.position.x+50,this.position.y+50,40,40),$"{count}");
    }
    public void update(MouseInfo mouseInfo, Vector2 offset){
        if((mouseInfo.position.x>position.x)&&(mouseInfo.position.x<position.x+50)&&(mouseInfo.position.y>position.y)&&(mouseInfo.position.y<position.y+50)){
        if(mouseInfo.up)Debug.Log("Over: "+circuit.draggedComponent);
            if(circuit.draggedComponent==null&&count>0&&mouseInfo.down){
                circuit.add((Component)Activator.CreateInstance(component.GetType(), new Vector2(this.position.x - offset.x, this.position.y + 75 - offset.y)));
                count--;
            }else if(circuit.draggedComponent!=null&&mouseInfo.up&&circuit.draggedComponent.isDeletable){
                Debug.Log("Delete");
                if(this.component.GetType()==circuit.draggedComponent.GetType()&&mouseInfo.up){
                    mouseInfo.up = false;
                    circuit.delete(circuit.draggedComponent);
                    count++;
                }
            }
        }
    }
}

public class Component{
    public Vector2 position;
    public List<Pin> pins;
    public bool isDeletable = true;
    public string name = "Component";
    public Vector2 size = new Vector2(50, 50);
    public Texture2D sprite = null;

    public Component(Vector2 position){
        this.position = position;
        this.pins = new List<Pin>();
        this.sprite = Info.mainTexture;
    }
    virtual public void display(Vector2 offset, MouseInfo mouseInfo){
        GUI.DrawTexture(new Rect(this.position.x+offset.x, this.position.y+offset.y, this.size.x, this.size.y), this.sprite);
        int i = 0;
        foreach (Pin pin in pins)
        {
            pin.display(offset, mouseInfo);
            i++;
        }
    }
    public void addPin(Pin pin, Type type){
        pin.type = type;
        pin.baseComponent = this;
        this.pins.Add(pin);
    }
    virtual public void update(Vector2 offset,MouseInfo mouseInfo){
        foreach (Pin pin in pins)
        {
            pin.update();
        }
    }
}

public enum Type
{
    Input,
    Output
}

public class Pin{
    public Vector2 position;
    public Type type = Type.Output;
    public float value = 0f;
    public List<Pin> connections = new List<Pin>();
    public Component baseComponent;
    public Pin(Vector2 position){
        this.position = position;
    }
    public void display(Vector2 offset, MouseInfo mouseInfo){
        circuitUtils.drawRectangle(new Vector2(this.position.x+this.baseComponent.position.x+offset.x-10, this.position.y+this.baseComponent.position.y+offset.y-10), new Vector2(20, 20), (this.value>=.5)?Color.red:Color.blue);
        if(this.type == Type.Output){
            return;
        }
        foreach (Pin component in connections)
        {
            if(component.type == Type.Output){
                Drawing.DrawLine(this.position+this.baseComponent.position+offset, component.position+component.baseComponent.position+offset, (this.value>=.5)?Color.red:Color.blue, 5, false);
            }
        }
    }
    public void update(){
        if(this.type == Type.Output){
            return;
        }
        this.value = 0f;
        foreach (Pin pin in this.connections)
        {
            if(pin.type == Type.Output){
                this.value = (pin.value<this.value)?this.value:pin.value;
            }
        }
    }
    public void connect(Pin pin){
        if(this.type == Type.Input){
            this.connections.Add(pin);
        }else{
            pin.connections.Add(this);
        }
    }
}

public class Button : Component{
    public Button(Vector2 position): base(position){
        this.position = position;
        this.addPin(new Pin(new Vector2(this.size.x, this.size.y/2)), Type.Output);
        this.name = "Button";
        this.sprite = Info.Button;
    }
    /*public override void display(Texture2D mainTexture, Texture2D pinTexture, Vector2 offset, MouseInfo mouseInfo){
        if(mouseInfo.down&&(mouseInfo.position.x>position.x+offset.x)&&(mouseInfo.position.x<position.x+offset.x+50)&&(mouseInfo.position.y>position.y+offset.y)&&(mouseInfo.position.y<position.y+offset.y+50)){
            this.pins[0].value = (this.pins[0].value-1)*(this.pins[0].value-1);
        }
        GUI.DrawTexture(new Rect(this.position.x+offset.x, this.position.y+offset.y, 50, 50), mainTexture);
        
        int i = 0;
        foreach (Pin pin in pins)
        {
            pin.display((this.pins[i].value>.5)? mainTexture : pinTexture, offset+position, mouseInfo);
            i++;
        }
        Debug.Log("Button: "+this.pins[0].value);
    }*/
    public override void update(Vector2 offset, MouseInfo mouseInfo){
        base.update(offset, mouseInfo);
        if(mouseInfo.down&&(mouseInfo.position.x>position.x+offset.x)&&(mouseInfo.position.x<position.x+offset.x+50)&&(mouseInfo.position.y>position.y+offset.y)&&(mouseInfo.position.y<position.y+offset.y+50)){
            this.pins[0].value = (this.pins[0].value-1)*(this.pins[0].value-1);
            this.sprite = (this.pins[0].value>.5)?Info.ButtonActive : Info.Button;
        }
    }
}

public class AGate : Component{
    public AGate(Vector2 position): base(position){
        this.position = position;
        this.addPin(new Pin(new Vector2(0, this.size.y/4)), Type.Input);
        this.addPin(new Pin(new Vector2(0, 3*this.size.y/4)), Type.Input);
        this.addPin(new Pin(new Vector2(this.size.x, this.size.y/2)), Type.Output);
        this.name = "Button";
        this.sprite = Info.AND;
    }
    /*public override void display(Texture2D mainTexture, Texture2D pinTexture, Vector2 offset, MouseInfo mouseInfo){
        if(mouseInfo.down&&(mouseInfo.position.x>position.x+offset.x)&&(mouseInfo.position.x<position.x+offset.x+50)&&(mouseInfo.position.y>position.y+offset.y)&&(mouseInfo.position.y<position.y+offset.y+50)){
            this.pins[0].value = (this.pins[0].value-1)*(this.pins[0].value-1);
        }
        GUI.DrawTexture(new Rect(this.position.x+offset.x, this.position.y+offset.y, 50, 50), mainTexture);
        
        int i = 0;
        foreach (Pin pin in pins)
        {
            pin.display((this.pins[i].value>.5)? mainTexture : pinTexture, offset+position, mouseInfo);
            i++;
        }
        Debug.Log("Button: "+this.pins[0].value);
    }*/
    public override void update(Vector2 offset, MouseInfo mouseInfo){
        base.update(offset, mouseInfo);
        this.pins[2].value = this.pins[0].value*this.pins[1].value;
    }
}
public class OGate : Component{
    public OGate(Vector2 position): base(position){
        this.position = position;
        this.addPin(new Pin(new Vector2(0, this.size.y/4)), Type.Input);
        this.addPin(new Pin(new Vector2(0, 3*this.size.y/4)), Type.Input);
        this.addPin(new Pin(new Vector2(this.size.x, this.size.y/2)), Type.Output);
        this.name = "Button";
        this.sprite = Info.OR;
    }
    /*public override void display(Texture2D mainTexture, Texture2D pinTexture, Vector2 offset, MouseInfo mouseInfo){
        if(mouseInfo.down&&(mouseInfo.position.x>position.x+offset.x)&&(mouseInfo.position.x<position.x+offset.x+50)&&(mouseInfo.position.y>position.y+offset.y)&&(mouseInfo.position.y<position.y+offset.y+50)){
            this.pins[0].value = (this.pins[0].value-1)*(this.pins[0].value-1);
        }
        GUI.DrawTexture(new Rect(this.position.x+offset.x, this.position.y+offset.y, 50, 50), mainTexture);
        
        int i = 0;
        foreach (Pin pin in pins)
        {
            pin.display((this.pins[i].value>.5)? mainTexture : pinTexture, offset+position, mouseInfo);
            i++;
        }
        Debug.Log("Button: "+this.pins[0].value);
    }*/
    public override void update(Vector2 offset, MouseInfo mouseInfo){
        base.update(offset, mouseInfo);
        this.pins[2].value = this.pins[0].value+this.pins[1].value;
    }
}

public class Lamp : Component{

    public Lamp(Vector2 position): base(position){
        this.position = position;
        this.pins = new List<Pin>();
        this.addPin(new Pin(new Vector2(0, 25)),Type.Input);
        this.name = "Lamp";
        this.sprite = Info.Lamp;
    }
    /*public override void display(Texture2D mainTexture, Texture2D pinTexture, Vector2 offset, MouseInfo mouseInfo){
        GUI.DrawTexture(new Rect(this.position.x+offset.x, this.position.y+offset.y, 50, 50), (this.pins[0].value==1)?pinTexture:mainTexture);
        int i = 0;
        foreach (Pin pin in pins)
        {
            pin.display((this.pins[i].value>.5)? mainTexture : pinTexture, offset+position, mouseInfo);
            i++;
        }
        Debug.Log("Lamp: "+this.pins[0].value);
    }*/
    public override void update(Vector2 offset, MouseInfo mouseInfo){
        base.update(offset, mouseInfo);
        this.sprite = (this.pins[0].value>.5)?Info.LampActive : Info.Lamp;
    }
}

public class Signal : Component{
    public Signal(Vector2 position): base(position){
        this.position = position;
        this.pins = new List<Pin>();
        this.pins.Add(new Pin(new Vector2(0, 25)));
        this.name = "Signal";
    }
    /*public override void display(Texture2D mainTexture, Texture2D pinTexture, Vector2 offset, MouseInfo mouseInfo){
        GUI.DrawTexture(new Rect(this.position.x+offset.x, this.position.y+offset.y, 50, 50), (this.pins[0].value==1)?pinTexture:mainTexture);
        int i = 0;
        foreach (Pin pin in pins)
        {
            pin.display((this.pins[i].value>.5)? mainTexture : pinTexture, offset+position, mouseInfo);
            i++;
        }
        Debug.Log("Lamp: "+this.pins[0].value);
    }*/
}

public class Osziloskop : Component{
    Texture2D drawTex;
    public int[] currents = new int[500];
    public Osziloskop(Vector2 position): base(position){
        this.position = position;
        this.pins = new List<Pin>();
        this.pins.Add(new Pin(new Vector2(-13, 25)));
        this.name = "Osziloskop";
        this.drawTex = new Texture2D(100,100);
        for(int i=0;i<this.currents.Length;i++){
            this.currents[i] = 0;
        }
    }
    /*public override void display(Texture2D mainTexture, Texture2D pinTexture, Vector2 offset, MouseInfo mouseInfo){
        //GUI.DrawTexture(new Rect(this.position.x+offset.x, this.position.y+offset.y, 50, 50), (this.pins[0].value==1)?pinTexture:mainTexture);
        int i = 0;
        this.drawTex = new Texture2D(1,1);
        this.drawTex.SetPixel(0,0,Color.blue);
        while(i<this.currents.Length){
            GUI.DrawTexture(new Rect(i*50/this.currents.Length+this.position.x+offset.x,(int)(25+Math.Sin((Time.realtimeSinceStartupAsDouble*2+i)/4)*25)+this.position.y+offset.y,1,1),this.drawTex);
            i++;
        }
        //GUI.DrawTexture(new Rect(this.position.x+offset.x, this.position.y+offset.y, 50, 50), this.drawTex);
    }*/
}

public class MouseInfo{
    public Vector2 position;
    public bool pressed;
    public bool down;
    public bool up;
    public MouseInfo(){

    }
}