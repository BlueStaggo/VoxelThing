namespace PDS;

[AttributeUsage(
    AttributeTargets.Class
    | AttributeTargets.Struct
)]
public sealed class PdsAutoSerializableAttribute : Attribute
{
}