using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.VaultAgent.MessagingContract.TransferSigningRequests;
using VaultApi.Common.Persistence.Blockchains;
using VaultApi.Common.Persistence.TransferSigningRequests;
using VaultApi.Common.Persistence.Vaults;
using VaultApi.Common.ReadModels.TransferSigningRequests;

namespace VaultApi.Worker.MessageConsumers
{
    public class TransferSigningRequestUpdatesConsumer : IConsumer<TransferSigningRequestUpdated>
    {
        private readonly ITransferSigningRequestsRepository _transferSigningRequestsRepository;
        private readonly IVaultsRepository _vaultsRepository;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly ILogger<TransferSigningRequestUpdatesConsumer> _logger;

        public TransferSigningRequestUpdatesConsumer(
            ITransferSigningRequestsRepository transferSigningRequestsRepository,
            IVaultsRepository vaultsRepository,
            IBlockchainsRepository blockchainsRepository,
            ILogger<TransferSigningRequestUpdatesConsumer> logger)
        {
            _transferSigningRequestsRepository = transferSigningRequestsRepository;
            _vaultsRepository = vaultsRepository;
            _blockchainsRepository = blockchainsRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TransferSigningRequestUpdated> context)
        {
            var @event = context.Message;

            var blockchain = await _blockchainsRepository.GetByIdAsync(@event.BlockchainId);

            if (blockchain == null)
                throw new Exception($"Blockchain not found. Id: {@event.BlockchainId}");

            var vault = await _vaultsRepository.GetByIdAsync(@event.VaultId);

            if (vault == null)
                throw new Exception($"Vault not found. Id: {@event.VaultId}");

            var transactionSigningRequest = new TransferSigningRequest
            {
                Id = @event.Id,
                TransferId = @event.TransferId,
                TenantId = @event.TenantId,
                VaultId = @event.VaultId,
                VaultType = vault.Type,
                Blockchain = new Blockchain
                {
                    Id = @event.BlockchainId,
                    NetworkType = blockchain.NetworkType,
                    ProtocolCode = blockchain.Protocol.Code,
                    DoubleSpendingProtectionType = blockchain.Protocol.DoubleSpendingProtectionType
                },
                BuiltTransaction = @event.BuiltTransaction,
                SigningAddresses = @event.SigningAddresses,
                CoinsToSpend = @event.CoinsToSpend
                    ?.Select(x => new Common.ReadModels.TransferSigningRequests.Coin(x.Id,x.Asset,x.Value,x.Address,x.Redeem))
                    .ToArray(),
                RejectionReason = @event.RejectionReason switch
                {
                    RejectionReason.Other => TransferRejectionReason.Other,
                    RejectionReason.UnknownBlockchain => TransferRejectionReason.UnknownBlockchain,
                    RejectionReason.UnwantedTransaction => TransferRejectionReason.UnwantedTransaction,
                    null => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(@event.RejectionReason),
                        @event.RejectionReason,
                        null)
                },
                RejectionReasonMessage = @event.RejectionReasonMessage,
                State = @event.State switch
                {
                    Swisschain.Sirius.VaultAgent.MessagingContract.TransferSigningRequests.TransferSigningRequestState.Pending
                    => Common.ReadModels.TransferSigningRequests.TransferSigningRequestState.Pending,
                    Swisschain.Sirius.VaultAgent.MessagingContract.TransferSigningRequests.TransferSigningRequestState.Completed
                    => Common.ReadModels.TransferSigningRequests.TransferSigningRequestState.Completed,
                    Swisschain.Sirius.VaultAgent.MessagingContract.TransferSigningRequests.TransferSigningRequestState.Stale
                    => Common.ReadModels.TransferSigningRequests.TransferSigningRequestState.Stale,
                    Swisschain.Sirius.VaultAgent.MessagingContract.TransferSigningRequests.TransferSigningRequestState.Rejected
                    => Common.ReadModels.TransferSigningRequests.TransferSigningRequestState.Rejected,
                    _ => throw new ArgumentOutOfRangeException(nameof(@event.State), @event.State, null)
                },
                Document = @event.Document,
                Signature = @event.Signature,
                Group = @event.Group,
                Sequence = @event.Sequence,
                CreatedAt = @event.CreatedAt,
                UpdatedAt = @event.UpdatedAt
            };

            await _transferSigningRequestsRepository.InsertOrUpdateAsync(transactionSigningRequest);

            _logger.LogInformation($"{nameof(TransferSigningRequestUpdated)} has been processed {{@context}}",
                @event);
        }
    }
}
