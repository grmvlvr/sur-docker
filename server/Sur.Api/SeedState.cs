namespace Sur.Api;

static partial class Fn
{
    public static AppState SeedState()
    {
        var state = new AppState
        {
            Directorates = [new Named { Id = "d-prod", Name = "Производство" }, new Named { Id = "d-fin", Name = "Финансы" }],
            Processes =
            [
                new ProcessItem { Id = "p-out", Name = "Выпуск продукции", DirectorateId = "d-prod", OwnerId = "u-own" },
                new ProcessItem { Id = "p-buy", Name = "Закупки", DirectorateId = "d-prod", OwnerId = "u-own" },
                new ProcessItem { Id = "p-bud", Name = "Бюджетирование", DirectorateId = "d-fin", OwnerId = "u-fin" }
            ],
            Objects = [new AssetItem { Id = "o-line", Name = "Линия сборки", Hourly = 15000 }, new AssetItem { Id = "o-db", Name = "Сервер БД", Hourly = 8000 }],
            Hazards =
            [
                new HazardItem { Id = "h-break", Name = "Поломка оборудования", Group = "Оборудование" },
                new HazardItem { Id = "h-supply", Name = "Срыв поставки", Group = "Поставки" },
                new HazardItem { Id = "h-law", Name = "Изменение требований", Group = "Нормативка" }
            ],
            Categories =
            [
                new Named { Id = "c-budget", Name = "Превышение бюджета" },
                new Named { Id = "c-equip", Name = "Оборудование" },
                new Named { Id = "c-law", Name = "Изменения в законодательстве" }
            ],
            Users =
            [
                new UserItem { Id = "u-admin", Name = "Администратор", Login = "admin@sur.local", Roles = ["ADMIN"], DirectorateId = "d-prod" },
                new UserItem { Id = "u-ceo", Name = "Генеральный директор", Login = "ceo@sur.local", Roles = ["CEO"] },
                new UserItem { Id = "u-dir", Name = "Директор производства", Login = "director@sur.local", Roles = ["DIRECTOR"], DirectorateId = "d-prod" },
                new UserItem { Id = "u-own", Name = "Владелец выпуска", Login = "owner@sur.local", Roles = ["OWNER"], DirectorateId = "d-prod" },
                new UserItem { Id = "u-emp", Name = "Линейный сотрудник", Login = "employee@sur.local", Roles = ["EMPLOYEE"], DirectorateId = "d-prod" },
                new UserItem { Id = "u-spec", Name = "Специалист по оценке рисков", Login = "specialist@sur.local", Roles = ["RISK_SPECIALIST"], DirectorateId = "d-prod" },
                new UserItem { Id = "u-fin", Name = "Владелец бюджета", Login = "finance@sur.local", Roles = ["OWNER", "DIRECTOR"], DirectorateId = "d-fin" }
            ]
        };
        return state;
    }
}
