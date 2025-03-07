﻿#region Licence
/* The MIT License (MIT)
Copyright © 2015 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the “Software”), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. */

#endregion

using System;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using Paramore.Brighter.Core.Tests.CommandProcessors.TestDoubles;
using Paramore.Brighter.Scope;
using Xunit;

namespace Paramore.Brighter.Core.Tests.CommandProcessors
{
    [Collection("CommandProcessor")]
    public class PostCommandTests : IDisposable
    {
        private readonly CommandProcessor _commandProcessor;
        private readonly MyCommand _myCommand = new MyCommand();
        private readonly Message _message;
        private readonly FakeOutboxSync _fakeOutboxSync;
        private readonly FakeMessageProducer _fakeMessageProducer;

        public PostCommandTests()
        {
            _myCommand.Value = "Hello World";

            _fakeOutboxSync = new FakeOutboxSync();
            _fakeMessageProducer = new FakeMessageProducer();

            _message = new Message(
                new MessageHeader(_myCommand.Id, "MyCommand", MessageType.MT_COMMAND),
                new MessageBody(JsonSerializer.Serialize(_myCommand, JsonSerialisationOptions.Options))
                );

            var messageMapperRegistry =
                new MessageMapperRegistry(new SimpleMessageMapperFactory((_) => new MyCommandMessageMapper()));
            messageMapperRegistry.Register<MyCommand, MyCommandMessageMapper>();

            _commandProcessor = CommandProcessorBuilder.With()
                .Handlers(new HandlerConfiguration(new SubscriberRegistry(), new EmptyHandlerFactorySync()))
                .DefaultPolicy()
                .ExternalBus(new MessagingConfiguration(_fakeMessageProducer, messageMapperRegistry), _fakeOutboxSync)
                .RequestContextFactory(new InMemoryRequestContextFactory())
                .Build();
        }

        [Fact]
        public void When_Posting_With_A_Default_Policy()
        {
            _commandProcessor.Post(_myCommand);

            //should store the message in the sent outbox
            _fakeOutboxSync
                .Get()
                .SingleOrDefault(msg => msg.Id == _message.Id)
                .Should().NotBeNull();
            //should send a message via the messaging gateway
            _fakeMessageProducer.MessageWasSent.Should().BeTrue();
            // should convert the command into a message
            _fakeOutboxSync.Get().First().Should().Be(_message);
        }

        public void Dispose()
        {
            CommandProcessor.ClearExtServiceBus();
        }

        internal class EmptyHandlerFactorySync : IAmAHandlerFactorySync
        {
            public IHandleRequests Create(Type handlerType, IAmALifetime lifetimeScope)
            {
                return null;
            }

            public void Release(IHandleRequests handler) {}
            public IBrighterScope CreateScope() => new Unscoped();
        }
    }
}
