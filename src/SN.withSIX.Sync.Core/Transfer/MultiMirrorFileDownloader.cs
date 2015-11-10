// <copyright company="SIX Networks GmbH" file="MultiMirrorFileDownloader.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using SmartAssembly.Attributes;
using SN.withSIX.Sync.Core.Transfer.MirrorSelectors;
using SN.withSIX.Sync.Core.Transfer.Specs;

namespace SN.withSIX.Sync.Core.Transfer
{
    public class MultiMirrorFileDownloader : IMultiMirrorFileDownloader
    {
        const int MillisecondsTimeout = 1000;
        readonly IFileDownloader _downloader;
        readonly IMirrorSelector _mirrorStrategy;

        public MultiMirrorFileDownloader(IFileDownloader downloader, IMirrorSelector mirrorSelector) {
            _downloader = downloader;
            _mirrorStrategy = mirrorSelector;
        }

        public void Download(MultiMirrorFileDownloadSpec spec) {
            spec.Start();
            try {
                while (true) {
                    var host = _mirrorStrategy.GetHost();
                    if (TryDownload(spec, host))
                        break;
                    Thread.Sleep(MillisecondsTimeout);
                }
            } catch (Exception) {
                spec.Fail();
                throw;
            }
        }

        public async Task DownloadAsync(MultiMirrorFileDownloadSpec spec) {
            spec.Start();
            try {
                while (true) {
                    var host = _mirrorStrategy.GetHost();
                    spec.UpdateHost(host);
                    if (await TryDownloadAsync(spec, host).ConfigureAwait(false))
                        break;
                    await Task.Delay(MillisecondsTimeout).ConfigureAwait(false);
                }
            } catch (Exception) {
                spec.Fail();
                throw;
            }
        }

        public void Download(MultiMirrorFileDownloadSpec spec, CancellationToken token) {
            spec.Start();
            try {
                while (true) {
                    token.ThrowIfCancellationRequested();
                    var host = _mirrorStrategy.GetHost();
                    if (TryDownload(spec, host))
                        break;
                    Thread.Sleep(MillisecondsTimeout);
                }
            } catch (Exception) {
                spec.Fail();
                throw;
            }
        }

        public async Task DownloadAsync(MultiMirrorFileDownloadSpec spec, CancellationToken token) {
            spec.Start();
            try {
                while (true) {
                    token.ThrowIfCancellationRequested();
                    var host = _mirrorStrategy.GetHost();
                    spec.UpdateHost(host);
                    if (await TryDownloadAsync(spec, host).ConfigureAwait(false))
                        break;
                    await Task.Delay(MillisecondsTimeout, token).ConfigureAwait(false);
                }
            } catch (Exception) {
                spec.Fail();
                throw;
            }
        }

        bool TryDownload(MultiMirrorFileDownloadSpec spec, Uri host) {
            try {
                TryDownloadFile(spec, host);
                spec.End();
                return true;
            } catch (TransferException) {} catch (VerificationError) {}
            return false;
        }

        async Task<bool> TryDownloadAsync(MultiMirrorFileDownloadSpec spec, Uri host) {
            try {
                await TryDownloadFileAsync(spec, host).ConfigureAwait(false);
                spec.End();
                return true;
            } catch (TransferException) {} catch (VerificationError) {}
            return false;
        }

        protected void TryDownloadFile(MultiMirrorFileDownloadSpec spec, Uri host) {
            try {
                _downloader.Download(BuildSpec(spec, host));
            } catch (Exception) {
                _mirrorStrategy.Failure(host);
                throw;
            }
            _mirrorStrategy.Success(host);
        }

        protected async Task TryDownloadFileAsync(MultiMirrorFileDownloadSpec spec, Uri host) {
            try {
                await _downloader.DownloadAsync(BuildSpec(spec, host))
                    .ConfigureAwait(false);
            } catch (Exception) {
                _mirrorStrategy.Failure(host);
                throw;
            }
            _mirrorStrategy.Success(host);
        }

        static FileDownloadSpec BuildSpec(MultiMirrorFileDownloadSpec spec, Uri host) {
            return spec.Progress == null
                ? new FileDownloadSpec(spec.GetUri(host), spec.LocalFile) {
                    Verification = spec.Verification,
                    CancellationToken = spec.CancellationToken
                }
                : new FileDownloadSpec(spec.GetUri(host), spec.LocalFile, spec.Progress) {
                    Verification = spec.Verification,
                    CancellationToken = spec.CancellationToken
                };
        }
    }

    [DoNotObfuscate]
    public class VerificationError : Exception
    {
        public VerificationError(string message) : base(message) {}
    }
}