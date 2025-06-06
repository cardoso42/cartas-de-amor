namespace CartasDeAmor.Application.Interfaces;

public interface IAccountService
{
    bool CreateAccount(string username, string password);
}