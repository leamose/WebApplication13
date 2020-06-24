using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using App.Metrics;
using App.Metrics.Timer;
using App.Metrics.Counter;
using App.Metrics.Meter;
using System.Diagnostics;
using App.Metrics.Gauge;

namespace WebApplication13.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        private static readonly Random Rnd = new Random();

        private readonly IMetrics _metrics;

        public ValuesController(IMetrics metrics)
        {
            if (metrics == null)
            {
                throw new ArgumentNullException(nameof(metrics));
            }

            _metrics = metrics;
        }

        // private IMetrics _metrics;
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {

            var httpStatusMeter = new MeterOptions
            {
                Name = "Http Status_Leandro",
                MeasurementUnit = Unit.Calls
            };

            _metrics.Measure.Meter.Mark(httpStatusMeter, "200");
            _metrics.Measure.Meter.Mark(httpStatusMeter, "500");
            _metrics.Measure.Meter.Mark(httpStatusMeter, "401");

            //_metrics.Measure.Counter.Increment(httpStatusCounter, "200");
            //_metrics.Measure.Counter.Increment(httpStatusCounter, "500");
            //_metrics.Measure.Counter.Increment(httpStatusCounter, "401");
           

            _metrics.Measure.Counter.Increment(MetricsRegistry.Counters.TestCounter);
            _metrics.Measure.Counter.Increment(MetricsRegistry.Counters.TestCounter, 4);
            _metrics.Measure.Counter.Decrement(MetricsRegistry.Counters.TestCounter, 2);

            var process = Process.GetCurrentProcess();
            var physicalMemoryGauge = new FunctionGauge(() => process.WorkingSet64);

            _metrics.Measure.Gauge.SetValue(MetricsRegistry.Gauges.TestGauge, () => physicalMemoryGauge.Value);

            _metrics.Measure.Gauge.SetValue(MetricsRegistry.Gauges.DerivedGauge,
                () => new DerivedGauge(physicalMemoryGauge, g => g / 1024.0 / 1024.0));

            var cacheHits = _metrics.Provider.Meter.Instance(MetricsRegistry.Meters.CacheHits);
            var calls = _metrics.Provider.Timer.Instance(MetricsRegistry.Timers.DatabaseQueryTimer);

            var cacheHit = Rnd.Next(0, 2) == 0;
            if (cacheHit)
            {
                cacheHits.Mark();
            }

            using (calls.NewContext())
            {
                Thread.Sleep(cacheHit ? 10 : 100);
            }

            using (_metrics.Measure.Apdex.Track(MetricsRegistry.ApdexScores.TestApdex))
            {
                Thread.Sleep(cacheHit ? 10 : 100);
            }

            _metrics.Measure.Gauge.SetValue(MetricsRegistry.Gauges.CacheHitRatioGauge, () => new HitRatioGauge(cacheHits, calls, m => m.OneMinuteRate));

            var histogram = _metrics.Provider.Histogram.Instance(MetricsRegistry.Histograms.TestHAdvancedistogram);
            histogram.Update(Rnd.Next(1, 20));

            _metrics.Measure.Histogram.Update(MetricsRegistry.Histograms.TestHistogram, Rnd.Next(20, 40));

            _metrics.Measure.Timer.Time(MetricsRegistry.Timers.TestTimer, () => Thread.Sleep(20), "value1");
            _metrics.Measure.Timer.Time(MetricsRegistry.Timers.TestTimer, () => Thread.Sleep(25), "value2");

            using (_metrics.Measure.Timer.Time(MetricsRegistry.Timers.TestTimerTwo))
            {
                Thread.Sleep(15);
            }

            using (_metrics.Measure.Timer.Time(MetricsRegistry.Timers.TestTimerTwo, "value1"))
            {
                Thread.Sleep(20);
            }

            using (_metrics.Measure.Timer.Time(MetricsRegistry.Timers.TestTimerTwo, "value2"))
            {
                Thread.Sleep(25);
            }


            return new string[] { "value1", "value2" };

            //var httpStatusMeter = new MeterOptions
            //{
            //    Name = "Http Status",
            //    MeasurementUnit = Unit.Calls
            //};

            //_metrics.Measure.Meter.Mark(httpStatusMeter, "200");
            //_metrics.Measure.Meter.Mark(httpStatusMeter, "500");
            //_metrics.Measure.Meter.Mark(httpStatusMeter, "401");
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value, [FromServices] IMetrics _metrics)
        {
            Random random = new Random();

            //TimerOptions requestTimer = new TimerOptions()
            //{
            //    Name = "teste metricas",
            //    MeasurementUnit = Unit.Requests,
            //    DurationUnit = TimeUnit.Milliseconds,
            //    RateUnit = TimeUnit.Milliseconds
            //};

            //using (_metrics.Measure.Timer.Time(requestTimer))
            //{
            //    Thread.Sleep(random.Next(10) * 1000);
            //}
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
