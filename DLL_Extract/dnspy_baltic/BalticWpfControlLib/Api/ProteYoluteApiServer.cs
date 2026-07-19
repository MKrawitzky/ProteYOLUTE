// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using BalticWpfControlLib.Data;

namespace BalticWpfControlLib.Api
{
    /// <summary>
    /// Embedded REST API and WebSocket server for ProteYOLUTE.
    /// Serves status, data, and a web dashboard on localhost:8742.
    /// </summary>
    public class ProteYoluteApiServer : IDisposable
    {
        private HttpListener _listener;
        private CancellationTokenSource _cts;
        private readonly ProteYoluteDb _db;
        private readonly SmartColumnManager _columnManager;
        private readonly PressureAnomalyDetector _anomalyDetector;
        private readonly JavaScriptSerializer _json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        private readonly List<StreamWriter> _sseClients = new List<StreamWriter>();
        private readonly object _sseLock = new object();
        private bool _disposed;

        public int Port { get; }
        public bool IsRunning { get; private set; }

        // Live system state (updated by Lua bridge)
        public SystemStatus CurrentStatus { get; set; } = new SystemStatus();

        public ProteYoluteApiServer(ProteYoluteDb db, SmartColumnManager columnManager,
            PressureAnomalyDetector anomalyDetector, int port = 8742)
        {
            _db = db;
            _columnManager = columnManager;
            _anomalyDetector = anomalyDetector;
            Port = port;

            // Subscribe to data events for SSE broadcasting
            _db.DataLogged += OnDataLogged;
        }

        public void Start()
        {
            if (IsRunning) return;

            _cts = new CancellationTokenSource();
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{Port}/");

            try
            {
                _listener.Start();
                IsRunning = true;
                Task.Run(() => ListenLoop(_cts.Token));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProteYOLUTE API server failed to start: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (!IsRunning) return;
            _cts?.Cancel();
            _listener?.Stop();
            IsRunning = false;

            lock (_sseLock)
            {
                foreach (var client in _sseClients)
                {
                    try { client.Close(); } catch { }
                }
                _sseClients.Clear();
            }
        }

        private async Task ListenLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _listener.IsListening)
            {
                try
                {
                    var ctx = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequest(ctx));
                }
                catch (ObjectDisposedException) { break; }
                catch (HttpListenerException) { break; }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"API error: {ex.Message}");
                }
            }
        }

        private void HandleRequest(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var resp = ctx.Response;
            resp.Headers.Add("Access-Control-Allow-Origin", "*");
            resp.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            resp.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            if (req.HttpMethod == "OPTIONS")
            {
                resp.StatusCode = 204;
                resp.Close();
                return;
            }

            try
            {
                string path = req.Url.AbsolutePath.TrimEnd('/').ToLowerInvariant();
                object result = null;
                int statusCode = 200;

                switch (path)
                {
                    // ─── Dashboard ────────────────────────────────────
                    case "":
                    case "/":
                    case "/dashboard":
                        ServeDashboard(resp);
                        return;

                    // ─── Server-Sent Events (real-time stream) ────────
                    case "/api/stream":
                        HandleSseConnection(ctx);
                        return;

                    // ─── System Status ────────────────────────────────
                    case "/api/status":
                        result = CurrentStatus;
                        break;

                    case "/api/health":
                        result = new
                        {
                            counters = _db.GetAllCounters(),
                            active_column = _columnManager.ActiveColumnId.HasValue
                                ? _db.GetColumnHealth(_columnManager.ActiveColumnId.Value)
                                : null,
                            active_trap = _columnManager.ActiveTrapId.HasValue
                                ? _db.GetColumnHealth(_columnManager.ActiveTrapId.Value)
                                : null,
                            alerts = _db.GetActiveAlerts().Take(20).ToList()
                        };
                        break;

                    // ─── Runs ─────────────────────────────────────────
                    case "/api/runs":
                        int limit = GetQueryInt(req, "limit", 50);
                        string status = GetQueryString(req, "status");
                        string sql = status != null
                            ? "SELECT * FROM runs WHERE status = @s ORDER BY start_time DESC LIMIT @l"
                            : "SELECT * FROM runs ORDER BY start_time DESC LIMIT @l";
                        var parms = status != null
                            ? new[] { "@s", status, "@l", limit.ToString() }
                            : new[] { "@l", limit.ToString() };
                        result = _db.Query(sql, parms);
                        break;

                    case "/api/runs/current":
                        result = _db.Query("SELECT * FROM runs WHERE status = 'running' ORDER BY start_time DESC LIMIT 1");
                        break;

                    // ─── Columns ──────────────────────────────────────
                    case "/api/columns":
                        string role = GetQueryString(req, "role");
                        result = _columnManager.GetAllColumns(role);
                        break;

                    case "/api/columns/health":
                        if (_columnManager.ActiveColumnId.HasValue)
                            result = _db.GetColumnHealth(_columnManager.ActiveColumnId.Value);
                        else
                            result = new { error = "No active column" };
                        break;

                    case "/api/columns/precheck":
                        result = _columnManager.PreRunCheck();
                        break;

                    // ─── Templates ────────────────────────────────────
                    case "/api/templates":
                        result = _db.ListTemplates();
                        break;

                    // ─── Diagnostics ──────────────────────────────────
                    case "/api/diagnostics/latest":
                        result = _db.Query(
                            "SELECT * FROM diagnostics ORDER BY timestamp DESC LIMIT 50");
                        break;

                    // ─── Calibrations ─────────────────────────────────
                    case "/api/calibrations":
                        result = _db.Query(
                            "SELECT * FROM calibrations ORDER BY timestamp DESC LIMIT 20");
                        break;

                    // ─── Alerts ───────────────────────────────────────
                    case "/api/alerts":
                        result = _db.GetActiveAlerts();
                        break;

                    case "/api/alerts/all":
                        result = _db.Query(
                            "SELECT * FROM alerts ORDER BY timestamp DESC LIMIT 100");
                        break;

                    // ─── Errors ───────────────────────────────────────
                    case "/api/errors":
                        result = _db.Query(
                            "SELECT * FROM errors ORDER BY timestamp DESC LIMIT 50");
                        break;

                    // ─── Pressure Data ────────────────────────────────
                    case "/api/pressure/recent":
                        int minutes = GetQueryInt(req, "minutes", 5);
                        long sinceMs = DateTimeOffset.UtcNow.AddMinutes(-minutes).ToUnixTimeMilliseconds();
                        result = _db.Query(
                            "SELECT * FROM pressure_log WHERE timestamp_ms > @t ORDER BY timestamp_ms",
                            new[] { "@t", sinceMs.ToString() });
                        break;

                    // ─── Audit Trail ──────────────────────────────────
                    case "/api/audit":
                        result = _db.Query(
                            "SELECT * FROM audit_log ORDER BY timestamp DESC LIMIT 100");
                        break;

                    // ─── System Info ──────────────────────────────────
                    case "/api/info":
                        result = new
                        {
                            name = "ProteYOLUTE",
                            version = "2.0.0",
                            author = "Michael Krawitzky",
                            copyright = "Copyright (c) 2025-2026 Michael Krawitzky",
                            api_version = "1.0",
                            database = _db.DbPath,
                            uptime_seconds = (DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime).TotalSeconds
                        };
                        break;

                    default:
                        // Check for parameterized routes
                        if (path.StartsWith("/api/runs/") && path.Split('/').Length == 4)
                        {
                            string runIdStr = path.Split('/')[3];
                            if (int.TryParse(runIdStr, out int runId))
                            {
                                result = new
                                {
                                    run = _db.Query("SELECT * FROM runs WHERE run_id = @r",
                                        new[] { "@r", runId.ToString() }),
                                    pressure = _db.Query(
                                        "SELECT * FROM pressure_log WHERE run_id = @r ORDER BY timestamp_ms",
                                        new[] { "@r", runId.ToString() }),
                                    flow = _db.Query(
                                        "SELECT * FROM flow_log WHERE run_id = @r ORDER BY timestamp_ms",
                                        new[] { "@r", runId.ToString() }),
                                    gradient = _db.Query(
                                        "SELECT * FROM gradient_log WHERE run_id = @r ORDER BY timestamp_ms",
                                        new[] { "@r", runId.ToString() }),
                                    valves = _db.Query(
                                        "SELECT * FROM valve_events WHERE run_id = @r ORDER BY timestamp_ms",
                                        new[] { "@r", runId.ToString() }),
                                    errors = _db.Query(
                                        "SELECT * FROM errors WHERE run_id = @r ORDER BY timestamp",
                                        new[] { "@r", runId.ToString() })
                                };
                                break;
                            }
                        }
                        statusCode = 404;
                        result = new { error = "Not found", path };
                        break;
                }

                string json = _json.Serialize(result);
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                resp.StatusCode = statusCode;
                resp.ContentType = "application/json; charset=utf-8";
                resp.ContentLength64 = buffer.Length;
                resp.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                try
                {
                    string errJson = _json.Serialize(new { error = ex.Message });
                    byte[] buffer = Encoding.UTF8.GetBytes(errJson);
                    resp.StatusCode = 500;
                    resp.ContentType = "application/json";
                    resp.ContentLength64 = buffer.Length;
                    resp.OutputStream.Write(buffer, 0, buffer.Length);
                }
                catch { }
            }
            finally
            {
                try { resp.Close(); } catch { }
            }
        }

        // ─── Server-Sent Events (Real-Time Stream) ───────────────────────

        private void HandleSseConnection(HttpListenerContext ctx)
        {
            var resp = ctx.Response;
            resp.ContentType = "text/event-stream";
            resp.Headers.Add("Cache-Control", "no-cache");
            resp.Headers.Add("Connection", "keep-alive");

            var writer = new StreamWriter(resp.OutputStream, Encoding.UTF8) { AutoFlush = true };

            lock (_sseLock)
            {
                _sseClients.Add(writer);
            }

            // Send initial status
            try
            {
                writer.WriteLine($"event: connected");
                writer.WriteLine($"data: {{\"message\":\"ProteYOLUTE stream connected\"}}");
                writer.WriteLine();

                // Keep connection alive
                while (!_cts.IsCancellationRequested)
                {
                    Thread.Sleep(15000);
                    writer.WriteLine(":keepalive");
                    writer.WriteLine();
                }
            }
            catch
            {
                lock (_sseLock)
                {
                    _sseClients.Remove(writer);
                }
            }
        }

        private void OnDataLogged(string eventType, object data)
        {
            string json = _json.Serialize(data);
            string sseMessage = $"event: {eventType}\ndata: {json}\n\n";

            lock (_sseLock)
            {
                var dead = new List<StreamWriter>();
                foreach (var client in _sseClients)
                {
                    try
                    {
                        client.Write(sseMessage);
                        client.Flush();
                    }
                    catch
                    {
                        dead.Add(client);
                    }
                }
                foreach (var d in dead)
                    _sseClients.Remove(d);
            }
        }

        // ─── Web Dashboard ───────────────────────────────────────────────

        private void ServeDashboard(HttpListenerResponse resp)
        {
            string html = GenerateDashboardHtml();
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            resp.ContentType = "text/html; charset=utf-8";
            resp.ContentLength64 = buffer.Length;
            resp.OutputStream.Write(buffer, 0, buffer.Length);
        }

        private string GenerateDashboardHtml()
        {
            // Try to load premium dashboard from file
            try
            {
                string dashPath = Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "",
                    "Api", "dashboard.html");
                if (!File.Exists(dashPath))
                {
                    // Try relative to plugin directory
                    dashPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        @"Bruker\HyStar\LcPlugin\PrivateData\Bruker proteoElute\DLL_Extract\dnspy_baltic\BalticWpfControlLib\Api\dashboard.html");
                }
                if (File.Exists(dashPath))
                    return File.ReadAllText(dashPath, System.Text.Encoding.UTF8);
            }
            catch { /* Fall through to embedded version */ }

            // Fallback: embedded minimal dashboard
            return @"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<title>ProteYOLUTE Dashboard</title>
<style>
  * { margin: 0; padding: 0; box-sizing: border-box; }
  body { font-family: 'Segoe UI', system-ui, sans-serif; background: #0a0a0f; color: #e0e0e0; }
  .header { background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%); padding: 16px 24px;
            border-bottom: 2px solid #0f3460; display: flex; align-items: center; gap: 16px; }
  .header h1 { font-size: 22px; font-weight: 600; color: #e94560; }
  .header .subtitle { color: #8888aa; font-size: 13px; }
  .header .status-dot { width: 10px; height: 10px; border-radius: 50%; background: #00ff88;
                        box-shadow: 0 0 8px #00ff88; animation: pulse 2s infinite; }
  @keyframes pulse { 0%,100% { opacity:1; } 50% { opacity:0.5; } }
  .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(380px, 1fr));
          gap: 16px; padding: 16px; }
  .card { background: #12121a; border: 1px solid #1e1e2e; border-radius: 12px;
          padding: 20px; position: relative; overflow: hidden; }
  .card::before { content: ''; position: absolute; top: 0; left: 0; right: 0; height: 3px;
                  background: linear-gradient(90deg, #e94560, #0f3460); }
  .card h2 { font-size: 14px; text-transform: uppercase; letter-spacing: 1px;
             color: #8888aa; margin-bottom: 12px; }
  .metric { display: flex; justify-content: space-between; align-items: baseline;
            padding: 6px 0; border-bottom: 1px solid #1a1a2a; }
  .metric:last-child { border-bottom: none; }
  .metric .label { color: #aaa; font-size: 13px; }
  .metric .value { font-size: 18px; font-weight: 600; font-variant-numeric: tabular-nums; }
  .metric .value.good { color: #00ff88; }
  .metric .value.warn { color: #ffaa00; }
  .metric .value.bad { color: #ff4455; }
  .metric .unit { color: #666; font-size: 12px; margin-left: 4px; }
  .chart-container { width: 100%; height: 200px; position: relative; }
  canvas { width: 100% !important; height: 100% !important; }
  .alert-list { max-height: 300px; overflow-y: auto; }
  .alert { padding: 8px 12px; border-radius: 6px; margin-bottom: 6px; font-size: 13px; }
  .alert.warning { background: #332800; border-left: 3px solid #ffaa00; }
  .alert.error { background: #330011; border-left: 3px solid #ff4455; }
  .alert.info { background: #001133; border-left: 3px solid #4488ff; }
  .alert .time { color: #666; font-size: 11px; }
  .health-bar { height: 8px; background: #1a1a2a; border-radius: 4px; overflow: hidden; margin-top: 4px; }
  .health-bar .fill { height: 100%; border-radius: 4px; transition: width 0.5s; }
  .counter-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 8px; }
  .counter { text-align: center; padding: 12px; background: #0d0d15; border-radius: 8px; }
  .counter .num { font-size: 24px; font-weight: 700; color: #e94560; }
  .counter .desc { font-size: 11px; color: #666; margin-top: 4px; }
  .refresh-btn { background: #1e1e2e; border: 1px solid #333; color: #aaa; padding: 6px 16px;
                 border-radius: 6px; cursor: pointer; font-size: 12px; }
  .refresh-btn:hover { background: #2a2a3e; color: #fff; }
  ::-webkit-scrollbar { width: 6px; }
  ::-webkit-scrollbar-track { background: #0a0a0f; }
  ::-webkit-scrollbar-thumb { background: #333; border-radius: 3px; }
</style>
</head>
<body>
<div class=""header"">
  <div class=""status-dot"" id=""statusDot""></div>
  <div>
    <h1>ProteYOLUTE</h1>
    <div class=""subtitle"">Intelligent Nano-Flow LC Control</div>
  </div>
  <div style=""flex:1""></div>
  <button class=""refresh-btn"" onclick=""refreshAll()"">Refresh</button>
  <div class=""subtitle"" id=""clock""></div>
</div>

<div class=""grid"">
  <!-- Live Pressure -->
  <div class=""card"">
    <h2>Live Pressure</h2>
    <div class=""metric"">
      <span class=""label"">Channel A</span>
      <span class=""value good"" id=""pressA"">--<span class=""unit"">bar</span></span>
    </div>
    <div class=""metric"">
      <span class=""label"">Channel B</span>
      <span class=""value good"" id=""pressB"">--<span class=""unit"">bar</span></span>
    </div>
    <div class=""chart-container"">
      <canvas id=""pressureChart""></canvas>
    </div>
  </div>

  <!-- Live Flow -->
  <div class=""card"">
    <h2>Live Flow</h2>
    <div class=""metric"">
      <span class=""label"">Channel A</span>
      <span class=""value good"" id=""flowA"">--<span class=""unit"">&micro;L/min</span></span>
    </div>
    <div class=""metric"">
      <span class=""label"">Channel B</span>
      <span class=""value good"" id=""flowB"">--<span class=""unit"">&micro;L/min</span></span>
    </div>
    <div class=""chart-container"">
      <canvas id=""flowChart""></canvas>
    </div>
  </div>

  <!-- Column Health -->
  <div class=""card"">
    <h2>Smart Column</h2>
    <div id=""columnInfo"">
      <div class=""metric"">
        <span class=""label"">Column</span>
        <span class=""value"" id=""colName"">--</span>
      </div>
      <div class=""metric"">
        <span class=""label"">Performance</span>
        <span class=""value"" id=""colScore"">--%</span>
      </div>
      <div class=""health-bar""><div class=""fill"" id=""colBar"" style=""width:0%;background:#00ff88""></div></div>
      <div class=""metric"">
        <span class=""label"">Injections</span>
        <span class=""value"" id=""colInj"">--</span>
      </div>
      <div class=""metric"">
        <span class=""label"">Est. Remaining</span>
        <span class=""value"" id=""colRemain"">--</span>
      </div>
      <div class=""metric"">
        <span class=""label"">Health Status</span>
        <span class=""value"" id=""colHealth"">--</span>
      </div>
    </div>
  </div>

  <!-- Trap Health -->
  <div class=""card"">
    <h2>Smart Trap</h2>
    <div id=""trapInfo"">
      <div class=""metric"">
        <span class=""label"">Trap</span>
        <span class=""value"" id=""trapName"">--</span>
      </div>
      <div class=""metric"">
        <span class=""label"">Performance</span>
        <span class=""value"" id=""trapScore"">--%</span>
      </div>
      <div class=""health-bar""><div class=""fill"" id=""trapBar"" style=""width:0%;background:#00ff88""></div></div>
      <div class=""metric"">
        <span class=""label"">Injections</span>
        <span class=""value"" id=""trapInj"">--</span>
      </div>
      <div class=""metric"">
        <span class=""label"">Health Status</span>
        <span class=""value"" id=""trapHealth"">--</span>
      </div>
    </div>
  </div>

  <!-- System Health -->
  <div class=""card"">
    <h2>System Health</h2>
    <div class=""counter-grid"">
      <div class=""counter""><div class=""num"" id=""totalRuns"">0</div><div class=""desc"">Total Runs</div></div>
      <div class=""counter""><div class=""num"" id=""totalInj"">0</div><div class=""desc"">Injections</div></div>
      <div class=""counter""><div class=""num"" id=""pumpHours"">0</div><div class=""desc"">Pump Hours</div></div>
      <div class=""counter""><div class=""num"" id=""valveSwitches"">0</div><div class=""desc"">Valve Switches</div></div>
    </div>
    <div class=""metric"" style=""margin-top:12px"">
      <span class=""label"">Max Pressure (A)</span>
      <span class=""value"" id=""maxPressA"">--<span class=""unit"">bar</span></span>
    </div>
    <div class=""metric"">
      <span class=""label"">Last Calibration</span>
      <span class=""value"" id=""lastCal"">--</span>
    </div>
  </div>

  <!-- Active Alerts -->
  <div class=""card"">
    <h2>Active Alerts</h2>
    <div class=""alert-list"" id=""alertList"">
      <div class=""alert info"">No active alerts</div>
    </div>
  </div>

  <!-- Recent Runs -->
  <div class=""card"" style=""grid-column: span 2"">
    <h2>Recent Runs</h2>
    <div id=""runsList"" style=""max-height:300px;overflow-y:auto"">
      <table style=""width:100%;font-size:13px;border-collapse:collapse"">
        <thead><tr style=""color:#888;text-align:left"">
          <th style=""padding:6px"">#</th><th>Method</th><th>Start</th><th>Duration</th><th>Status</th>
        </tr></thead>
        <tbody id=""runsBody""></tbody>
      </table>
    </div>
  </div>
</div>

<script>
const API = window.location.origin;
const pressureData = { A: [], B: [], labels: [] };
const flowData = { A: [], B: [], labels: [] };
const MAX_POINTS = 120;

function updateClock() {
  document.getElementById('clock').textContent = new Date().toLocaleTimeString();
}
setInterval(updateClock, 1000);
updateClock();

async function fetchJson(path) {
  try {
    const r = await fetch(API + path);
    return await r.json();
  } catch { return null; }
}

async function refreshStatus() {
  const s = await fetchJson('/api/status');
  if (!s) return;
  updateValue('pressA', s.pressure_a, 'bar', 0, 1200);
  updateValue('pressB', s.pressure_b, 'bar', 0, 1200);
  updateValue('flowA', s.flow_a, '&micro;L/min');
  updateValue('flowB', s.flow_b, '&micro;L/min');

  if (s.pressure_a != null) {
    pressureData.A.push(s.pressure_a);
    pressureData.B.push(s.pressure_b || 0);
    pressureData.labels.push('');
    if (pressureData.A.length > MAX_POINTS) {
      pressureData.A.shift(); pressureData.B.shift(); pressureData.labels.shift();
    }
    drawChart('pressureChart', pressureData, 'bar');
  }
  if (s.flow_a != null) {
    flowData.A.push(s.flow_a);
    flowData.B.push(s.flow_b || 0);
    flowData.labels.push('');
    if (flowData.A.length > MAX_POINTS) {
      flowData.A.shift(); flowData.B.shift(); flowData.labels.shift();
    }
    drawChart('flowChart', flowData, 'uL/min');
  }
}

async function refreshHealth() {
  const h = await fetchJson('/api/health');
  if (!h) return;
  const c = h.counters || {};
  document.getElementById('totalRuns').textContent = fmt(c.total_runs);
  document.getElementById('totalInj').textContent = fmt(c.total_injections);
  document.getElementById('pumpHours').textContent = fmt(c.pump_a_hours, 1);
  document.getElementById('valveSwitches').textContent =
    fmt((c.valve_a_switches||0)+(c.valve_b_switches||0)+(c.valve_i_switches||0)+(c.valve_t_switches||0));
  document.getElementById('maxPressA').innerHTML = fmt(c.max_pressure_ever_a,0) + '<span class=""unit"">bar</span>';

  if (c.last_calibration_date > 0) {
    const d = new Date(c.last_calibration_date * 1000);
    const days = Math.floor((Date.now() - d) / 86400000);
    document.getElementById('lastCal').textContent = days + ' days ago';
    document.getElementById('lastCal').className = 'value ' + (days > 30 ? 'warn' : 'good');
  }

  // Column health
  if (h.active_column) {
    const col = h.active_column;
    document.getElementById('colName').textContent = col.Name || '--';
    document.getElementById('colScore').textContent = (col.PerformanceScore||0).toFixed(0) + '%';
    document.getElementById('colScore').className = 'value ' + scoreClass(col.PerformanceScore);
    document.getElementById('colBar').style.width = (col.PerformanceScore||0) + '%';
    document.getElementById('colBar').style.background = scoreColor(col.PerformanceScore);
    document.getElementById('colInj').textContent = col.TotalInjections || 0;
    document.getElementById('colRemain').textContent = col.EstimatedRemainingInjections || '--';
    document.getElementById('colHealth').textContent = col.HealthStatus || '--';
    document.getElementById('colHealth').className = 'value ' + scoreClass(col.PerformanceScore);
  }

  // Trap health
  if (h.active_trap) {
    const trap = h.active_trap;
    document.getElementById('trapName').textContent = trap.Name || '--';
    document.getElementById('trapScore').textContent = (trap.PerformanceScore||0).toFixed(0) + '%';
    document.getElementById('trapScore').className = 'value ' + scoreClass(trap.PerformanceScore);
    document.getElementById('trapBar').style.width = (trap.PerformanceScore||0) + '%';
    document.getElementById('trapBar').style.background = scoreColor(trap.PerformanceScore);
    document.getElementById('trapInj').textContent = trap.TotalInjections || 0;
    document.getElementById('trapHealth').textContent = trap.HealthStatus || '--';
  }

  // Alerts
  if (h.alerts && h.alerts.length > 0) {
    const al = document.getElementById('alertList');
    al.innerHTML = h.alerts.map(a =>
      `<div class=""alert ${a.severity}"">${a.message}<div class=""time"">${a.timestamp}</div></div>`
    ).join('');
  }
}

async function refreshRuns() {
  const runs = await fetchJson('/api/runs?limit=15');
  if (!runs) return;
  const tbody = document.getElementById('runsBody');
  tbody.innerHTML = runs.map(r => {
    const dur = r.end_time ? timeDiff(r.start_time, r.end_time) : 'running...';
    const sc = r.status === 'completed' ? 'good' : r.status === 'failed' ? 'bad' : 'warn';
    return `<tr style=""border-bottom:1px solid #1a1a2a"">
      <td style=""padding:6px;color:#666"">${r.run_id}</td>
      <td>${r.method||''}</td><td style=""color:#888"">${r.start_time||''}</td>
      <td style=""color:#888"">${dur}</td>
      <td><span class=""value ${sc}"" style=""font-size:13px"">${r.status}</span></td>
    </tr>`;
  }).join('');
}

function refreshAll() { refreshStatus(); refreshHealth(); refreshRuns(); }

// Real-time SSE connection
function connectSSE() {
  const es = new EventSource(API + '/api/stream');
  es.addEventListener('pressure', e => {
    const d = JSON.parse(e.data);
    if (d.channel === 'A') updateValue('pressA', d.pressure, 'bar');
    if (d.channel === 'B') updateValue('pressB', d.pressure, 'bar');
  });
  es.addEventListener('flow', e => {
    const d = JSON.parse(e.data);
    if (d.channel === 'A') updateValue('flowA', d.flow, '&micro;L/min');
    if (d.channel === 'B') updateValue('flowB', d.flow, '&micro;L/min');
  });
  es.addEventListener('alert', e => { refreshHealth(); });
  es.addEventListener('run.started', e => { refreshRuns(); });
  es.addEventListener('run.ended', e => { refreshRuns(); refreshHealth(); });
  es.onerror = () => { setTimeout(connectSSE, 5000); };
  document.getElementById('statusDot').style.background = '#00ff88';
}

function updateValue(id, val, unit, warnAt, critAt) {
  const el = document.getElementById(id);
  if (val == null) return;
  let cls = 'good';
  if (critAt && val > critAt) cls = 'bad';
  else if (warnAt && val > warnAt) cls = 'warn';
  el.innerHTML = (typeof val === 'number' ? val.toFixed(val < 10 ? 3 : 0) : val) +
    '<span class=""unit"">' + unit + '</span>';
  el.className = 'value ' + cls;
}

function drawChart(canvasId, data, unit) {
  const canvas = document.getElementById(canvasId);
  const ctx = canvas.getContext('2d');
  const w = canvas.width = canvas.parentElement.clientWidth;
  const h = canvas.height = canvas.parentElement.clientHeight;

  ctx.clearRect(0, 0, w, h);
  if (data.A.length < 2) return;

  const allVals = [...data.A, ...data.B];
  const maxV = Math.max(...allVals) * 1.1 || 1;
  const minV = Math.min(0, Math.min(...allVals));

  // Grid
  ctx.strokeStyle = '#1a1a2a'; ctx.lineWidth = 1;
  for (let i = 0; i < 5; i++) {
    const y = h - (h * i / 4);
    ctx.beginPath(); ctx.moveTo(0, y); ctx.lineTo(w, y); ctx.stroke();
    ctx.fillStyle = '#444'; ctx.font = '10px monospace';
    ctx.fillText((minV + (maxV-minV)*i/4).toFixed(0), 4, y - 2);
  }

  function drawLine(arr, color) {
    ctx.beginPath(); ctx.strokeStyle = color; ctx.lineWidth = 2;
    arr.forEach((v, i) => {
      const x = (i / (arr.length - 1)) * w;
      const y = h - ((v - minV) / (maxV - minV)) * h;
      i === 0 ? ctx.moveTo(x, y) : ctx.lineTo(x, y);
    });
    ctx.stroke();
  }
  drawLine(data.A, '#4488ff');
  drawLine(data.B, '#e94560');

  // Legend
  ctx.fillStyle = '#4488ff'; ctx.fillRect(w-80, 8, 12, 3);
  ctx.fillStyle = '#aaa'; ctx.font = '10px sans-serif'; ctx.fillText('Ch A', w-64, 12);
  ctx.fillStyle = '#e94560'; ctx.fillRect(w-80, 18, 12, 3);
  ctx.fillStyle = '#aaa'; ctx.fillText('Ch B', w-64, 22);
}

function fmt(n, d) { return n != null ? (d != null ? Number(n).toFixed(d) : Math.round(n).toString()) : '0'; }
function scoreClass(s) { return s >= 70 ? 'good' : s >= 40 ? 'warn' : 'bad'; }
function scoreColor(s) { return s >= 70 ? '#00ff88' : s >= 40 ? '#ffaa00' : '#ff4455'; }
function timeDiff(a, b) {
  const ms = new Date(b) - new Date(a);
  const m = Math.floor(ms/60000);
  return m < 60 ? m + ' min' : (m/60).toFixed(1) + ' hr';
}

// Boot
refreshAll();
connectSSE();
setInterval(refreshStatus, 2000);
setInterval(refreshHealth, 30000);
setInterval(refreshRuns, 15000);
</script>
</body>
</html>";
        }

        // ─── Helpers ─────────────────────────────────────────────────────

        private string GetQueryString(HttpListenerRequest req, string key)
        {
            return req.QueryString[key];
        }

        private int GetQueryInt(HttpListenerRequest req, string key, int defaultValue)
        {
            string val = req.QueryString[key];
            return int.TryParse(val, out int result) ? result : defaultValue;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Stop();
            _listener?.Close();
        }
    }

    public class SystemStatus
    {
        public double? pressure_a { get; set; }
        public double? pressure_b { get; set; }
        public double? flow_a { get; set; }
        public double? flow_b { get; set; }
        public double? percent_b { get; set; }
        public double? temperature_c { get; set; }
        public string valve_a { get; set; }
        public string valve_b { get; set; }
        public string valve_i { get; set; }
        public string valve_t { get; set; }
        public string system_state { get; set; }
        public long? active_run_id { get; set; }
        public string active_method { get; set; }
        public double? gradient_progress_pct { get; set; }
        public double? pump_a_volume_remaining { get; set; }
        public double? pump_b_volume_remaining { get; set; }
    }
}
