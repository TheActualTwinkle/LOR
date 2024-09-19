using System.ComponentModel;

namespace TelegramBotApp.Application.Factories.Common;

// ReSharper disable once ClassNeverInstantiated.Global
public class TelegramCommandMetadata
{
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string Command { get; set; } = string.Empty;
    
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string Description { get; set; } = string.Empty;
    
    // ReSharper disable once UnusedMember.Global
    [DefaultValue(null)]
    public string? ButtonDescriptionText { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class TelegramCallbackQueryMetadata
{
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string Query { get; set; } = string.Empty;
}