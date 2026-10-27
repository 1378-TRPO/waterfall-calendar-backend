using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Calendar.Api.Data
{
    public static class LowercaseEnumConversion
    {
        /// <summary>Хранит enum строкой в нижнем регистре ("meeting", "accepted"), как в Design Doc.</summary>
        public static PropertyBuilder<TEnum> HasLowercaseConversion<TEnum>(this PropertyBuilder<TEnum> builder)
            where TEnum : struct, Enum =>
            builder.HasConversion(new ValueConverter<TEnum, string>(
                value => value.ToString().ToLowerInvariant(),
                text => Enum.Parse<TEnum>(text, true)));
    }
}
