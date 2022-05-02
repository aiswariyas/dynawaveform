#Introduction 

The Repository Contains the Demo App application of MPPGv4 webservice with the following operations

    1. ProcessCardSwipe
    2. ProcessData
    3. ProcessKeyManualEntry
    4. ProcessKeyPadEntry
    5. ProcessToken

# Clone the repository
 1. Navigate to the main page of the  **repository**. 
 2. Under the  **repository**  name, click  **Clone** .
 3. Use any Git Client(eg.: GitBash, Git Hub for windows, Source tree ) to  **clone**  the  **repository**  using HTTPS.

***Note***: reference for  [Cloning a Github Repository](https://help.github.com/en/articles/cloning-a-repository)

# Getting Started  

-   Install Latest version of Java 1.8.
-   Install latest version of apache netbeans. 
-   To view the source code, open the projects from apache netbeans.
   
###Latest releases

-   Initial release with all commits and changes as on November 12th 2019.

# Build and Test
 
 - To build and run From Netbeans IDE, build the ***MppgV4DemoApp*** project , and run the MppgV4DemoApp.
 - To build and run from console
  - Make Sure Java and Ant is in your  PATH

    ``ant -f "<<Path Of Cloned Folder>>\MppgV4DemoApp"  -Dnb.internal.action.name=clean clean``

    ``ant -f "<<Path Of Cloned Folder>>\MppgV4DemoApp"  -Dnb.internal.action.name=rebuild clean jar``

    ``java -cp "<<Path Of Cloned Folder>>\MppgV4DemoApp\dist\MppgV4DemoApp.jar" MppgV4DemoApp.MppgV4DemoApp``
