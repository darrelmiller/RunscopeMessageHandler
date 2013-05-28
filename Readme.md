Runscope Message Handler
========================

Runscope Message Handler for use with System.Net.Http.HttpClient

- Requires a free Runscope account, [sign up here](https://www.runscope.com/signup)
- Automatically create Runscope URLs for your requests
- Portable library, works with .net 4.5, Windows Phone 7.5 and 8, and Windows Store Apps.  

### Installation

    PM> Install-Package Runscope.Contrib -Pre   


### Example

    using System.Net.Http;
    using Runscope.Contrib;
    
    var runscopeHandler = new RunscopeMessageHandler("bucketKey", new HttpClientHandler());
 
    var httpClient = new HttpClient(runscopeHandler);
 
    var response = await httpClient.GetAsync("https://api.github.com");
