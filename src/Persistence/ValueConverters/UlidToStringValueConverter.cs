using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.ValueConverters;

public sealed class UlidToStringValueConverter() : ValueConverter<Ulid, string>(
    to => to.ToString(),
    from => Ulid.Parse(from));