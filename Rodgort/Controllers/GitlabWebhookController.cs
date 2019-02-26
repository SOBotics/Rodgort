using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Disposable = System.Reactive.Disposables.Disposable;

namespace Rodgort.Controllers
{
    [Route("api/[controller]")]
    public class GitlabWebhookController : Controller
    {
        private readonly ILogger<GitlabWebhookController> _logger;

        public static IObservable<bool> PipelineStatus;
        private static Action<bool> _updatePipelinesStatus;
        private static int _runCount;
        private static readonly object _runCountLocker = new object();

        static GitlabWebhookController()
        {
            var replaySubject = new ReplaySubject<bool>(1);
            Observable.Create<bool>(o =>
            {
                _updatePipelinesStatus = o.OnNext;
                return Disposable.Empty;
            }).Subscribe(replaySubject);

            PipelineStatus = replaySubject;
        }

        public GitlabWebhookController(ILogger<GitlabWebhookController> logger)
        {
            _logger = logger;
        }

        [HttpPost("pipelines")]
        public void ProcessPipelinesWebhook([FromBody] PipelineHook request)
        {
            var startingStates = new[] {"running"};
            var endingStates = new[] {"canceled", "success", "failed" };

            _logger.LogDebug("Received: " + JsonConvert.SerializeObject(request));
            if (!string.IsNullOrEmpty(request?.ObjectAttributes?.Status) && !string.IsNullOrEmpty(request?.ObjectAttributes?.Ref))
            {
                if (!string.Equals(request.ObjectAttributes.Ref, "master"))
                {
                    _logger.LogDebug("Not processing, as the build wasn't on master");
                    return;
                }

                int currentRunCount;
                lock (_runCountLocker)
                {
                    if (startingStates.Contains(request.ObjectAttributes.Status))
                        _runCount++;
                    else if (endingStates.Contains(request.ObjectAttributes.Status))
                        _runCount--;

                    if (_runCount < 0)
                        _runCount = 0;

                    currentRunCount = _runCount;
                }
                _updatePipelinesStatus(currentRunCount > 0);
            }
            else
            {
                _logger.LogDebug("Failed to process webhook: " + JsonConvert.SerializeObject(request));
            }
        }
    }

    public class PipelineHook
    {
        [JsonProperty("object_kind")]
        public string ObjectKind { get; set; }

        [JsonProperty("object_attributes")]
        public PiplineHookObjectAttributes ObjectAttributes { get; set; }
    }

    public class PiplineHookObjectAttributes
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public string Ref { get; set; }
    }
}
