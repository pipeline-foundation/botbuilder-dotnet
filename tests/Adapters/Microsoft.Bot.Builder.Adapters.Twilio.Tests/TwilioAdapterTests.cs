﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;
using Moq;
using Twilio.Rest.Api.V2010.Account;
using Xunit;

namespace Microsoft.Bot.Builder.Adapters.Twilio.Tests
{
    public class TwilioAdapterTests
    {
        private readonly TwilioAdapterOptions _testOptions = new TwilioAdapterOptions("Test", "Test", "Test", new Uri("http://contoso.com"));

        [Fact]
        public void ConstructorWithTwilioApiWrapperSucceeds()
        {
            Assert.NotNull(new TwilioAdapter(new Mock<TwilioClientWrapper>(_testOptions).Object));
        }

        [Fact]
        public void ConstructorShouldFailWithNullTwilioApiWrapper()
        {
            Assert.Throws<ArgumentNullException>(() => { new TwilioAdapter((TwilioClientWrapper)null); });
        }

        [Fact]
        public async void SendActivitiesAsyncShouldSucceed()
        {
            var activity = new Mock<Activity>().SetupAllProperties();
            activity.Object.Type = "message";
            activity.Object.Attachments = new List<Attachment> { new Attachment(contentUrl: "http://example.com") };
            activity.Object.Conversation = new ConversationAccount(id: "MockId");
            activity.Object.Text = "Hello, Bot!";

            const string resourceIdentifier = "Mocked Resource Identifier";
            var twilioApi = new Mock<TwilioClientWrapper>(_testOptions);
            twilioApi.Setup(x => x.SendMessage(It.IsAny<CreateMessageOptions>())).Returns(Task.FromResult(resourceIdentifier));

            var twilioAdapter = new TwilioAdapter(twilioApi.Object);
            var resourceResponses = await twilioAdapter.SendActivitiesAsync(null, new Activity[] { activity.Object }, default).ConfigureAwait(false);

            Assert.True(resourceResponses[0].Id == resourceIdentifier);
        }

        [Fact]
        public async void SendActivitiesAsyncShouldFailWithActivityTypeNotMessage()
        {
            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(_testOptions).Object);

            var activity = new Activity()
            {
                Type = ActivityTypes.Event,
            };

            Activity[] activities = { activity };

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await twilioAdapter.SendActivitiesAsync(new TurnContext(twilioAdapter, activity), activities, default);
            });
        }

        [Fact]
        public async void ProcessAsyncShouldSucceedWithHttpBody()
        {
            var payload = File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"/Files/NoMediaPayload.txt"));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()));
            var twilioApi = new Mock<TwilioClientWrapper>(_testOptions);
            var twilioAdapter = new TwilioAdapter(twilioApi.Object);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();
            
            httpRequest.SetupGet(req => req.Body).Returns(stream);

            twilioApi.SetupAllProperties();
            twilioApi.Setup(x => x.ValidateSignature(It.IsAny<HttpRequest>(), It.IsAny<Dictionary<string, string>>())).Returns(true);

            bot.SetupAllProperties();
            httpResponse.Setup(res => res.Body.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback((byte[] data, int offset, int length, CancellationToken token) =>
                {
                    if (length > 0)
                    {
                        var actual = Encoding.UTF8.GetString(data);
                    }
                });

            await twilioAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, default);

            bot.Verify(b => b.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async void ProcessAsyncShouldSucceedWithNullHttpBody()
        {
            var payload = File.ReadAllText(PathUtils.NormalizePath(Directory.GetCurrentDirectory() + @"/Files/NoMediaPayload.txt"));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload.ToString()));
            var twilioApi = new Mock<TwilioClientWrapper>(_testOptions);
            var twilioAdapter = new TwilioAdapter(twilioApi.Object);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();

            httpRequest.SetupGet(req => req.Body).Returns(stream);

            twilioApi.SetupAllProperties();
            twilioApi.Setup(x => x.ValidateSignature(It.IsAny<HttpRequest>(), It.IsAny<Dictionary<string, string>>())).Returns(true);

            bot.SetupAllProperties();
            httpResponse.Setup(res => res.Body.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback((byte[] data, int offset, int length, CancellationToken token) =>
                {
                    if (length > 0)
                    {
                        var actual = Encoding.UTF8.GetString(data);
                    }
                });

            await twilioAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, bot.Object, default);

            bot.Verify(b => b.OnTurnAsync(It.IsAny<TurnContext>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async void ProcessAsyncShouldFailWithNullHttpRequest()
        {
            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(_testOptions).Object);
            var httpResponse = new Mock<HttpResponse>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await twilioAdapter.ProcessAsync(null, httpResponse.Object, bot.Object, default);
            });
        }

        [Fact]
        public async void ProcessAsyncShouldFailWithNullHttpResponse()
        {
            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(_testOptions).Object);
            var httpRequest = new Mock<HttpRequest>();
            var bot = new Mock<IBot>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await twilioAdapter.ProcessAsync(httpRequest.Object, null, bot.Object, default);
            });
        }

        [Fact]
        public async void ProcessAsyncShouldFailWithNullBot()
        {
            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(_testOptions).Object);
            var httpRequest = new Mock<HttpRequest>();
            var httpResponse = new Mock<HttpResponse>();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await twilioAdapter.ProcessAsync(httpRequest.Object, httpResponse.Object, null, default);
            });
        }

        [Fact]
        public async void UpdateActivityAsyncShouldThrowNotSupportedException()
        {
            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(_testOptions).Object);
            var activity = new Activity();

            using (var turnContext = new TurnContext(twilioAdapter, activity))
            {
                await Assert.ThrowsAsync<NotSupportedException>(async () =>
                {
                    await twilioAdapter.UpdateActivityAsync(turnContext, activity, default);
                });
            }
        }

        [Fact]
        public async void DeleteActivityAsyncShouldThrowNotSupportedException()
        {
            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(_testOptions).Object);
            var activity = new Activity();
            var conversationReference = new ConversationReference();

            using (var turnContext = new TurnContext(twilioAdapter, activity))
            {
                await Assert.ThrowsAsync<NotSupportedException>(async () =>
                {
                    await twilioAdapter.DeleteActivityAsync(turnContext, conversationReference, default);
                });
            }
        }

        [Fact]
        public async void ContinueConversationAsyncShouldSucceed()
        {
            var callbackInvoked = false;
            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(_testOptions).Object);
            var conversationReference = new ConversationReference();

            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                callbackInvoked = true;
                return Task.CompletedTask;
            }

            await twilioAdapter.ContinueConversationAsync(conversationReference, BotsLogic, default);
            
            Assert.True(callbackInvoked);
        }

        [Fact]
        public async void ContinueConversationAsyncShouldFailWithNullConversationReference()
        {
            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(_testOptions).Object);

            Task BotsLogic(ITurnContext turnContext, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await twilioAdapter.ContinueConversationAsync(null, BotsLogic, default); });
        }

        [Fact]
        public async void ContinueConversationAsyncShouldFailWithNullLogic()
        {
            var twilioAdapter = new TwilioAdapter(new Mock<TwilioClientWrapper>(_testOptions).Object);
            var conversationReference = new ConversationReference();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await twilioAdapter.ContinueConversationAsync(conversationReference, null, default); });
        }
    }
}