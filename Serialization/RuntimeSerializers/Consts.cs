using System.Reflection;
using System.Runtime.CompilerServices;

namespace Serialization.RuntimeSerializers;

public static class Consts
{
    public const BindingFlags BindingFlag = BindingFlags.Public | BindingFlags.Instance;

    public delegate void MyDelegate(ref object a);
        
    public static void A(MyDelegate d, ref  object a)
    {
        d.Invoke(ref a);
    }
}