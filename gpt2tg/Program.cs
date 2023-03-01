using OpenAI.GPT3.Extensions;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenAIService(o =>
{
    o.ApiKey = builder.Configuration.GetSection("OpenAi:ApiKey").Value;
});


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var botClient = new TelegramBotClient(builder.Configuration.GetSection("Telegram:Token").Value);

app.MapPost("/post", async (Post post, IOpenAIService openAiService) =>
{
    var completionResult = await openAiService.Completions.CreateCompletion(new CompletionCreateRequest()
    {
        Prompt = post.Text,
        Model = Models.TextDavinciV3
    });

    if (completionResult.Successful)
    {
        var choiceResponse = completionResult.Choices.FirstOrDefault();

        var messageResult = await botClient.SendTextMessageAsync(post.ChatId, choiceResponse.Text);

        Console.WriteLine();
    }
    else
    {
        if (completionResult.Error == null)
        {
            throw new Exception("Unknown Error");
        }
        Console.WriteLine($"{completionResult.Error.Code}: {completionResult.Error.Message}");
    }
});

app.Run();

internal record Post(string Text, long ChatId);