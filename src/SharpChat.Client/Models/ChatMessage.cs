namespace Messenger.BlazorClient.Models;

public record ChatMessage(
    string User, 
    string Text, 
    string Timestamp,
    bool IsOwnMessage
);