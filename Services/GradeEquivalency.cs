namespace GradeProgressMonitoring.Services;

public static class GradeEquivalency
{
    // Uses exact rule: 75–<78 => 3.00, 78–<81 => 2.75, ... 99+ => 1.00, below 75 => 5.00
    public static decimal ToEquivalent(decimal percent)
    {
        if (percent >= 99m) return 1.00m;
        if (percent >= 96m) return 1.25m;
        if (percent >= 93m) return 1.50m;
        if (percent >= 90m) return 1.75m;
        if (percent >= 87m) return 2.00m;
        if (percent >= 84m) return 2.25m;
        if (percent >= 81m) return 2.50m;
        if (percent >= 78m) return 2.75m;
        if (percent >= 75m) return 3.00m;
        return 5.00m;
    }
}
