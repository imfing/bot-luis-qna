using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using bot_luis_qna.Log;

namespace bot_luis_qna.Dialogs
{
    [LuisModel("{Your LUIS app id}", "Your LUIS key", LuisApiVersion.V2, "southeastasia.api.cognitive.microsoft.com")]

    [Serializable]
    public class LuisDialog : LuisDialog<object>
    {
        static LogHelper loghelper = new LogHelper(AppDomain.CurrentDomain.BaseDirectory + @"/log/Log.txt");

        /// <summary>
        /// LUIS Logic
        /// </summary>

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I do not know'{result.Query}'";

            // No intent found, then try asking QnA Knowlegebase
            Dialogs.QnADialog qna = new Dialogs.QnADialog();
            if (!qna.TryQuery(result.Query, out message))
                message = $"Sorry, I do not know'{result.Query}'";
            await context.PostAsync(message);
            context.Done(1);    // Go back to Root
        }

    }
}