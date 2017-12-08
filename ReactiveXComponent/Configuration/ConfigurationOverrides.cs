﻿using ReactiveXComponent.Common;

namespace ReactiveXComponent.Configuration
{
    public class ConfigurationOverrides
    {
        public string Host { get; set; }

        public string Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public WebSocketType? WebSocketType { get; set; }
    }
}
