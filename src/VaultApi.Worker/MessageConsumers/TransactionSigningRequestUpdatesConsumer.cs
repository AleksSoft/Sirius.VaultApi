using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Sirius.VaultAgent.MessagingContract.Transactions;
using VaultApi.Common.Persistence.Blockchains;
using VaultApi.Common.Persistence.Transactions;
using VaultApi.Common.Persistence.Vaults;
using VaultApi.Common.ReadModels.Transactions;
using UserContext = VaultApi.Common.ReadModels.Transactions.UserContext;

namespace VaultApi.Worker.MessageConsumers
{
    public class TransactionSigningRequestUpdatesConsumer : IConsumer<TransactionSigningRequestUpdated>
    {
        private readonly ITransactionSigningRequestsRepository _transactionSigningRequestsRepository;
        private readonly IVaultsRepository _vaultsRepository;
        private readonly IBlockchainsRepository _blockchainsRepository;
        private readonly ILogger<TransactionSigningRequestUpdatesConsumer> _logger;

        public TransactionSigningRequestUpdatesConsumer(
            ITransactionSigningRequestsRepository transactionSigningRequestsRepository,
            IVaultsRepository vaultsRepository,
            IBlockchainsRepository blockchainsRepository,
            ILogger<TransactionSigningRequestUpdatesConsumer> logger)
        {
            _transactionSigningRequestsRepository = transactionSigningRequestsRepository;
            _vaultsRepository = vaultsRepository;
            _blockchainsRepository = blockchainsRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TransactionSigningRequestUpdated> context)
        {
            var @event = context.Message;

            var blockchain = await _blockchainsRepository.GetByIdAsync(@event.BlockchainId);

            if (blockchain == null)
                throw new Exception($"Blockchain not found. Id: {@event.BlockchainId}");

            var vault = await _vaultsRepository.GetByIdAsync(@event.VaultId);

            if (vault == null)
                throw new Exception($"Vault not found. Id: {@event.VaultId}");

            var transactionSigningRequest = new TransactionSigningRequest
            {
                Id = @event.Id,
                TenantId = @event.TenantId,
                SigningAddresses = @event.SigningAddresses,
                BuiltTransaction = @event.BuiltTransaction,
                State = @event.State switch
                {
                    Swisschain.Sirius.VaultAgent.MessagingContract.Transactions.TransactionSigningRequestState.Pending
                    => Common.ReadModels.Transactions.TransactionSigningRequestState.Pending,
                    Swisschain.Sirius.VaultAgent.MessagingContract.Transactions.TransactionSigningRequestState.Completed
                    => Common.ReadModels.Transactions.TransactionSigningRequestState.Completed,
                    Swisschain.Sirius.VaultAgent.MessagingContract.Transactions.TransactionSigningRequestState.Stale
                    => Common.ReadModels.Transactions.TransactionSigningRequestState.Stale,
                    Swisschain.Sirius.VaultAgent.MessagingContract.Transactions.TransactionSigningRequestState.Rejected
                    => Common.ReadModels.Transactions.TransactionSigningRequestState.Rejected,
                    _ => throw new ArgumentOutOfRangeException(nameof(@event.State), @event.State, null)
                },
                RejectionReason = @event.RejectionReason switch
                {
                    RejectionReason.Other => TransactionRejectionReason.Other,
                    RejectionReason.UnknownBlockchain => TransactionRejectionReason
                        .UnknownBlockchain,
                    RejectionReason.UnwantedTransaction => TransactionRejectionReason
                        .UnwantedTransaction,
                    null => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(@event.RejectionReason),
                        @event.RejectionReason,
                        null)
                },
                RejectionReasonMessage = @event.RejectionReasonMessage,
                VaultId = @event.VaultId,
                OperationId = @event.OperationId,
                OperationType = @event.OperationType,
                BlockchainId = @event.BlockchainId,
                Component = @event.Component,
                VaultType = vault.Type,
                NetworkType = blockchain.NetworkType,
                ProtocolCode = blockchain.Protocol.Code,
                DoubleSpendingProtectionType = blockchain.Protocol.DoubleSpendingProtectionType,
                CoinsToSpend = @event.CoinsToSpend
                    ?.Select(x => new Common.ReadModels.Transactions.Coin(x.Id,x.Asset,x.Value,x.Address,x.Redeem))
                    .ToArray(),
                CreatedAt = @event.CreatedAt,
                UpdatedAt = @event.UpdatedAt,
                UserContext = new UserContext()
                {
                    AccountReferenceId = @event.UserContext.AccountReferenceId,
                    ApiKeyId = @event.UserContext.ApiKeyId,
                    PassClientIp = @event.UserContext.PassClientIp,
                    UserId = @event.UserContext.UserId,
                    WithdrawalReferenceId = @event.UserContext.WithdrawalReferenceId,
                    WithdrawalParamsSignature = @event.UserContext.WithdrawalParamsSignature
                }
            };

            await _transactionSigningRequestsRepository.InsertOrUpdateAsync(transactionSigningRequest);

            _logger.LogInformation($"{nameof(TransactionSigningRequestUpdated)} has been processed {{@context}}",
                @event);
        }
    }
}
