1. Copy `Service-VaultApi` and `Service-VaultApi-Worker` to the https://github.com/swisschain/kubernetes-swisschain/tree/master/Kubernetes/03.Pods/VaultApi. 
2. Add services namespace if necessary.
3. Add your product-wide settings to the `Settings/product-name/common.json`
4. Add your service-wide settings to the `Settings/product-name/service-name.json`
5. Use placeholders like `${PlaceHolderName}` for the secrets and shared values. Think about placeholder name - it can be iether global-scoped, product-scoped,
or service scope.
    * For the global scoped placeholders don't  use any prefixes (eg. `${DefaultLoggingLevel}`)
    * For the product scoped placeholders use `VaultApi` as the prefix (eg. `${VaultApi-RabbitMq-Login}`)
    * For the service scoped placeholders use `VaultApi-VaultApi` as the prefix (eg. `${VaultApi-VaultApi-DbConnectionString}`)
6. If there is no such a file yet, copy `Settings/common.json` to https://github.com/swisschain/kubernetes-swisschain/tree/master/Settings/common.json or update it
7. If there is no such a file yet, copy `Settings/product-name/common.json` to https://github.com/swisschain/kubernetes-swisschain/tree/master/Settings/sirius/common.json or update it
8. Copy `Settings/product-name/service-name.json` to https://github.com/swisschain/kubernetes-swisschain/tree/master/Settings/sirius/vault-api.json
5. Put placeholders with values to the settings blob in Azure Storage (TODO: specify the blob here)