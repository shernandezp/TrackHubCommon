// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using Microsoft.Extensions.DependencyInjection;

namespace Common.Mediator;

/// <summary>
/// Mediator implementation for sending requests and publishing notifications.
/// Utilizes dependency injection to resolve handlers and pipeline behaviors.
/// </summary>
public class MediatorDispatcher(IServiceProvider provider) : ISender, IPublisher
{
    /// <summary>
    /// The service provider used to resolve handlers and behaviors.
    /// </summary>
    private readonly IServiceProvider _provider = provider;

    /// <summary>
    /// Sends the specified request asynchronously.
    /// </summary>
    /// <param name="request">The request to be sent. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        await Send<Unit>(request, cancellationToken);
    }

    /// <summary>
    /// Sends a request to the appropriate handler and returns the result.
    /// </summary>
    /// <remarks>This method resolves the appropriate handler for the given request type and executes it.  If
    /// any pipeline behaviors are registered, they are executed in reverse order before invoking the handler.</remarks>
    /// <typeparam name="TResult">The type of the result returned by the handler.</typeparam>
    /// <param name="request">The request to be processed. Must implement <see cref="IRequest{TResult}"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response of type <typeparamref
    /// name="TResult"/>.</returns>
    public async Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResult));
        var handler = _provider.GetRequiredService(handlerType);

        // Find the correct Handle method on the interface
        var handleMethod = handlerType.GetMethod("Handle", [requestType, typeof(CancellationToken)]);
        if (handleMethod == null)
            throw new InvalidOperationException($"Handler for {handlerType.Name} does not implement required Handle method.");

        var behaviors = _provider.GetServices(typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResult)))
                                 .Cast<dynamic>().Reverse().ToList();

        Func<Task<TResult>> handlerDelegate = () =>
            (Task<TResult>)handleMethod.Invoke(handler, [request, cancellationToken])!;

        foreach (var behavior in behaviors)
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.HandleAsync((dynamic)request, next, cancellationToken);
        }
        return await handlerDelegate();
    }

    /// <summary>
    /// Publishes a notification to all registered notification handlers.
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification.</typeparam>
    /// <param name="notification">The notification instance.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        // Resolve all notification handlers
        var handlers = _provider.GetServices<INotificationHandler<TNotification>>().ToList();
        var tasks = handlers.Select(h => h.Handle(notification, cancellationToken));
        await Task.WhenAll(tasks);
    }
}
