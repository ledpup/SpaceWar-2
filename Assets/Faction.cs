using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction
{
    internal static Color Colour(string tag)
    {
        switch (tag)
        {
            case "Faction1":
                return Color.green;
            case "Faction2":
                return Color.red;
            case "Faction3":
                return Color.magenta;
        }
        return Color.yellow;
    }
}
