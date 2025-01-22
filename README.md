# Smart Flow Connectors

This project connects to ServiceNow to extract knowledge articles and store them in an Azure Blob Storage container.

## Overview
- Retrieves knowledge articles from ServiceNow.
- Lands them into a specified Azure Blob Storage container.

## Setup
1. Update your appsettings.Development.json (or use environment variables) with:
   - ServiceNowInstanceUrl
   - ServiceNowUsername
   - ServiceNowPassword
   - StorageAccountName
   - ContentStorageContainer

2. Ensure your Azure Storage account is properly configured.
3. Build and run the project.

## Usage
- Run the application to automatically fetch and store knowledge articles.
- Check your Azure Blob Storage container to verify the imported articles.