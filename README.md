




# Overview
[![NuGet](https://img.shields.io/badge/nuget-v1.0.0-blue.svg)](https://www.nuget.org/packages/Bwrx.Api/1.0.0)
High-throughput, low-overhead API designed to monitor and prevent bad actors from accessing your application
## Installation
Install the Bwrx NuGet package
`install Bwrx.Api`
### Authentication
A `CloudServiceCredentials` instance is necessary to establish a persistent connection with the Botworks Cloud. Authentication meta is stored in JSON format
```json
{
	"Type": "service_account",
	"project_id": "{Project ID}",
	"private_key_id": "{Your private key ID}",
	"private_key": "{Your private key}",
	"client_email": "{Your custom email address}",
	"client_id": "{Your client ID}",
	"auth_uri": "https://accounts.google.com/o/oauth2/auth",
	"token_uri": "https://oauth2.googleapis.com/token",
	"auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
	"client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/deploy%40eshop-puddle.iam.gserviceaccount.com"
}
```
Deserialize the credentials
```csharp
var cloudServiceCredentials = JsonConvert.DeserializeObject<CloudServiceCredentials>(Resources.CloudServiceCredentials);
```
Note, this examples assumes that the `GcpServiceCredentials` JSON values are stored in a local `Resource` file
### Configuration
A `ClientConfigSettings` instance is necessary to configure the Botworks API. Configuration meta is stored in JSON format 
```json
{
	"ProjectId": "{Project ID}",
	"PublisherTopicId": "{Publisher Topic ID}",
	"BlockingHttpStatusCode": {HTTP status code retured when blocking a HTTP request},
	"IpAddressHeaderName": "{Default IP address HTTP header name}"
}
```
Deserialize the configuration settings
```csharp
var clientConfigSettings = JsonConvert.DeserializeObject<ClientConfigSettings>(Resource.ClientConfigSettings);
```
Note, this examples assumes that the `ClientConfigSettings` JSON values are stored in a local `Resource` file

### Initialisation
Starting the `Agent` establishes a background process that publishes data to Botworks Cloud
```csharp
Agent.Instance.Start(cloudServiceCredentials, clientConfigSettings);
```
## Usage
### Monitoring Endpoints
An event is a generic data model that contains metadata relevant to your application, e.g., a `Flight Search`. These events are cached in memory, and uploaded in batches at regular intervals. Add events as they occur in your application using the `MonitorAttribute` class
```csharp
[Monitor("{Endpoint name - e.g., flight-search}")]
public IEnumerable<string> Get()
{            
    // Your code
}
```
### Blocking Bots
Add the `BlockingDelegatingHandler` to the `Register` method in your app's `WebApiConfig` class
```csharp
var clientConfigSettings =
    JsonConvert.DeserializeObject<ClientConfigSettings>(Resources.ClientConfigSettings);

var bwrxDelegatingHandler = new BlockingDelegatingHandler(
    clientConfigSettings.IpAddressHeaderName,
    clientConfigSettings.BlockingHttpStatusCode);
```
### Error Handling
Errors are handled implicitly, so that the your application process flow is not interrupted. You can subscribe to any error thrown
##### `Agent.Instance.AddEventMetaFailed`
> An event could not be added to the cache
> ###### Parameters
> `Exception`, *Exception*
	> The `Exception` instance that raised the event
##### `Agent.Instance.BlacklistAddIpAddressFailed`
> An IP address could not be added to the blacklist
> ###### Parameters 
> `IpAddress`, *IPAddress*
	> The `IPAddress` that could not be added to the blacklist
> `Exception`, *Exception*
	> The `Exception` instance that raised the event
##### `Agent.Instance.BlacklistCouldNotParseIpAddress`
> The blacklist could not parse an IP address
> ###### Parameters
> `IpAddress`, *string*
	> The text that could not be parsed to an `IPAddress` instance
> `Exception`, *Exception*
	> The `Exception` instance that raised the event
##### `Agent.Instance.BlacklistGetLatestListFailed`
> The most up-to-date blacklist could not be retrieved
> ###### Parameters
> `Exception`, *Exception*
	> The `Exception` instance that raised the event 
##### `Agent.Instance.ClearCacheFailed`
> The cache could not be cleared manually
> ###### Parameters
> `Exception`, *Exception*
	> The `Exception` instance that raised the event 
##### `Agent.Instance.CloudDatabaseConnectionFailed`
> The Cloud database connection could not be established
> ###### Parameters
> `Exception`, *Exception*
	> The `Exception` instance that raised the event 
##### `Agent.Instance.CouldNotGetIpAddressHttpHeaderValues`
> HTTP header values that contain IP address metadata could not be retrieved from the current HTTP request context	
> ###### Parameters
> `IPAddressHeaderName`, *string*
	> The name of the HTTP header that contains IP address metadata
> `Exception`, *Exception*
	> The `Exception` instance that raised the event 
##### `Agent.Instance.CouldNotParseIpAddressHttpHeaderValues`
> HTTP header values containing IP address metadata could not be parsed
> ###### Parameters
> `IpAddressHttpHeaderValues`, *IEnumerable[string]*
	> The HTTP header values containing IP address metadata
> `Exception`, *Exception*
	> The `Exception` instance that raised the event
##### `Agent.Instance.EventMetadataPublishJobExecutionFailed`
> Events could not be published to the Botworks Cloud
> ###### Parameters
> `Exception`, *Exception*
	> The `Exception` instance that raised the event
##### `Agent.Instance.GetWhitelistJobExecutionFailed`
> Unable to download the whitelist
> ###### Parameters
> `Exception`, *Exception*
	> The `Exception` instance that raised the event
##### `Agent.Instance.GetBlacklistJobExecutionFailed`
> Unable to download the blacklist
> ###### Parameters
> `Exception`, *Exception*
	> The `Exception` instance that raised the event
##### `Agent.Instance.GetEventMetadataPayloadBatchFailed`
> Unable to prepare the event batch for publishing to the Botworks cloud
> ###### Parameters
> `NumEventsCached`, *int*
	> The number of events remaining in the cache
> `Exception`, *Exception*
	> The `Exception` instance that raised the event
##### `Agent.Instance.JobSchedulerStartFailed`
> The job scheduler service could not be started
> ###### Parameters
> `Exception`, *Exception*
	> The `Exception` instance that raised the event
##### `Agent.Instance.InitialisationFailed`
> The event transmission service could not be started
> ###### Parameters
> `Exception`, *Exception*
	> The `Exception` instance that raised the event
##### `Agent.Instance.TransmissionFailed`
> An event-batch could not be published to the Botworks cloud
> ###### Parameters
> `Exception`, *Exception*
	> The `Exception` instance that raised the event
##### `Agent.Instance.WhitelistAddIpAddressFailed`
> An IP address could not be added to the whitelist
> ###### Parameters
> `IpAddress`, *IPAddress*
	> The `IPAddress` that could not be added to the whitelist
> `Exception`, *Exception*
	> The `Exception` instance that raised the event
##### `Agent.Instance.WhitelistCouldNotParseIpAddress`
> The whitelist could not parse an IP address
> ###### Parameters
> `IpAddress`, *string*
	> The text that could not be parsed to an `IPAddress` instance
> `Exception`, *Exception*
	> The `Exception` instance that raised the event
##### `Agent.Instance.WhitelistGetLatestListFailed`
> The most up-to-date whitelist could not be retrieved
> ###### Parameters
> `Exception`, *Exception*
	> The `Exception` instance that raised the event
##### `BlockingDelegatingHandler.CouldNotGetIpAddressHttpHeaderValues`
> HTTP header values that contain IP address metadata could not be retrieved from the current HTTP request context	
> ###### Parameters
> `IPAddressHeaderName`, *string*
	> The name of the HTTP header that contains IP address metadata
> `Exception`, *Exception*
	> The `Exception` instance that raised the event 
##### `BlockingDelegatingHandler.BlacklistCouldNotParseIpAddress`
> The `BlockingDelegatingHandler` could not parse an IP address
> ###### Parameters
> `IpAddress`, *string*
	> The text that could not be parsed to an `IPAddress` instance
> `Exception`, *Exception*
	> The `Exception` instance that raised the event
### Subscribing to Notifications
Your application can subscribe to any successful operation
##### `BlockingDelegatingHandler.BlacklistedIpAddressDetected`
> A command issued from blacklisted IP address(es) has attempted to access your application
> ###### Parameters
> `IPAddresses`, *IEnumerable[IPAddress]*
	> The IP address(es) from which the command was issued
> `PassiveMode`, *boolean*
	> Indicates whether or not the Botworks API is operating in passive mode (tracks, but does not block blacklisted IP addresses; defaults to *false*)
##### `Agent.Instance.BlacklistGotLatestList`
> The latest blacklist has been downloaded
> ###### Parameters
> `NumIpAddresses`, *int*
	> The number of IP addresses in the latest blacklist
##### `Agent.Instance.BlacklistIpAddressAdded`
> An IP address has been added to the blacklist
> ###### Parameters
> `IPAddress`, *IPAddress*
	> The IP address that has been added to the blacklist
##### `Agent.Instance.BlacklistListUpdated`
> The blacklist has been updated to the latest version
##### `Agent.Instance.DataTransmitted`
> A batch of events has been transmitted to the Botworks Cloud
> ###### Parameters
> `NumItemsTransmitted`, *int*
	> The number of events transmitted in the batch
##### `Agent.Instance.EventMetaAdded`
> An event has been added to the cache
> ###### Parameters
> `EventMeta`, *object*
	> The event that has been added to the cache
##### `Agent.Instance.GotEventMetadataPayloadBatch`
> A batch of events has been removed from the cache
> ###### Parameters
> `NumItemsReturned`, *int*
	> The number of items removed from the cache
> `NumEventsCached`, *int*
	> The number of items remaining in the cache
##### `Agent.Instance.WhitelistGotLatestList`
> The latest whitelist has been downloaded
> ###### Parameters
> `NumIpAddresses`, *int*
	> The number of IP addresses in the latest whitelist
##### `Agent.Instance.WhitelistIpAddressAdded`
> An IP address has been added to the whitelist
> ###### Parameters
> `IPAddress`, *IPAddress*
	> The IP address that has been added to the whitelist
##### `Agent.Instance.WhitelistListUpdated`
> The whitelist has been updated to the latest version