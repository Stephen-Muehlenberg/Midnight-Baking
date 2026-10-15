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
    [Export] private AudioStreamPlayer3D stationPlayer;
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
            stations = new( )
            {
                { Channel.CLASSICAL, new StationInfo(classicalTracks) },
                { Channel.JAZZ, new StationInfo(jazzTracks) },
                { Channel.ASMR, new StationInfo(asmrTracks) }
            };
            // TODO Decide if we want a random radio time offset.
            // That is, should radio stations always start at the same point across all games?
            // Or should each game have the radio stations start at a different random point?
            //randomStartTimeOffset = new RandomNumberGenerator().RandfRange(0, 1800); // Offset audio start time by up to 30 minutes.
        }

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
        Debug.Print($"SetChannel({(int)channel})");
        this.channel = channel;

        //        stationFrequencyIndicator.Position = Vector3.Left * (int) channel / 10f;

        if (channel == Channel.OFF)
        {
//            stationFrequencyBacklight.Hide();
            stationPlayer.Stop();
            staticPlayer.Stop();
        }
        else
        {
//            stationFrequencyBacklight.Show();
            float startTime = (Time.GetTicksMsec() / 1000f) + randomStartTimeOffset;
            if (channel == Channel.STATIC)
            {
                stationPlayer.Stop();
                staticPlayer.Play(startTime);
                staticPlayer.VolumeLinear = 0.01f;
            }
            else
            {
                var station = stations[channel];
                (var stream, var startOffset) = station.GetStreamAndStartOffsetAtTime(startTime);
                stationPlayer.Stream = stream;
                stationPlayer.Play(startOffset);
                staticPlayer.Play(startTime);
                staticPlayer.VolumeLinear = 0.002f;
            }
        }
    }

    private class StationInfo
    {
        public AudioStream[] streams;
        public float duration;
        /// <summary>Indicates how many seconds it would take to reach each stream, if they were all played in order from the beginning. E.g. streams of length { 10s, 15s, 12s } would have start times of { 0s, 10s, 25s }.</summary>
        public float[] streamStartTimes;

        public StationInfo(AudioStream[] streams)
        {
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