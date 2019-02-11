using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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

        public static IObservable<string> PipelineStatus;
        private static Action<string> _updatePipelinesStatus;

        static GitlabWebhookController()
        {
            var replaySubject = new ReplaySubject<string>(1);
            Observable.Create<string>(o =>
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
            _logger.LogDebug("Received: " + JsonConvert.SerializeObject(request));
            if (!string.IsNullOrEmpty(request?.ObjectAttributes?.Status))
                _updatePipelinesStatus(request.ObjectAttributes.Status);
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
    }
}
