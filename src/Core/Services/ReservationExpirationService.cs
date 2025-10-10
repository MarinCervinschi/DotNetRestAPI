using src.Core.Entities;
using src.Core.Interfaces.Repositories;

namespace src.Core.Services;

public enum ExpirationHandlingStrategy
{
    RestoreBookOnly,
    DeleteReservation
}

public class ReservationExpirationService(
    IServiceProvider serviceProvider,
    ILogger<ReservationExpirationService> logger)
    : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    private const ExpirationHandlingStrategy Strategy = ExpirationHandlingStrategy.RestoreBookOnly;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ReservationExpirationService started with strategy: {Strategy}", Strategy);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredReservationsAsync();
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing expired reservations");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task ProcessExpiredReservationsAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var reservationRepository = scope.ServiceProvider.GetRequiredService<IReservationRepository>();

        logger.LogInformation("Checking for expired reservations...");

        var allReservations = await reservationRepository.GetAllAsync();
        var expiredReservations = allReservations
            .Where(r => r.ExpirationDate <= DateTime.UtcNow)
            .ToList();

        if (expiredReservations.Count != 0)
        {
            logger.LogInformation("Found {Count} expired reservations", expiredReservations.Count);

            foreach (var reservation in expiredReservations)
            {
                try
                {
                    await ProcessSingleExpiredReservation(reservation, reservationRepository);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing expired reservation {ReservationId}", reservation.Id);
                }
            }
        }
        else
        {
            logger.LogDebug("No expired reservations found");
        }
    }

    private async Task ProcessSingleExpiredReservation(
        Reservation reservation,
        IReservationRepository reservationRepository)
    {
        switch (Strategy)
        {
            case ExpirationHandlingStrategy.RestoreBookOnly:
                logger.LogInformation(
                    "Restoring book {BookId} availability for expired reservation {ReservationId} (keeping reservation history)",
                    reservation.BookId, reservation.Id);

                var restored = await reservationRepository.RestoreBookAvailabilityAsync(reservation.BookId);

                if (restored)
                {
                    logger.LogInformation(
                        "Successfully restored book {BookId} availability for expired reservation {ReservationId}",
                        reservation.BookId, reservation.Id);
                }
                else
                {
                    logger.LogWarning(
                        "Failed to restore book {BookId} availability for expired reservation {ReservationId}",
                        reservation.BookId, reservation.Id);
                }

                break;

            /*case ExpirationHandlingStrategy.DeleteReservation:
                logger.LogInformation("Deleting expired reservation {ReservationId} for book {BookId}",
                    reservation.Id, reservation.BookId);

                var deleted = await reservationRepository.DeleteReservationWithBookUpdateAsync(reservation.Id);

                if (deleted)
                {
                    logger.LogInformation(
                        "Successfully deleted expired reservation {ReservationId} and restored book {BookId} availability",
                        reservation.Id, reservation.BookId);
                }
                else
                {
                    logger.LogWarning("Failed to delete expired reservation {ReservationId}", reservation.Id);
                }

                break;

            default:
                logger.LogWarning("Unknown expiration handling strategy: {Strategy}", Strategy);
                break;*/
        }
    }
}