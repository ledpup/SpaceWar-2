using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHud : MonoBehaviour {

    public Text SpeedText;
    public Text NameText;
    public Text ArmourText;
    public Text FuelText;

    void Start () {
        var controllerName = name.Replace("(Clone)", "");

        if (controllerName.StartsWith("Player"))
        {

            Vector2 anchorMin = Vector2.zero, anchorMax = Vector2.zero;
            int x = 0;
            if (controllerName == "Player1")
            {
                anchorMin = new Vector2(0, 1);
                anchorMax = new Vector2(0, 1);
                x = 20;
            }
            else if (controllerName == "Player2")
            {
                anchorMin = new Vector2(1, 1);
                anchorMax = new Vector2(1, 1);
                x = -100;
            }

            var canvas = GameObject.Find("Canvas");
            NameText = CreateTextElement(canvas, controllerName, "Name", x, -20, anchorMin, anchorMax);
            NameText.text = controllerName;
            NameText.color = Faction.Colour(tag);

            SpeedText = CreateTextElement(canvas, controllerName, "Speed", x, -40, anchorMin, anchorMax);
            ArmourText = CreateTextElement(canvas, controllerName, "Armour", x, -60, anchorMin, anchorMax);
            FuelText = CreateTextElement(canvas, controllerName, "Fuel", x, -80, anchorMin, anchorMax);
        }
    }

    private Text CreateTextElement(GameObject canvas, string controllerName, string elementName, int x, int y, Vector2 anchorMin, Vector2 anchorMax)
    {

        var go = new GameObject(controllerName + elementName);
        go.transform.parent = canvas.transform;

        var text = go.AddComponent<Text>();

        var rectTransform = text.GetComponent<RectTransform>();

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.offsetMin = new Vector2(x, y - 30f);
        rectTransform.offsetMax = new Vector2(x + 160f, y);

        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.color = Color.white;

        return text;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
