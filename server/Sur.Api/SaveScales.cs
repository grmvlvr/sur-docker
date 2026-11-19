namespace Sur.Api;

static partial class Fn
{
    public static Result SaveScales(Domain domain, ScaleInput input)
    {
        if (!IsAdmin(domain)) return Fail("Шкалы меняет администратор.");
        if (input.ImpactFrom.Count != 5) return Fail("Нужны пять нижних границ тяжести.");
        if (input.ImpactFrom[0] != 0 || input.ImpactFrom.Zip(input.ImpactFrom.Skip(1)).Any(pair => pair.First >= pair.Second))
            return Fail("Границы тяжести должны начинаться с 0 и возрастать.");
        if (!(0 <= input.OwnerMaxScore && input.OwnerMaxScore < input.DirectorMaxScore && input.DirectorMaxScore < input.CeoMaxScore && input.CeoMaxScore < 25))
            return Fail("Пороги принятия: 0 ≤ владелец < директор < CEO < 25.");
        var previous = domain.State.Bands.Select(band => band.From).ToList();
        domain.State.Bands = input.ImpactFrom.Select((from, index) => new ImpactBand
        {
            Level = index + 1,
            From = from,
            To = index == 4 ? null : input.ImpactFrom[index + 1],
            Name = domain.State.Bands.FirstOrDefault(band => band.Level == index + 1)?.Name ?? from.ToString()
        }).ToList();
        if (input.Likelihood.Count == 5) domain.State.Likelihoods = input.Likelihood;
        domain.State.Thresholds.OwnerMaxScore = input.OwnerMaxScore;
        domain.State.Thresholds.DirectorMaxScore = input.DirectorMaxScore;
        domain.State.Thresholds.CeoMaxScore = input.CeoMaxScore;
        var bandsChanged = !previous.SequenceEqual(input.ImpactFrom);
        if (input.ApplyToActive && bandsChanged) RecalcActive(domain);
        else if (input.ApplyToActive) RelabelActive(domain);
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
