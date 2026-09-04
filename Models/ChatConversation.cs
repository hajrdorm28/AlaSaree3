using System.ComponentModel.DataAnnotations;

namespace AlaSaree3.Models
{
    /// <summary>
    /// A single chat session between a user (authenticated or guest) and the AI shopping
    /// assistant. Guests are tracked by an anonymous SessionKey stored in a cookie.
    /// </summary>
    public class ChatConversation
    {
        public int Id { get; set; }

        /// <summary>
        /// Set when the chatting user is logged in.
        /// </summary>
        public string? CustomerId { get; set; }
        public virtual ApplicationUser? Customer { get; set; }

        /// <summary>
        /// Random opaque key used to correlate messages for guests (or as a stable per-tab
        /// session id for logged-in users too). Always set.
        /// </summary>
        [Required]
        [StringLength(64)]
        public string SessionKey { get; set; } = string.Empty;

        /// <summary>
        /// Product the user was viewing when they opened the chat, if any. Gives the assistant
        /// immediate context for questions like "Is this available in size M?".
        /// </summary>
        public int? ContextProductId { get; set; }
        public virtual Product? ContextProduct { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
