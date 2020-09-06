using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.VaultAgent.MessagingContract.KeyKeepers;
using VaultApi.Common.Persistence.KeyKeepers;
using VaultApi.Common.ReadModels.KeyKeepers;

namespace VaultApi.Worker.MessageConsumers
{
    public class KeyKeeperUpdatedConsumer : IConsumer<KeyKeeperUpdated>
    {
        private readonly ILogger<KeyKeeperUpdatedConsumer> _logger;
        private readonly IKeyKeepersRepository _keyKeepersRepository;

        public KeyKeeperUpdatedConsumer(
            ILogger<KeyKeeperUpdatedConsumer> logger,
            IKeyKeepersRepository keyKeepersRepository)
        {
            _logger = logger;
            _keyKeepersRepository = keyKeepersRepository;
        }

        public async Task Consume(ConsumeContext<KeyKeeperUpdated> context)
        {
            var @event = context.Message;

            var keyKeeper = new KeyKeeper
            {
                Id = @event.KeyKeeperId,
                TenantId = @event.TenantId,
                KeyId = @event.KeyId,
                Description = @event.Description,
                CreatedAt = @event.CreatedAt,
                UpdatedAt = @event.UpdatedAt
            };

            await _keyKeepersRepository.InsertOrUpdateAsync(keyKeeper);

            _logger.LogInformation($"{nameof(KeyKeeperUpdated)} has been processed {{@context}}", @event);
        }
    }
}
