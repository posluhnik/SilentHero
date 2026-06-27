using System;

class NPC
{
    public string name {  get; private set; }
    public string description { get; private set; }
    public int timesTalked { get; private set; } = 0;
    public Dictionary<string, List<string>> dialogues { get; private set; }

    public NPC(string name, string description, int timesTalked, Dictionary<string, List<string>> dialogues)
    {
        this.name = name;
        this.description = description;
        this.dialogues = dialogues;
    }

    public List<string> GetDialogue()
    {
        List<string> dialogue = dialogues[timesTalked.ToString()];

        if (timesTalked < dialogues[timesTalked.ToString()].Count() -1)
        {
            timesTalked += 1;
        }
            
        return dialogue;
    }
}