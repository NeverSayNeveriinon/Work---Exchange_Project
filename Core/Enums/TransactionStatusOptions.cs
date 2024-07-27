using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Core.Enums;

// [JsonConverter(typeof(StringEnumConverter))]
public enum TransactionStatusOptions
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Failed = 3
}