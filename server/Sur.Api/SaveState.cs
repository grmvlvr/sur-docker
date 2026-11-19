using System.Text.Json;

namespace Sur.Api;

static partial class Fn
{
    public static Result SaveState(Domain domain)
    {
        try
        {
            lock (domain.Gate)
            {
                File.WriteAllText(domain.StorePath, JsonSerializer.Serialize(domain.State, JsonOptions()));
            }
            return Ok();
        }
        catch (IOException)
        {
            return Fail("Не удалось записать состояние на диск.");
        }
        catch (UnauthorizedAccessException)
        {
            return Fail("Нет доступа к файлу состояния.");
        }
    }
}
