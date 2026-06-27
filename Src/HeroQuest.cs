using System;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;

class HeroQuest
{
    // global managers data for save arrays
    public static Dictionary<string, Room> Rooms { get; private set; }
    public static Dictionary<string, Mob> Mobs { get; private set; }
    public static Dictionary<string, Item> Items { get; private set; }
    public static Dictionary<string, NPC> Npcs { get; private set; }
    public static Dictionary<string, BloodSpell> BloodSpells { get; private set; }

    public static Character SilentHero { get; } = new Character();
    public static bool IsRunning { get; set; } = true;
    public static string CurrentRoomId { get; set; } = "room_0";

    static void Main()
    {
        Console.CursorVisible = true;

        if (!TryLoadGameData()) return;

        Console.CursorVisible = false;
        UIEngine.DrawInterfaceGrid();

        // main loop for game state not crash stack overflow very important
        while (IsRunning)
        {
            Room currentRoom = Rooms[CurrentRoomId];

            // if monster alive in room we fight him now
            if (currentRoom.mobs != "no" && Mobs.ContainsKey(currentRoom.mobs) && Mobs[currentRoom.mobs].hp > 0)
            {
                GameplayManager.ExecuteBattle(currentRoom.mobs);
            }
            else
            {
                GameplayManager.ExecuteRoomExploration(currentRoom);
            }
        }

        Console.Clear();
        Console.CursorVisible = true;
        Console.WriteLine("Game over.");
    }

    private static bool TryLoadGameData()
    {
        try
        {
            Rooms = GenerateEverything<Room>.Generate("Json/Rooms.json");
            Mobs = GenerateEverything<Mob>.Generate("Json/Mobs.json");
            Items = GenerateEverything<Item>.Generate("Json/Items.json");
            Npcs = GenerateEverything<NPC>.Generate("Json/NPCs.json");
            BloodSpells = GenerateEverything<BloodSpell>.Generate("Json/BloodSpells.json");

            if (Rooms == null || !Rooms.ContainsKey(CurrentRoomId))
            {
                UIEngine.SetScreenData("Starting Room", $"JSON loaded, but {CurrentRoomId} not found.", "Error");
            }
            else
            {
                UIEngine.SetScreenData(Rooms[CurrentRoomId].name, Rooms[CurrentRoomId].description, "room");
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("=== CRITICAL ERROR LOADING JSON DATA ===");
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"\nCurrent Working Directory: {Environment.CurrentDirectory}");
            Console.WriteLine("Please copy your .json files into the directory listed above!\n\nPress any key to exit...");
            Console.ReadKey();
            return false;
        }
    }
}

// big logic manager for screens combat and items
static class GameplayManager
{
    private static readonly List<string> RoomCommandsBase = new() { "look around", "inventory", "end it" };
    private static readonly List<string> BattleCommandsBase = new() { "attack", "check spells", "end it" };
    private static readonly List<string> DialogueCommands = new() { "silence", "end it" };
    private static readonly List<string> InventoryCommands = new() { "use", "equip", "exit" };

    public static void ExecuteBattle(string mobId)
    {
        Mob mob = HeroQuest.Mobs[mobId];
        Character hero = HeroQuest.SilentHero;
        Weapon heroWeapon = (Weapon)hero.equipedItems["weapon"];

        UIEngine.SetScreenData(mob.name, mob.description, "battle");

        bool heroMove = true;
        double currentBloodMagicBuff = 0.0;
        var currentBattleCommands = new List<string>(BattleCommandsBase);

        while (HeroQuest.IsRunning && mob.hp > 0 && hero.hp > 0)
        {
            UIEngine.UpdateStats(hero, mob);
            UIEngine.UpdateCommands(currentBattleCommands);
            UIEngine.UpdateGameScreen();
            mob.hp = Math.Round(mob.hp, 1);
            if (heroMove)
            {
                string input = UIEngine.GetPlayerInput();

                if (input == "attack")
                {
                    UIEngine.SetDescription(heroWeapon.attackMessage);
                    mob.hp -= (heroWeapon.damage * hero.damageBuff);
                    heroMove = false;
                }
                else if (input == "check spells")
                {
                    if (!currentBattleCommands.Contains("cast")) currentBattleCommands.Add("cast");
                    string spellsList = string.Join("\n", hero.spells.Select(s => HeroQuest.BloodSpells[s].name));
                    UIEngine.SetDescription(spellsList);
                }
                else if (input.StartsWith("cast "))
                {
                    string spellName = input.Substring(5).Trim();
                    var spellEntry = HeroQuest.BloodSpells.Values.FirstOrDefault(s => s.name.ToLower() == spellName);

                    if (spellEntry != null)
                    {
                        heroMove = false;
                        if (spellEntry is DamageSpell damageSpell)
                        {
                            mob.hp -= damageSpell.damage;
                            hero.hp -= damageSpell.damageToUser;
                        }
                        else if (spellEntry is BuffSpell buffSpell)
                        {
                            currentBloodMagicBuff += buffSpell.buff;
                            hero.damageBuff += buffSpell.buff;
                            hero.hp -= buffSpell.damageToUser;
                            hero.NewStats(hero, currentBloodMagicBuff);
                        }
                    }
                }
                else if (input == "end it")
                {
                    HeroQuest.IsRunning = false;
                }
            }
            else
            {
                UIEngine.SetDescription(mob.attackMessage);
                hero.hp -= mob.damage;
                heroMove = true;
            }
        }

        if (hero.hp > 0)
        {
            hero.damageBuff -= currentBloodMagicBuff;
            hero.NewStats(hero, 0);
            UIEngine.SetScreenData(HeroQuest.Rooms[HeroQuest.CurrentRoomId].name, HeroQuest.Rooms[HeroQuest.CurrentRoomId].description, "room");
        }
        else
        {
            HeroQuest.IsRunning = false;
        }
    }

    public static void ExecuteRoomExploration(Room room)
    {
        Character hero = HeroQuest.SilentHero;
        hero.NewStats(hero, 0);
        var currentRoomCommands = new List<string>(RoomCommandsBase);

        foreach (string exit in room.exits.Keys)
            currentRoomCommands.Add($"go {exit}");

        if (room.NPCs != "no")
            currentRoomCommands.Add("approach");

        UIEngine.SetScreenData(room.name, room.description, "room");
        UIEngine.UpdateStats(hero);

        while (HeroQuest.IsRunning && HeroQuest.Rooms[HeroQuest.CurrentRoomId] == room)
        {
            UIEngine.UpdateCommands(currentRoomCommands);
            UIEngine.UpdateGameScreen();

            string input = UIEngine.GetPlayerInput();

            if (input == "look around")
            {
                UIEngine.SetDescription(room.betterDescription);
                if (room.items != "no" && !currentRoomCommands.Contains("take"))
                {
                    currentRoomCommands.Add("take");
                }
            }
            else if (input.StartsWith("go "))
            {
                string exitDirection = input.Substring(3).Trim();
                if (room.exits.ContainsKey(exitDirection))
                {
                    HeroQuest.CurrentRoomId = room.exits[exitDirection];
                    return; // break method for Main look new room id
                }
            }
            else if (input == "take" && room.items != "no")
            {
                Item item = HeroQuest.Items[room.items];
                room.items = "no";
                hero.inventory.Add(item);
                hero.NewStats(hero, 0);

                currentRoomCommands.Remove("take");
                room.betterDescription = "Nothing interesting here anymore.";
                UIEngine.SetScreenData(item.name, item.description, "room");
            }
            else if (input == "approach" && room.NPCs != "no")
            {
                ExecuteDialogue(room.NPCs);
                return;
            }
            else if (input == "inventory")
            {
                ExecuteInventory();
                return;
            }
            else if (input == "end it")
            {
                HeroQuest.IsRunning = false;
            }
        }
    }

    private static void ExecuteDialogue(string npcId)
    {
        NPC npc = HeroQuest.Npcs[npcId];
        List<string> dialogueLines = npc.GetDialogue();

        // if npc have zero talking we leave
        if (dialogueLines == null || dialogueLines.Count == 0) return;

        int currentLineIndex = 0;

        // force insert first text npc for screen
        UIEngine.SetScreenData(npc.name, dialogueLines[currentLineIndex], "dialogue");
        UIEngine.UpdateCommands(DialogueCommands);

        while (HeroQuest.IsRunning)
        {
            UIEngine.UpdateGameScreen();
            string input = UIEngine.GetPlayerInput(); // player click enter for read

            if (input == "end it")
            {
                HeroQuest.IsRunning = false;
                return;
            }

            // go to next text array after player do enter button
            currentLineIndex++;

            // if talking finished we close dialogue softly
            if (currentLineIndex >= dialogueLines.Count)
            {
                break;
            }

            // show next phrase on display
            UIEngine.SetDescription(dialogueLines[currentLineIndex]);
        }
    }

    private static void ExecuteInventory()
    {
        Character hero = HeroQuest.SilentHero;
        UIEngine.SetScreenData("Inventory", "", "inventory");
        UIEngine.UpdateCommands(InventoryCommands);

        while (HeroQuest.IsRunning)
        {
            UIEngine.UpdateStats(hero);

            // print list things with interfaces linq style text
            string itemList = string.Join("\n", hero.inventory.Select(item =>
                item is IEquipable ? $"{item.name} [Equipable]" :
                item is IUseable ? $"{item.name} [Useable]" : item.name));

            UIEngine.SetDescription(string.IsNullOrEmpty(itemList) ? "Your inventory is empty." : itemList);
            UIEngine.UpdateGameScreen();

            string input = UIEngine.GetPlayerInput();

            if (input == "exit") return;

            if (input.StartsWith("equip "))
            {
                string itemName = input.Substring(6).Trim();
                Item item = hero.inventory.FirstOrDefault(i => i.name.ToLower() == itemName);

                if (item is IEquipable equipable)
                {
                    hero.inventory.Remove(item);
                    if (item is Weapon weapon) weapon.Equip(hero);
                    if (item is Accessory acc) acc.Equip(hero);
                    hero.NewStats(hero, 0);
                }
            }
            else if (input.StartsWith("use "))
            {
                string itemName = input.Substring(4).Trim();
                Item item = hero.inventory.FirstOrDefault(i => i.name.ToLower() == itemName);

                if (item is IUseable useable && item is Potion potion)
                {
                    hero.inventory.Remove(item);
                    potion.Use(hero);
                }
            }
        }
    }
}

// graphics print machine and user clicks
static class UIEngine
{
    private const int Width = 120;
    private const int Height = 30;
    private const int TopZoneHeight = 15;

    private static string _eventName = string.Empty;
    private static string _description = string.Empty;
    private static string _commands = string.Empty;
    private static string _stats = string.Empty;
    private static string _state = "room";

    public static void SetScreenData(string eventName, string desc, string state)
    {
        _eventName = eventName;
        _description = desc;
        _state = state;
    }

    public static void SetDescription(string desc) => _description = desc;

    public static void UpdateCommands(List<string> commandsList)
    {
        _commands = string.Join("\n", commandsList);
    }

    public static void UpdateStats(Character hero, Mob mob = null)
    {
        _stats = $"[ HERO STATS ]\nHP: {hero.hp}\nDMG: {hero.damage}";
        if (mob != null)
        {
            _stats += $"\n\n[ ENEMY STATS ]\nHP: {mob.hp}\nDMG: {mob.damage}";
        }
    }

    public static void DrawInterfaceGrid()
    {
        Console.Clear();
        Console.SetCursorPosition(0, TopZoneHeight);
        Console.Write(new string('─', Width));

        for (int y = TopZoneHeight + 1; y < Height - 2; y++)
        {
            Console.SetCursorPosition(70, y); Console.Write('│');
            Console.SetCursorPosition(100, y); Console.Write('│');
        }

        Console.SetCursorPosition(0, Height - 3);
        Console.Write(new string('─', Width));
    }

    public static void UpdateGameScreen()
    {
        RenderBlock($"                    ██████  ██▓ ██▓    ▓█████  ███▄    █ ▄▄▄█████▓    ██░ ██ ▓█████  ██▀███   ▒█████  \r\n                  ▒██    ▒ ▓██▒▓██▒    ▓█   ▀  ██ ▀█   █ ▓  ██▒ ▓▒   ▓██░ ██▒▓█   ▀ ▓██ ▒ ██▒▒██▒  ██▒\r\n                  ░ ▓██▄   ▒██▒▒██░    ▒███   ▓██  ▀█ ██▒▒ ▓██░ ▒░   ▒██▀▀██░▒███   ▓██ ░▄█ ▒▒██░  ██▒\r\n                    ▒   ██▒░██░▒██░    ▒▓█  ▄ ▓██▒  ▐▌██▒░ ▓██▓ ░    ░▓█ ░██ ▒▓█  ▄ ▒██▀▀█▄  ▒██   ██░\r\n                  ▒██████▒▒░██░░██████▒░▒████▒▒██░   ▓██░  ▒██▒ ░    ░▓█▒░██▓░▒████▒░██▓ ▒██▒░ ████▓▒░\r\n                  ▒ ▒▓▒ ▒ ░░▓  ░ ▒░▓  ░░░ ▒░ ░░ ▒░   ▒ ▒   ▒ ░░       ▒ ░░▒░▒░░ ▒░ ░░ ▒▓ ░▒▓░░ ▒░▒░▒░ \r\n                  ░ ░▒  ░ ░ ▒ ░░ ░ ▒  ░ ░ ░  ░░ ░░   ░ ▒░    ░        ▒ ░▒░ ░ ░ ░  ░  ░▒ ░ ▒░  ░ ▒ ▒░ \r\n                  ░  ░  ░   ▒ ░  ░ ░      ░      ░   ░ ░   ░          ░  ░░ ░   ░     ░░   ░ ░ ░ ░ ▒  \r\n                        ░   ░      ░  ░   ░  ░         ░              ░  ░  ░   ░  ░   ░         ░ ░  \r\n                                                                                                      ", 0, 0, Width, TopZoneHeight);
        RenderBlock($"{_eventName}\n\n{WrapText(_description, 68)}", 0, TopZoneHeight + 1, 68, 10);
        RenderBlock(_commands, 72, TopZoneHeight + 1, 26, 10);
        RenderBlock(_stats, 102, TopZoneHeight + 1, 16, 10);
    }

    public static string GetPlayerInput()
    {
        int inputY = Height - 2;
        Console.SetCursorPosition(0, inputY);
        Console.Write(new string(' ', Width));
        Console.SetCursorPosition(0, inputY);
        Console.Write("-> ");

        Console.CursorVisible = true;
        string input = Console.ReadLine()?.ToLower().Trim() ?? string.Empty;
        Console.CursorVisible = false;
        return input;
    }

    private static void RenderBlock(string text, int startX, int startY, int width, int height)
    {
        string[] lines = text.Split('\n');
        for (int i = 0; i < height; i++)
        {
            Console.SetCursorPosition(startX, startY + i);
            Console.Write(new string(' ', width));
            Console.SetCursorPosition(startX, startY + i);

            if (i < lines.Length)
            {
                string safeLine = lines[i].Length > width ? lines[i].Substring(0, width) : lines[i];
                Console.Write(safeLine);
            }
        }
    }

    private static string WrapText(string text, int width)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        // fast algorithm for break long strings without crash box limit 68
        string[] words = text.Replace("\n", " \n ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var lines = new List<string>();
        string currentLine = "";

        foreach (var word in words)
        {
            if (word == "\n")
            {
                lines.Add(currentLine.TrimEnd());
                currentLine = "";
                continue;
            }
            if (currentLine.Length + word.Length + 1 > width)
            {
                lines.Add(currentLine.TrimEnd());
                currentLine = word + " ";
            }
            else
            {
                currentLine += word + " ";
            }
        }
        if (currentLine.Length > 0) lines.Add(currentLine.TrimEnd());
        return string.Join("\n", lines);
    }
}