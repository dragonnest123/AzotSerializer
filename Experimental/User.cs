using MemoryPack;

namespace Experimental;

[MemoryPackable]
public partial class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public KeyValuePair<int, string> A { get; set; }
    public string[][] Array { get; set; }

    public User(int id, string name, string email)
    {
        
    }

    public void Login()
    {
        
    }

    public void Register(string username, string password)
    {
        
    }
}