using LaunchPlanner.Core.Models;
using LaunchPlanner.Core.Scheduling;

namespace LaunchPlanner.Core.Persistence;

public interface IEventRepository
{
    string SchedulePath { get; }
    ScheduleParseResult Load();
    void Save(IReadOnlyList<ScheduledEvent> events);
    FiredLog LoadFired();
    void SaveFired(FiredLog fired);
}
