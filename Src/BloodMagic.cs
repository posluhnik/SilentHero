using System;
using System.Text.Json.Serialization;
using System.Xml.Linq;

[JsonDerivedType(typeof(BuffSpell), typeDiscriminator: "buff")]
[JsonDerivedType(typeof(DamageSpell), typeDiscriminator: "damage")]
class BloodSpell
{
    public string name {  get; private set; }
    public string description { get; private set; }
    public double damageToUser { get; private set; }
    public string useMessage { get; private set; }

    public BloodSpell(string name, string description, double damageToUser, string useMessage)
    {
        this.name = name;
        this.description = description;
        this.damageToUser = damageToUser;
        this.useMessage = useMessage;
    }
}

class BuffSpell : BloodSpell
{
    public string effect { get; private set; }
    public double buff {  get; private set; }

    public BuffSpell(string name, string description, double damageToUser, string useMessage, string effect, double buff) : base(name, description, damageToUser, useMessage)
    {
        this.effect = effect;
        this.buff = buff;
    }
}

class DamageSpell : BloodSpell
{
    public double damage { get; private set; }

    public DamageSpell(string name, string description, double damageToUser, string useMessage, double damage) : base(name, description, damageToUser, useMessage)
    {
        this.damage = damage;
    }
}