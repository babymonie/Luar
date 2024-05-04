# Luar: Simplifying Lua Runtime

Luar is a Lua runtime environment designed to provide functionality similar to Node.js but with an easier learning curve. With Luar, you can harness the power of Lua for various tasks, from accessing your filesystem to creating HTTP servers and beyond. 

## Key Features:

### 1. Filesystem Access:
Easily manipulate files and directories with built-in filesystem access functions.

### 2. HTTP Server:
Quickly set up HTTP servers for web applications or APIs using Luar's intuitive APIs.

### 3. Process Interaction:
Access and manipulate the current process environment with ease, enabling seamless integration with other system components.

### 4. JSON Class:
Effortlessly work with JSON data using Luar's dedicated JSON class, simplifying data exchange and manipulation.

### 5. Convert Class:
Convert between different data types and formats effortlessly with Luar's versatile Convert class.

### 6. Virtual Keyboard and Mouse Key (DirectX Compatible):
Integrate virtual keyboard and mouse key functionalities directly into your Lua applications, providing compatibility with DirectX for enhanced gaming experiences and beyond.
### 7. Requests:
Easily make HTTP requests to fetch data from the internet using Luar's built-in Requests module. Retrieve web pages, consume APIs, and more with simple and intuitive API calls.

## Built on C#, Compatible Everywhere:

Luar is built on C#, ensuring robust performance and flexibility. However, don't fret about platform compatibility! Luar is designed to work seamlessly on Linux, macOS, and Windows environments, providing a consistent experience across different operating systems.

### Creating a Luar Extension:

1. **Implement the Interface:**
   Create a class that implements the `IRuntimeExtension` interface. This interface defines a method named `RegisterClassesAndFunctions`, where you can register your custom classes and functions with the Lua runtime.

   ```csharp
   using LuaRuntime;

   public class CustomExtension : IRuntimeExtension
   {
       public void RegisterClassesAndFunctions(RuntimeManager runtimeManager)
       {
           // Register custom classes and functions here
           runtimeManager.RegisterClass(new CustomClass(), "CustomClass");
           runtimeManager.RegisterFunction("your function name that lua will call with ", new Action(TheMethodThatYouHave));
       }
   }
   ```

2. **Define Your Custom Class:**
   Define your custom class with the methods you want to expose to Lua. Make sure your methods are not asynchronous or static.

   ```csharp
   public class CustomClass
   {
       // Your methods
   }
   ```

3. **Build Your Extension into a DLL:**
   Build your project into a DLL. Ensure that it's built with .NET Core and not the Windows-only version.

4. **Usage:**
   To use your extension in a Luar project, place the DLL in the libs folder where you have Luar installed at. Then, in your Lua script, you can import and use the classes and functions defined in your extension.

#### Installing a Luar Extension:
1. **Specify Repository:**
   To install your Luar package, users should use the Luar package manager and specify the repository where your package is hosted. They can do this by running the following command:
   ```
   luar install username/repoTitle
   ```
   Replace `username/repoTitle` with the GitHub username and repository name where your Luar package is located.

2. **Confirm Installation:**
   After running the installation command, users should see a confirmation message indicating that your Luar package has been successfully installed. They can then proceed to use it in their Lua projects.

3. **Manual Download (if necessary):**
   If the package manager encounters any issues or if the dependencies are not included in the installation process, users may need to manually download the DLL and dependencies files from your GitHub repository and include them in their project.

### Providing Your Luar Package:

1. **Create a GitHub Repository:**
   Start by creating a GitHub repository for your Luar package. Make sure the repository name matches the name of your Luar package.

2. **Upload Dependencies and DLL:**
   Upload the dependencies file (deps) and the DLL file of your Luar extension to the main folder of the repository. Ensure that there is only one DLL file, and it's the main DLL required for your extension.

3. **Write Documentation:**
   Include clear and concise documentation in your repository's README file. Provide instructions on how users can install and use your Luar package, including any dependencies they need to be aware of.

### Making Your Package Installable:

1. **Inform Users:**
   In your documentation, inform users that they can install your Luar package using the Luar package manager. Explain the steps they need to follow to install it from your GitHub repository.

2. **Provide Repository Link:**
   Share the link to your GitHub repository with users who want to install your Luar package. They'll need this link to specify the repository when installing the package.

3. **Manual Download:**
   Since Luar's package manager is not as robust as others, users may need to manually download the DLL and dependencies files from your repository if they're not included in the installation process.

With these steps, you can create custom extensions for Luar, package them into DLLs, and easily install or uninstall them in your Luar projects.
## Why Luar?

Luar aims to make Lua a compelling alternative to Node.js by providing a streamlined runtime environment with a focus on simplicity and ease of use. Whether you're a seasoned Lua developer or just starting out, Luar empowers you to build powerful applications with confidence.

Then, dive into our comprehensive documentation to explore all the features Luar has to offer and start building your next project with Lua!

## Contributing:

Luar is an open-source project, and we welcome contributions from the community. Whether you want to report a bug, suggest a feature, or submit a pull request, we'd love to hear from you! Check out our GitHub repository to get involved.


## Let's Build Something Amazing:

With Luar, the possibilities are endless. Join us in unlocking the full potential of Lua and revolutionizing the way you build applications. Happy coding! 🚀

# Check Out Example Lua Files:

To help you get started with Luar, we've provided several example Lua files showcasing its key features and functionalities. These examples cover various use cases, from basic filesystem operations to setting up HTTP servers and working with JSON data. Feel free to explore these examples to gain a better understanding of how Luar can streamline your development process.

### 1. FileSystemExample.lua:
This example demonstrates how to perform common filesystem operations using Luar's built-in filesystem access functions. Learn how to create, read, write, and delete files and directories effortlessly.

### 2. server.lua:
In this example, you'll discover how easy it is to set up an HTTP server for your web applications or APIs using Luar's intuitive APIs. Dive into the code to see how to handle HTTP requests and responses efficiently.

### 3. ProcessInteractionExample.lua:
Explore this example to learn how to access and manipulate the current process environment using Luar. Discover how to seamlessly integrate with other system components for enhanced functionality.

### 4. JsonExample.lua:
Work with JSON data effortlessly by exploring this example. Learn how to use Luar's dedicated JSON class to simplify data exchange and manipulation tasks.

### 5. ConvertExample.lua:
This example demonstrates how to convert between different data types and formats seamlessly using Luar's versatile Convert class. Explore the code to see how easy it is to perform data conversions in your Lua applications.

### 6. DirectXIntegrationExample.lua:
Integrate virtual keyboard and mouse key functionalities directly into your Lua applications with this example. Learn how Luar provides compatibility with DirectX for enhanced gaming experiences and beyond.

### 7. Requests.lua:
Easily make HTTP requests to fetch data from the internet using Luar's built-in Requests module. Retrieve web pages, consume APIs, and more with simple and intuitive API calls.

### 8. Custom Extensions: Users can extend Luar's functionality by registering custom classes and functions, providing flexibility to tailor the runtime environment to specific needs.
### 9. Package Installer/Uninstaller: Luar now supports package installation and uninstallation, simplifying the process of adding external functionality to Lua projects.
### 9. Socket Support: Addition of socket support enhances Luar's networking capabilities, enabling users to implement various network communication tasks.

Feel free to dive into these examples, experiment with the code, and incorporate them into your projects. If you have any questions or need further assistance, don't hesitate to reach out to the Luar community or refer to our comprehensive documentation. Happy coding! 🚀


We have include a sample Luar Library to add on Luar
