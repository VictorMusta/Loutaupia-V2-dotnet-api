using System;
using System.Text.RegularExpressions;
using Loutaupia_V2_dotnet_api.Core.Domain.Exceptions;
namespace Loutaupia_V2_dotnet_api.Core.Domain.Entities;
public class Player
{
    private string _username = string.Empty;
    private string _email = string.Empty;
    public Guid PlayerId { get; private set; }
    public string Username
    {
        get => _username;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Username cannot be empty");
            if (value.Length < 3 || value.Length > 20)
                throw new DomainException("Username must be between 3 and 20 characters");
            if (!Regex.IsMatch(value, "^[a-zA-Z0-9_]+$"))
                throw new DomainException("Username can only contain alphanumeric characters and underscores");
            _username = value;
        }
    }
    public string Email
    {
        get => _email;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Email cannot be empty");
            if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new DomainException("Invalid email format");
            _email = value.ToLowerInvariant();
        }
    }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public Inventory? Inventory { get; set; }
    public CurrencyWallet? Wallet { get; set; }
    private Player()
    {
    }
    public Player(string username, string email, string passwordHash)
    {
        PlayerId = Guid.NewGuid();
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}
