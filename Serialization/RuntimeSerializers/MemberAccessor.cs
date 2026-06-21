using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Serialization.RuntimeSerializers;

internal class MemberAccessor
{
    public required Type Type { get; init; }
    public required Func<object, object?> Getter { get; init; }
    public required Action<object, object?> Setter { get; init; }

    public static MemberAccessor Create(PropertyInfo property)
    {
        var (getter, setter) = BuildAccessor(property.DeclaringType!, obj => Expression.Property(obj, property));

        return new MemberAccessor
        {
            Type = property.PropertyType,
            Getter = getter,
            Setter = setter
        };
    }

    public static MemberAccessor Create(FieldInfo field)
    {
        var (getter, setter) = BuildAccessor(field.DeclaringType!, obj => Expression.Field(obj, field));

        return new MemberAccessor
        {
            Type = field.FieldType,
            Getter = getter,
            Setter = setter
        };
    }

    private static (Func<object, object?>, Action<object, object?>) BuildAccessor(
        Type declaringType, Func<Expression, MemberExpression> memberSelector)
    {
        var objParam = Expression.Parameter(typeof(object), "obj");
        var valueParam = Expression.Parameter(typeof(object), "value");
        
        var objectRealType = declaringType.IsValueType 
            ? Expression.Unbox(objParam, declaringType) 
            : Expression.Convert(objParam, declaringType);
        
        var memberExp = memberSelector(objectRealType);

        var getter = Expression.Lambda<Func<object, object?>>(
            Expression.Convert(memberExp, typeof(object)), objParam).Compile();

        var setter = Expression.Lambda<Action<object, object?>>(
            Expression.Assign(memberExp, Expression.Convert(valueParam, memberExp.Type)),
            objParam, valueParam).Compile();

        return (getter, setter);
    }
}