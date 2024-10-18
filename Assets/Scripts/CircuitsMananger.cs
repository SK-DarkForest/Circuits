using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitsMananger : MonoBehaviour
{
    // Start is called before the first frame update
    private Circuit circuit;
    private bool buttonPressed = false;
    public bool dragging = false;
    private MouseInfo mouseInfo;
    private Texture2D mainTexture, pinTexture;
    public bool solved = false;
    void Start()
    {
        mainTexture = new Texture2D(1, 1);
        mainTexture.SetPixel(0, 0, Color.white);
        mainTexture.Apply();
        pinTexture = new Texture2D(1, 1);
        pinTexture.SetPixel(0, 0, Color.red);
        pinTexture.Apply();
        Info.mainTexture = mainTexture;
        Info.pinTexture = pinTexture;
        this.mouseInfo = new();
        Button button = new Button(new Vector2(100, 150));
        Button button2 = new Button(new Vector2(100, 350));
        Lamp lamp = new Lamp(new Vector2(200, 100));
        Lamp lamp2 = new Lamp(new Vector2(300, 100));
        Lamp lamp3 = new Lamp(new Vector2(300, 300));
        AGate AND = new AGate(new Vector2(200, 200));
        OGate OR = new OGate(new Vector2(200, 300));
        circuit = new();
        circuit.add(button);
        circuit.add(button2);
        circuit.add(lamp);
        circuit.add(lamp2);
        circuit.add(lamp3);
        circuit.add(AND);
        circuit.add(OR);
        circuit.addSpawner(new Spawner(new Button(new Vector2(100,0)),5));
        circuit.addSpawner(new Spawner(new Lamp(new Vector2(200,0)),5));
        circuit.addSpawner(new Spawner(new AGate(new Vector2(300,0)),5));
        circuit.addSpawner(new Spawner(new OGate(new Vector2(400,0)),5));
        button.pins[0].connect(lamp.pins[0]);
        button.pins[0].connect(AND.pins[0]);
        button2.pins[0].connect(AND.pins[1]);
        button2.pins[0].connect(OR.pins[1]);
        button.pins[0].connect(OR.pins[0]);
        AND.pins[2].connect(lamp2.pins[0]);
        OR.pins[2].connect(lamp3.pins[0]);
        List<Pin> pins = new List<Pin>();
        pins.Add(lamp.pins[0]);
        pins.Add(lamp2.pins[0]);
        float[] filters = {.5f,.5f};
        circuit.addSolvedCallback(pins, filters, Callback);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnGUI() {
        if(Event.current.type == EventType.MouseDown)
        {
            buttonPressed = true;
        }
        if(Event.current.type == EventType.MouseUp)
        {
            mouseInfo.up = buttonPressed;
            buttonPressed = false;
            circuit.draggedComponent = null;
            //circuit.draggedPin = null;
        }

        if(buttonPressed && Event.current.type == EventType.MouseDrag&&!circuit.mouseOverPinsOrComponent(mouseInfo)&&!circuit.isDraggingPin)
        {
            if(!circuit.moveComponent(Event.current.mousePosition)){
                circuit.offset.x += Event.current.delta.x;
                circuit.offset.y += Event.current.delta.y;
            }
        }

        mouseInfo.position = Event.current.mousePosition;
        mouseInfo.down = (Input.GetMouseButtonDown(0)&&!mouseInfo.pressed);
        mouseInfo.pressed = Input.GetMouseButton(0);
        circuit.display(mouseInfo);
        circuit.update(mouseInfo);
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

