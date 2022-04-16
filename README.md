# CosmosPark

This repo contains a solution that creates a serverless chat application with a gamified experience that stores 
data in Azure Cosmos DB, Azure Functions and Azure EventGrid for events processing, Azure WebPubSub for websocket client messaging and
Azure Static WebApps for hosting


# Potential use cases

- Create and modelling chat application 
- Integrate leaderboard and live scheduled tasks for each users
- Customize maps with various realtime markers
- Track statistics of a individual user 


# CosmosPark Premier League architecture

![https://cosmospark.iamdivakarkumar.com/](./images/cosmospark.png)


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




#Workflow

The solution is built on 

- Clients
- Websockets for messaging
- APIs and business logic for events processing
- Storage and infrastructure services



# Application Screens

![https://cosmospark.iamdivakarkumar.com/](./images/Cosmic-Azlearn-1.png)
![https://cosmospark.iamdivakarkumar.com/](./images/Cosmic-Azlearn-2.png)
![https://cosmospark.iamdivakarkumar.com/](./images/Cosmic-Azlearn-3.png)
![https://cosmospark.iamdivakarkumar.com/](./images/Cosmic-Azlearn-4.png)
