using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using AlaSaree3.Data;
using AlaSaree3.DTOs.Ai;
using AlaSaree3.Models;
using AlaSaree3.Services.Interfaces;
using AlaSaree3.ViewModels.Chatbot;

namespace AlaSaree3.Services.Implementations
{
    public class ChatbotService : IChatbotService
    {
        private const int MaxHistoryMessages = 12;

        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ChatbotService> _logger;

        public ChatbotService(ApplicationDbContext context, IHttpClientFactory httpClientFactory, ILogger<ChatbotService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<ChatSendResponse> SendMessageAsync(ChatSendRequest request, string? customerId, bool isAuthenticated)
        {
            var conversation = await GetOrCreateConversationAsync(request.SessionKey, customerId, request.ContextProductId);

            // Persist the user's message first so it's never lost even if the AI call fails.
            var userMessage = new ChatMessage
            {
                ConversationId = conversation.Id,
                Role = ChatMessageRole.User,
                Content = request.Message.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _context.ChatMessages.Add(userMessage);
            conversation.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var history = await _context.ChatMessages
                .Where(m => m.ConversationId == conversation.Id)
                .OrderByDescending(m => m.CreatedAt)
                .Take(MaxHistoryMessages)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new AiChatHistoryItem
                {
                    Role = m.Role == ChatMessageRole.User ? "user" : "assistant",
                    Content = m.Content
                })
                .ToListAsync();

            var aiRequest = new AiChatRequest
            {
                SessionId = request.SessionKey,
                CustomerId = customerId,
                IsAuthenticated = isAuthenticated,
                Message = request.Message.Trim(),
                ContextProductId = request.ContextProductId ?? conversation.ContextProductId,
                History = history
            };

            AiChatResponse? aiResponse;
            try
            {
                var client = _httpClientFactory.CreateClient("AiService");
                var httpResponse = await client.PostAsJsonAsync("/chat", aiRequest);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AI service returned {StatusCode} for session {SessionKey}", httpResponse.StatusCode, request.SessionKey);
                    return BuildUnavailableResponse();
                }

                aiResponse = await httpResponse.Content.ReadFromJsonAsync<AiChatResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reach AI assistant service for session {SessionKey}", request.SessionKey);
                return BuildUnavailableResponse();
            }

            if (aiResponse == null || string.IsNullOrWhiteSpace(aiResponse.Reply))
            {
                return BuildUnavailableResponse();
            }

            var assistantMessage = new ChatMessage
            {
                ConversationId = conversation.Id,
                Role = ChatMessageRole.Assistant,
                Content = aiResponse.Reply,
                CreatedAt = DateTime.UtcNow
            };
            _context.ChatMessages.Add(assistantMessage);
            await _context.SaveChangesAsync();

            return new ChatSendResponse
            {
                Success = true,
                Reply = aiResponse.Reply,
                Actions = aiResponse.Actions.Select(a => new ChatActionResultViewModel
                {
                    Type = a.Type,
                    Success = a.Success,
                    Description = a.Description
                }).ToList()
            };
        }

        public async Task<List<ChatHistoryItemViewModel>> GetHistoryAsync(string sessionKey, string? customerId)
        {
            var conversation = await FindConversationAsync(sessionKey, customerId);
            if (conversation == null)
            {
                return new List<ChatHistoryItemViewModel>();
            }

            return await _context.ChatMessages
                .Where(m => m.ConversationId == conversation.Id)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatHistoryItemViewModel
                {
                    Role = m.Role == ChatMessageRole.User ? "user" : "assistant",
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();
        }

        private async Task<ChatConversation?> FindConversationAsync(string sessionKey, string? customerId)
        {
            if (!string.IsNullOrEmpty(customerId))
            {
                var byCustomer = await _context.ChatConversations
                    .Where(c => c.CustomerId == customerId && c.SessionKey == sessionKey)
                    .OrderByDescending(c => c.LastActivityAt)
                    .FirstOrDefaultAsync();
                if (byCustomer != null) return byCustomer;
            }

            return await _context.ChatConversations
                .Where(c => c.SessionKey == sessionKey)
                .OrderByDescending(c => c.LastActivityAt)
                .FirstOrDefaultAsync();
        }

        private async Task<ChatConversation> GetOrCreateConversationAsync(string sessionKey, string? customerId, int? contextProductId)
        {
            var existing = await FindConversationAsync(sessionKey, customerId);
            if (existing != null)
            {
                if (!string.IsNullOrEmpty(customerId) && existing.CustomerId == null)
                {
                    // Guest conversation later logged in - attach it to the account.
                    existing.CustomerId = customerId;
                }

                if (contextProductId.HasValue)
                {
                    existing.ContextProductId = contextProductId;
                }

                return existing;
            }

            var conversation = new ChatConversation
            {
                SessionKey = sessionKey,
                CustomerId = customerId,
                ContextProductId = contextProductId,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow
            };

            _context.ChatConversations.Add(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        private static ChatSendResponse BuildUnavailableResponse()
        {
            return new ChatSendResponse
            {
                Success = false,
                Reply = "Sorry, the shopping assistant is temporarily unavailable. Please try again in a moment, " +
                        "or browse products directly while you wait.",
                ErrorMessage = "ai_service_unavailable"
            };
        }
    }
}
