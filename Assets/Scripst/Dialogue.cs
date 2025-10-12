using System.Collections.Generic;

[System.Serializable]
public class Dialogue
{
    public string speaker;
    public string text;
    public List<DialogueOption> options;

    public Dialogue(string speaker, string text)
    {
        this.speaker = speaker;
        this.text = text;
        this.options = new List<DialogueOption>();
    }
}

[System.Serializable]
public class DialogueOption
{
    public string text;
    public string nextDialogueId;

    public DialogueOption(string text, string nextDialogueId)
    {
        this.text = text;
        this.nextDialogueId = nextDialogueId;
    }
}
