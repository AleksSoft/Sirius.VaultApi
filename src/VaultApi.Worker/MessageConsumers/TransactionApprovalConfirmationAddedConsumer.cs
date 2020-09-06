using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.VaultAgent.MessagingContract.TransactionApprovalConfirmations;
using VaultApi.Common.Persistence.KeyKeepers;
using VaultApi.Common.Persistence.TransactionApprovalConfirmations;

namespace VaultApi.Worker.MessageConsumers
{
    public class TransactionApprovalConfirmationAddedConsumer : IConsumer<TransactionApprovalConfirmationAdded>
    {
        private readonly ITransactionApprovalConfirmationsRepository _transactionApprovalConfirmationsRepository;
        private readonly IKeyKeepersRepository _keyKeepersRepository;
        private readonly ILogger<BlockchainUpdatesConsumer> _logger;

        public TransactionApprovalConfirmationAddedConsumer(
            ITransactionApprovalConfirmationsRepository transactionApprovalConfirmationsRepository,
            IKeyKeepersRepository keyKeepersRepository,
            ILogger<BlockchainUpdatesConsumer> logger)
        {
            _transactionApprovalConfirmationsRepository = transactionApprovalConfirmationsRepository;
            _keyKeepersRepository = keyKeepersRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TransactionApprovalConfirmationAdded> context)
        {
            var @event = context.Message;

            var keyKeeper = await _keyKeepersRepository.GetByIdOrDefaultAsync(@event.KeyKeeperId);
            
            if(keyKeeper == null)
                throw new Exception($"Key keeper not found. Id: {@event.KeyKeeperId}");

            var transactionApprovalConfirmation =
                new Common.ReadModels.TransactionApprovalConfirmations.TransactionApprovalConfirmation
                {
                    Id = @event.TransactionApprovalConfirmationId,
                    TenantId = @event.TenantId,
                    TransactionApprovalRequestId = @event.TransactionApprovalRequestId,
                    KeyId = keyKeeper.KeyId,
                    Message = @event.Message,
                    Secret = @event.Secret,
                    Status = @event.Status switch
                    {
                        TransactionApprovalConfirmationStatus.Confirmed =>
                        Common.ReadModels.TransactionApprovalConfirmations.TransactionApprovalConfirmationStatus
                            .Confirmed,
                        TransactionApprovalConfirmationStatus.Rejected =>
                        Common.ReadModels.TransactionApprovalConfirmations.TransactionApprovalConfirmationStatus
                            .Rejected,
                        TransactionApprovalConfirmationStatus.Skipped =>
                        Common.ReadModels.TransactionApprovalConfirmations.TransactionApprovalConfirmationStatus
                            .Skipped,
                        _ => throw new InvalidEnumArgumentException(nameof(@event.Status),
                            (int) @event.Status,
                            typeof(TransactionApprovalConfirmationStatus))
                    },
                    CreatedAt = @event.CreatedAt
                };

            await _transactionApprovalConfirmationsRepository.InsertOrIgnoreAsync(transactionApprovalConfirmation);

            _logger.LogInformation($"{nameof(TransactionApprovalConfirmationAdded)} has been processed {{@context}}",
                @event);
        }
    }
}
