using System;

class Character
{
    public string name { get; private set; } = "Silent Hero";

    private readonly double baseMaxHp = 100;
    private readonly double baseDamage = 5;

    public double hp { get; set; } = 100;
    public double maxHp { get; set; } = 100;
    public double hpBuff { get; set; } = 1.0;
    public double damageBuff { get; set; } = 1.0;
    public double damage { get; set; } = 5;

    public List<string> spells { get; set; } = ["spell_0", "spell_1"];
    public List<Item> inventory { get; set; } = new List<Item>();

    public Dictionary<string, Item> equipedItems { get; set; } = new Dictionary<string, Item>
    {
        ["weapon"] = new Weapon(
            "Worn Bandages",
            "Old, bloody linen wrapped around your knuckles. Your only weapon for now.",
            4,
            "You strike hard with your cloth-wrapped fists, drawing dark blood."
        ),

        ["accessory"] = new Accessory(
            "Iron Punishment",
            "The heavy, cold mask welded onto your head. It locks your jaw and seals your voice, but protects your broken skull.",
            "silence",
            0.0,
            0.15
        )
    };

    public void NewStats(Character hero, double bloodMagicBuff)
    {
        hero.damageBuff = 1.0 + bloodMagicBuff;
        hero.hpBuff = 1.0;

        if (hero.equipedItems.TryGetValue("accessory", out Item currentAccessory) && currentAccessory is Accessory acc)
        {
            hero.damageBuff += acc.damageBuff;
            hero.hpBuff += acc.hpBuff;
        }

        if (hero.equipedItems.TryGetValue("weapon", out Item currentWeapon) && currentWeapon is Weapon weapon)
        {
            hero.damage = Math.Round(weapon.damage * hero.damageBuff, 1);
        }
        else
        {
            hero.damage = Math.Round(baseDamage * hero.damageBuff, 1);
        }

        double oldMaxHp = hero.maxHp;
        hero.maxHp = Math.Round(hero.baseMaxHp * hero.hpBuff, 1);

        if (hero.maxHp > oldMaxHp && hero.hp == oldMaxHp)
        {
            hero.hp = hero.maxHp;
        }

        hero.hp = Math.Round(hero.hp, 1);
        if (hero.hp > hero.maxHp)
        {
            hero.hp = hero.maxHp;
        }
    }
}