using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using ioanDataMapping;
/// <summary>
/// A gui for mappings in IOAN. 
/// </summary>
public class ioanDataMapper : MonoBehaviour
{
    [Header("taps")]
    public DataMapper tapMagnitudeToPitch;
    public DataMapper tapIntensityToVelocity;
    public DataMapper tapAngleToTimbre;
    public DataMapper tapColorToInstrument;

    [Header("drones")]
    public DataMapper droneMagnitudeToPitch;
    public DataMapper droneRmsToVelocity;
    public DataMapper dronePeriodToMaxDuration;
    public DataMapper droneSnrToModulationMix;
    [Space(5)]
    public DataMapper droneMagRmsToDroneVib;
    public DataMapper droneMagPeriodToDroneSpeed;
    public DataMapper droneMagSnrToDroneHarm;
    public DataMapper droneSnrToDroneFb;
    public DataMapper droneMagSnrToInstrument;
    public DataMapper droneColorToInstrument;
    public DataMapper droneRmsToInstrument;

    [Header("sequence")]
    public DataMapper sequenceLightcurveMagToPitch;
    public DataMapper sequenceRmsToTempo;
    public DataMapper sequencePeriodToDuration;
    public DataMapper sequenceSnrToRythmicVariation;
    public DataMapper magSnrToInstrument;
    [Header("sequence pitch new")]
    public SequenceMapper sequenceMapper;

    [Header("wave")]
    public DataMapper waveSnrMapToPitch;
    public DataMapper waveRmsMapToPitch;
    public DataMapper waveTeffMapToPitch;
    public DataMapper waveVarMapToPitch;


    [Header("scale mapping")]
    [HideInInspector]public bool bypassQuantization = false;
    public bool useDynamicScaleTransposition = false;
    public ScaleMapper mappedScale;


    Config config; 
    private void Start()
    {
        Config config = Config.Instance;

        if (config.Data.scale != "default")
        {
            bool scaleExists = System.Enum.TryParse<ScaleMapper.DefinedScales>(config.Data.scale, true, out mappedScale.scale);

            if (mappedScale.scale == ScaleMapper.DefinedScales.user && scaleExists) mappedScale.userDefinedScale = config.Data.userScale;
            if (config.Data.pitchFieldSize != -1) mappedScale.pitchFieldSizeInSemitones = config.Data.pitchFieldSize;
        }
    }

    public void UpdateAudioRangeValues(ConfigData data)
    {
        //Update Taps
        tapMagnitudeToPitch.newLowValue = data.tapMagToPitchRange.x;
        tapMagnitudeToPitch.newHighValue = data.tapMagToPitchRange.y;
        tapMagnitudeToPitch.SetCurve(data.tapMagToPitchRange.z);

        tapIntensityToVelocity.newLowValue = data.tapIntToVelocityRange.x;
        tapIntensityToVelocity.newHighValue = data.tapIntToVelocityRange.y;
        tapIntensityToVelocity.SetCurve(data.tapIntToVelocityRange.z);

        tapAngleToTimbre.newLowValue = data.tapAngleToTimbreRange.x;
        tapAngleToTimbre.newHighValue = data.tapAngleToTimbreRange.y;
        tapAngleToTimbre.SetCurve(data.tapAngleToTimbreRange.z);

        tapColorToInstrument.newLowValue = data.tapColorToInstrumentRange.x;
        tapColorToInstrument.newHighValue = data.tapColorToInstrumentRange.y;
        tapColorToInstrument.SetCurve(data.tapColorToInstrumentRange.z);

        //Update Drones
        droneMagnitudeToPitch.newLowValue = data.droneMagToPitchRange.x;
        droneMagnitudeToPitch.newHighValue = data.droneMagToPitchRange.y;
        droneMagnitudeToPitch.SetCurve(data.droneMagToPitchRange.z);

        droneRmsToVelocity.newLowValue = data.droneRMSToVelocityRange.x;
        droneRmsToVelocity.newHighValue = data.droneRMSToVelocityRange.y;
        droneRmsToVelocity.SetCurve(data.droneRMSToVelocityRange.z);

        dronePeriodToMaxDuration.newLowValue = data.dronePeriodToMaxDurRange.x;
        dronePeriodToMaxDuration.newHighValue = data.dronePeriodToMaxDurRange.y;
        dronePeriodToMaxDuration.SetCurve(data.dronePeriodToMaxDurRange.z);

        droneSnrToModulationMix.newLowValue = data.droneSNRToModMixRange.x;
        droneSnrToModulationMix.newHighValue = data.droneSNRToModMixRange.y;
        droneSnrToModulationMix.SetCurve(data.droneSNRToModMixRange.z);

        droneMagRmsToDroneVib.newLowValue = data.droneMagRMSToDroneVibRange.x;
        droneMagRmsToDroneVib.newHighValue = data.droneMagRMSToDroneVibRange.y;
        droneMagRmsToDroneVib.SetCurve(data.droneMagRMSToDroneVibRange.z);

        droneMagPeriodToDroneSpeed.newLowValue = data.droneMagPeriodToDroneSpeedRange.x;
        droneMagPeriodToDroneSpeed.newHighValue = data.droneMagPeriodToDroneSpeedRange.y;
        droneMagPeriodToDroneSpeed.SetCurve(data.droneMagPeriodToDroneSpeedRange.z);

        droneMagSnrToDroneHarm.newLowValue = data.droneMagSNRToDroneHarmRange.x;
        droneMagSnrToDroneHarm.newHighValue = data.droneMagSNRToDroneHarmRange.y;
        droneMagSnrToDroneHarm.SetCurve(data.droneMagSNRToDroneHarmRange.z);

        droneSnrToDroneFb.newLowValue = data.droneSNRToDroneFBRange.x;
        droneSnrToDroneFb.newHighValue = data.droneSNRToDroneFBRange.y;
        droneSnrToDroneFb.SetCurve(data.droneSNRToDroneFBRange.z);

        droneMagSnrToInstrument.newLowValue = data.droneMagSNRToInstrumentRange.x;
        droneMagSnrToInstrument.newHighValue = data.droneMagSNRToInstrumentRange.y;
        droneMagSnrToInstrument.SetCurve(data.droneMagSNRToInstrumentRange.z);

        droneColorToInstrument.newLowValue = data.droneColorToInstrumentRange.x;
        droneColorToInstrument.newHighValue = data.droneColorToInstrumentRange.y;
        droneColorToInstrument.SetCurve(data.droneColorToInstrumentRange.z);

        droneRmsToInstrument.newLowValue = data.droneRMSToInstrumentRange.x;
        droneRmsToInstrument.newHighValue = data.droneRMSToInstrumentRange.y;
        droneRmsToInstrument.SetCurve(data.droneRMSToInstrumentRange.z);

        //Update Sequence
        sequenceLightcurveMagToPitch.newLowValue = data.seqLightcurveMagToPitchRange.x;
        sequenceLightcurveMagToPitch.newHighValue = data.seqLightcurveMagToPitchRange.y;
        sequenceLightcurveMagToPitch.SetCurve(data.seqLightcurveMagToPitchRange.z);

        sequenceRmsToTempo.newLowValue = data.seqRMSToTempoRange.x;
        sequenceRmsToTempo.newHighValue = data.seqRMSToTempoRange.y;
        sequenceRmsToTempo.SetCurve(data.seqRMSToTempoRange.z);

        sequencePeriodToDuration.newLowValue = data.seqPeriodToDurationRange.x;
        sequencePeriodToDuration.newHighValue = data.seqPeriodToDurationRange.y;
        sequencePeriodToDuration.SetCurve(data.seqPeriodToDurationRange.z);

        sequenceSnrToRythmicVariation.newLowValue = data.seqSNRToRhythmicVarRange.x;
        sequenceSnrToRythmicVariation.newHighValue = data.seqSNRToRhythmicVarRange.y;
        sequenceSnrToRythmicVariation.SetCurve(data.seqSNRToRhythmicVarRange.z);

        magSnrToInstrument.newLowValue = data.seqMagSNRToInstrumentRange.x;
        magSnrToInstrument.newHighValue = data.seqMagSNRToInstrumentRange.y;
        magSnrToInstrument.SetCurve(data.seqMagSNRToInstrumentRange.z);

        sequenceMapper.magRangeToPitchRange.newLowValue = data.seqmapMagRangetoPitchRange.x;
        sequenceMapper.magRangeToPitchRange.newHighValue = data.seqmapMagRangetoPitchRange.y;
        sequenceMapper.magRangeToPitchRange.SetCurve(data.seqmapMagRangetoPitchRange.z);

        sequenceMapper.magMeanToPitchCenter.newLowValue = data.seqmapMagMeantoPitchCenterRange.x;
        sequenceMapper.magMeanToPitchCenter.newHighValue = data.seqmapMagMeantoPitchCenterRange.y;
        sequenceMapper.magMeanToPitchCenter.SetCurve(data.seqmapMagMeantoPitchCenterRange.z);

        //Update Wave
        waveSnrMapToPitch.newLowValue = data.waveSNRMapToPitchRange.x;
        waveSnrMapToPitch.newHighValue = data.waveSNRMapToPitchRange.y;
        waveSnrMapToPitch.SetCurve(data.waveSNRMapToPitchRange.z);

        waveRmsMapToPitch.newLowValue = data.waveRMSMapToPitchRange.x;
        waveRmsMapToPitch.newHighValue = data.waveRMSMapToPitchRange.y;
        waveRmsMapToPitch.SetCurve(data.waveRMSMapToPitchRange.z);

        waveTeffMapToPitch.newLowValue = data.waveTeffMapToPitchRange.x;
        waveTeffMapToPitch.newHighValue = data.waveTeffMapToPitchRange.y;
        waveTeffMapToPitch.SetCurve(data.waveTeffMapToPitchRange.z);

        waveVarMapToPitch.newLowValue = data.waveVarMapToPitchRange.x;
        waveVarMapToPitch.newHighValue = data.waveVarMapToPitchRange.y;
        waveVarMapToPitch.SetCurve(data.waveVarMapToPitchRange.z);
    }
}

///Data mapping classes moved to ioanDataMapping