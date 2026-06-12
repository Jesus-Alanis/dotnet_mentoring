namespace Domain.Enums;

public enum ConsistencyLevel : byte
{
    Strong,
    Eventual,
    ReadAfterWrite
}