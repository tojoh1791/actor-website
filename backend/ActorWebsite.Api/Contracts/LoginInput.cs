namespace ActorWebsite.Api.Contracts;

public sealed record LoginInput(string? Email, string? Password, bool RememberMe = false);
