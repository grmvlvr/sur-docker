namespace Sur.Api;

static partial class Fn
{
    public static string ModeLabel(string? mode) => mode switch
    {
        "treat" => "Обработать",
        "split" => "Разделить",
        "transfer" => "Передать",
        "accept" => "Принять",
        null or "" => "не выбран",
        _ => mode
    };
}
