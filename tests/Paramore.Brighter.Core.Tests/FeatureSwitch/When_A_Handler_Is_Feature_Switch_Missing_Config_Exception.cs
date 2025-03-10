﻿#region Licence
/* The MIT License (MIT)
Copyright © 2014 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

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
using FluentAssertions;
using Paramore.Brighter.Core.Tests.CommandProcessors.TestDoubles;
using Paramore.Brighter.Core.Tests.FeatureSwitch.TestDoubles;
using Paramore.Brighter.FeatureSwitch;
using Polly.Registry;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter.Extensions.DependencyInjection;
using Xunit;
using Paramore.Brighter.FeatureSwitch.Handlers;

namespace Paramore.Brighter.Core.Tests.FeatureSwitch
{
    [Collection("CommandProcessor")] 
    public class FeatureSwitchByConfigMissingConfigStrategyExceptionTests : IDisposable
    {
        private readonly MyCommand _myCommand = new MyCommand();
        private readonly SubscriberRegistry _registry;
        private readonly ServiceProviderHandlerFactory _handlerFactory;
        private readonly IAmAFeatureSwitchRegistry _featureSwitchRegistry;

        private CommandProcessor _commandProcessor;
        private Exception _exception;
        ServiceProvider _provider;

        public FeatureSwitchByConfigMissingConfigStrategyExceptionTests()
        {
            _registry = new SubscriberRegistry();
            _registry.Register<MyCommand, MyFeatureSwitchedConfigHandler>();

            var container = new ServiceCollection();
            container.AddSingleton<MyFeatureSwitchedConfigHandler>();
            container.AddTransient<FeatureSwitchHandler<MyCommand>>();
            container.AddSingleton<IBrighterOptions>(new BrighterOptions() {HandlerLifetime = ServiceLifetime.Transient});

            _provider = container.BuildServiceProvider();
            _handlerFactory = new ServiceProviderHandlerFactory(_provider);
            
            _featureSwitchRegistry = new FakeConfigRegistry();
        }        

        [Fact]
        public void When_Sending_A_Command_To_The_Processor_When_A_Feature_Switch_Has_No_Config_And_Strategy_Is_Exception()
        {
            _featureSwitchRegistry.MissingConfigStrategy = MissingConfigStrategy.Exception;

            _commandProcessor = CommandProcessorBuilder
                .With()
                .ConfigureFeatureSwitches(_featureSwitchRegistry)
                .Handlers(new HandlerConfiguration(_registry, _handlerFactory))
                .DefaultPolicy()
                .NoExternalBus()
                .RequestContextFactory(new InMemoryRequestContextFactory())
                .Build();

            _exception = Catch.Exception(() => _commandProcessor.Send(_myCommand));

            _exception.Should().BeOfType<ConfigurationException>();
            _exception.Should().NotBeNull();
            _exception.Message.Should().Contain($"Handler of type {nameof(MyFeatureSwitchedConfigHandler)} does not have a Feature Switch configuration!");

            _provider.GetService<MyFeatureSwitchedConfigHandler>().DidReceive(_myCommand).Should().BeFalse(); 
        }

        public void Dispose()
        {
            CommandProcessor.ClearExtServiceBus();
        }
    }
}
