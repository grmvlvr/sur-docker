namespace Sur.Api;

static partial class Fn
{
    public static Result SaveThresholds(Domain domain, ThresholdInput input)
    {
        if (!IsAdmin(domain)) return Fail("Пороги согласования меняет администратор.");
        if (input.OwnerCost < 0 || input.DirectorCost < input.OwnerCost || input.CeoCost < input.DirectorCost)
            return Fail("Пороги согласования не могут убывать и быть отрицательными.");
        domain.State.Thresholds.OwnerCost = input.OwnerCost;
        domain.State.Thresholds.DirectorCost = input.DirectorCost;
        domain.State.Thresholds.CeoCost = input.CeoCost;
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
