namespace LandingBack.Services.Interfaces
{
    public interface IVisitaJobService
    {
        Task ScheduleVisitaReminderAsync(int visitaId);
        Task CancelScheduledReminderAsync(int visitaId);
        Task ProcessRemindersAsync();
        Task SendReminderEmailAsync(int visitaId);
    }
}
