// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc.Infrastructure
{
    internal sealed class SystemTextJsonResultExecutor : IActionResultExecutor<JsonResult>
    {
        private static readonly string DefaultContentType = new MediaTypeHeaderValue("application/json")
        {
            Encoding = Encoding.UTF8
        }.ToString();

        private readonly JsonOptions _options;
        private readonly ILogger<SystemTextJsonResultExecutor> _logger;
        private readonly AsyncEnumerableReader _asyncEnumerableReaderFactory;

        public SystemTextJsonResultExecutor(
            IOptions<JsonOptions> options,
            ILogger<SystemTextJsonResultExecutor> logger,
            IOptions<MvcOptions> mvcOptions)
        {
            _options = options.Value;
            _logger = logger;
            _asyncEnumerableReaderFactory = new AsyncEnumerableReader(mvcOptions.Value);
        }

        public async Task ExecuteAsync(ActionContext context, JsonResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var jsonSerializerOptions = GetSerializerOptions(result);

            var response = context.HttpContext.Response;

            ResponseContentTypeHelper.ResolveContentTypeAndEncoding(
                result.ContentType,
                response.ContentType,
                (DefaultContentType, Encoding.UTF8),
                MediaType.GetEncoding,
                out var resolvedContentType,
                out var resolvedContentTypeEncoding);

            response.ContentType = resolvedContentType;

            if (result.StatusCode != null)
            {
                response.StatusCode = result.StatusCode.Value;
            }

            Log.JsonResultExecuting(_logger, result.Value);

            var value = result.Value;
            var objectType = value?.GetType() ?? typeof(object);

            // Keep this code in sync with SystemTextJsonOutputFormatter
            var responseStream = response.Body;
            if (resolvedContentTypeEncoding.CodePage == Encoding.UTF8.CodePage)
            {
                await JsonSerializer.SerializeAsync(responseStream, value, objectType, jsonSerializerOptions);
                await responseStream.FlushAsync();
            }
            else
            {
                // JsonSerializer only emits UTF8 encoded output, but we need to write the response in the encoding specified by
                // selectedEncoding
                var transcodingStream = Encoding.CreateTranscodingStream(response.Body, resolvedContentTypeEncoding, Encoding.UTF8, leaveOpen: true);

                ExceptionDispatchInfo? exceptionDispatchInfo = null;
                try
                {
                    await JsonSerializer.SerializeAsync(transcodingStream, value, objectType, jsonSerializerOptions);
                    await transcodingStream.FlushAsync();
                }
                catch (Exception ex)
                {
                    // TranscodingStream may write to the inner stream as part of it's disposal.
                    // We do not want this exception "ex" to be eclipsed by any exception encountered during the write. We will stash it and
                    // explicitly rethrow it during the finally block.
                    exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
                }
                finally
                {
                    try
                    {
                        await transcodingStream.DisposeAsync();
                    }
                    catch when (exceptionDispatchInfo != null)
                    {
                    }

                    exceptionDispatchInfo?.Throw();
                }
            }
        }

        private JsonSerializerOptions GetSerializerOptions(JsonResult result)
        {
            var serializerSettings = result.SerializerSettings;
            if (serializerSettings == null)
            {
                return _options.JsonSerializerOptions;
            }
            else
            {
                if (serializerSettings is not JsonSerializerOptions settingsFromResult)
                {
                    throw new InvalidOperationException(Resources.FormatProperty_MustBeInstanceOfType(
                        nameof(JsonResult),
                        nameof(JsonResult.SerializerSettings),
                        typeof(JsonSerializerOptions)));
                }

                return settingsFromResult;
            }
        }

        private static class Log
        {
            private static readonly LogDefineOptions SkipEnabledCheckLogOptions = new() { SkipEnabledCheck = true };

            private static readonly Action<ILogger, string?, Exception?> _jsonResultExecuting = LoggerMessage.Define<string?>(
                LogLevel.Information,
                new EventId(1, "JsonResultExecuting"),
                "Executing JsonResult, writing value of type '{Type}'.",
                SkipEnabledCheckLogOptions);

            // EventId 2 BufferingAsyncEnumerable

            public static void JsonResultExecuting(ILogger logger, object? value)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    var type = value == null ? "null" : value.GetType().FullName;
                    _jsonResultExecuting(logger, type, null);
                }
            }
        }
    }
}
