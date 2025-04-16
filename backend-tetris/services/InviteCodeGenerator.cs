namespace backend_tetris.services;

public static class InviteCodeGenerator
{
    private static readonly Random Random = new();
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string GenerateInviteCode(int length = 5)
    {
        var inviteCode = new char[length];
        for (var i = 0; i < length; i++)
        {
            inviteCode[i] = Characters[Random.Next(Characters.Length)];
        }
        return new string(inviteCode);
    }
}