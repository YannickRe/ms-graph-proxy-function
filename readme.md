# Microsoft Graph Proxy Function
## Summary
Microsoft Graph contains an enormous amount of information that might be useful to show in a Power BI report. Up until May 31st, 2018 this was possible out of the box, but it stopped working ([issue report](https://community.powerbi.com/t5/Issues/Error-getting-OData-from-Microsoft-Graph-Access-to-the-resource/idi-p/430087)).

This solution is a temporary workaround until Microsoft fixes the issue or releases a dedicated Microsoft Graph connector for Power BI.

## How it works
The code is an Azure Function that accepts any get request and passes the request on to the Microsoft Graph. Upon receiving the result, some transformation is done to make sure subsequent requests (eg. paging) get send through the Azure Function (instead of directly to the Microsoft Graph).

### Examples
```
GET https://{function-app-name}.azurewebsites.net/v1.0/users
```
```
GET https://{function-app-name}.azurewebsites.net/v1.0/users?$select=id,displayName
```
```
GET https://{function-app-name}.azurewebsites.net/beta/teams/{teamId}
```

## More information
Detailed explanation coming soon in a blogpost on https://yannick.reekmans.be

## Disclaimer
THIS CODE IS PROVIDED AS IS WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

## Getting started
### Installation
1. Create an Azure Function App in the Azure Portal
2. Clone the repository
3. Deploy the code into the newly created Azure Function App
4. In the Azure Portal, open the Function App  
    1. Underneath Settings, open __Identity__
        * Select tab __System assigned__
        * Set Status to '__On__'
        * Save
    3. Open __Authentication__
        * Select: '__Add an identity provider__':
            * Identity provider: '__Microsoft__'
            * App registration type: '__Create new app registration__'
            * Supported account types: '__Current tenant - Single tenant__'
            * Restrict access: '__Require authentication__'
            * Unauthenticated requests: '__HTTP 401__'
        * Select: '__Add__'
    4. Open __Authentication__
        * Edit the identity provider that was created in the previous step:
            * Allowed token audiences, make sure you have two entries (amek sure you use the correct URL of your function app):
                * __https://[function-name].azurewebsites.net/.auth/login/aad/callback__
                * __https://[function-name].azurewebsites.net__
            * Save  
5. In the Azure Portal, create a KeyVault
    1. While creating
        * Open __Access policies__
        * Add new
        * Select principal: '*Name of your Function App*'
        * Secrets permissions: '__Get__'
        * Close blades, click __Create__
    2. Open the new Azure Key Vault
        * Copy and store the url for later
        * Secrets
        * Click __Generate/Import__
            * Upload options: '__Manual__'
            * Name: '__ClientSecret__'
            * Value: '*the secret from the Azure AD Application step*'
            * Content type: '*leave empty*'
            * Set activation date? '__Unchecked__'
            * Set expiration date? '__Unchecked__'
            * Enabled? '__Yes__'
            * Click Create
6. In the Azure Portal, open the Azure Function App
    1. Open __Platform features__
    2. Open __Application settings__, add the following keys with values:
        * Authority: '__https://login.microsoftonline.com__'
        * ClientId: '*Client Id previously copied*'
        * ClientSecretName: '__ClientSecret__'  
        * GraphBaseUrl: '__https://graph.microsoft.com__'
        * KeyVaultUri: '*Url copied from the Azure Key Vault*'
        * TenantId: '*id of your Azure AD tenant*'
        * UseApplicationPermissions: '*true or false*'

### Assign permissions to Microsoft Graph
1. In the Azure Portal, open __Azure Active Directory__
2. Open __App registrations__, and find __Microsoft Graph Proxy Function__ in the list
3. Open the __Settings__ and go into __Permissions__
4. Add __Microsoft Graph__ as API and select the required permissions.
    * Application Permissions when UseApplicationPermissions is true, and you want the Azure Function to connect to the Microsoft Graph
    * Delegated Permissions when UseApplicationPermissions is set to false, and you want to access the Microsoft Graph on behalf of the logged in user

### Using in Power BI Desktop
1. Add a data source, select OData.
2. Enter the url of the Azure Function and append the Microsoft Graph query
3. When asked for authentication, select __Organizational Account__ and enter the credentials of a user account.

## Known issues
* Access reports as JSON from the /beta endpoint results in a parsing issue in Power BI. Since the function is just passing through information, this issue is probably caused by Microsoft Graph or Power BI
* Accessing large datasets might result in a `429 Too Many Requests` response. Power BI doesn't handle this gracefully and just errors out.
