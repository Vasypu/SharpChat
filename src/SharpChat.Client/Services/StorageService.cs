using Messenger.BlazorClient.Models.StorageModels;
using Messenger.BlazorClient.Storage;
using Tavenem.Blazor.IndexedDB;

namespace Messenger.BlazorClient.Services;

/// <summary>
/// Сервис для работы с локальным хранилищем IndexedDB
/// Использует Tavenem.Blazor.IndexedDB для эффективного хранения и извлечения данных
/// </summary>
public class StorageService : IStorageService
{
    private readonly IndexedDb _db;
    private readonly ILogger<StorageService> _logger;

    public StorageService(
        [FromKeyedServices(IndexedDbConfig.DatabaseName)] IndexedDb db,
        ILogger<StorageService> logger)
    {
        _db = db;
        _logger = logger;
    }

    #region Messages

    public async Task SaveMessageAsync(Message message)
    {
        try
        {
            var store = _db[IndexedDbConfig.MessagesStore];
            await store.StoreItemAsync(message);
            _logger.LogDebug("Сообщение {MessageId} сохранено в IndexedDB", message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении сообщения {MessageId}", message.Id);
            throw;
        }
    }

    public async Task SaveMessagesAsync(IEnumerable<Message> messages)
    {
        try
        {
            var store = _db[IndexedDbConfig.MessagesStore];
            foreach (var message in messages)
            {
                await store.StoreItemAsync(message);
            }
            var count = messages.Count();
            _logger.LogDebug("Сохранено {Count} сообщений в IndexedDB", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при пакетном сохранении сообщений");
            throw;
        }
    }

    public async Task<List<Message>> GetMessagesForChatAsync(string chatId, int limit = 50, DateTime? before = null)
    {
        try
        {
            var store = _db[IndexedDbConfig.MessagesStore];

            // Получаем все сообщения чата
            var allMessagesList = new List<Message>();
            await foreach (var msg in store.GetAllAsync<Message>())
            {
                allMessagesList.Add(msg);
            }

            // Фильтруем по chatId
            var query = allMessagesList.Where(m => m.ChatId == chatId);

            // Если указана дата "before", фильтруем по времени
            if (before.HasValue)
            {
                var beforeTicks = before.Value.Ticks;
                query = query.Where(m => m.TimestampUtcTicks < beforeTicks);
            }

            // Сортируем по убыванию времени и берем нужное количество
            var messages = query
                .OrderByDescending(m => m.TimestampUtcTicks)
                .Take(limit)
                .ToList();

            _logger.LogDebug("Загружено {Count} сообщений для чата {ChatId}", messages.Count, chatId);
            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке сообщений для чата {ChatId}", chatId);
            return new List<Message>();
        }
    }

    public async Task UpdateMessageSyncStatusAsync(string messageId, bool isSynced)
    {
        try
        {
            var store = _db[IndexedDbConfig.MessagesStore];
            var message = await store.GetItemAsync<Message>(messageId);
            
            if (message != null)
            {
                message.IsSynced = isSynced;
                await store.StoreItemAsync(message);
                _logger.LogDebug("Статус синхронизации сообщения {MessageId} обновлен: {IsSynced}", messageId, isSynced);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении статуса синхронизации сообщения {MessageId}", messageId);
            throw;
        }
    }

    public async Task<List<Message>> GetUnsyncedMessagesAsync()
    {
        try
        {
            var store = _db[IndexedDbConfig.MessagesStore];
            
            // Получаем все сообщения
            var allMessagesList = new List<Message>();
            await foreach (var msg in store.GetAllAsync<Message>())
            {
                allMessagesList.Add(msg);
            }
            
            // Фильтруем неотправленные
            var unsyncedMessages = allMessagesList
                .Where(m => !m.IsSynced)
                .OrderBy(m => m.TimestampUtcTicks)
                .ToList();

            _logger.LogDebug("Найдено {Count} несинхронизированных сообщений", unsyncedMessages.Count);
            return unsyncedMessages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении несинхронизированных сообщений");
            return new List<Message>();
        }
    }

    public async Task<Message?> GetMessageByIdAsync(string messageId)
    {
        try
        {
            var store = _db[IndexedDbConfig.MessagesStore];
            return await store.GetItemAsync<Message>(messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении сообщения {MessageId}", messageId);
            return null;
        }
    }

    public async Task MarkMessagesAsReadAsync(string chatId)
    {
        try
        {
            var store = _db[IndexedDbConfig.MessagesStore];

            // Получаем все сообщения
            var allMessagesList = new List<Message>();
            await foreach (var msg in store.GetAllAsync<Message>())
            {
                allMessagesList.Add(msg);
            }

            // Фильтруем непрочитанные сообщения этого чата
            var unreadMessages = allMessagesList
                .Where(m => m.ChatId == chatId && !m.IsRead)
                .ToList();

            // Помечаем как прочитанные
            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                await store.StoreItemAsync(message);
            }

            _logger.LogDebug("Сообщения чата {ChatId} помечены как прочитанные", chatId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при пометке сообщений как прочитанных для чата {ChatId}", chatId);
            throw;
        }
    }

    public async Task DeleteMessageAsync(string messageId)
    {
        try
        {
            var store = _db[IndexedDbConfig.MessagesStore];
            await store.RemoveItemAsync(messageId);
            _logger.LogDebug("Сообщение {MessageId} удалено", messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении сообщения {MessageId}", messageId);
            throw;
        }
    }

    #endregion

    #region Chats

    public async Task SaveChatAsync(Chat chat)
    {
        try
        {
            var store = _db[IndexedDbConfig.ChatsStore];
            await store.StoreItemAsync(chat);
            _logger.LogDebug("Чат {ChatId} сохранен", chat.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении чата {ChatId}", chat.Id);
            throw;
        }
    }

    public async Task<List<Chat>> GetChatsAsync()
    {
        try
        {
            var store = _db[IndexedDbConfig.ChatsStore];
            
            // Получаем все чаты
            var allChatsList = new List<Chat>();
            await foreach (var chat in store.GetAllAsync<Chat>())
            {
                allChatsList.Add(chat);
            }
            
            // Сортируем по времени последнего сообщения
            var chats = allChatsList
                .OrderByDescending(c => c.LastMessageTimestampTicks)
                .ToList();

            _logger.LogDebug("Загружено {Count} чатов", chats.Count);
            return chats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке списка чатов");
            return new List<Chat>();
        }
    }

    public async Task<Chat?> GetChatByIdAsync(string chatId)
    {
        try
        {
            var store = _db[IndexedDbConfig.ChatsStore];
            return await store.GetItemAsync<Chat>(chatId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении чата {ChatId}", chatId);
            return null;
        }
    }

    public async Task DeleteChatAsync(string chatId)
    {
        try
        {
            var messageStore = _db[IndexedDbConfig.MessagesStore];
            
            // Получаем все сообщения
            var allMessagesList = new List<Message>();
            await foreach (var msg in messageStore.GetAllAsync<Message>())
            {
                allMessagesList.Add(msg);
            }
            
            // Удаляем все сообщения чата
            var messages = allMessagesList.Where(m => m.ChatId == chatId).ToList();
            
            foreach (var message in messages)
            {
                await messageStore.RemoveItemAsync(message.Id);
            }

            // Удаляем сам чат
            var chatStore = _db[IndexedDbConfig.ChatsStore];
            await chatStore.RemoveItemAsync(chatId);
            
            _logger.LogDebug("Чат {ChatId} и его сообщения удалены", chatId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении чата {ChatId}", chatId);
            throw;
        }
    }

    public async Task UpdateChatLastMessageAsync(string chatId, string lastMessageText, DateTime timestamp)
    {
        try
        {
            var store = _db[IndexedDbConfig.ChatsStore];
            var chat = await store.GetItemAsync<Chat>(chatId);

            if (chat != null) {
                chat.LastMessageText = lastMessageText;
                chat.LastMessageTimestamp = timestamp;
                await store.StoreItemAsync(chat);

                _logger.LogDebug("Последнее сообщение чата {ChatId} обновлено", chatId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении последнего сообщения чата {ChatId}", chatId);
            throw;
        }
    }

    #endregion

    #region Database Management

    public async Task ClearAllDataAsync()
    {
        try
        {
            var messageStore = _db[IndexedDbConfig.MessagesStore];
            var chatStore = _db[IndexedDbConfig.ChatsStore];
            
            await messageStore.ClearAsync();
            await chatStore.ClearAsync();
            
            _logger.LogInformation("Все данные IndexedDB очищены");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при очистке базы данных");
            throw;
        }
    }

    #endregion
}
