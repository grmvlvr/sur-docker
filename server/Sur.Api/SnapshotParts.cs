namespace Sur.Api;

static partial class Fn
{
    public static List<SnapshotPart> SnapshotParts(Domain domain, RiskItem risk, string snapshotId, string at, UserItem author)
    {
        var parts = new List<SnapshotPart>
        {
            new(SnapshotPartKind.Title, Value: "Audit Snapshot"),
            new(SnapshotPartKind.Subtitle, Value: "Снимок состояния риска для аудита"),
            new(SnapshotPartKind.Field, "snapshot_id", snapshotId),
            new(SnapshotPartKind.Field, "Сформирован", at),
            new(SnapshotPartKind.Field, "Автор", author.Name),
            new(SnapshotPartKind.Field, "Логин автора", author.Login),
            new(SnapshotPartKind.Blank),
            new(SnapshotPartKind.Section, Value: "Риск"),
            new(SnapshotPartKind.Field, "ID", risk.Id),
            new(SnapshotPartKind.Field, "Название", risk.Title),
            new(SnapshotPartKind.Field, "Статус", risk.Status),
            new(SnapshotPartKind.Field, "Описание", risk.Description),
            new(SnapshotPartKind.Field, "Опасность", FindHazard(domain, risk.HazardId)?.Name ?? "не сохранена"),
            new(SnapshotPartKind.Field, "Категория", risk.CategoryId == null ? "не указана" : domain.State.Categories.FirstOrDefault(item => item.Id == risk.CategoryId)?.Name ?? "не сохранена"),
            new(SnapshotPartKind.Field, "Уровень принятия", LevelLabel(risk.AcceptanceLevel)),
            new(SnapshotPartKind.Field, "Режим", ModeLabel(risk.Assessment?.ResponseMode))
        };

        parts.Add(new(SnapshotPartKind.Blank));
        parts.Add(new(SnapshotPartKind.Section, Value: "Связи с процессами"));
        if (risk.Processes.Count == 0) parts.Add(new(SnapshotPartKind.Note, Value: "нет сохранённых связей"));
        foreach (var link in risk.Processes)
        {
            var name = FindProcess(domain, link.ProcessId)?.Name ?? link.ProcessId;
            var from = string.IsNullOrEmpty(link.LinkedAt) ? "начало не сохранено" : link.LinkedAt;
            var to = string.IsNullOrEmpty(link.UnlinkedAt) ? "действует" : link.UnlinkedAt;
            parts.Add(new(SnapshotPartKind.Field, link.IsPrimary ? "Основной процесс" : "Процесс", name));
            parts.Add(new(SnapshotPartKind.Field, "Период", $"с {from} · до {to}"));
        }

        parts.Add(new(SnapshotPartKind.Blank));
        parts.Add(new(SnapshotPartKind.Section, Value: "Оценки"));
        if (risk.Assessment == null) parts.Add(new(SnapshotPartKind.Note, Value: "нет текущей оценки"));
        else AddAssessment(parts, "Текущая оценка", risk.Assessment, domain);
        if (risk.History.Count == 0) parts.Add(new(SnapshotPartKind.Note, Value: "исторических версий нет"));
        foreach (var version in risk.History) AddAssessment(parts, "Историческая версия", version, domain);
        if (risk.Assessment is { IsActive: true, Score: > 0 })
            parts.Add(new(SnapshotPartKind.Field, "Позиция на heat-map", $"вероятность {risk.Assessment.Likelihood} · тяжесть {risk.Assessment.Impact}"));
        else if (risk.Assessment is { Score: 0 })
            parts.Add(new(SnapshotPartKind.Note, Value: "балл 0, в матрицу не входит"));
        else
            parts.Add(new(SnapshotPartKind.Note, Value: "нет действующей оценки, в матрицу не входит"));

        parts.Add(new(SnapshotPartKind.Blank));
        parts.Add(new(SnapshotPartKind.Section, Value: "Меры"));
        var treatments = domain.State.Treatments.Where(item => item.RiskId == risk.Id).ToList();
        if (treatments.Count == 0) parts.Add(new(SnapshotPartKind.Note, Value: "мер нет"));
        foreach (var treatment in treatments)
        {
            var process = FindProcess(domain, treatment.ProcessId)?.Name ?? treatment.ProcessId;
            var executor = treatment.ExecutorId == null ? "не назначен" : domain.State.Users.FirstOrDefault(user => user.Id == treatment.ExecutorId)?.Name ?? "не сохранён";
            parts.Add(new(SnapshotPartKind.Field, "Мера", treatment.Title));
            parts.Add(new(SnapshotPartKind.Field, "Статус", treatment.Status));
            parts.Add(new(SnapshotPartKind.Field, "Процесс", process));
            parts.Add(new(SnapshotPartKind.Field, "План / факт", $"{treatment.CostPlanned} / {(treatment.CostActual?.ToString() ?? "не указан")}"));
            parts.Add(new(SnapshotPartKind.Field, "Срок", treatment.Due ?? "не указан"));
            parts.Add(new(SnapshotPartKind.Field, "Исполнитель", executor));
            var approval = domain.State.Approvals.LastOrDefault(item => item.TreatmentId == treatment.Id);
            if (approval == null)
            {
                parts.Add(new(SnapshotPartKind.Note, Value: "согласование не сохранено"));
            }
            else
            {
                var who = approval.DecidedById == null ? "" : domain.State.Users.FirstOrDefault(user => user.Id == approval.DecidedById)?.Name;
                parts.Add(new(SnapshotPartKind.Field, "Согласование", $"{LevelLabel(approval.Level)} · {approval.Status}"));
                parts.Add(new(SnapshotPartKind.Field, "Снимок маршрута", approval.TriggerCondition ?? "не сохранён"));
                parts.Add(new(SnapshotPartKind.Field, "Создан", approval.CreatedAt ?? "не сохранён"));
                parts.Add(new(SnapshotPartKind.Field, "Решение", $"{approval.DecidedAt ?? "нет"}{(string.IsNullOrEmpty(who) ? "" : " · " + who)}"));
                if (!string.IsNullOrWhiteSpace(approval.DecisionComment))
                    parts.Add(new(SnapshotPartKind.Field, "Комментарий решения", approval.DecisionComment!));
            }
            if (string.IsNullOrEmpty(treatment.Jira))
            {
                parts.Add(new(SnapshotPartKind.Field, "MOCK", string.IsNullOrEmpty(treatment.TaskError) ? "задача не создана" : treatment.TaskError!));
            }
            else
            {
                parts.Add(new(SnapshotPartKind.Field, "MOCK-ключ", treatment.Jira));
                parts.Add(new(SnapshotPartKind.Field, "MOCK-статус", treatment.TaskStatus ?? "не сохранён"));
                parts.Add(new(SnapshotPartKind.Field, "MOCK-создана", treatment.TaskCreatedAt ?? "не сохранено"));
                parts.Add(new(SnapshotPartKind.Field, "MOCK-ссылка", "не предоставлена"));
            }
            parts.Add(new(SnapshotPartKind.Blank));
        }

        parts.Add(new(SnapshotPartKind.Section, Value: "Комментарии"));
        var comments = domain.State.Comments.Where(item => item.RiskId == risk.Id && !item.IsDeleted).ToList();
        if (comments.Count == 0) parts.Add(new(SnapshotPartKind.Note, Value: "комментариев нет"));
        foreach (var comment in comments)
        {
            var who = comment.IsSystem ? "Система" : domain.State.Users.FirstOrDefault(user => user.Id == comment.AuthorId)?.Name ?? "автор не сохранён";
            parts.Add(new(SnapshotPartKind.Field, comment.At, $"{who}: {comment.Text}"));
        }

        parts.Add(new(SnapshotPartKind.Blank));
        parts.Add(new(SnapshotPartKind.Section, Value: "События"));
        var events = domain.State.Log.Where(item => item.RiskId == risk.Id).ToList();
        if (events.Count == 0) parts.Add(new(SnapshotPartKind.Note, Value: "сохранённых событий нет"));
        foreach (var item in events) parts.Add(new(SnapshotPartKind.Field, item.At, $"{item.Author}: {item.Text}"));
        return parts;
    }

    static void AddAssessment(List<SnapshotPart> parts, string title, Assessment assessment, Domain domain)
    {
        var who = string.IsNullOrEmpty(assessment.AuthorId)
            ? "автор не сохранён"
            : domain.State.Users.FirstOrDefault(user => user.Id == assessment.AuthorId)?.Name ?? "автор не сохранён";
        parts.Add(new(SnapshotPartKind.Field, title, $"вероятность {assessment.Likelihood} · тяжесть {assessment.Impact} · балл {assessment.Score}"));
        parts.Add(new(SnapshotPartKind.Field, "Уровень / когда", $"{LevelLabel(assessment.Level)} · {(assessment.At ?? "не сохранено")} · {who}"));
    }
}

