using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Player
{
    public string Name;
    public Color Color;
    public int points = 0;

    public Player(string name, Color color)
    {
        Name = name;
        Color = color;
    }
}
