namespace CartasDeAmor.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        bool UserExists(string username);
        void AddUser(string username, string hashedPassword);
    }
}
