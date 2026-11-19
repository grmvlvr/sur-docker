namespace Sur.Api;

static partial class Fn
{
    public static void EnsureDefaults(AppState state)
    {
        if (state.Roles.Count == 0)
        {
            state.Roles =
            [
                new RoleItem { Code = "ADMIN", Name = "Администратор", Rights = ["admin"] },
                new RoleItem { Code = "CEO", Name = "Генеральный директор", Rights = RoleRights.Map["CEO"].ToList() },
                new RoleItem { Code = "RISK_SPECIALIST", Name = "Специалист по оценке рисков", Rights = RoleRights.Map["RISK_SPECIALIST"].ToList() },
                new RoleItem { Code = "DIRECTOR", Name = "Директор", Rights = RoleRights.Map["DIRECTOR"].ToList() },
                new RoleItem { Code = "OWNER", Name = "Владелец процесса", Rights = RoleRights.Map["OWNER"].ToList() },
                new RoleItem { Code = "EMPLOYEE", Name = "Линейный сотрудник", Rights = RoleRights.Map["EMPLOYEE"].ToList() }
            ];
        }
        if (state.Likelihoods.Count == 0)
        {
            state.Likelihoods =
            [
                new LevelName { Level = 1, Name = "Раз в год" },
                new LevelName { Level = 2, Name = "Один раз в квартал" },
                new LevelName { Level = 3, Name = "Один раз в месяц" },
                new LevelName { Level = 4, Name = "Каждую неделю" },
                new LevelName { Level = 5, Name = "Каждый день" }
            ];
        }
        if (state.Bands.Count == 0)
        {
            state.Bands = Rules.Impacts.Select(item => new ImpactBand { Level = item.Level, From = item.From, To = item.To, Name = item.Name }).ToList();
        }
        foreach (var user in state.Users)
        {
            if (string.IsNullOrEmpty(user.PasswordHash)) user.PasswordHash = HashPassword(DemoPassword);
            if (!user.Login.Contains('@')) user.Login = user.Login.Trim().ToLowerInvariant() + "@sur.local";
        }
    }
}
