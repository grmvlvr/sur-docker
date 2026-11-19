namespace Sur.Api;

static partial class Fn
{
    public static Result SaveProcess(Domain domain, string? id, ProcessInput input)
    {
        if (!IsAdmin(domain)) return Fail("Оргструктуру меняет администратор.");
        if (string.IsNullOrWhiteSpace(input.Name)) return Fail("Укажите название процесса.");
        var owner = domain.State.Users.FirstOrDefault(user => user.Id == input.OwnerId && user.IsActive);
        if (owner == null) return Fail("Укажите активного владельца процесса.");
        if (string.IsNullOrEmpty(id))
        {
            domain.State.Processes.Add(new ProcessItem
            {
                Id = "p" + ++domain.State.Seq,
                Name = input.Name.Trim(),
                DirectorateId = input.DirectorateId,
                OwnerId = input.OwnerId,
                IsActive = true
            });
        }
        else
        {
            var process = domain.State.Processes.FirstOrDefault(item => item.Id == id);
            if (process == null) return Fail("Процесс не найден.");
            var ownerChanged = process.OwnerId != input.OwnerId;
            process.Name = input.Name.Trim();
            process.DirectorateId = input.DirectorateId;
            process.OwnerId = input.OwnerId;
            process.IsActive = input.IsActive;
            if (ownerChanged)
            {
                foreach (var risk in domain.State.Risks)
                {
                    var touched = false;
                    foreach (var link in risk.Processes.Where(item => item.ProcessId == id && string.IsNullOrEmpty(item.UnlinkedAt)))
                    {
                        link.OwnerId = input.OwnerId;
                        touched = true;
                    }
                    if (touched) SystemComment(domain, risk.Id, $"Владелец процесса «{process.Name}» изменён на {owner.Name}.");
                }
            }
        }
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
