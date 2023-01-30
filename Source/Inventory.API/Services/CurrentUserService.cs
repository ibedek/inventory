﻿using System.Security.Claims;
using Inventory.Application.Interfaces;

namespace Inventory.API.Services;

public class CurrentUserService : ICurrentUser
{
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        Username = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? "API User";
    }
    
    public string Username { get; }
}