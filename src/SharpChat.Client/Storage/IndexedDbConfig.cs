namespace Messenger.BlazorClient.Storage;

/// <summary>
/// Конфигурация IndexedDB для SharpChat
/// </summary>
public static class IndexedDbConfig
{
    public const string DatabaseName = "SharpChatDB";
    public const int DatabaseVersion = 1;

    // Имена хранилищ (object stores)
    public const string MessagesStore = "messages";
    public const string ChatsStore = "chats";
    public const string Id = "id";

    /// <summary>
    /// Список всех хранилищ в базе данных
    /// </summary>
    public static string[] ObjectStores => new[] { MessagesStore, ChatsStore };
}