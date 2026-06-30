using System.Linq.Expressions;
using System.Reflection;

namespace Serialization.RuntimeSerialization;

internal class MemberAccessor
{
    public required Type Type { get; init; }
    public required Func<object, object?> Getter { get; init; }
    public required Action<object, object?> Setter { get; init; }
    
    public static MemberAccessor Create(PropertyInfo property)
        => new() 
        {
            Type = property.PropertyType,
            Getter = BuildGetDelegate<object?>(property.DeclaringType!, property.Name),
            Setter = BuildSetDelegate(property.DeclaringType!, property.Name)
        };

    public static MemberAccessor Create(FieldInfo field)
        => new()
        {
            Type = field.FieldType,
            Getter = BuildGetDelegate<object?>(field.DeclaringType!, field.Name),
            Setter = BuildSetDelegate(field.DeclaringType!, field.Name)
        };

    public static Func<object, T> BuildGetDelegate<T>(Type declaredType, string memberName)
    {
        var objParam = Expression.Parameter(typeof(object), "obj");
        
        var owner = BuildOwner(objParam, declaredType);
        var member = Expression.PropertyOrField(owner, memberName);
        
        Expression result = typeof(T) == typeof(object) || member.Type.IsValueType
            ? Expression.Convert(member, typeof(T))
            : member;
        
        return Expression.Lambda<Func<object, T>>(result, objParam).Compile();
    }

    public static Action<object, object?> BuildSetDelegate(Type declaredType, string memberName)
    {
        var objParam = Expression.Parameter(typeof(object), "obj");
        var valueParam = Expression.Parameter(typeof(object), "value");
        
        var owner = BuildOwner(objParam, declaredType);
        var member = Expression.PropertyOrField(owner, memberName);
        
        var assign = Expression.Assign(member, Expression.Convert(valueParam, member.Type));
        
        return Expression.Lambda<Action<object, object?>>(assign, objParam, valueParam).Compile();
    }

    private static UnaryExpression BuildOwner(ParameterExpression objParam, Type declaredType)
        => declaredType.IsValueType
            ? Expression.Unbox(objParam, declaredType)
            : Expression.Convert(objParam, declaredType);
}