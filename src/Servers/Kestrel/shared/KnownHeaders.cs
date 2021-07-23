// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http.HPack;
using System.Reflection;
using System.Text;
using Microsoft.Net.Http.Headers;

namespace CodeGenerator
{
    public class KnownHeaders
    {
        public static readonly KnownHeader[] RequestHeaders;
        public static readonly KnownHeader[] ResponseHeaders;
        public static readonly KnownHeader[] ResponseTrailers;
        public static readonly string[] InternalHeaderAccessors = new[]
        {
            HeaderNames.Allow,
            HeaderNames.AltSvc,
            HeaderNames.TransferEncoding,
            HeaderNames.ContentLength,
            HeaderNames.Connection,
            HeaderNames.Scheme,
            HeaderNames.Path,
            HeaderNames.Method,
            HeaderNames.Authority,
            HeaderNames.Host,
        };

        public static readonly string[] DefinedHeaderNames = typeof(HeaderNames).GetFields(BindingFlags.Static | BindingFlags.Public).Select(h => h.Name).ToArray();

        public static readonly string[] ObsoleteHeaderNames = new[]
        {
            HeaderNames.DNT,
        };

        public static readonly string[] PsuedoHeaderNames = new[]
        {
            "Authority", // :authority
            "Method", // :method
            "Path", // :path
            "Scheme", // :scheme
            "Status" // :status
        };

        public static readonly string[] NonApiHeaders =
            ObsoleteHeaderNames
            .Concat(PsuedoHeaderNames)
            .ToArray();

        public static readonly string[] ApiHeaderNames =
            DefinedHeaderNames
            .Except(NonApiHeaders)
            .ToArray();

        public static readonly long InvalidH2H3ResponseHeadersBits;

        static KnownHeaders()
        {
            var requestPrimaryHeaders = new[]
            {
                HeaderNames.Accept,
                HeaderNames.Connection,
                HeaderNames.Host,
                HeaderNames.UserAgent
            };
            var responsePrimaryHeaders = new[]
            {
                HeaderNames.Connection,
                HeaderNames.Date,
                HeaderNames.ContentType,
                HeaderNames.Server,
                HeaderNames.ContentLength,
            };
            var commonHeaders = new[]
            {
                HeaderNames.CacheControl,
                HeaderNames.Connection,
                HeaderNames.Date,
                HeaderNames.GrpcEncoding,
                HeaderNames.KeepAlive,
                HeaderNames.Pragma,
                HeaderNames.TransferEncoding,
                HeaderNames.Upgrade,
                HeaderNames.Via,
                HeaderNames.Warning,
                HeaderNames.ContentType,
            };
            // http://www.w3.org/TR/cors/#syntax
            var corsRequestHeaders = new[]
            {
                HeaderNames.Origin,
                HeaderNames.AccessControlRequestMethod,
                HeaderNames.AccessControlRequestHeaders,
            };
            var requestHeadersExistence = new[]
            {
                HeaderNames.Connection,
                HeaderNames.TransferEncoding,
            };
            var requestHeadersCount = new[]
            {
                HeaderNames.Host
            };
            RequestHeaders = commonHeaders.Concat(new[]
            {
                HeaderNames.Authority,
                HeaderNames.Method,
                HeaderNames.Path,
                HeaderNames.Scheme,
                HeaderNames.Accept,
                HeaderNames.AcceptCharset,
                HeaderNames.AcceptEncoding,
                HeaderNames.AcceptLanguage,
                HeaderNames.Authorization,
                HeaderNames.Cookie,
                HeaderNames.Expect,
                HeaderNames.From,
                HeaderNames.GrpcAcceptEncoding,
                HeaderNames.GrpcTimeout,
                HeaderNames.Host,
                HeaderNames.IfMatch,
                HeaderNames.IfModifiedSince,
                HeaderNames.IfNoneMatch,
                HeaderNames.IfRange,
                HeaderNames.IfUnmodifiedSince,
                HeaderNames.MaxForwards,
                HeaderNames.ProxyAuthorization,
                HeaderNames.Referer,
                HeaderNames.Range,
                HeaderNames.TE,
                HeaderNames.Translate,
                HeaderNames.UserAgent,
                HeaderNames.UpgradeInsecureRequests,
                HeaderNames.RequestId,
                HeaderNames.CorrelationContext,
                HeaderNames.TraceParent,
                HeaderNames.TraceState,
                HeaderNames.Baggage,
            })
            .Concat(corsRequestHeaders)
            .OrderBy(header => header)
            .OrderBy(header => !requestPrimaryHeaders.Contains(header))
            .Select((header, index) => new KnownHeader
            {
                Name = header,
                Index = index,
                PrimaryHeader = requestPrimaryHeaders.Contains(header),
                ExistenceCheck = requestHeadersExistence.Contains(header),
                FastCount = requestHeadersCount.Contains(header)
            })
            .Concat(new[] { new KnownHeader
            {
                Name = HeaderNames.ContentLength,
                Index = -1,
                PrimaryHeader = requestPrimaryHeaders.Contains(HeaderNames.ContentLength)
            }})
            .ToArray();

            var responseHeadersExistence = new[]
            {
                HeaderNames.Connection,
                HeaderNames.Server,
                HeaderNames.Date,
                HeaderNames.TransferEncoding
            };
            var enhancedHeaders = new[]
            {
                HeaderNames.Connection,
                HeaderNames.Server,
                HeaderNames.Date,
                HeaderNames.TransferEncoding
            };
            // http://www.w3.org/TR/cors/#syntax
            var corsResponseHeaders = new[]
            {
                HeaderNames.AccessControlAllowCredentials,
                HeaderNames.AccessControlAllowHeaders,
                HeaderNames.AccessControlAllowMethods,
                HeaderNames.AccessControlAllowOrigin,
                HeaderNames.AccessControlExposeHeaders,
                HeaderNames.AccessControlMaxAge,
            };
            ResponseHeaders = commonHeaders.Concat(new[]
            {
                HeaderNames.AcceptRanges,
                HeaderNames.Age,
                HeaderNames.Allow,
                HeaderNames.AltSvc,
                HeaderNames.ETag,
                HeaderNames.Location,
                HeaderNames.ProxyAuthenticate,
                HeaderNames.ProxyConnection,
                HeaderNames.RetryAfter,
                HeaderNames.Server,
                HeaderNames.SetCookie,
                HeaderNames.Vary,
                HeaderNames.Expires,
                HeaderNames.WWWAuthenticate,
                HeaderNames.ContentRange,
                HeaderNames.ContentEncoding,
                HeaderNames.ContentLanguage,
                HeaderNames.ContentLocation,
                HeaderNames.ContentMD5,
                HeaderNames.LastModified,
                HeaderNames.Trailer,
            })
            .Concat(corsResponseHeaders)
            .OrderBy(header => header)
            .OrderBy(header => !responsePrimaryHeaders.Contains(header))
            .Select((header, index) => new KnownHeader
            {
                Name = header,
                Index = index,
                EnhancedSetter = enhancedHeaders.Contains(header),
                ExistenceCheck = responseHeadersExistence.Contains(header),
                PrimaryHeader = responsePrimaryHeaders.Contains(header)
            })
            .Concat(new[] { new KnownHeader
            {
                Name = HeaderNames.ContentLength,
                Index = 63,
                EnhancedSetter = enhancedHeaders.Contains(HeaderNames.ContentLength),
                PrimaryHeader = responsePrimaryHeaders.Contains(HeaderNames.ContentLength)
            }})
            .ToArray();

            ResponseTrailers = new[]
            {
                HeaderNames.ETag,
                HeaderNames.GrpcMessage,
                HeaderNames.GrpcStatus
            }
            .OrderBy(header => header)
            .OrderBy(header => !responsePrimaryHeaders.Contains(header))
            .Select((header, index) => new KnownHeader
            {
                Name = header,
                Index = index,
                EnhancedSetter = enhancedHeaders.Contains(header),
                ExistenceCheck = responseHeadersExistence.Contains(header),
                PrimaryHeader = responsePrimaryHeaders.Contains(header)
            })
            .ToArray();

            var invalidH2H3ResponseHeaders = new[]
            {
                HeaderNames.Connection,
                HeaderNames.TransferEncoding,
                HeaderNames.KeepAlive,
                HeaderNames.Upgrade,
                HeaderNames.ProxyConnection
            };

            InvalidH2H3ResponseHeadersBits = ResponseHeaders
                .Where(header => invalidH2H3ResponseHeaders.Contains(header.Name))
                .Select(header => 1L << header.Index)
                .Aggregate((a, b) => a | b);
        }

        static string Each<T>(IEnumerable<T> values, Func<T, string> formatter)
        {
            return values.Any() ? values.Select(formatter).Aggregate((a, b) => a + b) : "";
        }

        static string Each<T>(IEnumerable<T> values, Func<T, int, string> formatter)
        {
            return values.Any() ? values.Select(formatter).Aggregate((a, b) => a + b) : "";
        }

        static string AppendSwitch(IEnumerable<IGrouping<int, KnownHeader>> values) =>
             $@"switch (name.Length)
            {{{Each(values, byLength => $@"
                case {byLength.Key}:{AppendSwitchSection(byLength.Key, byLength.OrderBy(h => h, KnownHeaderComparer.Instance).ToList())}
                    break;")}
            }}";

        static string AppendHPackSwitch(IEnumerable<HPackGroup> values) =>
             $@"switch (index)
            {{{Each(values, header => $@"{Each(header.HPackStaticTableIndexes, index => $@"
                case {index}:")}
                    {AppendHPackSwitchSection(header)}")}
            }}";

        static string AppendValue(bool returnTrue = false) =>
             $@"// Matched a known header
                if ((_previousBits & flag) != 0)
                {{
                    // Had a previous string for this header, mark it as used so we don't clear it OnHeadersComplete or consider it if we get a second header
                    _previousBits ^= flag;

                    // We will only reuse this header if there was only one previous header
                    if (values.Count == 1)
                    {{
                        var previousValue = values.ToString();
                        // Check lengths are the same, then if the bytes were converted to an ascii string if they would be the same.
                        // We do not consider Utf8 headers for reuse.
                        if (previousValue.Length == value.Length &&
                            StringUtilities.BytesOrdinalEqualsStringAndAscii(previousValue, value))
                        {{
                            // The previous string matches what the bytes would convert to, so we will just use that one.
                            _bits |= flag;
                            return{(returnTrue ? " true" : "")};
                        }}
                    }}
                }}

                // We didn't have a previous matching header value, or have already added a header, so get the string for this value.
                var valueStr = value.GetRequestHeaderString(nameStr, EncodingSelector);
                if ((_bits & flag) == 0)
                {{
                    // We didn't already have a header set, so add a new one.
                    _bits |= flag;
                    values = new StringValues(valueStr);
                }}
                else
                {{
                    // We already had a header set, so concatenate the new one.
                    values = AppendValue(values, valueStr);
                }}";

        static string AppendHPackSwitchSection(HPackGroup group)
        {
            var header = group.Header;
            if (header.Name == HeaderNames.ContentLength)
            {
                return $@"var customEncoding = ReferenceEquals(EncodingSelector, KestrelServerOptions.DefaultHeaderEncodingSelector)
                        ? null : EncodingSelector(HeaderNames.ContentLength);
                    if (customEncoding == null)
                    {{
                        AppendContentLength(value);
                    }}
                    else
                    {{
                        AppendContentLengthCustomEncoding(value, customEncoding);
                    }}
                    return true;";
            }
            else
            {
                return $@"flag = {header.FlagBit()};
                    values = ref _headers._{header.Identifier};
                    nameStr = HeaderNames.{header.Identifier};
                    break;";
            }
        }

        static string AppendSwitchSection(int length, IList<KnownHeader> values)
        {
            var useVarForFirstTerm = values.Count > 1 && values.Select(h => h.FirstNameIgnoreCaseSegment()).Distinct().Count() == 1;
            var firstTermVarExpression = values.Select(h => h.FirstNameIgnoreCaseSegment()).FirstOrDefault();
            var firstTermVar = $"firstTerm{length}";

            var start = "";
            if (useVarForFirstTerm)
            {
                start = $@"
                    var {firstTermVar} = {firstTermVarExpression};";
            }
            else
            {
                firstTermVar = "";
            }

            string GenerateIfBody(KnownHeader header, string extraIndent = "")
            {
                if (header.Name == HeaderNames.ContentLength)
                {
                    return $@"
                        {extraIndent}var customEncoding = ReferenceEquals(EncodingSelector, KestrelServerOptions.DefaultHeaderEncodingSelector)
                        {extraIndent}   ? null : EncodingSelector(HeaderNames.ContentLength);
                        {extraIndent}if (customEncoding == null)
                        {extraIndent}{{
                        {extraIndent}    AppendContentLength(value);
                        {extraIndent}}}
                        {extraIndent}else
                        {extraIndent}{{
                        {extraIndent}    AppendContentLengthCustomEncoding(value, customEncoding);
                        {extraIndent}}}
                        {extraIndent}return;";
                }
                else
                {
                    return $@"
                        {extraIndent}flag = {header.FlagBit()};
                        {extraIndent}values = ref _headers._{header.Identifier};
                        {extraIndent}nameStr = HeaderNames.{header.Identifier};";
                }
            }

            // Group headers together that have the same ignore equal case equals check for the first term.
            // There will probably only be more than one item in a group for Content-Encoding, Content-Language, Content-Location.
            var groups = values.GroupBy(header => header.EqualIgnoreCaseBytesFirstTerm())
                .OrderBy(g => g.First(), KnownHeaderComparer.Instance)
                .ToList();

            return start + $@"{Each(groups, (byFirstTerm, i) => $@"{(byFirstTerm.Count() == 1 ? $@"{Each(byFirstTerm, header => $@"
                    {(i > 0 ? "else " : "")}if ({header.EqualIgnoreCaseBytes(firstTermVar)})
                    {{{GenerateIfBody(header)}
                    }}")}" : $@"
                    if ({byFirstTerm.Key.Replace(firstTermVarExpression, firstTermVar)})
                    {{{Each(byFirstTerm, (header, i) => $@"
                        {(i > 0 ? "else " : "")}if ({header.EqualIgnoreCaseBytesSecondTermOnwards()})
                        {{{GenerateIfBody(header, extraIndent: "    ")}
                        }}")}
                    }}")}")}";
        }

        [DebuggerDisplay("{Name}")]
        public class KnownHeader
        {
            public string Name { get; set; }
            public int Index { get; set; }
            public string Identifier => ResolveIdentifier(Name);

            public byte[] Bytes => Encoding.ASCII.GetBytes($"\r\n{Name}: ");
            public int BytesOffset { get; set; }
            public int BytesCount { get; set; }
            public bool ExistenceCheck { get; set; }
            public bool FastCount { get; set; }
            public bool EnhancedSetter { get; set; }
            public bool PrimaryHeader { get; set; }
            public string FlagBit() => $"{"0x" + (1L << Index).ToString("x", CultureInfo.InvariantCulture)}L";
            public string TestBit() => $"(_bits & {"0x" + (1L << Index).ToString("x", CultureInfo.InvariantCulture)}L) != 0";
            public string TestTempBit() => $"(tempBits & {"0x" + (1L << Index).ToString("x", CultureInfo.InvariantCulture)}L) != 0";
            public string TestNotTempBit() => $"(tempBits & ~{"0x" + (1L << Index).ToString("x", CultureInfo.InvariantCulture)}L) == 0";
            public string TestNotBit() => $"(_bits & {"0x" + (1L << Index).ToString("x", CultureInfo.InvariantCulture)}L) == 0";
            public string SetBit() => $"_bits |= {"0x" + (1L << Index).ToString("x", CultureInfo.InvariantCulture)}L";
            public string ClearBit() => $"_bits &= ~{"0x" + (1L << Index).ToString("x", CultureInfo.InvariantCulture)}L";

            private string ResolveIdentifier(string name)
            {
                // Check the 3 lowercase headers
                switch (name)
                {
                    case "baggage": return "Baggage";
                    case "traceparent": return "TraceParent";
                    case "tracestate": return "TraceState";
                }

                var identifier = name.Replace("-", "");

                // Pseudo headers start with a colon. A colon isn't valid in C# names so
                // remove it and pascal case the header name. e.g. :path -> Path, :scheme -> Scheme.
                // This identifier will match the names in HeadersNames.cs
                if (identifier.StartsWith(':'))
                {
                    identifier = char.ToUpperInvariant(identifier[1]) + identifier.Substring(2);
                }

                return identifier;
            }

            private void GetMaskAndComp(string name, int offset, int count, out ulong mask, out ulong comp)
            {
                mask = 0;
                comp = 0;
                for (var scan = 0; scan < count; scan++)
                {
                    var ch = (byte)name[offset + count - scan - 1];
                    var isAlpha = (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');
                    comp = (comp << 8) + (ch & (isAlpha ? 0xdfu : 0xffu));
                    mask = (mask << 8) + (isAlpha ? 0xdfu : 0xffu);
                }
            }

            private string NameTerm(string name, int offset, int count, string type, string suffix)
            {
                GetMaskAndComp(name, offset, count, out var mask, out var comp);

                if (offset == 0)
                {
                    if (type == "byte")
                    {
                        return $"(nameStart & 0x{mask:x}{suffix})";
                    }
                    else
                    {
                        return $"(Unsafe.ReadUnaligned<{type}>(ref nameStart) & 0x{mask:x}{suffix})";
                    }
                }
                else
                {
                    if (type == "byte")
                    {
                        return $"(Unsafe.AddByteOffset(ref nameStart, (IntPtr){offset / count}) & 0x{mask:x}{suffix})";
                    }
                    else if ((offset / count) == 1)
                    {
                        return $"(Unsafe.ReadUnaligned<{type}>(ref Unsafe.AddByteOffset(ref nameStart, (IntPtr)sizeof({type}))) & 0x{mask:x}{suffix})";
                    }
                    else
                    {
                        return $"(Unsafe.ReadUnaligned<{type}>(ref Unsafe.AddByteOffset(ref nameStart, (IntPtr)({offset / count} * sizeof({type})))) & 0x{mask:x}{suffix})";
                    }
                }

            }

            private string EqualityTerm(string name, int offset, int count, string type, string suffix)
            {
                GetMaskAndComp(name, offset, count, out var mask, out var comp);

                return $"0x{comp:x}{suffix}";
            }

            private string Term(string name, int offset, int count, string type, string suffix)
            {
                GetMaskAndComp(name, offset, count, out var mask, out var comp);

                return $"({NameTerm(name, offset, count, type, suffix)} == {EqualityTerm(name, offset, count, type, suffix)})";
            }

            public string FirstNameIgnoreCaseSegment()
            {
                var result = "";
                if (Name.Length >= 8)
                {
                    result = NameTerm(Name, 0, 8, "ulong", "uL");
                }
                else if (Name.Length >= 4)
                {
                    result = NameTerm(Name, 0, 4, "uint", "u");
                }
                else if (Name.Length >= 2)
                {
                    result = NameTerm(Name, 0, 2, "ushort", "u");
                }
                else
                {
                    result = NameTerm(Name, 0, 1, "byte", "u");
                }

                return result;
            }

            public string EqualIgnoreCaseBytes(string firstTermVar = "")
            {
                if (!string.IsNullOrEmpty(firstTermVar))
                {
                    return EqualIgnoreCaseBytesWithVar(firstTermVar);
                }

                var result = "";
                var delim = "";
                var index = 0;
                while (index != Name.Length)
                {
                    if (Name.Length - index >= 8)
                    {
                        result += delim + Term(Name, index, 8, "ulong", "uL");
                        index += 8;
                    }
                    else if (Name.Length - index >= 4)
                    {
                        result += delim + Term(Name, index, 4, "uint", "u");
                        index += 4;
                    }
                    else if (Name.Length - index >= 2)
                    {
                        result += delim + Term(Name, index, 2, "ushort", "u");
                        index += 2;
                    }
                    else
                    {
                        result += delim + Term(Name, index, 1, "byte", "u");
                        index += 1;
                    }
                    delim = " && ";
                }
                return result;

                string EqualIgnoreCaseBytesWithVar(string firstTermVar)
                {
                    var result = "";
                    var delim = " && ";
                    var index = 0;
                    var isFirst = true;
                    while (index != Name.Length)
                    {
                        if (Name.Length - index >= 8)
                        {
                            if (isFirst)
                            {
                                result = $"({firstTermVar} == {EqualityTerm(Name, index, 8, "ulong", "uL")})";
                            }
                            else
                            {
                                result += delim + Term(Name, index, 8, "ulong", "uL");
                            }

                            index += 8;
                        }
                        else if (Name.Length - index >= 4)
                        {
                            if (isFirst)
                            {
                                result = $"({firstTermVar} == {EqualityTerm(Name, index, 4, "uint", "u")})";
                            }
                            else
                            {
                                result += delim + Term(Name, index, 4, "uint", "u");
                            }
                            index += 4;
                        }
                        else if (Name.Length - index >= 2)
                        {
                            if (isFirst)
                            {
                                result = $"({firstTermVar} == {EqualityTerm(Name, index, 2, "ushort", "u")})";
                            }
                            else
                            {
                                result += delim + Term(Name, index, 2, "ushort", "u");
                            }
                            index += 2;
                        }
                        else
                        {
                            if (isFirst)
                            {
                                result = $"({firstTermVar} == {EqualityTerm(Name, index, 1, "byte", "u")})";
                            }
                            else
                            {
                                result += delim + Term(Name, index, 1, "byte", "u");
                            }
                            index += 1;
                        }

                        isFirst = false;
                    }
                    return result;
                }
            }

            public string EqualIgnoreCaseBytesFirstTerm()
            {
                var result = "";
                if (Name.Length >= 8)
                {
                    result = Term(Name, 0, 8, "ulong", "uL");
                }
                else if (Name.Length >= 4)
                {
                    result = Term(Name, 0, 4, "uint", "u");
                }
                else if (Name.Length >= 2)
                {
                    result = Term(Name, 0, 2, "ushort", "u");
                }
                else
                {
                    result = Term(Name, 0, 1, "byte", "u");
                }

                return result;
            }

            public string EqualIgnoreCaseBytesSecondTermOnwards()
            {
                var result = "";
                var delim = "";
                var index = 0;
                var isFirst = true;
                while (index != Name.Length)
                {
                    if (Name.Length - index >= 8)
                    {
                        if (!isFirst)
                        {
                            result += delim + Term(Name, index, 8, "ulong", "uL");
                        }

                        index += 8;
                    }
                    else if (Name.Length - index >= 4)
                    {
                        if (!isFirst)
                        {
                            result += delim + Term(Name, index, 4, "uint", "u");
                        }
                        index += 4;
                    }
                    else if (Name.Length - index >= 2)
                    {
                        if (!isFirst)
                        {
                            result += delim + Term(Name, index, 2, "ushort", "u");
                        }
                        index += 2;
                    }
                    else
                    {
                        if (!isFirst)
                        {
                            result += delim + Term(Name, index, 1, "byte", "u");
                        }
                        index += 1;
                    }

                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        delim = " && ";
                    }
                }
                return result;
            }
        }

        public static string GeneratedFile()
        {

            var requestHeaders = RequestHeaders;
            Debug.Assert(requestHeaders.Length <= 64);
            Debug.Assert(requestHeaders.Max(x => x.Index) <= 62);

            // 63 for responseHeaders as it steals one bit for Content-Length in CopyTo(ref MemoryPoolIterator output)
            var responseHeaders = ResponseHeaders;
            Debug.Assert(responseHeaders.Length <= 63);
            Debug.Assert(responseHeaders.Count(x => x.Index == 63) == 1);

            var responseTrailers = ResponseTrailers;

            var allHeaderNames = RequestHeaders.Concat(ResponseHeaders).Concat(ResponseTrailers)
                .Select(h => h.Identifier).Distinct().OrderBy(n => n, StringComparer.InvariantCulture).ToArray();

            var loops = new[]
            {
                new
                {
                    Headers = requestHeaders,
                    HeadersByLength = requestHeaders.OrderBy(x => x.Name.Length).GroupBy(x => x.Name.Length),
                    ClassName = "HttpRequestHeaders",
                    Bytes = default(byte[])
                },
                new
                {
                    Headers = responseHeaders,
                    HeadersByLength = responseHeaders.OrderBy(x => x.Name.Length).GroupBy(x => x.Name.Length),
                    ClassName = "HttpResponseHeaders",
                    Bytes = responseHeaders.SelectMany(header => header.Bytes).ToArray()
                },
                new
                {
                    Headers = responseTrailers,
                    HeadersByLength = responseTrailers.OrderBy(x => x.Name.Length).GroupBy(x => x.Name.Length),
                    ClassName = "HttpResponseTrailers",
                    Bytes = responseTrailers.SelectMany(header => header.Bytes).ToArray()
                }
            };
            foreach (var loop in loops.Where(l => l.Bytes != null))
            {
                var offset = 0;
                foreach (var header in loop.Headers)
                {
                    header.BytesOffset = offset;
                    header.BytesCount += header.Bytes.Length;
                    offset += header.BytesCount;
                }
            }
            var s = $@"// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Buffers;
using System.IO.Pipelines;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;

#nullable enable

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http
{{
    internal enum KnownHeaderType
    {{
        Unknown,{Each(allHeaderNames, n => @"
        " + n + ",")}
    }}

    internal partial class HttpHeaders
    {{
        {GetHeaderLookup()}
    }}
{Each(loops, loop => $@"
    internal partial class {loop.ClassName} : IHeaderDictionary
    {{{(loop.Bytes != null ?
        $@"
        private static ReadOnlySpan<byte> HeaderBytes => new byte[]
        {{
            {Each(loop.Bytes, b => $"{b},")}
        }};"
        : "")}
        private HeaderReferences _headers;
{Each(loop.Headers.Where(header => header.ExistenceCheck), header => $@"
        public bool Has{header.Identifier} => {header.TestBit()};")}
{Each(loop.Headers.Where(header => header.FastCount), header => $@"
        public int {header.Identifier}Count => _headers._{header.Identifier}.Count;")}
{Each(loop.Headers.Where(header => Array.IndexOf(InternalHeaderAccessors, header.Name) >= 0), header => $@"
        public {(header.Name == HeaderNames.Connection ? "override " : "")}StringValues Header{header.Identifier}
        {{{(header.Name == HeaderNames.ContentLength ? $@"
            get
            {{
                StringValues value = default;
                if (_contentLength.HasValue)
                {{
                    value = new StringValues(HeaderUtilities.FormatNonNegativeInt64(_contentLength.Value));
                }}
                return value;
            }}
            set
            {{
                _contentLength = ParseContentLength(value);
            }}" : $@"
            get
            {{
                StringValues value = default;
                if ({header.TestBit()})
                {{
                    value = _headers._{header.Identifier};
                }}
                return value;
            }}
            set
            {{
                {header.SetBit()};
                _headers._{header.Identifier} = value; {(header.EnhancedSetter == false ? "" : $@"
                _headers._raw{header.Identifier} = null;")}
            }}")}
        }}")}
        {Each(loop.Headers.Where(header => header.Name != HeaderNames.ContentLength && !NonApiHeaders.Contains(header.Identifier)), header => $@"
        StringValues IHeaderDictionary.{header.Identifier}
        {{
            get
            {{
                var value = _headers._{header.Identifier};
                if ({header.TestBit()})
                {{
                    return value;
                }}
                return default;
            }}
            set
            {{
                if (_isReadOnly) {{ ThrowHeadersReadOnlyException(); }}

                var flag = {header.FlagBit()};
                if (value.Count > 0)
                {{{(loop.ClassName != "HttpRequestHeaders" ? $@"
                    ValidateHeaderValueCharacters(HeaderNames.{header.Identifier}, value, EncodingSelector);" : "")}
                    _bits |= flag;
                    _headers._{header.Identifier} = value;
                }}
                else
                {{
                    _bits &= ~flag;
                    _headers._{header.Identifier} = default;
                }}{(header.EnhancedSetter == false ? "" : $@"
                    _headers._raw{header.Identifier} = null;")}
            }}
        }}")}
        {Each(ApiHeaderNames.Where(header => header != "ContentLength" && !loop.Headers.Select(kh => kh.Identifier).Contains(header)), header => $@"
        StringValues IHeaderDictionary.{header}
        {{
            get
            {{
                StringValues value = default;
                if (!TryGetUnknown(HeaderNames.{header}, ref value))
                {{
                    value = default;
                }}
                return value;
            }}
            set
            {{
                if (_isReadOnly) {{ ThrowHeadersReadOnlyException(); }}{(loop.ClassName != "HttpRequestHeaders" ? $@"
                ValidateHeaderValueCharacters(HeaderNames.{header}, value, EncodingSelector);" : "")}
                SetValueUnknown(HeaderNames.{header}, value);
            }}
        }}")}
{Each(loop.Headers.Where(header => header.EnhancedSetter), header => $@"
        public void SetRaw{header.Identifier}(StringValues value, byte[] raw)
        {{
            {header.SetBit()};
            _headers._{header.Identifier} = value;
            _headers._raw{header.Identifier} = raw;
        }}")}
        protected override int GetCountFast()
        {{
            return (_contentLength.HasValue ? 1 : 0 ) + BitOperations.PopCount((ulong)_bits) + (MaybeUnknown?.Count ?? 0);
        }}

        protected override bool TryGetValueFast(string key, out StringValues value)
        {{
            value = default;
            switch (key.Length)
            {{{Each(loop.HeadersByLength, byLength => $@"
                case {byLength.Key}:
                {{{Each(byLength.OrderBy(h => !h.PrimaryHeader), header => $@"
                    if (ReferenceEquals(HeaderNames.{header.Identifier}, key))
                    {{{(header.Name == HeaderNames.ContentLength ? @"
                        if (_contentLength.HasValue)
                        {
                            value = HeaderUtilities.FormatNonNegativeInt64(_contentLength.Value);
                            return true;
                        }
                        return false;" : $@"
                        if ({header.TestBit()})
                        {{
                            value = _headers._{header.Identifier};
                            return true;
                        }}
                        return false;")}
                    }}")}
{Each(byLength.OrderBy(h => !h.PrimaryHeader), header => $@"
                    if (HeaderNames.{header.Identifier}.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {{{(header.Name == HeaderNames.ContentLength ? @"
                        if (_contentLength.HasValue)
                        {
                            value = HeaderUtilities.FormatNonNegativeInt64(_contentLength.Value);
                            return true;
                        }
                        return false;" : $@"
                        if ({header.TestBit()})
                        {{
                            value = _headers._{header.Identifier};
                            return true;
                        }}
                        return false;")}
                    }}")}
                    break;
                }}")}
            }}

            return TryGetUnknown(key, ref value);
        }}

        protected override void SetValueFast(string key, StringValues value)
        {{{(loop.ClassName != "HttpRequestHeaders" ? @"
            ValidateHeaderValueCharacters(key, value, EncodingSelector);" : "")}
            switch (key.Length)
            {{{Each(loop.HeadersByLength, byLength => $@"
                case {byLength.Key}:
                {{{Each(byLength.OrderBy(h => !h.PrimaryHeader), header => $@"
                    if (ReferenceEquals(HeaderNames.{header.Identifier}, key))
                    {{{(header.Name == HeaderNames.ContentLength ? $@"
                        _contentLength = ParseContentLength(value.ToString());" : $@"
                        {header.SetBit()};
                        _headers._{header.Identifier} = value;{(header.EnhancedSetter == false ? "" : $@"
                        _headers._raw{header.Identifier} = null;")}")}
                        return;
                    }}")}
{Each(byLength.OrderBy(h => !h.PrimaryHeader), header => $@"
                    if (HeaderNames.{header.Identifier}.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {{{(header.Name == HeaderNames.ContentLength ? $@"
                        _contentLength = ParseContentLength(value.ToString());" : $@"
                        {header.SetBit()};
                        _headers._{header.Identifier} = value;{(header.EnhancedSetter == false ? "" : $@"
                        _headers._raw{header.Identifier} = null;")}")}
                        return;
                    }}")}
                    break;
                }}")}
            }}

            SetValueUnknown(key, value);
        }}

        protected override bool AddValueFast(string key, StringValues value)
        {{{(loop.ClassName != "HttpRequestHeaders" ? @"
            ValidateHeaderValueCharacters(key, value, EncodingSelector);" : "")}
            switch (key.Length)
            {{{Each(loop.HeadersByLength, byLength => $@"
                case {byLength.Key}:
                {{{Each(byLength.OrderBy(h => !h.PrimaryHeader), header => $@"
                    if (ReferenceEquals(HeaderNames.{header.Identifier}, key))
                    {{{(header.Name == HeaderNames.ContentLength ? $@"
                        if (!_contentLength.HasValue)
                        {{
                            _contentLength = ParseContentLength(value);
                            return true;
                        }}
                        return false;" : $@"
                        if ({header.TestNotBit()})
                        {{
                            {header.SetBit()};
                            _headers._{header.Identifier} = value;{(header.EnhancedSetter == false ? "" : $@"
                            _headers._raw{header.Identifier} = null;")}
                            return true;
                        }}
                        return false;")}
                    }}")}
    {Each(byLength.OrderBy(h => !h.PrimaryHeader), header => $@"
                    if (HeaderNames.{header.Identifier}.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {{{(header.Name == HeaderNames.ContentLength ? $@"
                        if (!_contentLength.HasValue)
                        {{
                            _contentLength = ParseContentLength(value);
                            return true;
                        }}
                        return false;" : $@"
                        if ({header.TestNotBit()})
                        {{
                            {header.SetBit()};
                            _headers._{header.Identifier} = value;{(header.EnhancedSetter == false ? "" : $@"
                            _headers._raw{header.Identifier} = null;")}
                            return true;
                        }}
                        return false;")}
                    }}")}
                    break;
                }}")}
            }}

            return AddValueUnknown(key, value);
        }}

        protected override bool RemoveFast(string key)
        {{
            switch (key.Length)
            {{{Each(loop.HeadersByLength, byLength => $@"
                case {byLength.Key}:
                {{{Each(byLength.OrderBy(h => !h.PrimaryHeader), header => $@"
                    if (ReferenceEquals(HeaderNames.{header.Identifier}, key))
                    {{{(header.Name == HeaderNames.ContentLength ? @"
                        if (_contentLength.HasValue)
                        {
                            _contentLength = null;
                            return true;
                        }
                        return false;" : $@"
                        if ({header.TestBit()})
                        {{
                            {header.ClearBit()};
                            _headers._{header.Identifier} = default(StringValues);{(header.EnhancedSetter == false ? "" : $@"
                            _headers._raw{header.Identifier} = null;")}
                            return true;
                        }}
                        return false;")}
                    }}")}
    {Each(byLength.OrderBy(h => !h.PrimaryHeader), header => $@"
                    if (HeaderNames.{header.Identifier}.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {{{(header.Name == HeaderNames.ContentLength ? @"
                        if (_contentLength.HasValue)
                        {
                            _contentLength = null;
                            return true;
                        }
                        return false;" : $@"
                        if ({header.TestBit()})
                        {{
                            {header.ClearBit()};
                            _headers._{header.Identifier} = default(StringValues);{(header.EnhancedSetter == false ? "" : $@"
                            _headers._raw{header.Identifier} = null;")}
                            return true;
                        }}
                        return false;")}
                    }}")}
                    break;
                }}")}
            }}

            return RemoveUnknown(key);
        }}
{(loop.ClassName != "HttpRequestHeaders" ?
 $@"        protected override void ClearFast()
        {{
            MaybeUnknown?.Clear();
            _contentLength = null;
            var tempBits = _bits;
            _bits = 0;
            if(BitOperations.PopCount((ulong)tempBits) > 12)
            {{
                _headers = default(HeaderReferences);
                return;
            }}
            {Each(loop.Headers.Where(header => header.Identifier != "ContentLength").OrderBy(h => !h.PrimaryHeader), header => $@"
            if ({header.TestTempBit()})
            {{
                _headers._{header.Identifier} = default;
                if({header.TestNotTempBit()})
                {{
                    return;
                }}
                tempBits &= ~{"0x" + (1L << header.Index).ToString("x", CultureInfo.InvariantCulture)}L;
            }}
            ")}
        }}
" :
$@"        private void Clear(long bitsToClear)
        {{
            var tempBits = bitsToClear;
            {Each(loop.Headers.Where(header => header.Identifier != "ContentLength").OrderBy(h => !h.PrimaryHeader), header => $@"
            if ({header.TestTempBit()})
            {{
                _headers._{header.Identifier} = default;
                if({header.TestNotTempBit()})
                {{
                    return;
                }}
                tempBits &= ~{"0x" + (1L << header.Index).ToString("x" , CultureInfo.InvariantCulture)}L;
            }}
            ")}
        }}
")}
        protected override bool CopyToFast(KeyValuePair<string, StringValues>[] array, int arrayIndex)
        {{
            if (arrayIndex < 0)
            {{
                return false;
            }}
            {Each(loop.Headers.Where(header => header.Identifier != "ContentLength"), header => $@"
                if ({header.TestBit()})
                {{
                    if (arrayIndex == array.Length)
                    {{
                        return false;
                    }}
                    array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.{header.Identifier}, _headers._{header.Identifier});
                    ++arrayIndex;
                }}")}
                if (_contentLength.HasValue)
                {{
                    if (arrayIndex == array.Length)
                    {{
                        return false;
                    }}
                    array[arrayIndex] = new KeyValuePair<string, StringValues>(HeaderNames.ContentLength, HeaderUtilities.FormatNonNegativeInt64(_contentLength.Value));
                    ++arrayIndex;
                }}
            ((ICollection<KeyValuePair<string, StringValues>>?)MaybeUnknown)?.CopyTo(array, arrayIndex);

            return true;
        }}
        {(loop.ClassName == "HttpResponseHeaders" ? $@"
        internal bool HasInvalidH2H3Headers => (_bits & {InvalidH2H3ResponseHeadersBits}) != 0;
        internal void ClearInvalidH2H3Headers()
        {{
            _bits &= ~{InvalidH2H3ResponseHeadersBits};
        }}
        internal unsafe void CopyToFast(ref BufferWriter<PipeWriter> output)
        {{
            var tempBits = (ulong)_bits;
            // Set exact next
            var next = BitOperations.TrailingZeroCount(tempBits);

            // Output Content-Length now as it isn't contained in the bit flags.
            if (_contentLength.HasValue)
            {{
                output.Write(HeaderBytes.Slice(640, 18));
                output.WriteNumeric((ulong)ContentLength.GetValueOrDefault());
            }}
            if (tempBits == 0)
            {{
                return;
            }}

            ref readonly StringValues values = ref Unsafe.AsRef<StringValues>(null);
            do
            {{
                int keyStart;
                int keyLength;
                var headerName = string.Empty;
                switch (next)
                {{{Each(loop.Headers.OrderBy(h => h.Index).Where(h => h.Identifier != "ContentLength"), header => $@"
                    case {header.Index}: // Header: ""{header.Name}""
                        Debug.Assert({header.TestTempBit()});{(header.EnhancedSetter == false ? $@"
                        values = ref _headers._{header.Identifier};
                        keyStart = {header.BytesOffset};
                        keyLength = {header.BytesCount};" : $@"
                        if (_headers._raw{header.Identifier} != null)
                        {{
                            // Clear and set next as not using common output.
                            tempBits ^= {"0x" + (1L << header.Index).ToString("x", CultureInfo.InvariantCulture)}L;
                            next = BitOperations.TrailingZeroCount(tempBits);
                            output.Write(_headers._raw{header.Identifier});
                            continue; // Jump to next, already output header
                        }}
                        else
                        {{
                            values = ref _headers._{header.Identifier};
                            keyStart = {header.BytesOffset};
                            keyLength = {header.BytesCount};
                            headerName = HeaderNames.{header.Identifier};
                        }}")}
                        break; // OutputHeader
")}
                    default:
                        ThrowInvalidHeaderBits();
                        return;
                }}

                // OutputHeader
                {{
                    // Clear bit
                    tempBits ^= (1UL << next);
                    var encoding = ReferenceEquals(EncodingSelector, KestrelServerOptions.DefaultHeaderEncodingSelector)
                        ? null : EncodingSelector(headerName);
                    var valueCount = values.Count;
                    Debug.Assert(valueCount > 0);

                    var headerKey = HeaderBytes.Slice(keyStart, keyLength);
                    for (var i = 0; i < valueCount; i++)
                    {{
                        var value = values[i];
                        if (value != null)
                        {{
                            output.Write(headerKey);
                            if (encoding is null)
                            {{
                                output.WriteAscii(value);
                            }}
                            else
                            {{
                                output.WriteEncoded(value, encoding);
                            }}
                        }}
                    }}
                    // Set exact next
                    next = BitOperations.TrailingZeroCount(tempBits);
                }}
            }} while (tempBits != 0);
        }}" : "")}{(loop.ClassName == "HttpRequestHeaders" ? $@"
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void Append(ReadOnlySpan<byte> name, ReadOnlySpan<byte> value)
        {{
            ref byte nameStart = ref MemoryMarshal.GetReference(name);
            var nameStr = string.Empty;
            ref StringValues values = ref Unsafe.AsRef<StringValues>(null);
            var flag = 0L;

            // Does the name match any ""known"" headers
            {AppendSwitch(loop.Headers.GroupBy(x => x.Name.Length).OrderBy(x => x.Key))}

            if (flag != 0)
            {{
                {AppendValue()}
            }}
            else
            {{
                // The header was not one of the ""known"" headers.
                // Convert value to string first, because passing two spans causes 8 bytes stack zeroing in
                // this method with rep stosd, which is slower than necessary.
                nameStr = name.GetHeaderName();
                var valueStr = value.GetRequestHeaderString(nameStr, EncodingSelector);
                AppendUnknownHeaders(nameStr, valueStr);
            }}
        }}

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe bool TryHPackAppend(int index, ReadOnlySpan<byte> value)
        {{
            ref StringValues values = ref Unsafe.AsRef<StringValues>(null);
            var nameStr = string.Empty;
            var flag = 0L;

            // Does the HPack static index match any ""known"" headers
            {AppendHPackSwitch(GroupHPack(loop.Headers))}

            if (flag != 0)
            {{
                {AppendValue(returnTrue: true)}
                return true;
            }}
            else
            {{
                return false;
            }}
        }}" : "")}

        private struct HeaderReferences
        {{{Each(loop.Headers.Where(header => header.Identifier != "ContentLength"), header => @"
            public StringValues _" + header.Identifier + ";")}
            {Each(loop.Headers.Where(header => header.EnhancedSetter), header => @"
            public byte[]? _raw" + header.Identifier + ";")}
        }}

        public partial struct Enumerator
        {{
            // Compiled to Jump table
            public bool MoveNext()
            {{
                switch (_next)
                {{{Each(loop.Headers.Where(header => header.Identifier != "ContentLength"), header => $@"
                    case {header.Index}:
                        goto Header{header.Identifier};")}
                    {(!loop.ClassName.Contains("Trailers") ? $@"case {loop.Headers.Length - 1}:
                        goto HeaderContentLength;" : "")}
                    default:
                        goto ExtraHeaders;
                }}
                {Each(loop.Headers.Where(header => header.Identifier != "ContentLength"), header => $@"
                Header{header.Identifier}: // case {header.Index}
                    if ({header.TestBit()})
                    {{
                        _current = new KeyValuePair<string, StringValues>(HeaderNames.{header.Identifier}, _collection._headers._{header.Identifier});
                        {(loop.ClassName.Contains("Request") ? "" : @$"_currentKnownType = KnownHeaderType.{header.Identifier};
                        ")}_next = {header.Index + 1};
                        return true;
                    }}")}
                {(!loop.ClassName.Contains("Trailers") ? $@"HeaderContentLength: // case {loop.Headers.Length - 1}
                    if (_collection._contentLength.HasValue)
                    {{
                        _current = new KeyValuePair<string, StringValues>(HeaderNames.ContentLength, HeaderUtilities.FormatNonNegativeInt64(_collection._contentLength.Value));
                        {(loop.ClassName.Contains("Request") ? "" : @"_currentKnownType = KnownHeaderType.ContentLength;
                        ")}_next = {loop.Headers.Length};
                        return true;
                    }}" : "")}
                ExtraHeaders:
                    if (!_hasUnknown || !_unknownEnumerator.MoveNext())
                    {{
                        _current = default(KeyValuePair<string, StringValues>);
                        {(loop.ClassName.Contains("Request") ? "" : @"_currentKnownType = default;
                        ")}return false;
                    }}
                    _current = _unknownEnumerator.Current;
                    {(loop.ClassName.Contains("Request") ? "" : @"_currentKnownType = KnownHeaderType.Unknown;
                    ")}return true;
            }}
        }}
    }}
")}}}";

            // Temporary workaround for https://github.com/dotnet/runtime/issues/55688
            return s.Replace("{{", "{").Replace("}}", "}");
        }

        private static string GetHeaderLookup()
        {
            return @$"private readonly static HashSet<string> _internedHeaderNames = new HashSet<string>({DefinedHeaderNames.Length}, StringComparer.OrdinalIgnoreCase)
        {{{Each(DefinedHeaderNames, (h) => @"
            HeaderNames." + h + ",")}
        }};";
        }

        private static IEnumerable<HPackGroup> GroupHPack(KnownHeader[] headers)
        {
            var staticHeaders = new (int Index, HeaderField HeaderField)[H2StaticTable.Count];
            for (var i = 0; i < H2StaticTable.Count; i++)
            {
                staticHeaders[i] = (i + 1, H2StaticTable.Get(i));
            }

            var groupedHeaders = staticHeaders.GroupBy(h => Encoding.ASCII.GetString(h.HeaderField.Name)).Select(g =>
            {
                return new HPackGroup
                {
                    Name = g.Key,
                    Header = headers.SingleOrDefault(knownHeader => string.Equals(knownHeader.Name, g.Key, StringComparison.OrdinalIgnoreCase)),
                    HPackStaticTableIndexes = g.Select(h => h.Index).ToArray()
                };
            }).Where(g => g.Header != null).ToList();

            return groupedHeaders;
        }

        private class HPackGroup
        {
            public int[] HPackStaticTableIndexes { get; set; }
            public KnownHeader Header { get; set; }
            public string Name { get; set; }
        }

        private class KnownHeaderComparer : IComparer<KnownHeader>
        {
            public static readonly KnownHeaderComparer Instance = new KnownHeaderComparer();

            public int Compare(KnownHeader x, KnownHeader y)
            {
                // Primary headers appear first
                if (x.PrimaryHeader && !y.PrimaryHeader)
                {
                    return -1;
                }
                if (y.PrimaryHeader && !x.PrimaryHeader)
                {
                    return 1;
                }

                // Then alphabetical
                return StringComparer.InvariantCulture.Compare(x.Name, y.Name);
            }
        }
    }
}
