




# Overview
[![NuGet](https://img.shields.io/badge/myget-v1.0.0-blue.svg)](https://www.myget.org/feed/bwrx/package/nuget/Bwrx.Api)

High-throughput data publisher designed to transmit data from your application to [Google Cloud Pub/Sub](https://cloud.google.com/pubsub/docs/). Data is transmitted asynchronously from an in-memory cache, resulting in evenly-balanced throughput, and low resource overhead
## Installation
Install the `Bwrx` NuGet package
`install Bwrx.Api`
### Authentication
A `CloudServiceCredentials` instance is necessary to establish a persistent connection with the Botworks Cloud. Authentication meta is stored in JSON format
```json
{
	"Type": "service_account",
	"project_id": "{GCP Project ID}",
	"private_key_id": "{Your private key ID}",
	"private_key": "{Your private key}",
	"client_email": "{Your GCP custom email address}",
	"client_id": "{Your GCP client ID}",
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
Note, this examples assumes that the `GcpServiceCredentials` JSON values are stored in a local `Resource` file, although this is not recommended for live deployments
### Configuration
A `ClientConfigSettings` instance is necessary to configure the streaming component. Configuration meta is also stored in JSON format 
```json
{
	"ProjectId": "{GCP Project ID}",
	"TopicId": "{GCP Topic ID (Cloud Pub/Sub)}",
}
```
#### Configuration Explained
##### ProjectId
> The Google Cloud Project to which the streaming component will connect
##### TopicID
> The Google Cloud Pub/Sub Topic to which the streaming component will transmit data
Deserialize the configuration settings
```csharp
var clientConfigSettings = JsonConvert.DeserializeObject<ClientConfigSettings>(ClientConfigSettings);
```
Note, this examples assumes that the `ClientConfigSettings` JSON values are stored in a local `Resource` file, although this is not recommended for live deployments

### Initialisation
Starting the `Agent` establishes a background process that publishes data to Google Cloud Pub/Sub
```csharp
Agent.Instance.Start(cloudServiceCredentials, clientConfigSettings);
```
## Usage
### Caching Events
An event is a generic data model that contains metadata relevant to your application, e.g., a `Flight Search`. These events are cached in memory, and uploaded in batches at regular intervals. Add events as they are created in your API, using the `EventMetaCache` Singleton class
```csharp
dynamic payload = new ExpandoObject();
payload.Name = "PAYLOAD";

var headers = Agent.ParseHttpHeaders(Request.Headers);

EventMetaCache.Instance.Add(
	payload,
	"flight-search",
	Request.QueryString.Value,
	headers);
```
### Error Handling
Errors are handled implicitly, so that the your application process flow is not interrupted. You can subscribe to any thrown error
##### `Agent.Instance.AddEventMetaFailed`
> An event could not be added to the cache
##### `Agent.Instance.InitialisationFailed`
> The `Agent` could not start
##### `Agent.Instance.TransmissionFailed`
> A batch of events could not be transmitted to Google Cloud Pub/Sub
##### `Agent.Instance.GetEventMetadataPayloadBatchFailed`
> Cached events could not be removed from cache during batch-upload
##### `Agent.Instance.ClearCacheFailed`
> The event cache could not be cleared explicitly
##### `Agent.Instance.EventMetadataUploadJobExecutionFailed`
> The batch-upload background task occurrence did not execute successfully
### Subscribing to Notifications
Your application can subscribe to any data-streaming operation
##### Agent.Instance.DataTransmitted
> A batch of events has just been transmitted to Google Cloud Pub/Sub
> ###### Parameters
> `numItemsTransmitted`, *int*
	> The number of items that have been transmitted
##### Agent.Instance.EventMetaAdded
> An event has just been added to the cache
> ###### Parameters
> `eventMeta`, *object* 
	> The event added to the cache 
##### Agent.Instance.GotEventMetadataPayloadBatch
> A collection of events has just been removed from the cache, and batched for upload
> ###### Parameters
> `numItemsReturned`, *int*
	> The number of events removed from the cache, and batched for upload
>	
> `numEventsCached`, *int*
	> The number of events remaining in the cache