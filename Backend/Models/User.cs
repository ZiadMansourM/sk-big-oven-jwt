using System;
using FluentValidation;

namespace Backend.Models;


public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenCreated { get; set; }
    public DateTime TokenExpires { get; set; }

    public User() { }

    public User(Guid id, string username, byte[] passwordHash, byte[] passwordSalt)
    {
        Id = id;
        Username = username;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
    }

    public void Register(string username, string password)
    {
        CreatePasswordHashSalt(password, out byte[] hash, out byte[] salt);
        Id = Guid.NewGuid();
        Username = username;
        PasswordHash = hash;
        PasswordSalt = salt;
    }

    private void CreatePasswordHashSalt(string password, out byte[] hash, out byte[] salt)
    {
        using (var hmac = new System.Security.Cryptography.HMACSHA512())
        {
            salt = hmac.Key;
            hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }

    public void UpdatePassword(string password)
    {
        CreatePasswordHashSalt(password, out byte[] hash, out byte[] salt);
        PasswordHash = hash;
        PasswordSalt = salt;
    }

    public bool VerifyPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(PasswordHash);
        ArgumentNullException.ThrowIfNull(PasswordSalt);
        using (var hmac = new System.Security.Cryptography.HMACSHA512(PasswordSalt))
        {
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return hash.SequenceEqual(PasswordHash);
        }
    }
}

public class UserDTO
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public UserDTO(string username, string password)
    {
        Username = username;
        Password = password;
    }
}


public class UserValidator : AbstractValidator<UserDTO>
{
    public UserValidator()
    {
        RuleFor(user => user.Username).NotEmpty();
        RuleFor(user => user.Password).NotEmpty();
    }
}