# Build bot with LUIS and QnA Maker

A template for you to build with both LUIS and QnA Maker.

Sometimes LUIS or QnA Maker is not enough for you handle all the type of messages. Now you can call QnA Maker after LUIS fail to respond.

What's more, you can add question-answer pairs to QnA Maker database through chatting. You don't have to add, retrain and publish manually.

### LuisDialog.cs

This contains the LUIS dialog.

### QnADialog.cs

This contains the QnADialog class. Examples:

Query QnA when LUIS found no matching intent

```
string message = $"Sorry, I do not know'{result.Query}'";

// No intent found, then try asking QnA Knowlegebase

Dialogs.QnADialog qna = new Dialogs.QnADialog();
if (!qna.TryQuery(result.Query, out message))
    message = $"Sorry, I do not know'{result.Query}'";
await context.PostAsync(message);
```

Add QA pairs and publish it with your code.

```
Dialogs.QnADialog qna = new Dialogs.QnADialog();
await context.PostAsync("Learning...");
qna.AddPairs(question, answer);
qna.Publish();
await context.PostAsync($"Done! Try asking'{question}'");
```

### LogHelper.cs

A log class for you to write logs.

Usage:

```
static LogHelper loghelper = new LogHelper(AppDomain.CurrentDomain.BaseDirectory + @"/log/Log.txt");


loghelper.log("Your information here");
```

Your can find your logs at /log/Log.txt.