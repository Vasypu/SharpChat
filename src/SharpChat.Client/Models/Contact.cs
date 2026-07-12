namespace Messenger.BlazorClient.Models;

public class Contact
{
    public string Name { get; set; } = string.Empty;
    public List<ChatMessage> Messages { get; set; } = new();
}