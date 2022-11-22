.DEFAULT_GOAL		:=help
SHELL				:=/bin/bash

.PHONY: help
help: ## Show this help
	@grep -E '^[a-zA-Z\.\-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-30s\033[0m %s\n", $$1, $$2}'

##@ Set environment and corresponding configuratio
.PHONY: dev
dev:
	$(eval DEPLOY_ENV=dev)
	$(eval AZURE_SUBSCRIPTION=s165-teachingqualificationsservice-development)
	$(eval RESOURCE_NAME_PREFIX=s165d01)
	$(eval ENV_SHORT=dv)
	$(eval ENV_TAG=dev)

ci:	## Run in automation environment
	$(eval DISABLE_PASSCODE=true)
	$(eval AUTO_APPROVE=-auto-approve)
	$(eval SP_AUTH=true)

set-azure-resource-group-tags: ##Tags that will be added to resource group on it's creation in ARM template
	$(eval RG_TAGS=$(shell echo '{"Portfolio": "Early Years and Schools Group", "Parent Business":"Teaching Regulation Agency", "Product" : "Database of Qualified Teachers", "Service Line": "Teaching Workforce", "Service": "Teacher Services", "Service Offering": "Database of Qualified Teachers", "Environment" : "$(ENV_TAG)"}' | jq . ))

set-azure-template-tag:
	$(eval ARM_TEMPLATE_TAG=1.0.0)

set-azure-account: ${environment}
	echo "Logging on to ${AZURE_SUBSCRIPTION}"
	az account set -s ${AZURE_SUBSCRIPTION}

deploy-azure-resources: set-azure-account set-azure-template-tag set-azure-resource-group-tags# make dev deploy-azure-resources AUTO_APPROVE=1
	$(if $(AUTO_APPROVE), , $(error can only run with AUTO_APPROVE))
	az deployment sub create -l "West Europe" --template-uri "https://raw.githubusercontent.com/DFE-Digital/tra-shared-services/${ARM_TEMPLATE_TAG}/azure/resourcedeploy.json" \
		--parameters "resourceGroupName=${RESOURCE_NAME_PREFIX}-trngen-${ENV_SHORT}-rg" 'tags=${RG_TAGS}' \
			"tfStorageAccountName=${RESOURCE_NAME_PREFIX}trngentfstate${ENV_SHORT}" "tfStorageContainerName=trngen-tfstate" \
			"keyVaultName=${RESOURCE_NAME_PREFIX}-trngen-${ENV_SHORT}-kv"

validate-azure-resources: set-azure-account set-azure-template-tag set-azure-resource-group-tags# make dev validate-azure-resources
	az deployment sub create -l "West Europe" --template-uri "https://raw.githubusercontent.com/DFE-Digital/tra-shared-services/${ARM_TEMPLATE_TAG}/azure/resourcedeploy.json" \
		--parameters "resourceGroupName=${RESOURCE_NAME_PREFIX}-trngen-${ENV_SHORT}-rg" 'tags=${RG_TAGS}' \
			"tfStorageAccountName=${RESOURCE_NAME_PREFIX}trngentfstate${ENV_SHORT}" "tfStorageContainerName=trngen-tfstate" \
			"keyVaultName=${RESOURCE_NAME_PREFIX}-trngen-${ENV_SHORT}-kv" \
		--what-if

