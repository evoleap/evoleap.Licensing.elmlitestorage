# elmlite Storage Reference Implementation

This is the elmlite storage reference implementation that will store strings for a given object type and ID (GUID).

The reference implementation stores in flat files. Values stored are unencrypted are are named based on the object type and ID.

## Running the reference implementation

The reference implementation will run on Windows, Linux, or any platform supported by .NET 8. Download and install the [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) to run the code.

To run the reference implementation, clone this repo from GitHub and do the following:

```
~$ cd elmlitestorage/src/elmstoragerefimpl
~/elmlitestorage/src/elmstoragerefimpl$ dotnet run -c debug -- --Urls "http://*:5000"
```

This will build and run the server on port 5000. You can connect using [http://localhost:5000/]

## API Documentation

You code will need to implement the API specification for this reference implementation. To get the API spec, go to 
http://localhost:5000/swagger. The Open API JSON can be downloaded from http://localhost:5000/swagger/v1/swagger.json.

There are tools that can be used to generate the API code in various languages. See  [Swagger API Generator](https://github.com/swagger-api/swagger-codegen#to-generate-a-sample-client-library)
