namespace Inventory.Application.Models;

public class ResponseBaseModel<T>
{
    public bool Succeess { get; set; }
    public string[] Errors { get; set; } = new string[0];
    public T Result { get; set; } = default!;

    public static ResponseBaseModel<T> Succeeded(T result)
    {
        return new()
        {
            Succeess = true,
            Result = result
        };
    }

    public static ResponseBaseModel<T> Failure(string[] errors)
    {
        return new()
        {
            Succeess = false,
            Errors = errors
        };
    }
}