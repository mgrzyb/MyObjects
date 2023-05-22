using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyObjects.Infrastructure;

public class SystemTextReferenceConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Reference<>);
    }

    public override System.Text.Json.Serialization.JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var entityType = typeToConvert.GetGenericArguments()[0];
        return (System.Text.Json.Serialization.JsonConverter) Activator.CreateInstance(typeof(ReferenceConverter<>).MakeGenericType(entityType));
    }
}

public class ReferenceConverter<T> : System.Text.Json.Serialization.JsonConverter<Reference<T>> where T : Entity
{
    public override Reference<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TryGetInt32(out var id))
            return new Reference<T>(id);
        throw new InvalidOperationException("Failed to read reference from json reader");
    }

    public override void Write(Utf8JsonWriter writer, Reference<T> value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Id);
    }
}