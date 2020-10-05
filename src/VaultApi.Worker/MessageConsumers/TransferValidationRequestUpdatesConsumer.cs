using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.VaultAgent.MessagingContract.Transactions;
using Swisschain.Sirius.VaultAgent.MessagingContract.TransferValidationRequests;
using VaultApi.Common.Persistence.Blockchains;
using VaultApi.Common.Persistence.TransferValidationRequests;
using VaultApi.Common.Persistence.Vaults;
using VaultApi.Common.ReadModels.TransferValidationRequests;
using Asset = VaultApi.Common.ReadModels.TransferValidationRequests.Asset;
using TransferDetails = VaultApi.Common.ReadModels.TransferValidationRequests.TransferDetails;
using TransferValidationRequestRejectionReason = Swisschain.Sirius.VaultAgent.MessagingContract.TransferValidationRequests.TransferValidationRequestRejectionReason;
using TransferValidationRequestState = Swisschain.Sirius.VaultAgent.MessagingContract.TransferValidationRequests.TransferValidationRequestState;
using UserContext = VaultApi.Common.ReadModels.Transactions.UserContext;

namespace VaultApi.Worker.MessageConsumers
{
    public class TransferValidationRequestUpdatesConsumer : IConsumer<TransferValidationRequestUpdated>
    {
        private readonly IVaultsRepository _vaultsRepository;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly ILogger<TransferValidationRequestUpdatesConsumer> _logger;
        private readonly ITransferValidationRequestRepository _transferValidationRequestRepository;

        public TransferValidationRequestUpdatesConsumer(
            IVaultsRepository vaultsRepository,
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
            var blockchain = await _blockchainsRepository.GetByIdAsync(@event.Details.BlockchainId);

            if (blockchain == null)
                throw new Exception($"Blockchain not found. Id: {@event.Details.BlockchainId}");

            var transferValidationRequest = new TransferValidationRequest()
            {
                Id = @event.Id,
                CreatedAt = @event.CreatedAt,
                CustomerSignature = @event.CustomerSignature,
                Details = new TransferDetails()
                {
                    ProtocolId = blockchain.Protocol.Code,
                    NetworkType = blockchain.NetworkType,
                    UserContext = new UserContext()
                    {
                        WithdrawalParamsSignature = @event.Details.UserContext.WithdrawalParamsSignature,
                        AccountReferenceId = @event.Details.UserContext.AccountReferenceId,
                        ApiKeyId = @event.Details.UserContext.ApiKeyId,
                        PassClientIp = @event.Details.UserContext.PassClientIp,
                        UserId = @event.Details.UserContext.UserId,
                        WithdrawalReferenceId = @event.Details.UserContext.WithdrawalReferenceId
                    },
                    Amount = @event.Details.Amount,
                    Asset = new Asset()
                    {
                        Address = @event.Details.Asset.Address,
                        Id = @event.Details.Asset.Id,
                        Symbol = @event.Details.Asset.Symbol
                    },
                    BlockchainId = @event.Details.BlockchainId,
                    DestinationAddress = new Common.ReadModels.TransferValidationRequests.DestinationAddress()
                    {
                        Address = @event.Details.DestinationAddress.Address,
                        Group = @event.Details.DestinationAddress.Group,
                        Name = @event.Details.DestinationAddress.Name,
                        Tag = @event.Details.DestinationAddress.Tag,
                        TagType = @event.Details.DestinationAddress.TagType
                    },
                    FeeLimit = @event.Details.FeeLimit,
                    OperationId = @event.Details.OperationId,
                    SourceAddress = new Common.ReadModels.TransferValidationRequests.SourceAddress()
                    {
                        Name = @event.Details.SourceAddress.Name,
                        Group = @event.Details.SourceAddress.Group,
                        Address = @event.Details.SourceAddress.Address
                    },
                },
                RejectionReason = !@event.RejectionReason.HasValue ?
                    (Common.ReadModels.TransferValidationRequests.TransferValidationRequestRejectionReason?)null : @event.RejectionReason.Value switch
                    {
                        TransferValidationRequestRejectionReason.Other => 
                        Common.ReadModels.TransferValidationRequests.TransferValidationRequestRejectionReason.Other,
                        TransferValidationRequestRejectionReason.RejectedByPolicy => 
                        Common.ReadModels.TransferValidationRequests.TransferValidationRequestRejectionReason.RejectedByPolicy,
                        
                        _ => throw new ArgumentOutOfRangeException(nameof(@event.RejectionReason), @event.RejectionReason, null)
                    },
                RejectionReasonMessage = @event.RejectionReasonString,
                Sequence = @event.Sequence,
                SiriusSignature = @event.SiriusSignature,
                State = @event.State switch {
                    TransferValidationRequestState.Created => Common.ReadModels.TransferValidationRequests.TransferValidationRequestState.Created,
                    TransferValidationRequestState.Confirmed => Common.ReadModels.TransferValidationRequests.TransferValidationRequestState.Confirmed,
                    TransferValidationRequestState.Rejected => Common.ReadModels.TransferValidationRequests.TransferValidationRequestState.Rejected,
                    
                    _ => throw new ArgumentOutOfRangeException(nameof(@event.State), @event.State, null)
                },
                UpdatedAt = @event.UpdatedAt,
                VaultId = @event.VaultId,
                VaultType = vault.Type,
                TenantId = @event.TenantId
            };

            await _transferValidationRequestRepository.UpdateAsync(transferValidationRequest);

            _logger.LogInformation($"{nameof(TransferValidationRequestUpdated)} has been processed {{@context}}",
                @event);
        }
    }
}
