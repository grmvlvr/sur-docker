namespace Sur.Api;

static partial class Fn
{
    public static Result SetMode(Domain domain, string id, ModeInput input)
    {
        var risk = FindRisk(domain, id);
        if (risk?.Status != "Активен" || risk.Assessment is not { IsActive: true })
            return Fail("Режим выбирается у активного оценённого риска.");
        if (risk.Assessment.ResponseMode != null) return Fail("Режим на этот цикл уже выбран.");
        if (!IsPrimaryOwner(domain, risk)) return Fail("Режим выбирает владелец основного процесса.");
        switch (input.Mode)
        {
            case "accept": return AcceptRisk(domain, id);
            case "treat":
                if (!HasRight(domain, "edit_risk")) return Fail("Для режима «Обработать» нужно право редактирования.");
                risk.Assessment.ResponseMode = "treat";
                AppendLog(domain, id, "Выбран режим «Обработать»");
                var treated = SaveState(domain);
                return treated.Ok ? Ok() : treated;
            case "transfer":
                if (!HasRight(domain, "transfer_risk")) return Fail("Недостаточно прав для передачи риска.");
                var process = FindProcess(domain, input.ProcessId ?? "");
                if (process is not { IsActive: true }) return Fail("Выберите процесс для передачи.");
                if (TreatmentsOf(domain, id).Any(item => item.Status is not ("Отменено" or "Готово")))
                    return Fail("Передача запрещена, пока есть незавершённые меры, включая черновики.");
                risk.Assessment.ResponseMode = "transfer";
                risk.Assessment.IsActive = false;
                risk.History.Insert(0, risk.Assessment);
                risk.Assessment = null;
                risk.AcceptanceLevel = null;
                foreach (var link in risk.Processes.Where(item => string.IsNullOrEmpty(item.UnlinkedAt)))
                {
                    link.IsPrimary = false;
                    link.UnlinkedAt = Stamp();
                }
                risk.Processes.Add(new RiskProcess { ProcessId = process.Id, IsPrimary = true, OwnerId = process.OwnerId, LinkedAt = Stamp() });
                risk.Status = "Ожидает переоценки";
                AppendLog(domain, id, $"Риск передан в процесс «{process.Name}» и ожидает переоценки");
                var transferred = SaveState(domain);
                return transferred.Ok ? Ok() : transferred;
            case "split":
                if (!HasRight(domain, "split_risk")) return Fail("Недостаточно прав для разделения риска.");
                if (input.ProcessIds.Distinct().Count() < 2) return Fail("Для разделения выберите не меньше двух процессов.");
                if (input.PrimaryId == null || !input.ProcessIds.Contains(input.PrimaryId))
                    return Fail("Укажите основной процесс среди выбранных.");
                if (input.ProcessIds.Any(pid => FindProcess(domain, pid) is not { IsActive: true }))
                    return Fail("Все процессы разделения должны быть активны.");
                var blocked = TreatmentsOf(domain, id).Where(item => item.Status is not ("Готово" or "Отменено")).Select(item => item.ProcessId).ToHashSet();
                if (risk.Processes.Where(item => string.IsNullOrEmpty(item.UnlinkedAt)).Any(item => blocked.Contains(item.ProcessId) && !input.ProcessIds.Contains(item.ProcessId)))
                    return Fail("Нельзя исключить процесс, по которому есть незавершённая мера.");
                foreach (var link in risk.Processes.Where(item => string.IsNullOrEmpty(item.UnlinkedAt) && !input.ProcessIds.Contains(item.ProcessId)))
                {
                    link.IsPrimary = false;
                    link.UnlinkedAt = Stamp();
                }
                foreach (var pid in input.ProcessIds.Distinct())
                {
                    var current = risk.Processes.FirstOrDefault(item => item.ProcessId == pid && string.IsNullOrEmpty(item.UnlinkedAt));
                    if (current == null)
                        risk.Processes.Add(new RiskProcess { ProcessId = pid, IsPrimary = pid == input.PrimaryId, OwnerId = FindProcess(domain, pid)!.OwnerId, LinkedAt = Stamp() });
                    else
                        current.IsPrimary = pid == input.PrimaryId;
                }
                risk.Assessment.ResponseMode = "split";
                AppendLog(domain, id, "Риск разделён по процессам");
                var split = SaveState(domain);
                return split.Ok ? Ok() : split;
            default:
                return Fail("Неизвестный режим.");
        }
    }
}
