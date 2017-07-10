using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Threading;

namespace bot_luis_qna.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // Init converstion data for per user
            int learn = 0;
            if (!context.ConversationData.TryGetValue<int>("learn", out learn))
            {
                learn = 0;
                context.ConversationData.SetValue<int>("learn", learn);
            }
            string question = "", answer = "";
            if (!context.ConversationData.TryGetValue<string>("question", out question))
            {
                if (context.ConversationData.ContainsKey("question"))
                {
                    context.ConversationData.RemoveValue("question");
                }
                question = "?";
                context.ConversationData.SetValue<string>("question", question);
            }
            if (!context.ConversationData.TryGetValue<string>("answer", out answer))
            {
                if (context.ConversationData.ContainsKey("answer"))
                {
                    context.ConversationData.RemoveValue("answer");
                }
                answer = "?";
                context.ConversationData.SetValue<string>("answer", answer);
            }

            // States for Learning mode, you can teach your bot here.
            if (activity.Text == "Learn" && learn == 0)
            {
                await context.PostAsync($"We are now at learning mode, input your question:");
                learn = 1;
                context.ConversationData.SetValue<int>("learn", learn);
                context.Wait(MessageReceivedAsync);
            }
            else if (learn == 1)
            {
                context.ConversationData.SetValue<string>("question", activity.Text);
                await context.PostAsync($"Please input your answer: ");
                learn = 2;
                context.ConversationData.SetValue<int>("learn", learn);
                context.Wait(MessageReceivedAsync);
            }
            else if (learn == 2)
            {
                context.ConversationData.SetValue<string>("answer", activity.Text);
                if (context.ConversationData.TryGetValue<string>("question", out question) &&
                    context.ConversationData.TryGetValue<string>("answer", out answer))
                {
                    context.ConversationData.SetValue<string>("answer", activity.Text);
                    PromptDialog.Confirm(context, AfterConfirm, string.Format("Your question: '{0}'，answer: '{1}'. Confirm？", question, answer));
                }
                else
                {
                    await context.PostAsync("Error occured");
                    context.ConversationData.SetValue<int>("learn", 0);
                    context.Wait(MessageReceivedAsync);
                }
                context.ConversationData.SetValue<int>("learn", 0);
            }
            else
            {
                context.ConversationData.SetValue<int>("learn", 0);
                await context.Forward(new Dialogs.LuisDialog(), this.ResumeAfterLuisDialog, activity, CancellationToken.None);
            }

            // Wait for message anyway
            context.Wait(MessageReceivedAsync);
        }

        /// <summary>
        /// You can handle things after LUIS give the answer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task ResumeAfterLuisDialog(IDialogContext context, IAwaitable<object> result)
        {
            var ticketNumber = await result;
            //Do nothing until user send another message
            context.Wait(this.MessageReceivedAsync);
        }

        /// <summary>
        /// Confirm and add pairs to QnA Maker and publish it
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task AfterConfirm(IDialogContext context, IAwaitable<bool> result)
        {
            var confirm = await result;
            if (confirm)
            {
                string question = ".", answer = ".";
                if (context.ConversationData.TryGetValue<string>("question", out question) &&
                    context.ConversationData.TryGetValue<string>("answer", out answer))
                {
                    context.ConversationData.SetValue<int>("learn", 0);
                    Dialogs.QnADialog qna = new Dialogs.QnADialog();
                    await context.PostAsync("Learning...");
                    qna.AddPairs(question, answer);
                    qna.Publish();
                    await context.PostAsync($"Done! Try asking'{question}'");
                }
            }
            else
            {
                context.ConversationData.SetValue<int>("learn", 0);
                await context.PostAsync("Cancelled");
            }
        }
    }
}