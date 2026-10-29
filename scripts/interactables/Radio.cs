using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace MidnightBaking.scripts.interactables;

public partial class Radio : Interactable
{
    private enum Channel { OFF, CLASSICAL, JAZZ, STATIC, ASMR, }
    [Export] private Node3D stationFrequencyIndicator;
    [Export] private Node3D stationFrequencyBacklight;
    [Export] private Label3D nowPlayingLabel;
    [Export] private AudioStreamPlayer3D stationPlayer;
    // Have a separate static player so that we can always have background static playing in addition to the music.
    [Export] private AudioStreamPlayer3D staticPlayer;
    [Export] private AudioStream[] classicalTracks;
    [Export] private AudioStream[] jazzTracks;
    [Export] private AudioStream[] asmrTracks;

    private Channel channel;
    private Dictionary<Channel, StationInfo> stations;
    private float randomStartTimeOffset;

    protected override void _ResetToGameStartState()
    {
        // First time initialisation
        if (stations == null)
        {
            stations = new Dictionary<Channel, StationInfo>
            {
                { Channel.CLASSICAL, new StationInfo("Classical",0.01f, classicalTracks) },
                { Channel.JAZZ, new StationInfo("Jazz", 0.006f, jazzTracks) },
                { Channel.ASMR, new StationInfo("ASMR", 100f, asmrTracks) }
            };
            // TODO Decide if we want a random radio time offset.
            // That is, should radio stations always start at the same point across all games?
            // Or should each game have the radio stations start at a different random point?
            //randomStartTimeOffset = new RandomNumberGenerator().RandfRange(0, 1800); // Offset audio start time by up to 30 minutes.
        }

        nowPlayingLabel.Text = string.Empty;

        SetChannel(Channel.OFF);
        SetCanInteract(true);
    }

    protected override void _HandleClick()
    {
        ChangeToNextChannel();
    }

    private void ChangeToNextChannel()
    {
        Channel newChannel = channel + 1;
        if ((int) newChannel >= Enum.GetNames(typeof(Channel)).Length)
            newChannel = 0;
        SetChannel(newChannel);
    }

    private void SetChannel(Channel channel)
    {
        this.channel = channel;

        //        stationFrequencyIndicator.Position = Vector3.Left * (int) channel / 10f;

        if (channel == Channel.OFF)
        {
//            stationFrequencyBacklight.Hide();
            stationPlayer.Stop();
            staticPlayer.Stop();
            nowPlayingLabel.Text = string.Empty;
        }
        else
        {
//            stationFrequencyBacklight.Show();
            float startTime = (Time.GetTicksMsec() / 1000f) + randomStartTimeOffset;
            
            // Always play a small amount of background static, but play it louder if it's the dedicated static channel.
            staticPlayer.Play(startTime);
            staticPlayer.VolumeLinear = channel == Channel.STATIC
                ? 0.01f
                : 0.002f;
            
            if (channel == Channel.STATIC)
            {
                stationPlayer.Stop();
                SetNowPlayingLabel("[Static]");
            }
            else
            {
                var station = stations[channel];
                var (stream, startOffset) = station.GetStreamAndStartOffsetAtTime(startTime);
                stationPlayer.Stream = stream;
                stationPlayer.Play(startOffset);
                stationPlayer.VolumeLinear = station.volume;
                SetNowPlayingLabel(station.displayName);
            }
        }
    }

    private void SetNowPlayingLabel(string text)
    {
        nowPlayingLabel.Text = $"Now Playing: {text}";
    }

    private class StationInfo
    {
        public string displayName;
        public float volume;
        public AudioStream[] streams;
        public float duration;
        /// <summary>Indicates how many seconds it would take to reach each stream, if they were all played in order from the beginning. E.g. streams of length { 10s, 15s, 12s } would have start times of { 0s, 10s, 25s }.</summary>
        public float[] streamStartTimes;

        public StationInfo(string displayName, float volume, AudioStream[] streams)
        {
            this.displayName = displayName;
            this.volume = volume;
            this.streams = streams;
            streamStartTimes = new float[streams.Length];
            for (int i = 0; i < streams.Length; i++)
            {
                streamStartTimes[i] = duration;
                duration += (float)streams[i].GetLength();
            }
        }

        public Tuple<AudioStream, float> GetStreamAndStartOffsetAtTime(float time)
        {
            float wrappedTime = time;
            while (wrappedTime >= duration)
                wrappedTime -= duration;
            int streamIndex = streams.Length - 1;
            for (int i = 0; i < streams.Length - 1; i++)
            {
                if (streamStartTimes[i + 1] > wrappedTime)
                {
                    streamIndex = i;
                    break;
                }
            }
            return new(streams[streamIndex], wrappedTime - streamStartTimes[streamIndex]);
        } 
    }
}