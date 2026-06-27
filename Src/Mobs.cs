using System;


class Mob
{
    public string name { get; private set; }
    public string description { get; private set; }
    public string attackMessage { get; private set; }
    public double hp { get; set; }
    public double damage { get; private set; }

    public Mob(string name, string description, string attackMessage, double hp, double damage)
    {
        this.name = name;
        this.description = description;
        this.attackMessage = attackMessage;
        this.hp = hp;
        this.damage = damage;
    }
}