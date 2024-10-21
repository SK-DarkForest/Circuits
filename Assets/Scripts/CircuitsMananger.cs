using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CircuitsMananger : MonoBehaviour
{
    // Start is called before the first frame update
    private Circuit circuit;
    private bool buttonPressed = false;
    public bool dragging = false;
    private Texture2D mainTexture, pinTexture;
    public bool solved = false;
    private Lamp lamp, lamp2, lamp3;
    public static Texture2D load(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D loadedTexture = new Texture2D(2, 2);
        loadedTexture.LoadImage(fileData);
        return loadedTexture;
    }
    void Start()
    {
        Info.Button = load("Button.png");
        Info.Lamp = load("Lamp.png");
        Info.ButtonActive = load("ButtonActive.png");
        Info.LampActive = load("LampActive.png");
        Info.OR = load("OR.png");
        Info.AND = load("AND.png");
        mainTexture = new Texture2D(1, 1);
        mainTexture.SetPixel(0, 0, Color.white);
        mainTexture.Apply();
        pinTexture = new Texture2D(1, 1);
        pinTexture.SetPixel(0, 0, Color.red);
        pinTexture.Apply();
        Info.mainTexture = mainTexture;
        Info.pinTexture = pinTexture;
        //Circuit Setup
        Button button = new Button(new Vector2(100, 150));
        Button button2 = new Button(new Vector2(100, 350));
        lamp = new Lamp(new Vector2(200, 100));
        lamp2 = new Lamp(new Vector2(300, 100));
        lamp3 = new Lamp(new Vector2(300, 300));
        AGate AND = new AGate(new Vector2(200, 200));
        OGate OR = new OGate(new Vector2(200, 300));
        circuit = new();
        circuit.addPermanent(button);
        circuit.addPermanent(button2);
        circuit.addPermanent(lamp);
        circuit.addPermanent(lamp2);
        circuit.addPermanent(lamp3);
        circuit.addPermanent(AND);
        circuit.addPermanent(OR);
        circuit.addSpawner(new Spawner(new Button(new Vector2(100,0)),5));
        circuit.addSpawner(new Spawner(new Lamp(new Vector2(200,0)),5));
        circuit.addSpawner(new Spawner(new AGate(new Vector2(300,0)),5));
        circuit.addSpawner(new Spawner(new OGate(new Vector2(400,0)),5));
        //Check
        List<Pin> pins = new List<Pin>();
        pins.Add(lamp.pins[0]);
        pins.Add(lamp2.pins[0]);
        float[] filters = {.5f,.5f};
        circuit.addSolvedCallback(pins, filters, Callback);
    }

    // Update is called once per frame
    void Update()
    {
        if(mouseInfo.up) Debug.Log("Up");
        if(circuit.draggedComponent!=null)Debug.Log(circuit.draggedComponent);
        //Debug.Log(mouseInfo.up);
        //Debug.Log("1: "+lamp.pins[0].value+", 2: "+lamp2.pins[0].value+", 3: "+lamp3.pins[0].value);
        //Debug.Log("Circuit.isDraggingPin: "+circuit.isDraggingPin+" Circuit.draggedPin: "+circuit.draggedPin+ " Circuit.draggedComponent: "+circuit.draggedComponent+ "Dragging: "+dragging);
    }
    private void OnGUI() {
        if(Event.current.type == EventType.MouseDown)
        {
            buttonPressed = true;
        }
        if(Event.current.type == EventType.MouseUp)
        {
            //mouseInfo.up = buttonPressed;
            buttonPressed = false;
            //circuit.draggedPin = null;
        }
        mouseInfo.up = (Event.current.type == EventType.MouseUp);
        if(buttonPressed && Event.current.type == EventType.MouseDrag&&!circuit.isDraggingPin)
        {
            if(!circuit.moveComponent(Event.current.mousePosition)){
                circuit.offset.x += Event.current.delta.x;
                circuit.offset.y += Event.current.delta.y;
            }
        }

        mouseInfo.position = Event.current.mousePosition;
        mouseInfo.down = (Input.GetMouseButtonDown(0)&&!mouseInfo.pressed);
        mouseInfo.pressed = Input.GetMouseButton(0);
        circuit.display();
        circuit.update();
        if(solved) Message();
    }
    public void Callback(){
        Debug.Log("Solved!");
        solved = true;
    }
    public void Message(){
        float overlayWidth = 400;
        float overlayHeight = 200;
        float overlayX = (Screen.width - overlayWidth) / 2;
        float overlayY = (Screen.height - overlayHeight) / 2;

        // Draw a semi-transparent background
        Color color = Color.white;
        color.a = .5f;
        GUI.color = color;
        GUI.DrawTexture(new Rect(overlayX, overlayY, overlayWidth, overlayHeight), Info.mainTexture);

        // Reset the color to white for the text
        GUI.color = Color.black;

        // Set the font size (optional)
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 20;
        style.alignment = TextAnchor.MiddleCenter;

        // Display the title text
        GUI.Label(new Rect(overlayX, overlayY-50, overlayWidth, overlayHeight), "Du hast dein erstes Rätsel gelöst!", style);
        GUI.Label(new Rect(overlayX, overlayY+50, overlayWidth, overlayHeight), "Die Tür öffnet sich.", style);
    }
}

