namespace Sur.Api;

static class Rules
{
    public static readonly int[] Likelihoods = [1, 2, 3, 4, 5];

    public static readonly (int Level, decimal From, decimal? To, string Name)[] Impacts =
    [
        (1, 0, 100000, "Менее 100 000 ₽"),
        (2, 100000, 250000, "100 000 – 250 000 ₽"),
        (3, 250000, 500000, "250 000 – 500 000 ₽"),
        (4, 500000, 1000000, "500 000 – 1 000 000 ₽"),
        (5, 1000000, null, "Более 1 000 000 ₽")
    ];

    public static readonly Dictionary<string, string> Levels = new()
    {
        ["OWNER"] = "Владелец процесса",
        ["DIRECTOR"] = "Директор",
        ["CEO"] = "CEO",
        ["TREATMENT_REQUIRED"] = "Обязательная обработка"
    };

    public static readonly HashSet<string> OpenTreatment = ["На согласовании", "Согласовано", "Ожидает выполнения", "В работе", "Холд"];
    public static readonly HashSet<string> LimitingTreatment = ["На согласовании", "Согласовано", "Ожидает выполнения", "В работе"];
}
