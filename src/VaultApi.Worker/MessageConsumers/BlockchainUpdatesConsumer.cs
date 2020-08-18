using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.Integrations.MessagingContract.Blockchains;
using VaultApi.Common.Persistence.Blockchains;
using VaultApi.Common.ReadModels.Blockchains;

namespace VaultApi.Worker.MessageConsumers
{
    public class BlockchainUpdatesConsumer : IConsumer<BlockchainUpdated>
    {
        private readonly ILogger<BlockchainUpdatesConsumer> _logger;
        private readonly IBlockchainsRepository _blockchainsRepository;

        public BlockchainUpdatesConsumer(
            IBlockchainsRepository blockchainsRepository,
            ILogger<BlockchainUpdatesConsumer> logger)
        {
            _blockchainsRepository = blockchainsRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BlockchainUpdated> context)
        {
            var @event = context.Message;

            var blockchain = new Blockchain
            {
                Id = @event.BlockchainId,
                Name = @event.Name,
                NetworkType = @event.NetworkType,
                Protocol = new Common.ReadModels.Blockchains.Protocol
                {
                    Code = @event.Protocol.Code,
                    Name = @event.Protocol.Name,
                    DoubleSpendingProtectionType = @event.Protocol.DoubleSpendingProtectionType
                },
                TenantId = @event.TenantId,
                CreatedAt = @event.CreatedAt,
                UpdatedAt = @event.UpdatedAt,
            };

            await _blockchainsRepository.Upsert(blockchain);

            _logger.LogInformation($"{nameof(BlockchainUpdated)} has been processed {{@context}}", @event);
        }
    }
}
