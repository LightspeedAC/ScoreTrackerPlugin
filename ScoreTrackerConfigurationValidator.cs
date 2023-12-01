using FluentValidation;

namespace ScoreTrackerPlugin;

public class ScoreTrackerConfigurationValidator : AbstractValidator<ScoreTrackerConfiguration>
{
    
    public ScoreTrackerConfigurationValidator()
    {
        RuleFor(cfg => cfg.ServerType).NotNull().LessThanOrEqualTo(2).GreaterThanOrEqualTo(0);
        RuleFor(cfg => cfg.BroadcastMessages).NotNull();
    }
}
