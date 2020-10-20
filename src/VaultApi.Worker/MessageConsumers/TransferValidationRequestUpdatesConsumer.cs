using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.VaultAgent.MessagingContract.TransferValidationRequests;
using VaultApi.Common.Persistence.Blockchains;
using VaultApi.Common.Persistence.TransferValidationRequests;
using VaultApi.Common.Persistence.Vaults;
using VaultApi.Common.ReadModels.TransferValidationRequests;

namespace VaultApi.Worker.MessageConsumers
{
    public class TransferValidationRequestUpdatesConsumer : IConsumer<TransferValidationRequestUpdated>
    {
        private readonly IVaultsRepository _vaultsRepository;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly ILogger<TransferValidationRequestUpdatesConsumer> _logger;
        private readonly ITransferValidationRequestRepository _transferValidationRequestRepository;

        public TransferValidationRequestUpdatesConsumer(IVaultsRepository vaultsRepository,
            IBlockchainsRepository blockchainsRepository,
            ILogger<TransferValidationRequestUpdatesConsumer> logger,
            ITransferValidationRequestRepository transferValidationRequestRepository)
        {
            _vaultsRepository = vaultsRepository;
            _blockchainsRepository = blockchainsRepository;
            _logger = logger;
            _transferValidationRequestRepository = transferValidationRequestRepository;
        }

        public async Task Consume(ConsumeContext<TransferValidationRequestUpdated> context)
        {
            var @event = context.Message;

            var vault = await _vaultsRepository.GetByIdAsync(@event.VaultId);

            if (vault == null)
                throw new Exception($"Vault not found. Id: {@event.VaultId}");

            var blockchain = await _blockchainsRepository.GetByIdAsync(@event.BlockchainId);

            if (blockchain == null)
                throw new Exception($"Blockchain not found. Id: {@event.BlockchainId}");

            var transferValidationRequest = new TransferValidationRequest
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
                    ProtocolCode = blockchain.Protocol.Code
                },
                Asset = new Common.ReadModels.TransferValidationRequests.Asset
                {
                    Address = @event.Asset.Address,
                    Id = @event.Asset.Id,
                    Symbol = @event.Asset.Symbol
                },
                SourceAddress =
                    new Common.ReadModels.TransferValidationRequests.SourceAddress()
                    {
                        Name = @event.SourceAddress.Name,
                        Group = @event.SourceAddress.Group,
                        Address = @event.SourceAddress.Address
                    },
                DestinationAddress =
                    new Common.ReadModels.TransferValidationRequests.DestinationAddress()
                    {
                        Address = @event.DestinationAddress.Address,
                        Group = @event.DestinationAddress.Group,
                        Name = @event.DestinationAddress.Name,
                        Tag = @event.DestinationAddress.Tag,
                        TagType = @event.DestinationAddress.TagType
                    },
                Amount = @event.Amount,
                FeeLimit = @event.FeeLimit,
                TransferContext = new Common.ReadModels.TransferValidationRequests.TransferContext
                {
                    AccountReferenceId = @event.TransferContext.AccountReferenceId,
                    WithdrawalReferenceId = @event.TransferContext.WithdrawalReferenceId,
                    Component = @event.TransferContext.Component,
                    OperationType = @event.TransferContext.OperationType,
                    SourceGroup = @event.TransferContext.SourceGroup,
                    DestinationGroup = @event.TransferContext.DestinationGroup,
                    Document = @event.TransferContext.Document,
                    Signature = @event.TransferContext.Signature,
                    RequestContext = new Common.ReadModels.TransferValidationRequests.RequestContext
                    {
                        UserId = @event.TransferContext.RequestContext.UserId,
                        ApiKeyId = @event.TransferContext.RequestContext.ApiKeyId,
                        Ip = @event.TransferContext.RequestContext.Ip,
                        Timestamp = @event.TransferContext.RequestContext.Timestamp
                    }
                },
                Document = @event.Document,
                Signature = @event.Signature,
                RejectionReason = !@event.RejectionReason.HasValue
                    ? (Common.ReadModels.TransferValidationRequests.TransferValidationRequestRejectionReason?) null
                    : @event.RejectionReason.Value switch
                    {
                        Swisschain.Sirius.VaultAgent.MessagingContract.TransferValidationRequests
                            .TransferValidationRequestRejectionReason.Other =>
                        Common.ReadModels.TransferValidationRequests.TransferValidationRequestRejectionReason.Other,
                        Swisschain.Sirius.VaultAgent.MessagingContract.TransferValidationRequests
                            .TransferValidationRequestRejectionReason.RejectedByPolicy =>
                        Common.ReadModels.TransferValidationRequests.TransferValidationRequestRejectionReason
                            .RejectedByPolicy,
                        _ => throw new ArgumentOutOfRangeException(nameof(@event.RejectionReason),
                            @event.RejectionReason,
                            null)
                    },
                RejectionReasonMessage = @event.RejectionReasonMessage,
                State = @event.State switch
                {
                    Swisschain.Sirius.VaultAgent.MessagingContract.TransferValidationRequests
                        .TransferValidationRequestState.Created => Common.ReadModels.TransferValidationRequests
                        .TransferValidationRequestState.Created,
                    Swisschain.Sirius.VaultAgent.MessagingContract.TransferValidationRequests
                        .TransferValidationRequestState.Confirmed => Common.ReadModels.TransferValidationRequests
                        .TransferValidationRequestState.Confirmed,
                    Swisschain.Sirius.VaultAgent.MessagingContract.TransferValidationRequests
                        .TransferValidationRequestState.Rejected => Common.ReadModels.TransferValidationRequests
                        .TransferValidationRequestState.Rejected,
                    _ => throw new ArgumentOutOfRangeException(nameof(@event.State), @event.State, null)
                },
                Sequence = @event.Sequence,
                CreatedAt = @event.CreatedAt,
                UpdatedAt = @event.UpdatedAt
            };

            await _transferValidationRequestRepository.UpdateAsync(transferValidationRequest);

            _logger.LogInformation($"{nameof(TransferValidationRequestUpdated)} has been processed {{@context}}",
                @event);
        }
    }
}
