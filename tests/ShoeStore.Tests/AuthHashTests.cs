namespace ShoeStore.Tests;

public class AuthHashTests
{
    private const string AdminHash =
        "$2a$12$U0VTXizv6Gq15YX4ydz1a.lribmPkcAA7OIdOGLbeXR5JRGf/TkF.";

    [Fact]
    public void AdminPassword_MatchesStoredBcryptHash()
    {
        Assert.True(BCrypt.Net.BCrypt.Verify("uzWC67", AdminHash));
    }

    [Fact]
    public void WrongPassword_DoesNotMatchStoredBcryptHash()
    {
        Assert.False(BCrypt.Net.BCrypt.Verify("wrong-password", AdminHash));
    }
}
