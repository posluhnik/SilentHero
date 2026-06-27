using System;
using System.Text.Json.Serialization;
using System.Xml.Linq;


[JsonDerivedType(typeof(Item), typeDiscriminator: "base")]
[JsonDerivedType(typeof(Weapon), typeDiscriminator: "weapon")]
[JsonDerivedType(typeof(Accessory), typeDiscriminator: "accessory")]
[JsonDerivedType(typeof(Potion), typeDiscriminator: "potion")]
class Item
{
    public string name { get; private set; }
    public string description { get; private set; }

    public Item(string name, string description)
    {
        this.name = name;
        this.description = description;
    }
}

interface IUseable
{
    void Use(Character character);
}

interface IEquipable
{
    void Equip(Character character);
}



class Weapon: Item, IEquipable
{
    public int damage { get; set; }
    public string attackMessage { get; private set; }

    public Weapon(string name, string description, int damage, string attackMessage) : base(name, description)
    {
        this.damage = damage;
        this.attackMessage = attackMessage;
    }
    public void Equip(Character character)
    {
        
        character.equipedItems["weapon"] = this;
    }
}
class Accessory : Item, IEquipable
{
    public string effect { get; private set; }
    public double damageBuff { get; private set; }
    public double hpBuff { get; private set; }
    public double bloodMagicBuff { get; private set; }

    // Ошибка была тут: параметр теперь содержит только латинские буквы 'effect'
    public Accessory(string name, string description, string effect, double damageBuff, double hpBuff) : base(name, description)
    {
        this.effect = effect;
        this.damageBuff = damageBuff;
        this.hpBuff = hpBuff;
    }

    public void Equip(Character character)
    {
        character.equipedItems["accessory"] = this;
        // После экипировки обязательно вызываем пересчет статов!
        character.NewStats(character, 0);
    }
}
class Potion : Item, IUseable
{
    public string effect { get; private set; }
    public double buff { get; private set; }

    public Potion(string name, string description, string effect, double buff) : base(name, description)
    {
        this.effect = effect;
        this.buff = buff;
    }


    public void Use(Character character)
    {
        
        if (effect == "heal")
        {
            double overHeal = (character.hp + buff) - character.maxHp;
            if (overHeal > 0)
            {
                character.hp += buff;
                character.hp -= overHeal;
            }
            else
            {
                character.hp += buff;
            }
        }
    }
}
