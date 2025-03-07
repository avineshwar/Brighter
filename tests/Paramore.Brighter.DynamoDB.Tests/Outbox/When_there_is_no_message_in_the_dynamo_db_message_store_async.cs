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
using System.Threading.Tasks;
using Amazon;
using FluentAssertions;
using Paramore.Brighter.Outbox.DynamoDB;
using Xunit;

namespace Paramore.Brighter.DynamoDB.Tests.Outbox
{
    [Trait("Category", "DynamoDB")]
    public class DynamoDbOutboxEmptyStoreAsyncTests : DynamoDBOutboxBaseTest
    {
        private readonly Message _messageEarliest;
        private Message _storedMessage;
        private DynamoDbOutboxSync _dynamoDbOutboxSync;

        public DynamoDbOutboxEmptyStoreAsyncTests()
        {
            _messageEarliest = new Message(new MessageHeader(Guid.NewGuid(), "test_topic", MessageType.MT_DOCUMENT), new MessageBody("message body"));
            _dynamoDbOutboxSync = new DynamoDbOutboxSync(Client, new DynamoDbConfiguration(Credentials, RegionEndpoint.EUWest1, TableName));
        }

        [Fact]
        public async Task When_there_is_no_message_in_the_dynamo_db_outbox()
        {
            _storedMessage = await _dynamoDbOutboxSync.GetAsync(_messageEarliest.Id);

            //_should_return_a_empty_message
            _storedMessage.Header.MessageType.Should().Be(MessageType.MT_NONE);
        }
    }
}
