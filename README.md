# AzotSerializer

AzotSerializer — проект для бинарной сериализации в .NET. В репозитории есть:

- библиотека runtime-сериализации (`Serialization`) для объектов и структур;
- source generator (`CodeGeneration`), который генерирует строго типизированные сериализаторы для помеченных атрибутом моделей;

## Структура проекта

```text
AzotSerializer.slnx
CodeGeneration/   Roslyn source generator для типов с [ByteSerializable]
Serialization/    Runtime-примитивы, атрибуты и сериализаторы
Test/             xUnit-тесты и примеры использования
```

## Требования

- .NET SDK с поддержкой `net10.0` для runtime-библиотеки и тестов.
- Source generator таргетирует `netstandard2.0` и использует пакеты Roslyn из `Microsoft.CodeAnalysis`.

## Сериализация через source generator

Пометьте `partial`-класс, структуру, record или record struct атрибутом `[ByteSerializable]`. Генератор добавит в partial-тип instance-методы `Serialize` и статический метод `Deserialize`.

```csharp
using Serialization;

[ByteSerializable]
public partial class Person
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
```

Используйте сгенерированный API:

```csharp
var person = new Person { Id = 1, Name = "Ada" };

ReadOnlySpan<byte> bytes = person.Serialize();
var restored = Person.Deserialize(ref bytes);
```

### Исключение членов из сериализации

Используйте `[ByteIgnore]` для полей или свойств, которые не должны попадать в сгенерированный бинарный payload:

```csharp
[ByteSerializable]
public partial class Session
{
    public Guid Id { get; set; }

    [ByteIgnore]
    public string? TemporaryToken { get; set; }
}
```

### Требования генератора

- Сериализуемые типы должны быть объявлены как `partial`.
- Если сериализуемый тип является вложенным, его внешние типы тоже должны быть `partial`.
- Члены типа должны использовать нативно поддерживаемые типы или типы, также помеченные `[ByteSerializable]`.

## Runtime-сериализаторы

Пространство имён `Serialization.RuntimeSerialization.Serializers` предоставляет runtime-сериализаторы для случаев, когда сгенерированные методы не используются напрямую.

```csharp
using Serialization.RuntimeSerialization.Serializers;

var value = new Person { Id = 1, Name = "Ada" };
ReadOnlySpan<byte> bytes = ObjectSerializer.Serialize(value);

var buffer = bytes.ToArray();
var restored = ObjectSerializer.Deserialize<Person>(buffer);
```

Для структур используйте `StructSerializer`:

```csharp
using Serialization.RuntimeSerialization.Serializers;

var point = new Point { X = 10, Y = 20 };
ReadOnlySpan<byte> bytes = StructSerializer.Serialize(ref point);

var restored = StructSerializer.Deserialize<Point>(bytes.ToArray());
```

## Поддерживаемые сценарии

Сериализатор поддерживает сериализацию следующих сценариев:

- примитивные числовые типы, `bool`, `char`, `string`, `decimal`, `DateTime` и `TimeSpan`;
- nullable-значения и nullable-члены ссылочных типов;
- enum
- вложенные сериализуемые классы, структуры, records и record structs;
- массивы, влложенные массивы, списки, hashset, словари и вложенные коллекции;
- value tuples;
- unmanaged- и managed-структуры
- неизвестные во время компиляции типы через runtime сериализатор
