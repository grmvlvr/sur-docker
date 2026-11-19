namespace Sur.Api;

static partial class Fn
{
    public static Result ValidExecutor(Domain domain, string executorId, string directorateId)
    {
        var executor = domain.State.Users.FirstOrDefault(user => user.Id == executorId);
        if (executor is not { IsActive: true } || executor.DirectorateId != directorateId)
            return Fail("Исполнитель должен быть активным сотрудником дирекции процесса.");
        if (!executor.Roles.Contains("OWNER") && !executor.Roles.Contains("EMPLOYEE"))
            return Fail("Исполнителем может быть владелец или линейный сотрудник.");
        return Ok();
    }
}
