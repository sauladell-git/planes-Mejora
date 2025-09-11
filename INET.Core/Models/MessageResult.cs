using INET.Core.Enums;

namespace INET.Core.Models
{
    public class MessageResult
    {
        public string Text { get; set; }
        public MessageResultTypeEnum Type { get; set; }
        public bool FadeOut { get; set; }
    }
}
