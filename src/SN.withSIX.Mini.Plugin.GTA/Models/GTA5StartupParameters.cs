// <copyright company="SIX Networks GmbH" file="GTA5StartupParameters.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.ComponentModel;
using SN.withSIX.Mini.Core.Games;

namespace SN.withSIX.Mini.Plugin.GTA.Models
{
    public class GTA5StartupParameters : GTAStartupParameters
    {
        public GTA5StartupParameters(params string[] defaultParameters) : base(defaultParameters) {}
        [Category(GameSettingCategories.Benchmarking)]
        [Description("Automatically loads the in-game benchmark instead of single or multiplayer game modes")]
        public bool Benchmark
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Category(GameSettingCategories.Benchmarking)]
        [Description("Output frame times from the benchmark to help identify stuttering")]
        public bool BenchmarkFrameTimes
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Category(GameSettingCategories.Benchmarking)]
        [Description("Specifies the number of benchmark runs")]
        public string BenchmarkIterations
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Benchmarking)]
        [Description("Limits the benchmark to one of the four scenes")]
        public string BenchmarkPass
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Category(GameSettingCategories.Benchmarking)]
        [Description("Disables audio in the benchmark")]
        public bool BenchmarkNoAudio
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Disables Hyper Threading on CPUs")]
        public bool DisableHyperThreading
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Loads the game directly into a multiplayer match")]
        public bool GoStraightToMP
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Prevents the game from resetting graphics options when swapping GPUs")]
        public bool IgnoreDifferentVideoCard
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Specify the number of GPUs that should be utilized")]
        public string GPUCount
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description(
            "Overrides the Population Density setting, enabling you to manually specify the number of civilians. Use in concert with -vehicleLodBias to fine-tune Population Density to your tastes"
            )]
        public string PedLodBias
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
        [Description(
            "Sets Rockstar Social Club to offline mode, which helps accelerate single-player loading times, and eliminates any spoilerific pop-ups about friends' progress"
            )]
        public bool ScOfflineOnly
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description("Loads the game directly into multiplayer freemode")]
        public bool StraightIntoFreemode
        {
            get { return GetSwitchOrDefault(); }
            set { SetSwitchOrDefault(value); }
        }
        [Description(
            "Overrides the Population Density setting, enabling you to manually specify the number of vehicles. Use in concert with -pedLodBias to fine-tune Population Density to your tastes"
            )]
        public string VehicleLodBias
        {
            get { return GetPropertyOrDefault(); }
            set { SetPropertyOrDefault(value); }
        }
    }
}