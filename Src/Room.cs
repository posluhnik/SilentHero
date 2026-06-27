using System;


class Room
{
    public string name { get; private set; }
    public string description { get; private set; }
    public string betterDescription { get; set; }
    public string mobs { get; set; }
    public string items { get; set; }
    public string NPCs { get; set; }
    public Dictionary<string, string> exits { get; private set; }


    public Room(string name, string description, string betterDescription, string mobs, string items, string NPCs, Dictionary<string, string> exits)
    {
        this.name = name;
        this.description = description;
        this.betterDescription = betterDescription;
        this.mobs = mobs;
        this.items = items;
        this.NPCs = NPCs;
        this.exits = exits;
    }
}