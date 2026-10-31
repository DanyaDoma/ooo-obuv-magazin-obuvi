namespace ShoeStore.Core.Models;

public sealed record UserInfo(
    long Id,
    string FullName,
    string Login,
    Role Role,
    Department? Department);
