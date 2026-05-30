using Serialization;

namespace Experimental;


public class User
{
    [ByteSerializable]
    public partial class UserData
    {
        
    }
    
    public Guid Id { get; set; }
    public UserData Data { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    public void Login()
    {
        
    }

    public void Register(string username, string password)
    {
        
    }
}