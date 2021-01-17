#if DEVICE_SIMULATOR_ENABLED || UNITY_2021_1_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_2021_1_OR_NEWER
using UnityEditor.DeviceSimulation;
#elif DEVICE_SIMULATOR_ENABLED
using Unity.DeviceSimulator;
#endif
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.AdaptivePerformance.Simulator.Editor;
using UnityEngine.AdaptivePerformance;

namespace UnityEditor.AdaptivePerformance.Editor
{
    public class AdaptivePerformanceUIExtension :
#if UNITY_2021_1_OR_NEWER
        DeviceSimulatorPlugin
#elif DEVICE_SIMULATOR_ENABLED
        IDeviceSimulatorExtension
#endif
        , ISerializationCallbackReceiver
    {
#if UNITY_2021_1_OR_NEWER
        override public string title
#elif DEVICE_SIMULATOR_ENABLED
        public string extensionTitle
#endif
        { get { return "Adaptive Performance"; } }

#if UNITY_2021_1_OR_NEWER
        override public VisualElement OnCreateUI()
        {
            m_ExtensionFoldout = new VisualElement();
#elif DEVICE_SIMULATOR_ENABLED
        public void OnExtendDeviceSimulator(VisualElement visualElement)
        {
            m_ExtensionFoldout = visualElement;
#endif
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.adaptiveperformance/Editor/DeviceSimulator/AdaptivePerformanceExtension.uxml");
            m_ExtensionFoldout.Add(tree.CloneTree());

            m_ThermalFoldout = m_ExtensionFoldout.Q<Foldout>("thermal");
            m_ThermalFoldout.value = m_SerializationStates.thermalFoldout;
            m_WarningLevel = m_ExtensionFoldout.Q<EnumField>("thermal-warning-level");
            m_TemperatureLevel = m_ExtensionFoldout.Q<Slider>("thermal-temperature-level");
            m_TemperatureLevelField = m_ExtensionFoldout.Q<FloatField>("thermal-temperature-level-field");
            m_TemperatureTrend = m_ExtensionFoldout.Q<Slider>("thermal-temperature-trend");
            m_TemperatureTrendField = m_ExtensionFoldout.Q<FloatField>("thermal-temperature-trend-field");
            m_PerformanceFoldout = m_ExtensionFoldout.Q<Foldout>("performance");
            m_PerformanceFoldout.value = m_SerializationStates.performanceFoldout;
            m_TargetFPS = m_ExtensionFoldout.Q<SliderInt>("performance-target-fps");
            m_TargetFPSField = m_ExtensionFoldout.Q<IntegerField>("performance-target-fps-field");
            m_ControlAutoMode = m_ExtensionFoldout.Q<Toggle>("performance-control-auto-mode");
            m_CpuLevel = m_ExtensionFoldout.Q<SliderInt>("performance-cpu-level");
            m_CpuLevelField = m_ExtensionFoldout.Q<IntegerField>("performance-cpu-level-field");
            m_GpuLevel = m_ExtensionFoldout.Q<SliderInt>("performance-gpu-level");
            m_GpuLevelField = m_ExtensionFoldout.Q<IntegerField>("performance-gpu-level-field");
            m_Bottleneck = m_ExtensionFoldout.Q<EnumField>("performance-bottleneck");
            m_DevLogging = m_ExtensionFoldout.Q<Toggle>("developer-logging");
            m_DevLoggingFrequency = m_ExtensionFoldout.Q<IntegerField>("developer-logging-frequency");
            m_DeveloperFoldout = m_ExtensionFoldout.Q<Foldout>("developer-options");
            m_DeveloperFoldout.value = m_SerializationStates.developerFoldout;
            m_IndexerFoldout = m_ExtensionFoldout.Q<Foldout>("indexer");
            m_IndexerFoldout.value = m_SerializationStates.indexerFoldout;
            m_ThermalAction = m_IndexerFoldout.Q<EnumField>("indexer-thermal-action");
            m_PerformanceAction = m_IndexerFoldout.Q<EnumField>("indexer-performance-action");
            m_ScalersFoldout = m_ExtensionFoldout.Q<Foldout>("scalers");
            m_ScalersFoldout.value = m_SerializationStates.scalersFoldout;

            // Create settings for each one of the scalers
            Type ti = typeof(AdaptivePerformanceScaler);
            var scalerTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.adaptiveperformance/Editor/DeviceSimulator/ScalerControl.uxml");

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in asm.GetTypes())
                {
                    if (ti.IsAssignableFrom(t) && !t.IsAbstract)
                    {
                        var scalerInstance = ScriptableObject.CreateInstance(t) as AdaptivePerformanceScaler;
                        if (scalerInstance == null)
                            continue;

                        // Load the UI elements
                        var container = scalerTree.CloneTree();
                        var valueSlider = container.Q<Slider>("scaler-slider");
                        var toggle = container.Q<Toggle>("scaler-toggle");
                        var valueField = container.Q<IntegerField>("scaler-value");

                        // Set the values and additional config for these elements
                        toggle.labelElement.text = t.Name;
                        toggle.value = scalerInstance.Enabled;
                        toggle.name = $"{t.Name}-scaler-toggle";
                        valueSlider.value = scalerInstance.CurrentLevel;
                        valueSlider.name = $"{t.Name}-scaler-slider";
                        valueSlider.SetEnabled(scalerInstance.Enabled);
                        valueField.value = scalerInstance.CurrentLevel;
                        valueField.name = $"{t.Name}-scaler-value";
                        valueField.SetEnabled(scalerInstance.Enabled);

                        // Now set up the callback actions
                        toggle.RegisterCallback<ChangeEvent<bool>>(evt =>
                        {
                            var scaler = FindScalerObject(t);
                            if (scaler == null)
                                return;

                            scaler.Enabled = evt.newValue;
                            valueField.SetEnabled(evt.newValue);
                            valueSlider.SetEnabled(evt.newValue);
                        });

                        valueSlider.RegisterCallback<ChangeEvent<float>>(evt =>
                        {
                            var scaler = FindScalerObject(t);
                            if (scaler == null)
                                return;

                            scaler.OverrideLevel = (int)Mathf.Clamp(evt.newValue, 0, scaler.MaxLevel);
                            valueField.SetValueWithoutNotify(scaler.OverrideLevel);
                        });

                        valueField.RegisterCallback<ChangeEvent<int>>(evt =>
                        {
                            var scaler = FindScalerObject(t);
                            if (evt.newValue < 0 || scaler == null)
                                return;

                            scaler.OverrideLevel = Mathf.Clamp(evt.newValue, 0, scaler.MaxLevel);
                            valueSlider.SetValueWithoutNotify(scaler.OverrideLevel);
                            valueField.SetValueWithoutNotify(scaler.OverrideLevel);
                        });

                        m_ScalersFoldout.Add(container);
                    }
                }
            }


            m_WarningLevel.RegisterCallback<ChangeEvent<Enum>>(evt =>
            {
                SimulatorAdaptivePerformanceSubsystem subsystem = Subsystem();
                if (subsystem == null)
                    return;

                subsystem.WarningLevel = (WarningLevel)evt.newValue;
            });
            m_TemperatureLevel.RegisterCallback<ChangeEvent<float>>(evt =>
            {
                m_TemperatureLevelField.value = evt.newValue;
                SimulatorAdaptivePerformanceSubsystem subsystem = Subsystem();
                if (subsystem == null)
                    return;

                subsystem.TemperatureLevel = evt.newValue;
            });
            m_TemperatureLevelField.RegisterCallback<ChangeEvent<float>>(evt =>
            {
                var newTemperatureLevel = evt.newValue;
                if (newTemperatureLevel < m_TemperatureLevel.lowValue)
                {
                    newTemperatureLevel = m_TemperatureLevel.lowValue;
                    m_TemperatureLevelField.value = newTemperatureLevel;
                }
                if (newTemperatureLevel > m_TemperatureLevel.highValue)
                {
                    newTemperatureLevel = m_TemperatureLevel.highValue;
                    m_TemperatureLevelField.value = newTemperatureLevel;
                }

                m_TemperatureLevel.value = newTemperatureLevel;

                SimulatorAdaptivePerformanceSubsystem subsystem = Subsystem();
                if (subsystem == null)
                    return;

                subsystem.TemperatureLevel = newTemperatureLevel;
            });
            m_TemperatureTrend.RegisterCallback<ChangeEvent<float>>(evt =>
            {
                m_TemperatureTrendField.value = evt.newValue;

                SimulatorAdaptivePerformanceSubsystem subsystem = Subsystem();
                if (subsystem == null)
                    return;

                subsystem.TemperatureTrend = evt.newValue;
            });
            m_TemperatureTrendField.RegisterCallback<ChangeEvent<float>>(evt =>
            {
                var newTemperatureTrend = evt.newValue;
                if (newTemperatureTrend < m_TemperatureTrend.lowValue)
                {
                    newTemperatureTrend = m_TemperatureTrend.lowValue;
                    m_TemperatureTrendField.value = newTemperatureTrend;
                }
                if (newTemperatureTrend > m_TemperatureTrend.highValue)
                {
                    newTemperatureTrend = m_TemperatureTrend.highValue;
                    m_TemperatureTrendField.value = newTemperatureTrend;
                }

                m_TemperatureTrend.value = newTemperatureTrend;

                SimulatorAdaptivePerformanceSubsystem subsystem = Subsystem();
                if (subsystem == null)
                    return;

                subsystem.TemperatureTrend = newTemperatureTrend;
            });
            m_TargetFPS.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                // sync value field
                m_TargetFPSField.value = evt.newValue;

                Application.targetFrameRate = evt.newValue;
                SetBottleneck((PerformanceBottleneck)m_Bottleneck.value, Subsystem());
            });
            m_TargetFPSField.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                var newTargetFPS = evt.newValue;
                if (newTargetFPS < m_TargetFPS.lowValue)
                {
                    newTargetFPS = m_TargetFPS.lowValue;
                    m_TargetFPSField.SetValueWithoutNotify(newTargetFPS);
                }
                if (newTargetFPS > m_TargetFPS.highValue)
                {
                    newTargetFPS = m_TargetFPS.highValue;
                    m_TargetFPSField.SetValueWithoutNotify(newTargetFPS);
                }

                m_TargetFPS.value = newTargetFPS;

                Application.targetFrameRate = newTargetFPS;
                SetBottleneck((PerformanceBottleneck)m_Bottleneck.value, Subsystem());
            });
            m_ControlAutoMode.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                var ap = Holder.Instance;
                if (ap == null)
                    return;

                var ctrl = ap.DevicePerformanceControl;
                ctrl.AutomaticPerformanceControl = evt.newValue;

                SimulatorAdaptivePerformanceSubsystem subsystem = Subsystem();
                if (subsystem == null)
                    return;

                subsystem.AcceptsPerformanceLevel = true;
            });
            m_CpuLevel.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                // sync value field
                m_CpuLevelField.value = evt.newValue;

                var ap = Holder.Instance;
                if (ap == null)
                    return;
                ap.DevicePerformanceControl.CpuLevel = evt.newValue;
            });
            m_CpuLevelField.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                var newCPULevel = evt.newValue;
                if (newCPULevel < m_CpuLevel.lowValue)
                {
                    newCPULevel = m_CpuLevel.lowValue;
                    m_CpuLevelField.SetValueWithoutNotify(newCPULevel);
                }
                if (newCPULevel > m_CpuLevel.highValue)
                {
                    newCPULevel = m_CpuLevel.highValue;
                    m_CpuLevelField.SetValueWithoutNotify(newCPULevel);
                }

                m_CpuLevel.value = newCPULevel;

                var ap = Holder.Instance;
                if (ap == null)
                    return;
                ap.DevicePerformanceControl.CpuLevel = newCPULevel;
            });
            m_GpuLevel.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                // sync value field
                m_GpuLevelField.value = evt.newValue;

                var ap = Holder.Instance;
                if (ap == null)
                    return;
                ap.DevicePerformanceControl.GpuLevel = evt.newValue;
            });
            m_GpuLevelField.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                var newGPULevel = evt.newValue;
                if (newGPULevel < m_GpuLevel.lowValue)
                {
                    newGPULevel = m_GpuLevel.lowValue;
                    m_GpuLevelField.SetValueWithoutNotify(newGPULevel);
                }
                if (newGPULevel > m_GpuLevel.highValue)
                {
                    newGPULevel = m_GpuLevel.highValue;
                    m_GpuLevelField.SetValueWithoutNotify(newGPULevel);
                }

                m_GpuLevel.value = newGPULevel;

                var ap = Holder.Instance;
                if (ap == null)
                    return;
                ap.DevicePerformanceControl.GpuLevel = newGPULevel;
            });
            m_Bottleneck.RegisterCallback<ChangeEvent<Enum>>(evt =>
            {
                SimulatorAdaptivePerformanceSubsystem subsystem = Subsystem();
                if (subsystem == null)
                    return;

                SetBottleneck((PerformanceBottleneck)evt.newValue, subsystem);
            });
            m_DevLogging.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                var ap = Holder.Instance;
                if (ap == null)
                    return;
                var devSettings = ap.DevelopmentSettings;

                devSettings.Logging = evt.newValue;
            });
            m_DevLoggingFrequency.RegisterCallback<ChangeEvent<int>>(evt =>
            {
                var ap = Holder.Instance;
                if (ap == null)
                    return;
                var devSettings = ap.DevelopmentSettings;

                devSettings.LoggingFrequencyInFrames = evt.newValue;
            });

            m_ThermalAction.RegisterCallback<ChangeEvent<Enum>>(evt =>
            {
                var ap = Holder.Instance;
                if (ap == null)
                    return;

                var indexer = ap.Indexer;
                if (indexer == null)
                    return;

                SimulatorAdaptivePerformanceSubsystem subsystem = Subsystem();
                if (subsystem == null)
                    return;

                var temperatureTrend = 0.0f;
                var warningLevel = WarningLevel.NoWarning;
                var temperatureLevel = 0.0f;

                switch (evt.newValue)
                {
                    case StateAction.Stale:
                        temperatureTrend = 0;
                        warningLevel = WarningLevel.Throttling;
                        temperatureLevel = 0.7f;
                        break;
                    case StateAction.Decrease:
                        temperatureTrend = 0.3f;
                        warningLevel = WarningLevel.Throttling;
                        temperatureLevel = 0.7f;
                        break;
                    case StateAction.FastDecrease:
                        temperatureTrend = 0.6f;
                        warningLevel = WarningLevel.Throttling;
                        temperatureLevel = 1.0f;
                        break;
                    case StateAction.Increase:
                        temperatureTrend = 0;
                        warningLevel = WarningLevel.NoWarning;
                        temperatureLevel = 0.2f;
                        break;
                }

                // Set subsystem values
                subsystem.TemperatureLevel = temperatureLevel;
                subsystem.TemperatureTrend = temperatureTrend;
                subsystem.WarningLevel = warningLevel;

                // Update the UI to match
                m_TemperatureLevel.SetValueWithoutNotify(temperatureLevel);
                m_TemperatureTrend.SetValueWithoutNotify(temperatureTrend);
                m_WarningLevel.SetValueWithoutNotify(warningLevel);
            });

            m_PerformanceAction.RegisterCallback<ChangeEvent<Enum>>(evt =>
            {
                var ap = Holder.Instance;
                if (ap == null)
                    return;

                var indexer = ap.Indexer;
                if (indexer == null)
                    return;

                SimulatorAdaptivePerformanceSubsystem subsystem = Subsystem();
                if (subsystem == null)
                    return;

                var targetFrameRate = Application.targetFrameRate;
                if (m_OriginalTargetFramerate == 0)
                    m_OriginalTargetFramerate = targetFrameRate;

                // default target framerate is -1 to use default platform framerate so we assume it's 60
                if (targetFrameRate == -1)
                    targetFrameRate = 60;

                var frameMs = Holder.Instance.PerformanceStatus.FrameTiming.AverageFrameTime;
                switch (evt.newValue)
                {
                    case StateAction.Increase:
                    case StateAction.Stale:
                        targetFrameRate = m_OriginalTargetFramerate;
                        break;
                    case StateAction.Decrease:
                        targetFrameRate = (int)((1.0f + 0.2f) / frameMs);
                        break;
                    case StateAction.FastDecrease:
                        targetFrameRate = (int)((1.0f + 0.4f) / frameMs);
                        break;
                }

                Application.targetFrameRate = targetFrameRate;
            });

            EditorApplication.playModeStateChanged += LogPlayModeState;

            SyncAPSubsystemSettingsToEditor();
#if UNITY_2021_1_OR_NEWER
            return m_ExtensionFoldout;
#endif
        }

        VisualElement m_ExtensionFoldout;
        Foldout m_ThermalFoldout;
        EnumField m_WarningLevel;
        Slider m_TemperatureLevel;
        FloatField m_TemperatureLevelField;
        Slider m_TemperatureTrend;
        FloatField m_TemperatureTrendField;
        Foldout m_PerformanceFoldout;
        Toggle m_ControlAutoMode;
        SliderInt m_TargetFPS;
        IntegerField m_TargetFPSField;
        SliderInt m_CpuLevel;
        IntegerField m_CpuLevelField;
        SliderInt m_GpuLevel;
        IntegerField m_GpuLevelField;
        EnumField m_Bottleneck;
        Foldout m_DeveloperFoldout;
        Toggle m_DevLogging;
        IntegerField m_DevLoggingFrequency;
        Foldout m_IndexerFoldout;
        EnumField m_ThermalAction;
        EnumField m_PerformanceAction;
        Foldout m_ScalersFoldout;

        SimulatorAdaptivePerformanceSubsystem m_Subsystem;

        List<AdaptivePerformanceScaler> m_Scalers = new List<AdaptivePerformanceScaler>();

        int m_OriginalTargetFramerate = 0;

        [SerializeField, HideInInspector]
        AdaptivePerformanceStates m_SerializationStates;

        [System.Serializable]
        internal struct AdaptivePerformanceStates
        {
            public bool thermalFoldout;
            public bool performanceFoldout;
            public bool developerFoldout;
            public bool indexerFoldout;
            public bool scalersFoldout;
        }

        public void OnBeforeSerialize()
        {
            m_SerializationStates.thermalFoldout = m_ThermalFoldout.value;
            m_SerializationStates.performanceFoldout = m_PerformanceFoldout.value;
            m_SerializationStates.developerFoldout = m_DeveloperFoldout.value;
            m_SerializationStates.indexerFoldout = m_IndexerFoldout.value;
            m_SerializationStates.scalersFoldout = m_ScalersFoldout.value;
        }

        public void OnAfterDeserialize() {}

        void LogPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                SyncAPSubsystemSettingsToEditor();
                SyncScalerSettingsToEditor();
            }
        }

        void SyncScalerSettingsToEditor()
        {
            // Now go through all the scaler sliders and set the actual values. We can only do this properly
            // once the system has been initialized.
            Type ti = typeof(AdaptivePerformanceScaler);

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in asm.GetTypes())
                {
                    if (ti.IsAssignableFrom(t) && !t.IsAbstract)
                    {
                        var scaler = FindScalerObject(t);
                        if (scaler == null)
                            return;

                        var valueSlider = m_ScalersFoldout.Q<Slider>($"{t.Name}-scaler-slider");
                        var toggle = m_ScalersFoldout.Q<Toggle>($"{t.Name}-scaler-toggle");

                        valueSlider.highValue = scaler.MaxLevel;
                        toggle.value = scaler.Enabled;
                    }
                }
            }
        }

        void SyncAPSubsystemSettingsToEditor()
        {
            var ap = Holder.Instance;
            if (ap == null)
                return;

            var ctrl = ap.DevicePerformanceControl;
            var devSettings = ap.DevelopmentSettings;
            var perfMetrics = ap.PerformanceStatus.PerformanceMetrics;
            var thermalMetrics = ap.ThermalStatus.ThermalMetrics;

            m_WarningLevel.value = thermalMetrics.WarningLevel;
            m_TemperatureLevel.value = thermalMetrics.TemperatureLevel;
            m_TemperatureLevelField.value = thermalMetrics.TemperatureLevel;
            m_TemperatureTrend.value = thermalMetrics.TemperatureTrend;
            m_TemperatureTrendField.value = thermalMetrics.TemperatureTrend;
            m_TargetFPS.value = Application.targetFrameRate;
            m_TargetFPS.highValue = 140;
            m_TargetFPS.lowValue = 0;
            m_TargetFPSField.value = Application.targetFrameRate;
            m_ControlAutoMode.value = ctrl.AutomaticPerformanceControl;
            m_CpuLevel.value = ctrl.CpuLevel;
            m_CpuLevel.highValue = ctrl.MaxCpuPerformanceLevel;
            m_CpuLevel.lowValue = 0;
            m_CpuLevelField.value = ctrl.CpuLevel;
            m_GpuLevel.value = ctrl.GpuLevel;
            m_GpuLevel.highValue = ctrl.MaxGpuPerformanceLevel;
            m_GpuLevel.lowValue = 0;
            m_GpuLevelField.value = ctrl.GpuLevel;
            m_Bottleneck.value = perfMetrics.PerformanceBottleneck;
            m_DevLogging.value = devSettings.Logging;
            m_DevLoggingFrequency.value = devSettings.LoggingFrequencyInFrames;
            m_ThermalAction.value = ap.Indexer.ThermalAction;
            m_PerformanceAction.value = ap.Indexer.PerformanceAction;

            // Set bottleneck so we get CPU/GPU frametimes and a valid bottleneck
            SetBottleneck((PerformanceBottleneck)m_Bottleneck.value, Subsystem());
        }

        void SetBottleneck(PerformanceBottleneck performanceBottleneck, SimulatorAdaptivePerformanceSubsystem subsystem)
        {
            if (subsystem == null)
                return;

            var targetFrameRate = Application.targetFrameRate;

            // default target framerate is -1 to use default platform framerate so we assume it's 60
            if (targetFrameRate == -1)
                targetFrameRate = 60;

            var currentTargetFramerateHalfMS = 1.0f / targetFrameRate / 2.0f;
            var currentTargetFramerateMS = 1.0f / targetFrameRate;
            switch (performanceBottleneck)
            {
                case PerformanceBottleneck.CPU: // averageOverallFrametime > targetFramerate && averageCpuFrametime >= averageOverallFrametime
                    subsystem.NextCpuFrameTime = currentTargetFramerateMS + 0.001f;
                    subsystem.NextGpuFrameTime = currentTargetFramerateHalfMS;
                    subsystem.NextOverallFrameTime = currentTargetFramerateMS + 0.001f;
                    break;
                case PerformanceBottleneck.GPU: // averageOverallFrametime > targetFramerate && averageGpuFrametime >= averageOverallFrametime
                    subsystem.NextCpuFrameTime = currentTargetFramerateHalfMS;
                    subsystem.NextGpuFrameTime = currentTargetFramerateMS + 0.001f;
                    subsystem.NextOverallFrameTime = currentTargetFramerateMS + 0.001f;
                    break;
                case PerformanceBottleneck.TargetFrameRate: // averageOverallFrametime == targetFramerate
                    subsystem.NextCpuFrameTime = currentTargetFramerateHalfMS;
                    subsystem.NextGpuFrameTime = currentTargetFramerateHalfMS;
                    subsystem.NextOverallFrameTime = currentTargetFramerateMS;
                    break;
                //PerformanceBottleneck.Unknowe - averageOverallFrametime > targetFramerate
                default:
                    subsystem.NextCpuFrameTime = currentTargetFramerateHalfMS;
                    subsystem.NextGpuFrameTime = currentTargetFramerateHalfMS;
                    subsystem.NextOverallFrameTime = currentTargetFramerateMS + 0.001f;
                    break;
            }
        }

        AdaptivePerformanceScaler FindScalerObject(Type scalerType)
        {
            if (Holder.Instance == null || Holder.Instance.Indexer == null)
                return null;

            // We're only going to compile this list once so we need to combine all internal indexer lists of scalers.
            if (m_Scalers.Count == 0)
            {
                // First get unappplied scalers
                List<AdaptivePerformanceScaler> scalers = new List<AdaptivePerformanceScaler>();
                Holder.Instance.Indexer.GetUnappliedScalers(ref scalers);
                m_Scalers.AddRange(scalers);

                // Then add applied scalers
                Holder.Instance.Indexer.GetAppliedScalers(ref scalers);
                m_Scalers.AddRange(scalers);

                // Finally add disabled scalers
                Holder.Instance.Indexer.GetDisabledScalers(ref scalers);
                m_Scalers.AddRange(scalers);
            }

            return m_Scalers.Find(s => s.GetType() == scalerType);
        }

        SimulatorAdaptivePerformanceSubsystem Subsystem()
        {
            if (!Application.isPlaying)
                return null;

            var loader = AdaptivePerformanceGeneralSettings.Instance?.Manager.activeLoader;
            if (m_Subsystem == null && loader != null)
            {
                m_Subsystem = loader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            }
            return m_Subsystem;
        }
    }
}
#endif
