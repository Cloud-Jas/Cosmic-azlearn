# CosmosPark

This repo contains a solution that creates a serverless chat application with a gamified experience that stores 
data in Azure Cosmos DB, Azure Functions and Azure EventGrid for events processing, Azure WebPubSub for websocket client messaging and
Azure Static WebApps for hosting


# Potential use cases

- Create and model chat application 
- Integrate leaderboard and live scheduled tasks 
- Customize maps with realtime data
- Track statistics of a individual user 
- Work with event-driven & serverless architecture


# CosmosPark Premier League architecture

![https://cosmospark.iamdivakarkumar.com/](./images/cosmospark.png)

# CosmosDB DataFlow

![https://cosmospark.iamdivakarkumar.com/](./images/cosmospark-dataflow.png)


<p align="center">
  <b> Realtime Cosmic chat app built using various Azure services </b>
</p>


# Components

## Azure static webapps
[Azure staticwebapps](https://azure.microsoft.com/en-us/services/app-service/static/#overview) accelerate your app development with managed global availability for static content hosting and dynamic scale for integrated serverless APIs. 
In this architecture, Azure StaticWebApps is used as our hosting platform that hosts both frontend and backend REST APIs.

## Azure Maps
[Azure Maps](https://azure.microsoft.com/en-in/services/azure-maps/#azuremaps-overview) allows you to add maps, spatial analytics, and mobility solutions to your apps with geospatial APIs

## Azure Functions
[Azure Functions](https://azure.microsoft.com/en-us/services/functions/) is a serverless platform solution on Azure that allows developers to write compute-on-demand code, 
without having to maintain any of the underlying systems. In this architecture, Azure Functions can host APIs, and any work that needs to be done asynchronously, such as running periodic jobs and computing statistics over a certain period of time.




# Workflow

The solution is built on five pillars, including:

- Clients
- Communication components
- Events processing components
- APIs and business logic 
- Storage and infrastructure services


1. Device client (website/mobile) can access this application in browser. Browser pulls static resources and product images from Azure Static WebApps. We have Azure maps integrated in our landing page
that requires user to allow the location permissions. Once permission is provided user's location will be added as a marker on the map. <br> On the lower portion of the maps, user could find a place where list of tasks will be asigned dynamically for each users. To complete a task one should speak with a person from the specified location 
on the tasks. In order to do that , search for the particular location and select on any marker around that region, now users will be prompted to chat with them. Click on the button to initiate talks.
Once users have sent a message to another person from the specified location in the task he/she will be awarded with specific points based on the criticality of each tasks.Users can also check their rankings in the lower right corner of the screen, where the leaderboard is placed.
<br> The source of the frontend is found in <b>src/app</b> and consists of a plain HTML + Javascript application.

2. Fontend communicates with the backend APIs to perform business use cases. We have used Azure functions HTTP Triggers to expose these APIs. The source of this backend API can be found in **src/api/httpTrigger** and consists of a .NET 6 Azure function app.

    | Verb  | FunctionName |
    | ----- | ---- |
    | POST | CreateChat |
    | POST | CreateUser |
	| GET | GetAllChatsByUserId |
	| GET | GetAllMessagesByChatId |
	| GET | GetAllUsers |
	| GET | GetAllUsersByCountryCode |
	| GET | GetConnectionDetails |
	| GET | GetCosmosLeaderBoards |
	| GET | GetToken |
	| GET | GetUserById |
	| GET | LookupAddress |

3. For persisting the data we use Azure Cosmos DB and below are the containers. Azure CosmosDB is our core component from which all data are distributed to other event processing/communication components.
  - CosmicUsers
  - CosmicChats
  - CosmicUserChats
  - CosmicUserTasks
  - CosmicTaskValidator
  - CosmicLeaderboards
  - CosmicGlobalTimer
 
4. Change Feeds are enabled for few of the containers that needs to be listened for any changes and it will be passed on to different components in the downstream. We have used Azure Functions CosmosDB Triggers for
handling these business use cases. 

    | Verb  | ChangeFeedEnabled? |
    | ----- | ---- |
    | CosmicUsers | Yes |
	| CosmicChats | Yes |
	| CosmicUsers | Yes |
	| CosmicUserChats | Yes |
	| CosmicTaskValidator | No |
	| CosmicUserTasks | Yes |
	| CosmicLeaderboards | Yes |
	| CosmicGlobalTimer | No |
	

# Application Screens

![https://cosmospark.iamdivakarkumar.com/](./images/Cosmic-Azlearn-1.png)
![https://cosmospark.iamdivakarkumar.com/](./images/Cosmic-Azlearn-2.png)
![https://cosmospark.iamdivakarkumar.com/](./images/Cosmic-Azlearn-3.png)
![https://cosmospark.iamdivakarkumar.com/](./images/Cosmic-Azlearn-4.png)
