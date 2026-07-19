// ProteYOLUTE — Copyright (c) 2025-2026 Michael Krawitzky. All rights reserved.
// Licensed under proprietary terms. See LICENSE file for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BalticWpfControlLib.Data
{
    /// <summary>
    /// Core database engine for ProteYOLUTE. Replaces CSV logging with structured
    /// SQLite storage. Thread-safe, singleton, lazy-initialized.
    /// </summary>
    public sealed class ProteYoluteDb : IDisposable
    {
        private static readonly Lazy<ProteYoluteDb> _instance =
            new Lazy<ProteYoluteDb>(() => new ProteYoluteDb());
        public static ProteYoluteDb Instance => _instance.Value;

        private SQLiteConnection _conn;
        private readonly object _writeLock = new object();
        private readonly ConcurrentQueue<Action<SQLiteConnection>> _writeQueue =
            new ConcurrentQueue<Action<SQLiteConnection>>();
        private Timer _flushTimer;
        private bool _disposed;

        public string DbPath { get; private set; }

        // Event for real-time data subscribers (dashboard, API, etc.)
        public event Action<string, object> DataLogged;

        private ProteYoluteDb()
        {
            var pluginDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "ProteYOLUTE");
            if (!Directory.Exists(pluginDir))
                Directory.CreateDirectory(pluginDir);

            DbPath = Path.Combine(pluginDir, "proteyolute.db");
            Initialize();
        }

        /// <summary>
        /// Initialize with a custom path (for testing or alternate locations).
        /// </summary>
        public static ProteYoluteDb InitializeAt(string dbPath)
        {
            var db = Instance;
            if (db._conn != null)
            {
                db._conn.Close();
                db._conn.Dispose();
            }
            db.DbPath = dbPath;
            var dir = Path.GetDirectoryName(dbPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            db.Initialize();
            return db;
        }

        private void Initialize()
        {
            var connStr = $"Data Source={DbPath};Version=3;Journal Mode=WAL;Synchronous=Normal;Cache Size=10000;";
            _conn = new SQLiteConnection(connStr);
            _conn.Open();
            CreateSchema();
            _flushTimer = new Timer(_ => FlushQueue(), null, 1000, 1000);
        }

        private void CreateSchema()
        {
            ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS runs (
                    run_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    method TEXT NOT NULL,
                    procedure_name TEXT,
                    start_time TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                    end_time TEXT,
                    status TEXT NOT NULL DEFAULT 'running',
                    operator TEXT,
                    notes TEXT,
                    template_id INTEGER,
                    column_id INTEGER,
                    trap_id INTEGER
                );

                CREATE TABLE IF NOT EXISTS pressure_log (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    run_id INTEGER,
                    timestamp_ms INTEGER NOT NULL,
                    channel TEXT NOT NULL,
                    pressure_bar REAL NOT NULL,
                    setpoint_bar REAL,
                    FOREIGN KEY (run_id) REFERENCES runs(run_id)
                );

                CREATE TABLE IF NOT EXISTS flow_log (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    run_id INTEGER,
                    timestamp_ms INTEGER NOT NULL,
                    channel TEXT NOT NULL,
                    flow_ul_min REAL NOT NULL,
                    setpoint_ul_min REAL,
                    FOREIGN KEY (run_id) REFERENCES runs(run_id)
                );

                CREATE TABLE IF NOT EXISTS valve_events (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    run_id INTEGER,
                    timestamp_ms INTEGER NOT NULL,
                    valve_name TEXT NOT NULL,
                    from_position TEXT,
                    to_position TEXT NOT NULL,
                    angle_degrees REAL,
                    FOREIGN KEY (run_id) REFERENCES runs(run_id)
                );

                CREATE TABLE IF NOT EXISTS gradient_log (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    run_id INTEGER,
                    timestamp_ms INTEGER NOT NULL,
                    percent_b REAL NOT NULL,
                    flow_total_ul_min REAL,
                    pressure_a REAL,
                    pressure_b REAL,
                    FOREIGN KEY (run_id) REFERENCES runs(run_id)
                );

                CREATE TABLE IF NOT EXISTS diagnostics (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    run_id INTEGER,
                    timestamp TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                    test_name TEXT NOT NULL,
                    parameter TEXT NOT NULL,
                    value REAL,
                    value_text TEXT,
                    unit TEXT,
                    pass_fail TEXT,
                    threshold_min REAL,
                    threshold_max REAL,
                    FOREIGN KEY (run_id) REFERENCES runs(run_id)
                );

                CREATE TABLE IF NOT EXISTS calibrations (
                    cal_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    timestamp TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                    sensor TEXT NOT NULL,
                    cal_type TEXT NOT NULL,
                    channel TEXT,
                    old_value REAL,
                    new_value REAL,
                    result TEXT,
                    notes TEXT
                );

                CREATE TABLE IF NOT EXISTS errors (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    run_id INTEGER,
                    timestamp TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                    severity TEXT NOT NULL,
                    message TEXT NOT NULL,
                    context TEXT,
                    recovered INTEGER DEFAULT 0,
                    recovery_action TEXT,
                    FOREIGN KEY (run_id) REFERENCES runs(run_id)
                );

                CREATE TABLE IF NOT EXISTS health_counters (
                    key TEXT PRIMARY KEY,
                    value REAL NOT NULL DEFAULT 0,
                    last_updated TEXT NOT NULL DEFAULT (datetime('now','localtime'))
                );

                CREATE TABLE IF NOT EXISTS columns (
                    column_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    serial TEXT,
                    name TEXT NOT NULL,
                    column_type TEXT NOT NULL,
                    role TEXT NOT NULL DEFAULT 'analytical',
                    particle_size_um REAL,
                    inner_diameter_um REAL,
                    length_cm REAL,
                    max_pressure_bar REAL,
                    pore_size_angstrom REAL,
                    stationary_phase TEXT,
                    install_date TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                    retire_date TEXT,
                    total_injections INTEGER DEFAULT 0,
                    total_volume_ul REAL DEFAULT 0,
                    total_hours REAL DEFAULT 0,
                    baseline_pressure_bar REAL,
                    baseline_flow_ul_min REAL,
                    baseline_percent_b REAL,
                    current_performance_score REAL DEFAULT 100,
                    estimated_remaining_injections INTEGER,
                    status TEXT DEFAULT 'active',
                    notes TEXT
                );

                CREATE TABLE IF NOT EXISTS column_pressure_history (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    column_id INTEGER NOT NULL,
                    timestamp TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                    injection_number INTEGER,
                    pressure_at_baseline_conditions REAL,
                    flow_rate REAL,
                    percent_b REAL,
                    temperature_c REAL,
                    performance_score REAL,
                    FOREIGN KEY (column_id) REFERENCES columns(column_id)
                );

                CREATE TABLE IF NOT EXISTS templates (
                    template_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL UNIQUE,
                    description TEXT,
                    category TEXT,
                    author TEXT,
                    created TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                    modified TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                    parameters_json TEXT NOT NULL,
                    column_type TEXT,
                    flow_rate_min REAL,
                    flow_rate_max REAL,
                    gradient_duration_min REAL,
                    tags TEXT
                );

                CREATE TABLE IF NOT EXISTS alerts (
                    alert_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    timestamp TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                    alert_type TEXT NOT NULL,
                    severity TEXT NOT NULL,
                    message TEXT NOT NULL,
                    source TEXT,
                    acknowledged INTEGER DEFAULT 0,
                    acknowledged_by TEXT,
                    acknowledged_at TEXT,
                    auto_resolved INTEGER DEFAULT 0,
                    data_json TEXT
                );

                CREATE TABLE IF NOT EXISTS pressure_baselines (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    method TEXT NOT NULL,
                    column_id INTEGER,
                    gradient_time_pct REAL NOT NULL,
                    mean_pressure REAL NOT NULL,
                    std_pressure REAL NOT NULL,
                    sample_count INTEGER NOT NULL DEFAULT 1,
                    last_updated TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                    FOREIGN KEY (column_id) REFERENCES columns(column_id)
                );

                CREATE TABLE IF NOT EXISTS audit_log (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    timestamp TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                    user_id TEXT,
                    action TEXT NOT NULL,
                    entity_type TEXT,
                    entity_id TEXT,
                    old_value TEXT,
                    new_value TEXT,
                    reason TEXT,
                    hash TEXT
                );
            ");

            // Initialize health counters if empty
            ExecuteNonQuery(@"
                INSERT OR IGNORE INTO health_counters (key, value) VALUES
                ('pump_a_hours', 0),
                ('pump_b_hours', 0),
                ('pump_a_volume_total_ul', 0),
                ('pump_b_volume_total_ul', 0),
                ('valve_a_switches', 0),
                ('valve_b_switches', 0),
                ('valve_i_switches', 0),
                ('valve_t_switches', 0),
                ('max_pressure_ever_a', 0),
                ('max_pressure_ever_b', 0),
                ('total_runs', 0),
                ('total_injections', 0),
                ('last_calibration_date', 0),
                ('last_diagnostic_date', 0),
                ('system_start_count', 0);
            ");

            // Create indexes for performance
            ExecuteNonQuery(@"
                CREATE INDEX IF NOT EXISTS idx_pressure_run_time ON pressure_log(run_id, timestamp_ms);
                CREATE INDEX IF NOT EXISTS idx_flow_run_time ON flow_log(run_id, timestamp_ms);
                CREATE INDEX IF NOT EXISTS idx_valve_run_time ON valve_events(run_id, timestamp_ms);
                CREATE INDEX IF NOT EXISTS idx_gradient_run_time ON gradient_log(run_id, timestamp_ms);
                CREATE INDEX IF NOT EXISTS idx_errors_run ON errors(run_id);
                CREATE INDEX IF NOT EXISTS idx_alerts_type ON alerts(alert_type, acknowledged);
                CREATE INDEX IF NOT EXISTS idx_column_history ON column_pressure_history(column_id, injection_number);
                CREATE INDEX IF NOT EXISTS idx_baselines_method ON pressure_baselines(method, column_id);
                CREATE INDEX IF NOT EXISTS idx_runs_status ON runs(status, start_time);
            ");
        }

        // ─── Run Management ──────────────────────────────────────────────

        public long StartRun(string method, string procedure = null, string op = null,
            int? columnId = null, int? trapId = null)
        {
            long runId = 0;
            lock (_writeLock)
            {
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO runs (method, procedure_name, operator, column_id, trap_id)
                                        VALUES (@m, @p, @o, @c, @t);
                                        SELECT last_insert_rowid();";
                    cmd.Parameters.AddWithValue("@m", method);
                    cmd.Parameters.AddWithValue("@p", (object)procedure ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@o", (object)op ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@c", (object)columnId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@t", (object)trapId ?? DBNull.Value);
                    runId = (long)cmd.ExecuteScalar();
                }
                IncrementCounter("total_runs");
            }
            DataLogged?.Invoke("run.started", new { run_id = runId, method });
            return runId;
        }

        public void EndRun(long runId, string status = "completed", string notes = null)
        {
            lock (_writeLock)
            {
                ExecuteNonQuery(
                    @"UPDATE runs SET end_time = datetime('now','localtime'), status = @s, notes = @n
                      WHERE run_id = @r",
                    new[] { "@s", status, "@n", notes ?? "", "@r", runId.ToString() });
            }
            DataLogged?.Invoke("run.ended", new { run_id = runId, status });
        }

        // ─── Real-Time Data Logging (High Frequency) ─────────────────────

        public void LogPressure(long runId, string channel, double pressure, double? setpoint = null)
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _writeQueue.Enqueue(conn =>
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO pressure_log (run_id, timestamp_ms, channel, pressure_bar, setpoint_bar)
                                        VALUES (@r, @t, @c, @p, @s)";
                    cmd.Parameters.AddWithValue("@r", runId);
                    cmd.Parameters.AddWithValue("@t", ts);
                    cmd.Parameters.AddWithValue("@c", channel);
                    cmd.Parameters.AddWithValue("@p", pressure);
                    cmd.Parameters.AddWithValue("@s", (object)setpoint ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            });

            // Update max pressure counter
            var key = channel == "A" ? "max_pressure_ever_a" : "max_pressure_ever_b";
            var current = GetCounter(key);
            if (pressure > current)
                SetCounter(key, pressure);

            DataLogged?.Invoke("pressure", new { run_id = runId, channel, pressure, setpoint, timestamp_ms = ts });
        }

        public void LogFlow(long runId, string channel, double flow, double? setpoint = null)
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _writeQueue.Enqueue(conn =>
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO flow_log (run_id, timestamp_ms, channel, flow_ul_min, setpoint_ul_min)
                                        VALUES (@r, @t, @c, @f, @s)";
                    cmd.Parameters.AddWithValue("@r", runId);
                    cmd.Parameters.AddWithValue("@t", ts);
                    cmd.Parameters.AddWithValue("@c", channel);
                    cmd.Parameters.AddWithValue("@f", flow);
                    cmd.Parameters.AddWithValue("@s", (object)setpoint ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            });
            DataLogged?.Invoke("flow", new { run_id = runId, channel, flow, setpoint, timestamp_ms = ts });
        }

        public void LogValveSwitch(long runId, string valve, string fromPos, string toPos, double? angle = null)
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _writeQueue.Enqueue(conn =>
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO valve_events (run_id, timestamp_ms, valve_name, from_position, to_position, angle_degrees)
                                        VALUES (@r, @t, @v, @f, @tp, @a)";
                    cmd.Parameters.AddWithValue("@r", runId);
                    cmd.Parameters.AddWithValue("@t", ts);
                    cmd.Parameters.AddWithValue("@v", valve);
                    cmd.Parameters.AddWithValue("@f", (object)fromPos ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@tp", toPos);
                    cmd.Parameters.AddWithValue("@a", (object)angle ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            });

            // Increment valve switch counter
            string counterKey;
            switch (valve.ToUpperInvariant())
            {
                case "A": case "VALVE_A": case "PUMPA": counterKey = "valve_a_switches"; break;
                case "B": case "VALVE_B": case "PUMPB": counterKey = "valve_b_switches"; break;
                case "I": case "INJECTION": counterKey = "valve_i_switches"; break;
                case "T": case "TRAP": counterKey = "valve_t_switches"; break;
                default: counterKey = "valve_" + valve.ToLowerInvariant() + "_switches"; break;
            }
            IncrementCounter(counterKey);
            DataLogged?.Invoke("valve", new { run_id = runId, valve, from_position = fromPos, to_position = toPos });
        }

        public void LogGradient(long runId, double percentB, double? flowTotal = null,
            double? pressureA = null, double? pressureB = null)
        {
            var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _writeQueue.Enqueue(conn =>
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO gradient_log (run_id, timestamp_ms, percent_b, flow_total_ul_min, pressure_a, pressure_b)
                                        VALUES (@r, @t, @b, @f, @pa, @pb)";
                    cmd.Parameters.AddWithValue("@r", runId);
                    cmd.Parameters.AddWithValue("@t", ts);
                    cmd.Parameters.AddWithValue("@b", percentB);
                    cmd.Parameters.AddWithValue("@f", (object)flowTotal ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@pa", (object)pressureA ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@pb", (object)pressureB ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            });
            DataLogged?.Invoke("gradient", new { run_id = runId, percent_b = percentB, flow_total = flowTotal });
        }

        // ─── Diagnostics & Calibration ───────────────────────────────────

        public void LogDiagnostic(long? runId, string testName, string parameter,
            double? value, string unit, string passFail = null,
            double? threshMin = null, double? threshMax = null, string valueText = null)
        {
            lock (_writeLock)
            {
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO diagnostics
                        (run_id, test_name, parameter, value, value_text, unit, pass_fail, threshold_min, threshold_max)
                        VALUES (@r, @t, @p, @v, @vt, @u, @pf, @tmin, @tmax)";
                    cmd.Parameters.AddWithValue("@r", (object)runId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@t", testName);
                    cmd.Parameters.AddWithValue("@p", parameter);
                    cmd.Parameters.AddWithValue("@v", (object)value ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@vt", (object)valueText ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@u", (object)unit ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@pf", (object)passFail ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@tmin", (object)threshMin ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@tmax", (object)threshMax ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            DataLogged?.Invoke("diagnostic", new { test_name = testName, parameter, value, pass_fail = passFail });
        }

        public void LogCalibration(string sensor, string calType, string channel,
            double? oldValue, double? newValue, string result, string notes = null)
        {
            lock (_writeLock)
            {
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO calibrations
                        (sensor, cal_type, channel, old_value, new_value, result, notes)
                        VALUES (@s, @ct, @ch, @ov, @nv, @r, @n)";
                    cmd.Parameters.AddWithValue("@s", sensor);
                    cmd.Parameters.AddWithValue("@ct", calType);
                    cmd.Parameters.AddWithValue("@ch", (object)channel ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ov", (object)oldValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@nv", (object)newValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@r", result);
                    cmd.Parameters.AddWithValue("@n", (object)notes ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                SetCounter("last_calibration_date", DateTimeOffset.Now.ToUnixTimeSeconds());
            }
            DataLogged?.Invoke("calibration", new { sensor, cal_type = calType, result });
        }

        public void LogError(long? runId, string severity, string message,
            string context = null, bool recovered = false, string recoveryAction = null)
        {
            lock (_writeLock)
            {
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO errors
                        (run_id, severity, message, context, recovered, recovery_action)
                        VALUES (@r, @s, @m, @c, @rec, @ra)";
                    cmd.Parameters.AddWithValue("@r", (object)runId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@s", severity);
                    cmd.Parameters.AddWithValue("@m", message);
                    cmd.Parameters.AddWithValue("@c", (object)context ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@rec", recovered ? 1 : 0);
                    cmd.Parameters.AddWithValue("@ra", (object)recoveryAction ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            DataLogged?.Invoke("error", new { run_id = runId, severity, message, recovered });
        }

        // ─── Smart Column & Trap Management ──────────────────────────────

        public int RegisterColumn(string name, string type, string role,
            double? particleSize = null, double? innerDiameter = null, double? length = null,
            double? maxPressure = null, string serial = null, string phase = null)
        {
            lock (_writeLock)
            {
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO columns
                        (name, column_type, role, particle_size_um, inner_diameter_um, length_cm,
                         max_pressure_bar, serial, stationary_phase)
                        VALUES (@n, @t, @r, @ps, @id, @l, @mp, @s, @ph);
                        SELECT last_insert_rowid();";
                    cmd.Parameters.AddWithValue("@n", name);
                    cmd.Parameters.AddWithValue("@t", type);
                    cmd.Parameters.AddWithValue("@r", role);
                    cmd.Parameters.AddWithValue("@ps", (object)particleSize ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", (object)innerDiameter ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@l", (object)length ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@mp", (object)maxPressure ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@s", (object)serial ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ph", (object)phase ?? DBNull.Value);
                    return (int)(long)cmd.ExecuteScalar();
                }
            }
        }

        public void RecordColumnInjection(int columnId, double pressureAtBaseline,
            double flowRate, double percentB, double? tempC = null)
        {
            lock (_writeLock)
            {
                // Increment injection count
                ExecuteNonQuery(
                    "UPDATE columns SET total_injections = total_injections + 1 WHERE column_id = @id",
                    new[] { "@id", columnId.ToString() });

                // Get current injection number
                int injNum = 0;
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT total_injections FROM columns WHERE column_id = @id";
                    cmd.Parameters.AddWithValue("@id", columnId);
                    var result = cmd.ExecuteScalar();
                    if (result != null) injNum = Convert.ToInt32(result);
                }

                // Record pressure at baseline conditions
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO column_pressure_history
                        (column_id, injection_number, pressure_at_baseline_conditions,
                         flow_rate, percent_b, temperature_c, performance_score)
                        VALUES (@cid, @inj, @p, @f, @b, @t, @ps)";
                    cmd.Parameters.AddWithValue("@cid", columnId);
                    cmd.Parameters.AddWithValue("@inj", injNum);
                    cmd.Parameters.AddWithValue("@p", pressureAtBaseline);
                    cmd.Parameters.AddWithValue("@f", flowRate);
                    cmd.Parameters.AddWithValue("@b", percentB);
                    cmd.Parameters.AddWithValue("@t", (object)tempC ?? DBNull.Value);

                    // Calculate performance score
                    double score = CalculateColumnPerformance(columnId, pressureAtBaseline);
                    cmd.Parameters.AddWithValue("@ps", score);
                    cmd.ExecuteNonQuery();

                    // Update column performance
                    ExecuteNonQuery(
                        "UPDATE columns SET current_performance_score = @s WHERE column_id = @id",
                        new[] { "@s", score.ToString("F1"), "@id", columnId.ToString() });
                }
            }
            DataLogged?.Invoke("column.injection", new { column_id = columnId, pressure = pressureAtBaseline });
        }

        public double CalculateColumnPerformance(int columnId, double currentPressure)
        {
            // Get baseline pressure (first 5 injections average)
            double baseline = 0;
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT AVG(pressure_at_baseline_conditions) FROM column_pressure_history
                                    WHERE column_id = @id AND injection_number <= 5";
                cmd.Parameters.AddWithValue("@id", columnId);
                var result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value) return 100.0;
                baseline = Convert.ToDouble(result);
            }

            if (baseline <= 0) return 100.0;

            // Performance degrades as pressure increases (blockage) or decreases (channeling)
            double ratio = currentPressure / baseline;
            if (ratio >= 0.9 && ratio <= 1.1) return 100.0; // Within 10% = perfect
            if (ratio > 1.1)
            {
                // Pressure increasing = blocking. 2x baseline = 0% performance
                return Math.Max(0, 100.0 * (1.0 - (ratio - 1.0)));
            }
            // Pressure decreasing = channeling or void formation
            return Math.Max(0, 100.0 * ratio);
        }

        public int? EstimateRemainingInjections(int columnId)
        {
            var history = new List<double>();
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT pressure_at_baseline_conditions FROM column_pressure_history
                                    WHERE column_id = @id ORDER BY injection_number";
                cmd.Parameters.AddWithValue("@id", columnId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        history.Add(reader.GetDouble(0));
                }
            }

            if (history.Count < 10) return null; // Not enough data

            // Get max pressure for this column
            double maxPressure = 800; // default
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = "SELECT max_pressure_bar FROM columns WHERE column_id = @id";
                cmd.Parameters.AddWithValue("@id", columnId);
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    maxPressure = Convert.ToDouble(result);
            }

            // Simple linear regression on last 20 points
            int n = Math.Min(history.Count, 20);
            int startIdx = history.Count - n;
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            for (int i = 0; i < n; i++)
            {
                double x = startIdx + i;
                double y = history[startIdx + i];
                sumX += x; sumY += y; sumXY += x * y; sumX2 += x * x;
            }
            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double intercept = (sumY - slope * sumX) / n;

            if (slope <= 0) return null; // Pressure not increasing, column is stable

            // When will pressure hit max?
            double injectionsToMax = (maxPressure - intercept) / slope;
            int remaining = (int)(injectionsToMax - history.Count);
            return remaining > 0 ? remaining : 0;
        }

        public ColumnHealthReport GetColumnHealth(int columnId)
        {
            var report = new ColumnHealthReport { ColumnId = columnId };

            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT name, column_type, role, total_injections,
                    current_performance_score, max_pressure_bar, baseline_pressure_bar,
                    install_date, status FROM columns WHERE column_id = @id";
                cmd.Parameters.AddWithValue("@id", columnId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        report.Name = reader.GetString(0);
                        report.ColumnType = reader.GetString(1);
                        report.Role = reader.GetString(2);
                        report.TotalInjections = reader.GetInt32(3);
                        report.PerformanceScore = reader.GetDouble(4);
                        report.MaxPressure = reader.IsDBNull(5) ? 0 : reader.GetDouble(5);
                        report.BaselinePressure = reader.IsDBNull(6) ? 0 : reader.GetDouble(6);
                        report.InstallDate = reader.GetString(7);
                        report.Status = reader.GetString(8);
                    }
                }
            }

            report.EstimatedRemainingInjections = EstimateRemainingInjections(columnId);

            // Get recent pressure trend
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT pressure_at_baseline_conditions FROM column_pressure_history
                    WHERE column_id = @id ORDER BY injection_number DESC LIMIT 10";
                cmd.Parameters.AddWithValue("@id", columnId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        report.RecentPressures.Add(reader.GetDouble(0));
                }
            }
            report.RecentPressures.Reverse();

            // Determine health status
            if (report.PerformanceScore >= 90) report.HealthStatus = "Excellent";
            else if (report.PerformanceScore >= 70) report.HealthStatus = "Good";
            else if (report.PerformanceScore >= 50) report.HealthStatus = "Warning";
            else if (report.PerformanceScore >= 25) report.HealthStatus = "Poor";
            else report.HealthStatus = "Critical";

            // Generate alerts
            if (report.PerformanceScore < 50)
                report.Alerts.Add("Column performance below 50% — consider replacement");
            if (report.EstimatedRemainingInjections.HasValue && report.EstimatedRemainingInjections < 50)
                report.Alerts.Add($"Estimated {report.EstimatedRemainingInjections} injections remaining");
            if (report.RecentPressures.Count >= 5)
            {
                double recent = report.RecentPressures[report.RecentPressures.Count - 1];
                double earlier = report.RecentPressures[0];
                if (recent > earlier * 1.2)
                    report.Alerts.Add("Pressure increasing >20% over recent injections — possible blockage");
                if (recent < earlier * 0.8)
                    report.Alerts.Add("Pressure decreasing >20% over recent injections — possible channeling");
            }

            return report;
        }

        // ─── Health Counters ─────────────────────────────────────────────

        public void IncrementCounter(string key, double amount = 1)
        {
            lock (_writeLock)
            {
                ExecuteNonQuery(
                    @"INSERT INTO health_counters (key, value, last_updated) VALUES (@k, @a, datetime('now','localtime'))
                      ON CONFLICT(key) DO UPDATE SET value = value + @a, last_updated = datetime('now','localtime')",
                    new[] { "@k", key, "@a", amount.ToString() });
            }
        }

        public void SetCounter(string key, double value)
        {
            lock (_writeLock)
            {
                ExecuteNonQuery(
                    @"INSERT INTO health_counters (key, value, last_updated) VALUES (@k, @v, datetime('now','localtime'))
                      ON CONFLICT(key) DO UPDATE SET value = @v, last_updated = datetime('now','localtime')",
                    new[] { "@k", key, "@v", value.ToString() });
            }
        }

        public double GetCounter(string key)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = "SELECT value FROM health_counters WHERE key = @k";
                cmd.Parameters.AddWithValue("@k", key);
                var result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value ? Convert.ToDouble(result) : 0;
            }
        }

        public Dictionary<string, double> GetAllCounters()
        {
            var counters = new Dictionary<string, double>();
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = "SELECT key, value FROM health_counters";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        counters[reader.GetString(0)] = reader.GetDouble(1);
                }
            }
            return counters;
        }

        // ─── Alerts ──────────────────────────────────────────────────────

        public void RaiseAlert(string alertType, string severity, string message,
            string source = null, string dataJson = null)
        {
            lock (_writeLock)
            {
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO alerts (alert_type, severity, message, source, data_json)
                                        VALUES (@t, @s, @m, @src, @d)";
                    cmd.Parameters.AddWithValue("@t", alertType);
                    cmd.Parameters.AddWithValue("@s", severity);
                    cmd.Parameters.AddWithValue("@m", message);
                    cmd.Parameters.AddWithValue("@src", (object)source ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@d", (object)dataJson ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            DataLogged?.Invoke("alert", new { alert_type = alertType, severity, message });
        }

        public List<Dictionary<string, object>> GetActiveAlerts()
        {
            return Query("SELECT * FROM alerts WHERE acknowledged = 0 ORDER BY timestamp DESC");
        }

        // ─── Pressure Baseline Management ────────────────────────────────

        public void UpdatePressureBaseline(string method, int? columnId,
            double gradientTimePct, double pressure)
        {
            lock (_writeLock)
            {
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO pressure_baselines
                        (method, column_id, gradient_time_pct, mean_pressure, std_pressure, sample_count)
                        VALUES (@m, @c, @g, @p, 0, 1)
                        ON CONFLICT(id) DO UPDATE SET
                            mean_pressure = (mean_pressure * sample_count + @p) / (sample_count + 1),
                            std_pressure = ABS(@p - mean_pressure),
                            sample_count = sample_count + 1,
                            last_updated = datetime('now','localtime')";
                    cmd.Parameters.AddWithValue("@m", method);
                    cmd.Parameters.AddWithValue("@c", (object)columnId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@g", gradientTimePct);
                    cmd.Parameters.AddWithValue("@p", pressure);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public PressureBaseline GetPressureBaseline(string method, int? columnId, double gradientTimePct)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT mean_pressure, std_pressure, sample_count
                    FROM pressure_baselines
                    WHERE method = @m AND gradient_time_pct = @g
                    AND (column_id = @c OR (@c IS NULL AND column_id IS NULL))
                    LIMIT 1";
                cmd.Parameters.AddWithValue("@m", method);
                cmd.Parameters.AddWithValue("@c", (object)columnId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@g", gradientTimePct);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new PressureBaseline
                        {
                            MeanPressure = reader.GetDouble(0),
                            StdPressure = reader.GetDouble(1),
                            SampleCount = reader.GetInt32(2)
                        };
                    }
                }
            }
            return null;
        }

        // ─── Templates ───────────────────────────────────────────────────

        public int SaveTemplate(string name, string parametersJson, string description = null,
            string category = null, string author = null)
        {
            lock (_writeLock)
            {
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO templates
                        (name, parameters_json, description, category, author)
                        VALUES (@n, @p, @d, @c, @a)
                        ON CONFLICT(name) DO UPDATE SET
                            parameters_json = @p, description = @d, category = @c,
                            modified = datetime('now','localtime');
                        SELECT last_insert_rowid();";
                    cmd.Parameters.AddWithValue("@n", name);
                    cmd.Parameters.AddWithValue("@p", parametersJson);
                    cmd.Parameters.AddWithValue("@d", (object)description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@c", (object)category ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@a", (object)author ?? DBNull.Value);
                    return (int)(long)cmd.ExecuteScalar();
                }
            }
        }

        public string GetTemplate(string name)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = "SELECT parameters_json FROM templates WHERE name = @n";
                cmd.Parameters.AddWithValue("@n", name);
                return cmd.ExecuteScalar()?.ToString();
            }
        }

        public List<Dictionary<string, object>> ListTemplates()
        {
            return Query("SELECT template_id, name, description, category, author, created, modified FROM templates ORDER BY name");
        }

        // ─── Audit Log ───────────────────────────────────────────────────

        public void Audit(string action, string entityType = null, string entityId = null,
            string oldValue = null, string newValue = null, string reason = null, string userId = null)
        {
            lock (_writeLock)
            {
                using (var cmd = _conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO audit_log
                        (user_id, action, entity_type, entity_id, old_value, new_value, reason)
                        VALUES (@u, @a, @et, @ei, @ov, @nv, @r)";
                    cmd.Parameters.AddWithValue("@u", (object)userId ?? Environment.UserName);
                    cmd.Parameters.AddWithValue("@a", action);
                    cmd.Parameters.AddWithValue("@et", (object)entityType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ei", (object)entityId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ov", (object)oldValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@nv", (object)newValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@r", (object)reason ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ─── Generic Query Helpers ───────────────────────────────────────

        public List<Dictionary<string, object>> Query(string sql, string[] parameters = null)
        {
            var results = new List<Dictionary<string, object>>();
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = sql;
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i += 2)
                        cmd.Parameters.AddWithValue(parameters[i], parameters[i + 1]);
                }
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        results.Add(row);
                    }
                }
            }
            return results;
        }

        private void ExecuteNonQuery(string sql, string[] parameters = null)
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = sql;
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Length; i += 2)
                        cmd.Parameters.AddWithValue(parameters[i], parameters[i + 1]);
                }
                cmd.ExecuteNonQuery();
            }
        }

        private void FlushQueue()
        {
            if (_writeQueue.IsEmpty) return;

            lock (_writeLock)
            {
                using (var tx = _conn.BeginTransaction())
                {
                    int count = 0;
                    while (_writeQueue.TryDequeue(out var action) && count < 500)
                    {
                        action(_conn);
                        count++;
                    }
                    tx.Commit();
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _flushTimer?.Dispose();
            FlushQueue(); // Flush remaining writes
            _conn?.Close();
            _conn?.Dispose();
        }
    }

    // ─── Data Models ─────────────────────────────────────────────────────

    public class ColumnHealthReport
    {
        public int ColumnId { get; set; }
        public string Name { get; set; }
        public string ColumnType { get; set; }
        public string Role { get; set; }
        public int TotalInjections { get; set; }
        public double PerformanceScore { get; set; }
        public double MaxPressure { get; set; }
        public double BaselinePressure { get; set; }
        public string InstallDate { get; set; }
        public string Status { get; set; }
        public string HealthStatus { get; set; }
        public int? EstimatedRemainingInjections { get; set; }
        public List<double> RecentPressures { get; set; } = new List<double>();
        public List<string> Alerts { get; set; } = new List<string>();
    }

    public class PressureBaseline
    {
        public double MeanPressure { get; set; }
        public double StdPressure { get; set; }
        public int SampleCount { get; set; }
    }
}
