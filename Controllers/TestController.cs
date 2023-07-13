using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.SemanticFunctions;

namespace SemanticKernelDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        public TestController()
        {
        }

        private KernelBuilder GetBuilder()
        {
            string deploymentID = "DataExplorerQA1";
            string endpoint = "https://itsda-dataexplorer-qa-ai.openai.azure.com/";
            string key = "b64c670077be430185beb3e2017b2440";
            string key2 = "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";

            var builder = new KernelBuilder();

            //builder.WithAzureTextCompletionService(
            //        deploymentID,                  // Azure OpenAI Deployment Name
            //         endpoint, // Azure OpenAI Endpoint
            //        key);      // Azure OpenAI Key

            //// Alternative using OpenAI
            builder.WithOpenAITextCompletionService(
                     "text-davinci-003",               // OpenAI Model name
                     key2);     // OpenAI API Key

            return builder;
        }

        [HttpGet]
        [Route("test1")]
        public IActionResult Test1()
        {
            return Ok("Test1 is working");
        }

        [HttpGet]
        [Route("test2")]
        public async Task<IActionResult> Test2()
        {
            //var builder = new KernelBuilder();

            //builder.WithAzureTextCompletionService(
            //        deploymentID,                  // Azure OpenAI Deployment Name
            //         endpoint, // Azure OpenAI Endpoint
            //        key);      // Azure OpenAI Key

            //// Alternative using OpenAI
            ////builder.WithOpenAITextCompletionService(
            ////         "text-davinci-003",               // OpenAI Model name
            ////         "...your OpenAI API Key...");     // OpenAI API Key

            //var kernel = builder.Build();
            var builder = GetBuilder();
            var kernel = builder.Build();

            var prompt = @"{{$input}} One line TLDR with the fewest words.";

            var summarize = kernel.CreateSemanticFunction(prompt);

            string text1 = @"
                            1st Law of Thermodynamics - Energy cannot be created or destroyed.
                            2nd Law of Thermodynamics - For a spontaneous process, the entropy of the universe increases.
                            3rd Law of Thermodynamics - A perfect crystal at zero Kelvin has zero entropy.";

            string text2 = @"
                            1. An object at rest remains at rest, and an object in motion remains in motion at constant speed and in a straight line unless acted on by an unbalanced force.
                            2. The acceleration of an object depends on the mass of the object and the amount of force applied.
                            3. Whenever one object exerts a force on another object, the second object exerts an equal and opposite on the first.";

            Console.WriteLine(await summarize.InvokeAsync(text1));

            Console.WriteLine(await summarize.InvokeAsync(text2));

            ResultResponse result = new ResultResponse() { Text1 = text1, Text2 = text2, };
            return Ok(result);
            // Output:
            //   Energy conserved, entropy increases, zero entropy at 0K.
            //   Objects move in response to forces.
        }

        [HttpGet]
        [Route("test3")]
        public async Task<IActionResult> Test3()
        {
            //var builder = new KernelBuilder();

            //builder.WithAzureTextCompletionService(
            //        deploymentID,                  // Azure OpenAI Deployment Name
            //         endpoint, // Azure OpenAI Endpoint
            //        key);      // Azure OpenAI Key
            //var kernel = builder.Build();
            var builder = GetBuilder();
            var kernel = builder.Build();

            string translationPrompt = @"{{$input}} Translate the text to math.";

            string summarizePrompt = @"{{$input}} Give me a TLDR with the fewest words.";

            var translator = kernel.CreateSemanticFunction(translationPrompt);
            var summarize = kernel.CreateSemanticFunction(summarizePrompt);

            string inputText = @"
                                1st Law of Thermodynamics - Energy cannot be created or destroyed.
                                2nd Law of Thermodynamics - For a spontaneous process, the entropy of the universe increases.
                                3rd Law of Thermodynamics - A perfect crystal at zero Kelvin has zero entropy.";

            // Run two prompts in sequence (prompt chaining)
            var output = await kernel.RunAsync(inputText, translator, summarize);

            Console.WriteLine(output);

            return Ok(output.Result);
            // Output: ΔE = 0, ΔSuniv > 0, S = 0 at 0K.
        }

        [HttpGet]
        [Route("test4")]
        public async Task<IActionResult> Test4()
        {
            var builder = GetBuilder();
            var kernel = builder.Build();
            //            IKernel kernel = KernelBuilder.Create();
            //            kernel.Config.AddAzureTextCompletionService(
            //    deploymentID,                   // Azure OpenAI *Deployment ID*
            //    endpoint,    // Azure OpenAI *Endpoint*
            //    key,          // Azure OpenAI *Key*
            //    "Azure_curie"                           // alias used in the prompt templates' config.json
            //);

            string skPrompt = @"WRITE EXACTLY ONE JOKE or HUMOROUS STORY ABOUT THE TOPIC BELOW

                                JOKE MUST BE:
                                - G RATED
                                - WORKPLACE/FAMILY SAFE
                                NO SEXISM, RACISM OR OTHER BIAS/BIGOTRY

                                BE CREATIVE AND FUNNY. I WANT TO LAUGH.
                                +++++

                                {{$input}}
                                +++++

                                ";

            var promptConfig = new PromptTemplateConfig
            {
                Completion =
                    {
                        MaxTokens = 1000,
                        Temperature = 0.9,
                        TopP = 0.0,
                        PresencePenalty = 0.0,
                        FrequencyPenalty = 0.0,
                    }
            };

            var promptTemplate = new PromptTemplate(
                skPrompt,
                promptConfig,
                kernel
            );

            var functionConfig = new SemanticFunctionConfig(promptConfig, promptTemplate);
            var jokeFunction = kernel.RegisterSemanticFunction("FunSkill", "Joke", functionConfig);

            var result = await jokeFunction.InvokeAsync("time travel to dinosaur age");

            return Ok(result.Result);
        }


        //semantic-functions
        [HttpGet]
        [Route("test5")]
        public async Task<IActionResult> Test5()
        {
            //var builder = new KernelBuilder();

            //builder.WithAzureTextCompletionService(
            //        deploymentID,                  // Azure OpenAI Deployment Name
            //         endpoint, // Azure OpenAI Endpoint
            //        key);      // Azure OpenAI Key

            //// Alternative using OpenAI
            ////builder.WithOpenAITextCompletionService(
            ////         "text-davinci-003",               // OpenAI Model name
            ////         "...your OpenAI API Key...");     // OpenAI API Key

            //var kernel = builder.Build();

            var builder = GetBuilder();
            var kernel = builder.Build();

            //var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "path", "to", "your", "plugins", "folder");
            var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Plugins", "Sementic");

            // Import the OrchestratorPlugin from the plugins directory.
            var orchestratorPlugin = kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "OrchestratorPlugin");

            // Get the GetIntent function from the OrchestratorPlugin and run it
            var result = await orchestratorPlugin["GetIntent"]
             .InvokeAsync("I want to send an email to the marketing team celebrating their recent milestone.");

            if (!result.ErrorOccurred)
            {
                return Ok(result.Result);
            }
            return Ok("Error");
            // Output:
            //Send congratulatory email.
        }

        //semantic-functions
        [HttpGet]
        [Route("test6")]
        public async Task<IActionResult> Test6()
        {
            //var builder = new KernelBuilder();

            //builder.WithAzureTextCompletionService(
            //        deploymentID,                  // Azure OpenAI Deployment Name
            //         endpoint, // Azure OpenAI Endpoint
            //        key);      // Azure OpenAI Key

            //// Alternative using OpenAI
            ////builder.WithOpenAITextCompletionService(
            ////         "text-davinci-003",               // OpenAI Model name
            ////         "...your OpenAI API Key...");     // OpenAI API Key

            //var kernel = builder.Build();
            var builder = GetBuilder();
            var kernel = builder.Build();

            //var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "path", "to", "your", "plugins", "folder");
            var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Plugins", "Sementic");

            // Import the OrchestratorPlugin from the plugins directory.
            var orchestratorPlugin = kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "OrchestratorPlugin");
            var summarizationPlugin = kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "SummarizeSkill");

            // Create a new context and set the input, history, and options variables.
            var context = kernel.CreateNewContext();
            context["input"] = "Yes";
            context["history"] = @"Bot: How can I help you?
            User: My team just hit a major milestone and I would like to send them a message to congratulate them.
            Bot:Would you like to send an email?";
            context["options"] = "SendEmail, ReadEmail, SendMeeting, RsvpToMeeting, SendChat";

            // Run the GetIntent function with the context.
            var result = await orchestratorPlugin["GetIntent"].InvokeAsync(context);
            if (!result.ErrorOccurred)
            {
                return Ok(result.Result);
            }
            return Ok("Error");
            // Output:
            //Send congratulatory email.
        }

        //native-functions
        [HttpGet]
        [Route("test7")]
        public async Task<IActionResult> Test7()
        {
            //var builder = new KernelBuilder();

            //builder.WithAzureTextCompletionService(
            //        deploymentID,                  // Azure OpenAI Deployment Name
            //         endpoint, // Azure OpenAI Endpoint
            //        key);      // Azure OpenAI Key

            //// Alternative using OpenAI
            ////builder.WithOpenAITextCompletionService(
            ////         "text-davinci-003",               // OpenAI Model name
            ////         "...your OpenAI API Key...");     // OpenAI API Key

            //var kernel = builder.Build();
            var builder = GetBuilder();
            var kernel = builder.Build();
            var mathPlugin = kernel.ImportSkill(new MathPlugin(), "MathPlugin");

            // Run the Sqrt function
            var result1 = await mathPlugin["Sqrt"].InvokeAsync("64");
            Console.WriteLine(result1);

            // Run the Add function with multiple inputs
            var context = kernel.CreateNewContext();
            context["input"] = "3";
            context["number2"] = "7";
            var result = await mathPlugin["Add"].InvokeAsync(context);
            if (!result.ErrorOccurred)
            {
                return Ok(result.Result);
            }
            return Ok("Error");
            // Output:
            //10
        }

        //native-functions
        [HttpGet]
        [Route("test8")]
        public async Task<IActionResult> Test8()
        {
            //var builder = new KernelBuilder();

            //builder.WithAzureTextCompletionService(
            //        deploymentID,                  // Azure OpenAI Deployment Name
            //         endpoint, // Azure OpenAI Endpoint
            //        key);      // Azure OpenAI Key

            //// Alternative using OpenAI
            ////builder.WithOpenAITextCompletionService(
            ////         "text-davinci-003",               // OpenAI Model name
            ////         "...your OpenAI API Key...");     // OpenAI API Key

            //var kernel = builder.Build();
            var builder = GetBuilder();
            var kernel = builder.Build();
            var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Plugins", "Native");

            // Import the semantic functions
            kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "OrchestratorPlugin");
            kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "SummarizeSkill");

            // Import the native functions 
            var mathPlugin = kernel.ImportSkill(new MathPlugin(), "MathPlugin");
            var orchestratorPlugin = kernel.ImportSkill(new OrchestratorPlugin(kernel), "OrchestratorPlugin");

            // Make a request that runs the Sqrt function
            var result1 = await orchestratorPlugin["RouteRequest"].InvokeAsync("What is the square root of 634?");
            Console.WriteLine(result1);

            // Make a request that runs the Add function
            var result2 = await orchestratorPlugin["RouteRequest"].InvokeAsync("What is 42 plus 1513?");
            if (!result2.ErrorOccurred)
            {
                return Ok(result2.Result);
            }
            return Ok("Error");

            // Output:
            //10
        }

        //chaining-functions
        [HttpGet]
        [Route("test9")]
        public async Task<IActionResult> Test9()
        {
            //var builder = new KernelBuilder();

            //builder.WithAzureTextCompletionService(
            //        deploymentID,                  // Azure OpenAI Deployment Name
            //         endpoint, // Azure OpenAI Endpoint
            //        key);      // Azure OpenAI Key

            //// Alternative using OpenAI
            ////builder.WithOpenAITextCompletionService(
            ////         "text-davinci-003",               // OpenAI Model name
            ////         "...your OpenAI API Key...");     // OpenAI API Key

            //var kernel = builder.Build();
            var builder = GetBuilder();
            var kernel = builder.Build();

            var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Plugins", "Chaining");

            // Import the semantic functions
            kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "OrchestratorPlugin");
            kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "SummarizeSkill");

            // Import the native functions
            var mathPlugin = kernel.ImportSkill(new MathPlugin(), "MathPlugin");
            var orchestratorPlugin = kernel.ImportSkill(new OrchestratorPlugin(kernel), "OrchestratorPlugin");

            // Make a request that runs the Sqrt function
            var result1 = await orchestratorPlugin["RouteRequest2"]
                .InvokeAsync("What is the square root of 524?");
            Console.WriteLine(result1);

            // Make a request that runs the Add function
            var result2 = await orchestratorPlugin["RouteRequest"]
                .InvokeAsync("How many sheep would I have if I started with 3 and then got 7 more?");
            Console.WriteLine(result2);
            if (!result2.ErrorOccurred)
            {
                return Ok(result2.Result);
            }
            return Ok("Error");

            // Output:
        }


        //chaining-functions
        [HttpGet]
        [Route("test10")]
        public async Task<IActionResult> Test10()
        {
            var builder = GetBuilder();
            var kernel = builder.Build();

            string myJokePrompt = @"""Tell a short joke about {{$input}}.""";
            string myPoemPrompt = @"""Take this {{$input}} and convert it to a nursery rhyme.""";
            string myMenuPrompt = @"""Make this poem {{$input}} influence the three items in a coffee shop menu.The menu reads in enumerated form:""";

            var myJokeFunction = kernel.CreateSemanticFunction(myJokePrompt, maxTokens: 500);
            var myPoemFunction = kernel.CreateSemanticFunction(myPoemPrompt, maxTokens: 500);
            var myMenuFunction = kernel.CreateSemanticFunction(myMenuPrompt, maxTokens: 500);

            var myOutput = await kernel.RunAsync(new ContextVariables("Charlie Brown"),
                        myJokeFunction,
                        myPoemFunction,
                        myMenuFunction);

            Console.WriteLine(myOutput);

            if (!myOutput.ErrorOccurred)
            {
                return Ok(myOutput.Result);
            }
            return Ok("Error");

            // Output:
        }

        //Planner
        [HttpGet]
        [Route("test11")]
        public async Task<IActionResult> Test11()
        {
            //var builder = new KernelBuilder();

            //builder.WithAzureTextCompletionService(
            //        deploymentID,                  // Azure OpenAI Deployment Name
            //         endpoint, // Azure OpenAI Endpoint
            //        key);      // Azure OpenAI Key

            //// Alternative using OpenAI
            ////builder.WithOpenAITextCompletionService(
            ////         "text-davinci-003",               // OpenAI Model name
            ////         "...your OpenAI API Key...");     // OpenAI API Key

            //var kernel = builder.Build();
            var builder = GetBuilder();
            var kernel = builder.Build();

            var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Plugins", "Planner");
            // Import the semantic functions
            kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "OrchestratorPlugin");
            kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "SummarizeSkill");

            // Add the math plugin
            var mathPlugin = kernel.ImportSkill(new MathPlugin(), "MathPlugin");

            // Create a planner
            var planner = new SequentialPlanner(kernel);

            var ask = "I have $2130.23. How much would I have after it grew by 24% and after I spent $5 on a latte?";
            var plan = await planner.CreatePlanAsync(ask);

            // Console.WriteLine("Plan:\n");
            // Console.WriteLine(JsonSerializer.Serialize(plan, new JsonSerializerOptions { WriteIndented = true }));

            var result = await plan.InvokeAsync();

            Console.WriteLine("Plan results:");
            Console.WriteLine(result.Result.Trim());

            if (!result.ErrorOccurred)
            {
                return Ok(result.Result);
            }
            return Ok("Error");

            // Output:
            //2615.1829
        }
    }
}