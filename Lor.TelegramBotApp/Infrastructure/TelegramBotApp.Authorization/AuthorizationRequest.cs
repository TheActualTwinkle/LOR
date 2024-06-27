﻿using System.Text.Json;
using Newtonsoft.Json;
using TelegramBotApp.Authorization.Interfaces;

namespace TelegramBotApp.Authorization;

public readonly struct AuthorizationRequest(string fullName, DateTime? dateOfBirth = default)
{
    public string FullName { get; } = fullName;
    public DateTime? DateOfBirth { get; } = dateOfBirth;
}