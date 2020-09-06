using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.VaultAgent.MessagingContract.Wallets;
using VaultApi.Common.Persistence.Blockchains;
using VaultApi.Common.Persistence.Vaults;
using VaultApi.Common.Persistence.Wallets;
using VaultApi.Common.ReadModels.Wallets;

namespace VaultApi.Worker.MessageConsumers
{
    public class WalletGenerationRequestUpdatedConsumer : IConsumer<WalletGenerationRequestUpdated>
    {
        private readonly IWalletGenerationRequestRepository _walletGenerationRequestRepository;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly IVaultsRepository _vaultsRepository;
        private readonly ILogger<WalletGenerationRequestUpdatedConsumer> _logger;

        public WalletGenerationRequestUpdatedConsumer(
            IWalletGenerationRequestRepository walletGenerationRequestRepository,
            IBlockchainsRepository blockchainsRepository,
            IVaultsRepository vaultsRepository,
            ILogger<WalletGenerationRequestUpdatedConsumer> logger)
        {
            _walletGenerationRequestRepository = walletGenerationRequestRepository;
            _blockchainsRepository = blockchainsRepository;
            _vaultsRepository = vaultsRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<WalletGenerationRequestUpdated> context)
        {
            var @event = context.Message;

            var blockchain = await _blockchainsRepository.GetByIdAsync(@event.BlockchainId);

            if (blockchain == null)
                throw new Exception($"Blockchain not found. Id: {@event.BlockchainId}");

            var vault = await _vaultsRepository.GetByIdAsync(@event.VaultId);

            if (vault == null)
                throw new Exception($"Vault not found. Id: {@event.VaultId}");

            var walletGenerationRequest = new WalletGenerationRequest
            {
                Id = @event.WalletGenerationRequestId,
                TenantId = @event.TenantId,
                VaultId = @event.VaultId,
                RejectionReason = @event.RejectionReason == null
                    ? (WalletRejectionReason?) null
                    : @event.RejectionReason.Value switch
                    {
                        RejectionReason.Other => WalletRejectionReason.Other,
                        RejectionReason.UnknownBlockchain => WalletRejectionReason.UnknownBlockchain,
                        _ => throw new ArgumentOutOfRangeException(nameof(@event.RejectionReason.Value),
                            @event.RejectionReason.Value,
                            null)
                    },
                State = @event.State switch
                {
                    Swisschain.Sirius.VaultAgent.MessagingContract.Wallets.WalletGenerationRequestState.Pending =>
                    Common.ReadModels.Wallets.WalletGenerationRequestState.Pending,
                    Swisschain.Sirius.VaultAgent.MessagingContract.Wallets.WalletGenerationRequestState.Completed =>
                    Common.ReadModels.Wallets.WalletGenerationRequestState.Completed,
                    Swisschain.Sirius.VaultAgent.MessagingContract.Wallets.WalletGenerationRequestState.Stale =>
                    Common.ReadModels.Wallets.WalletGenerationRequestState.Stale,
                    Swisschain.Sirius.VaultAgent.MessagingContract.Wallets.WalletGenerationRequestState.Rejected =>
                    Common.ReadModels.Wallets.WalletGenerationRequestState.Rejected,
                    _ => throw new ArgumentOutOfRangeException(nameof(@event.State), @event.State, null)
                },
                BlockchainId = @event.BlockchainId,
                RejectionReasonMessage = @event.RejectionReasonMessage,
                Component = @event.Component,
                VaultType = vault.Type,
                NetworkType = blockchain.NetworkType,
                ProtocolCode = blockchain.Protocol.Code,
                CreatedAt = @event.CreatedAt,
                UpdatedAt = @event.UpdatedAt
            };

            await _walletGenerationRequestRepository.InsertOrUpdateAsync(walletGenerationRequest);

            _logger.LogInformation($"{nameof(WalletGenerationRequestUpdated)} has been processed {{@context}}", @event);
        }
    }
}
