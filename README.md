# Service Fabric Explorer Sample

## Entity Calculators

This sample provides basic building block for a real Service Fabric project. It implements minimum yet meaningful functionality but can be extended to encompass more Service Fabric features and/or other Azure services. 

### Scenario

A company has sales offices in different geographic locations organized by regions, countries and sales offices. The company executives would like to view aggregated measures for each of the hierarchy entities: sales offices, countries, regions and globally. The desired measures are: purchases, cancellations, sold items, revenue, tax and shipping. 

In addition, the app must open up a Web API to receive posts from the company transactional system that reports a sales event (i.e. items sold, revenue, tax and shipping). The app then processes the transaction and aggregates the sales measures in the geographic hierarchy.

The entity hierarchy looks like this:

![Entity Hierarchy](http://i.imgur.com/gUQtiRJ.png)

A real example of the entities might be something like this:

![Hierarchy Measures](http://i.imgur.com/lb5SGHL.png)

### Application architecture

The application is composed of individual services to perform the major functions of the application:

- ِِAPI Gateway
	- A stateless front-end service that hosts the Web API for interacting with the app. The gateway handles the HTTP requests and routes the transactions to the enqueuer service for further processing.
- Enqueuer Service
	- A stateful service that manages the lifetime of a transaction. A stateful actor is activated for each new transaction that's queued in its reliable queue. The actor represents the lifetime of the transaction and .
- Entity Actor
	- A stateful actor that manages the lifetime of a transaction request from the enqueuer service. The propagates the transaction processing to its parent if a parent exists. This allows for a very powerful mechanism to handle measure aggregation at each level of the hierarchy.

### Data flow

The app data flow looks like this:

![Data Flow & Micro Architecture](http://i.imgur.com/NYpUMBN.png)

#### Transaction Process Flow

1. A transactional system sends an HTTP POST request to `/api/entities` to signify a sales event. The transaction in JSON payload looks like this: 

```
{
    "transactionDate": "2016-04-07T05:46:09.373407+00:00",
    "transactionType": 0, 
    "entityType": 3,
    "businessKey": 31,
    "soldItems": 2,
    "revenue": 1900,
    "tax": 190,
    "shipping": 57
}
```

The `transactionType` denotes whether the transaction is a purchase or a cancellation while the `entityType` denotes whether the entity from which the transaction came from is a sales office, country, a region or global. 
 
2. The API Gateway receives the request, validates it and enqeues it to the Enqueuer Service. 
3. The enqueuer service dequeues the transaction from its queue and locates the actor responsible to process the transaction. The mechanism used to locate the actor relies on the entity type (i.e. sales office, country, region or global) and the business key. It is the combination of these two properties is what allows us to locate a specific actor to handle the request.
4. The entity actor then processes the transaction by re-calculating its state properties and propagating the request to its parent to also have a chance to process the transaction. The result is that all involved entity actors' state is kept up-to-date and properly aggregated.


#### Child-Parent Process Flow

Assuming that a sales office i.e. New York received a transaction, this illustration shows how the parent entities get processed:

![Child-Parent Process Flow](http://i.imgur.com/7SOcvKH.png)
 
### View Entity Measures Flow

A client system may send an HTTP GET request to `/api/entities/{type}/{businesskey}` to retrieve an entity view. This URI (`api/entities/3/31`) requests an entity view for type 3 (i.e. Sales Office) and business key 31. The entity view returned is a JSON payload that looks like this: 

```
{
  "parentName": "USA",
  "parentView": {
    "type": 2,
    "businessKey": 20,
    "name": "USA",
    "purchases": 3,
    "cancellations": 0,
    "itemsSold": 4,
    "revenue": 3800,
    "tax": 380,
    "shipping": 114
  },
  "thisView": {
    "type": 3,
    "businessKey": 31,
    "name": "Boston",
    "purchases": 3,
    "cancellations": 0,
    "itemsSold": 6,
    "revenue": 5700,
    "tax": 570,
    "shipping": 171
  },
  "childrenViews": {}
}
```
The entity view therefore is a JSON structure that describes the measures of the requested entity, its parent and its children. Because a sales office does not have any children, the children array is empty. If, however, we request `api/entities/0/0` to retrieve the entity view for the global entity, we will get a more meaningful entity view:

```
{
  "parentName": "",
  "parentView": null,
  "thisView": {
    "type": 0,
    "businessKey": 0,
    "name": "Global",
    "purchases": 3,
    "cancellations": 0,
    "itemsSold": 6,
    "revenue": 5700,
    "tax": 570,
    "shipping": 171
  },
  "childrenViews": {
    "americas": {
      "type": 1,
      "businessKey": 10,
      "name": "Americas",
      "purchases": 3,
      "cancellations": 0,
      "itemsSold": 6,
      "revenue": 5700,
      "tax": 570,
      "shipping": 171
    },
    "europe": {
      "type": 1,
      "businessKey": 110,
      "name": "Europe",
      "purchases": 3,
      "cancellations": 0,
      "itemsSold": 0,
      "revenue": 0,
      "tax": 0,
      "shipping": 0
    }
  }
}
```

### Concepts demonstrated in the app

#### Solution Layout

When you create a Service Fabric service, it creates two projects: one for the intefaces and one for the actual code. This makes sense but it becomes annoying when you have multiple services in the same solution. Instead I followed a different convention where I placed all the Service Fabric service interfaces in a single project called `Contracts`. Other solution projects add reference to thic contracts project so they can use the SF services. The solution layout looks like this:

![Solution Layout](http://i.imgur.com/cokkfCc.png)

IMHO, the Service Fabric services are more like nano services (not to be confused with nano server) while the Sevice Fabric App is like a traditional micro service. Actually, the Service Fabric Visual Studio solution allows you to add multiple Service Fabric Apps and share services in multiple apps. Therefore if we subject our sample app to a simple test to see if it fits the bill to be a micro service, we will notice that it meets all requirements:

- Has its own Api Gateway through which all communication from/to the service is conducted
- Has behind-the-scene stateful and actor services to handle the internal state and manage the data flow
- Can have a UI layer which could reflect the service view elements and may be included in an application or dashboard.   
 
#### Abstractions 

I tried to abstract everything in the sample app to make it unit-testable. In addition to the SF service interfaces, I abstracted all the shared services that I use in the app to handle the different functionality. The app currently uses the following interfaces:

* `IActorLocationService` - provides actors location service. It is used by the enqueuer service and the API Gateway to locate actors without directly relying on SF which will allow for testability 
* `IInsightsService` - provides insights service. Different providers can be plugged in and driven by configuration. For the app, it does not matter whether it is speaking to Application Insights or Raygun. 
* `ILoggerService` - provides logging services. Different providers can be plugged in and driven by configuration. For the app, it does not matter whether it is logging to ETW or a database.  
* `IOltpConnectorService` - provides an OLTP connector to retrieve the entities used in the app. Right now, the app demos how to change the configuration to allow the actors to load fake or real entities. The fake connector uses hard-coded entities. 
* `IServiceLocationService` - provides actors location service. It is mainly used by the API Gateway to locate the enqueuer service.
* `ISettingService` - provides setting service to read/write from configuration. It abstracts the SF way of retrieving configuration items such as whether we run against a fake or real OLTP connector. 
* `IUriBuilderService` - provides SF specific URI builder service. It is used whenever a service needs to be located.

#### Entity Actor Id

As described above, the entity actor is made up of the entity business key (some sort of a database id) and the entity type (i.e. sales office, country, region or global). The way the actor ID is constructed comes in really handy when the actor is first invoked. The actor can parse its own ID to determine the business key and entity type and use the `IOLTPConnectorService` to load the entity from a back-end including its parent and other needed properties. 

#### Actor Process Propagation using Reminders

When an actor receives a request from the enqueuer service to process a transaction, it does the following:
* Process this transaction by adjusting its own state using the transaction measures
* Locate its parent actor if a parent is available
* Send the same transaction to the parent to process it as well

Initially when I did this, the enqueuer service was waiting on the client to perform all these tasks which negatively impacted the app data flow. Then I figured out that the best way to handle this is to actually set an actor reminder in the process method which will trigger after the process method returns to the caller. When the reminder triggers, the actor processes the transaction and sends the same transaction to its own parent.

The use of reminder is quite handy to provide this asynchronous behavior and allows the actor to return immediately to its caller therefore enhancing the app throughput. Reminders are different than timers in the sense that reminders keep actors in memory until they are processed. In other words, to garbage collect, reminders must be removed. 

In addition, reminders allow actors to serialize an object or parameter in them. Once they are fired, the actor can de-serialize the object or parameter and make use of it. In our case, I am serializing the transaction object. This is why the transaction must be declared as `Serializable`:

```csharp
[Serializable]
[DataContract]
public class EntityTransaction
{
    [DataMember]
    public DateTime TransactionDate { get; set; }
    [DataMember]
    public TransactionTypes TransactionType { get; set; }
    [DataMember]
    public EntityTypes EntityType { get; set; }
    [DataMember]
    public int BusinessKey { get; set; }
    [DataMember]
    public int SoldItems { get; set; }
    [DataMember]
    public double Revenue { get; set; }
    [DataMember]
    public double Tax { get; set; }
    [DataMember]
    public double Shipping { get; set; }
}
```

#### Actor Events

Actors can emit events in certain conditions. This can be very useful as a mean to provide a simple sub/pub communication mechanism between actors and its clients. The documentation warns, however, that this communication mechanism be only between the actors and its clients ...not among actors.

Athough I did implemet an event interface in the entity actor so they can notify the enqueuer service when a process is completed, I really did not make use of it yet. This can be quite nice because it allows for a centralized place to process the completed transactions.
  
#### Coordinator Actor

Initially I had a coordinator actor in the sample app in addition to the entity actor. The purpose was to provide a collection of entities for the entity actor. So initially the entity actor would consult the coordinator actor to retrieve its own entity and other related functions. I realized later on that the singleton actor will be a bottleneck if all actors are to communicate with it all the time. So since then, I removed the coordinator actor and provided `IOLTPconnectorService` that connects with the back-end to retrieve the entities. To make it perform better in real apps, there is a need to add a cache layer to this connector to it can provide optimal response time.

#### Logging and Externalization of events

As briefly described above, the app has a simple interface for logging and externalization: `ILoggerService`. The interface abstracts the actual implementations as in the provider model. In order to provide a complete logging and externalization scenario, the `LoggerService` acts as a `EventSource` where it emits logging events whenever a log request arrives. In other words, the `ILoggerService` interface inherits from `ILogMessageEventSource` which is, as described, an event source for logging.

Here is how the logging and health sub-system looks like:

![Logging and Health Reporting](http://i.imgur.com/ndY9bqs.png) 

Whenever a log message arrives at the logger provider, it emits a logging event that is listened to by the available loggers i.e. ETW, Table Storage and possibly others. The actual loggers then perform what they need to do to store the log data into their stores. In the case of table storage, the table storage logger inserts into Azure directly. 

This logging mechanism provides a really good way to decouple logging from the application and allows the app to support multiple loggers seamlessly. 

Because events can be as a source of leaked memory, every caution is taken to unsubscribe from the listener as soon as logging in no longer needed. Also the listener implements the .NET 4.5 Weak reference listener that makes it safer and easier to deal with leaked event data due to strong references.

##### Profile Handler

The sample app uses a profile handler that dictates a certain structure of how to write log-friendly methods. Here is a sample of how the app methods are written to be log-friendly:

![Log Friendly Method](http://i.imgur.com/2QCprLK.png)

Notes:
- The `LOG_DATA` denotes the source module: service, controller, etc. 
- At the beginning of every method, a new handler is instantiated with a reference to a `ISettingService` instance. 
- The handler `Start` method starts a log sequence that belongs to this module and method by creating a correlation id to group all the log entries from a specific tag and method.
- The handler `info` or `error` can be called several times in the body of the method to indicate normal or abnormal conditions.
- At the method `finally` clause, the handler `Stop` is called either with an error or without an error. It is the handler `Stop` that denotes the duration by calculating the time it took from the `Start` to `Stop`  
- In the above example, correlation id `1` has 3 entries for `Enqueuer` and `SomeMethod` while correlation id `2` has 4 entries for `Actor` and `OtherMethod`. In addition, correlation id `2` shows that `OtherMethod` stopped abnormally i.e. with an error. 

I think the way of logging makes it really easy to follow the logs, understand them and perhaps visualize them. Here is a rudimentary example of how this log data can be visualized using PowerBI:

![Logs Visualization in PowerBI](http://i.imgur.com/Sd2ETat.png)

To view the above visualization interactively in a browser, please click the link below:

[Entity Calculators Log Data Simple Visualization](https://app.powerbi.com/view?r=eyJrIjoiZTdmOWE5M2ItMmM0My00MWE4LTlmZDQtOWEzNzBmZDJmYTgzIiwidCI6IjkxYmU4MWE0LWY5MGUtNGIxMy1hYTBlLWU3MWQyYmYzMDBiZSIsImMiOjZ9)
 
#### Configuration

As briefly described above, the app has `ISettingService` to provide an abstraction over the app configuration. In order to retrieve configuration items from the service settings, a SF interface `ICodePackageActivationContext` is needed. This means that the setting service must have a reliance on this interface which is implemented by SF service. Because configuration in SF is service bound, this arrangement seems to be necessary. 

BTW the API Gateway which is a pure ASP.NET project, I implement `ISettingService` in `ApiSettingService` which uses the normal `ConfigurationManager` to access the app settings.

The SF configuration is done via the `Settings.xml` of each service under `PackageRoot\Config`. Here is a sample:

```
<!-- This is used to provide application-level configuration settings -->
<Section Name="ServiceRunTimeConfig">
	<Parameter Name="InstrumentationKey" Value="" />
	<Parameter Name="OltpConnector" Value="" />
	<Parameter Name="OltpConnectionString" Value="" />
	<Parameter Name="LogsStorageConnectionString" Value="" />
	<Parameter Name="LogsStorageTableName" Value="" />
	<Parameter Name="IsEtwLogging" Value="" />
	<Parameter Name="IsAzureTableStorageLogging" Value="" />
</Section>
```  

In this case, we have several configuration items that we can override in Local and Cloud deployments:

* InstrumentationKey - App Insights Key if any
* OltpOnnector - Fake or Real
* OltpConnectionString - the OLTP database from which we must get he entities in real app
* LogsStorageConnectionString - the Azure storage account to which we send log data
* LogsStorageTableName - the Azure storage table name where we place the log entries. In our case, I have `devlogs` and `prodlogs` for local and Cloud deployment respectively.
* IsEtwLogging - if enabled, it will activate the ETW logging. Please refer to the [#logging-and-externalization-of-events](Logging and Externalization of events) for more information.
* IsAzureTableStorageLogging - if enabled, it will activate the Table Storage logging. Please refer to the [#logging-and-externalization-of-events](Logging and Externalization of events) for more information.

#### PowerShell to test endpoints

I created a PowerShell script to the app endpoints and to allow me to generate transaction load against the app. 
The script is located in the solution.

```
The Powershell Cmdlet that is useful is the one that generates load against the endpoint:

```
Generate-EntityTransactions -baseUrl http://localhost:8146 -iterations 100
```

The above says to target the app located at the base url and generate 100 random transactions.

