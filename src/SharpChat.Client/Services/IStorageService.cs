using Messenger.BlazorClient.Models.StorageModels;

namespace Messenger.BlazorClient.Services;

/// <summary>
/// Интерфейс сервиса для работы с локальным хранилищем (IndexedDB)
/// </summary>
public interface IStorageService
{
    #region Messages
    
    /// <summary>
    /// Сохраняет сообщение в локальное хранилище
    /// </summary>
    Task SaveMessageAsync(Message message);
    
    /// <summary>
    /// Сохраняет несколько сообщений одной транзакцией
    /// </summary>
    Task SaveMessagesAsync(IEnumerable<Message> messages);
    
    /// <summary>
    /// Получает историю сообщений для конкретного чата с пагинацией
    /// </summary>
    /// <param name="chatId">ID чата</param>
    /// <param name="limit">Максимальное количество сообщений для загрузки</param>
    /// <param name="before">Загрузить сообщения старше указанной даты (для пагинации)</param>
    /// <returns>Список сообщений, отсортированных от новых к старым</returns>
    Task<List<Message>> GetMessagesForChatAsync(string chatId, int limit = 50, DateTime? before = null);
    
    /// <summary>
    /// Обновляет статус синхронизации сообщения после подтверждения от сервера
    /// </summary>
    Task UpdateMessageSyncStatusAsync(string messageId, bool isSynced);
    
    /// <summary>
    /// Получает все неотправленные сообщения для доотправки после восстановления соединения
    /// </summary>
    Task<List<Message>> GetUnsyncedMessagesAsync();
    
    /// <summary>
    /// Получает сообщение по ID
    /// </summary>
    Task<Message?> GetMessageByIdAsync(string messageId);
    
    /// <summary>
    /// Помечает сообщения как прочитанные
    /// </summary>
    Task MarkMessagesAsReadAsync(string chatId);
    
    /// <summary>
    /// Удаляет сообщение
    /// </summary>
    Task DeleteMessageAsync(string messageId);
    
    #endregion
    
    #region Chats
    
    /// <summary>
    /// Сохраняет или обновляет чат
    /// </summary>
    Task SaveChatAsync(Chat chat);
    
    /// <summary>
    /// Получает список всех чатов, отсортированных по времени последнего сообщения
    /// </summary>
    Task<List<Chat>> GetChatsAsync();
    
    /// <summary>
    /// Получает чат по ID
    /// </summary>
    Task<Chat?> GetChatByIdAsync(string chatId);
    
    /// <summary>
    /// Удаляет чат и все его сообщения
    /// </summary>
    Task DeleteChatAsync(string chatId);
    
    /// <summary>
    /// Обновляет информацию о последнем сообщении в чате
    /// </summary>
    Task UpdateChatLastMessageAsync(string chatId, string lastMessageText, DateTime timestamp);
    
    #endregion
    
    #region Database Management
    
    /// <summary>
    /// Очищает всю базу данных
    /// </summary>
    Task ClearAllDataAsync();
    
    #endregion
}
