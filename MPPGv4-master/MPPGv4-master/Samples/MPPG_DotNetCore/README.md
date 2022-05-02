#Introduction 
The Repository Contains Demo Application For MPPG Web Service Operations includes the below operations

1. ProcessCardSwipe
2. ProcessData
3. ProcessKeyPadEntry
4. ProcessManualEntry
5. ProcessToken

# Clone the repository
 1. Navigate to the main page of the  **repository**. 
 2. Under the  **repository**  name, click  **Clone** .
 3. Use any Git Client(eg.: GitBash, Git Hub for windows, Source tree ) to  **clone**  the  **repository**  using HTTPS.

Note: reference for  [Cloning a Github Repository](https://help.github.com/en/articles/cloning-a-repository)

# Getting Started

1.  Install .net core 2.2

    - Demo app requires dotnet core 2.2 is installed

2.  Software dependencies( The Following nuget packages are automatically installed when we open and run the project), please recheck and add the references from nuget
 

     Microsoft.Extensions.DependencyInjection

     Microsoft.Extensions.Configuration

     Microsoft.Extensions.Configuration.EnvironmentVariables

     Microsoft.Extensions.Configuration.Json
     
     Microsoft.Extensions.Configuration.Binder

   
3.	Latest releases

    - Initial release with all commits and changes as on October 31st 2019

# Build and Test

 Steps to Build and run MPPGv4.DemoApp project ( .net core 2.2)

 From the cmd,  Navigate to the cloned folder and go to MPPGv4DemoApps
    
 Run the following commands
    
 ```dotnet clean MPPGv4.DemoApps.sln```

 ```dotnet build MPPGv4.DemoApps.sln```

 Navigate from command prompt to MPPGv4.DemoApp folder containing MPPGv4.DemoApp.csproj and run below command

 ```dotnet run --project MPPGv4.DemoApp.csproj```

 This should open the application running in console.

