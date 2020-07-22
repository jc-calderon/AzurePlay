# AzurePlay
A VOD(Video On Demand) project using .NET Core and a lot of Microsoft Azure Services
![Architecture](https://raw.githubusercontent.com/jc-calderon/AzurePlay/master/Architecture.png)

## Projects
|__Project__|__Tech__|__Deploy in Azure__|__Description__|
|:---------:|:-------:|:-------:|:-------:|
|AzurePlay.Common| C#, .NET Core 2.2, SqlClient, Newtonsoft.Json, AutoMapper | | Common stuffs, like models and Azure SQL database service. |
|AzurePlay.Functions.MediaManager|  C#, .NET Core 2.2, Refit  | Azure Function | This function is triggered when a movie is uploaded to Azure Storage, all the logic is here. |
|AzurePlay.Web| C#, Vue.js, .NET Core 2.2, REST API | App Service | The basic Web application for show and play movies. |
|Core| Azure KeyVault, Media Services, Storage, DI, log4net, Loggly | | The implementation of modules like Azure, logging, DI, etc.|

## Required Services 
* Microsoft Azure SQL Database
* Microsoft Azure Storage
* Microsoft Azure Key Vault
* Microsoft Azure Functions
* Microsoft Azure SignalR
* Microsoft Azure Media Services
* Microsoft Azure App Service
* Loggly account
* TMDb API key

## License
Licensed under the [MIT](LICENSE) license.
