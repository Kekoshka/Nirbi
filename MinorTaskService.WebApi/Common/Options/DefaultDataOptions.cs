namespace MinorTaskService.WebApi.Common.Options;

public class DefaultDataOptions
{
    public int GetMinorTaskLimit { get; set; } = 20;
    public int GetMinorTaskBetweenFrom { get; set; } = 21;
    public int GetMinorTaskBetweenTo { get; set; } = 40;
}
