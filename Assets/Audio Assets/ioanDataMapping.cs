using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// ioanDataMapping contains classes responsible for mapping data in IOAN
/// </summary>
namespace ioanDataMapping
{
    /// <summary>
    /// This class allows for easy data mapping and provides a gui in the editor for easy adjustments.  
    /// </summary>
    [System.Serializable]
    public class DataMapper
    {
        #region PUBLIC_VAR
        /// <summary>
        /// Low input value
        /// </summary>
        public float lowValue = 0;
        /// <summary>
        /// High input value
        /// </summary>
        public float highValue = 1;
        /// <summary>
        /// Numbers are transfered to this curve.  X is transfered to Y.
        /// </summary>
        public AnimationCurve transferFunction = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1), });
        /// <summary>
        /// The mapped low value
        /// </summary>
        public float newLowValue = 0;
        /// <summary>
        /// The mapped high value
        /// </summary>
        public float newHighValue = 1;
        /// <summary>
        /// Should the data be clamped the the newLowValue and newHighValue?
        /// </summary>
        public bool clamp = true;

        #endregion
        #region PUBIC_FUNC
        /// <summary>
        /// Maps a number based on the mapping variable 
        /// </summary>
        /// <param name="value">number to be mapped</param>
        /// <returns>returns the mapped value</returns>
        public float map(float value)
        {
            value = normalize(value);
            value = mapNormalized(value);

            return value;
        }

        public int mapInt(float value)
        {
            float floatValue = map(value);

            return Mathf.RoundToInt(floatValue);
        }
        /// <summary>
        /// Maps a number from 0-1 to the output range-- usefull for mapping pre-normalized data
        /// </summary>
        /// <param name="value">Normalized value scaled from 0-1</param>
        /// <returns>Mapped Value</returns>
        public float mapNormalized(float value)
        {
            if (clamp)
            {
                value = Mathf.Clamp01(value);
            }
            value = transferFunction.Evaluate(value);
            value *= (this.newHighValue - this.newLowValue);
            value += this.newLowValue;


            return value;
        }

        public int mapNormalizedInt(float value)
        {
            if (clamp)
            {
                value = Mathf.Clamp01(value);
            }
            value = mapNormalized(value);
            return Mathf.RoundToInt(value);
        }
        /// <summary>
        /// Normalizes a value based on input ranges
        /// </summary>
        /// <param name="value">value to be normalized</param>
        /// <returns>normalized value</returns>
        public float normalize(float value)
        {
            value -= this.lowValue;
            value /= (this.highValue - this.lowValue);
            if (clamp)
            {
                value = Mathf.Clamp01(value);
            }
            return value;
        }

        public void SetCurve(float c)
        {
            Keyframe[] ks = new Keyframe[100];

            for (int i = 0; i < ks.Length; i++)
            {
                ks[i] = (new Keyframe(i / 100.0f, Mathf.Pow(i / 100.0f, Mathf.Pow(2, c))));
            }
            transferFunction = new AnimationCurve(ks);
        }
        #endregion
    }
    /// <summary>
    /// the scale mapper class allows for a user to quantize midi notes to their closest solution based on a musical scale.
    /// </summary>
    [System.Serializable]
    public class ScaleMapper
    {
        #region PUBLIC_VARS
        /// <summary>
        /// Menu items for a user to select a scale.  This is somewhat limited at the moment in terms of the avalibility of scales
        /// </summary>
        public enum DefinedScales { user, diatonic, pentatonic, octatonic, diminished, wholeTone, quaterCommaDiatonic, justDiatonic, free };
        /// <summary>
        /// The menu for the scales that stores the users selection
        /// </summary>
        public DefinedScales scale;
        /// <summary>
        /// Trasposition level of the given scale in pitch class
        /// </summary>
        [Range(0, 12)]
        public float transposition = 0;
        /// <summary>
        /// A user defined scale 
        /// </summary>
        [Range(0, 12)]
        public float[] userDefinedScale = { 0, 1, 4, 5 };

        /// <summary>
        /// Array of number representing the pitch classes in a diatonic collection
        /// </summary>
        float[] diatonic = { 0, 2, 4, 5, 7, 9, 11 };
        /// <summary>
        ///  Array of number representing the pitch classes in a pentatonic collection
        /// </summary>
        float[] pentatonic = { 0, 2, 4, 7, 9 };
        /// <summary>
        ///  Array of number representing the pitch classes in a octatonic collection
        /// </summary>
        float[] octatonic = { 0, 1, 3, 4, 6, 7, 9, 10 };
        /// <summary>
        ///  Array of number representing the pitch classes in a fully dimished seventh chord
        /// </summary>
        float[] diminished = { 0, 3, 6, 10 };
        /// <summary>
        ///  Array of number representing the pitch classes in a whole tone collection
        /// </summary>
        float[] wholeTone = { 0, 2, 4, 6, 8, 10 };

        float[] quarterCommaDiatonic = { 0, 1.9316f, 3.7895f, 5.0526f, 6.9677f, 8.8974f, 10.8289f };
        float[] justDiatonic = {0, 2.04f, 3.86f, 4.98f, 7.02f, 8.84f, 10.88f};
        /// <summary>
        /// The pitches that will be mapped
        /// </summary>
        float[] pitches;
        /// <summary>
        /// Not really used, but can set the size of the pitch field in semitones 
        /// </summary>
        public float pitchFieldSizeInSemitones = 12f;

        bool free = false;
        #endregion

        #region PUBLIC_FUNC
        /// <summary>
        /// QUantizes a chromatic note to the nearest note in a selected scale.  
        /// </summary>
        /// <param name="pitch"></param>
        /// <returns>midi pitch</returns>
        public float map(float pitch)
        {
            if (free)
            {
                return pitch;
            }
            retrieveScale();
            int octave = Mathf.FloorToInt(pitch / pitchFieldSizeInSemitones);
            float pitchClass = pitch % pitchFieldSizeInSemitones;
            float snap = 0;
            float snapDiff = 100;


            for (int i = 0; i < pitches.Length; i++)
            {
                if (Mathf.Abs(pitchClass - pitches[i]) < snapDiff)
                {
                    snapDiff = Mathf.Abs(pitchClass - (pitches[i] + transposition));
                    snap = pitches[i] + transposition;
                }
            }

            return snap + pitchFieldSizeInSemitones * octave;      
        


        }

        public float[] retrieveScale()
        {
            switch (scale)
            {
                case DefinedScales.user:
                    pitches = userDefinedScale;
                    break;
                case DefinedScales.diatonic:
                    pitches = diatonic;
                    break;
                case DefinedScales.pentatonic:
                    pitches = pentatonic;
                    break;
                case DefinedScales.octatonic:
                    pitches = octatonic;
                    break;
                case DefinedScales.diminished:
                    pitches = diminished;
                    break;
                case DefinedScales.wholeTone:
                    pitches = wholeTone;
                    break;
                case DefinedScales.quaterCommaDiatonic:
                    pitches = quarterCommaDiatonic;
                    break;
                case DefinedScales.justDiatonic:
                    pitches = justDiatonic;
                    break;
                case DefinedScales.free:
                    free = true;
                    break;
            }
            return pitches;
        }
        #endregion
        /// <summary>
        /// Maps a pitch sequence
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public List<float> mapSequence(List<float> sequence)
        {
            List<float> quantized = new List<float>();
            foreach (float note in sequence)
            {
                float newNote = map(note);
                quantized.Add(newNote);
            }
            return quantized;
        }

    }
    /// <summary>
    /// A special mapper for pitch sequences 
    /// </summary>
    [System.Serializable]
    public class SequenceMapper
    {
        /// <summary>
        /// Maps the mag range to a pitch range
        /// </summary>
        public DataMapper magRangeToPitchRange;
        /// <summary>
        /// Maps the mean of the pitches to a centeral pitch
        /// </summary>
        public DataMapper magMeanToPitchCenter;
        /// <summary>
        /// Maps a lightcurve to a pitch sequence
        /// </summary>
        /// <param name="lightCurve">list of magnitudes</param>
        /// <returns></returns>
        public List<float> map(List<float> lightCurve)
        {

            float min = lightCurve.Min();
            float max = lightCurve.Max();
            float range = max - min;
            float mean = lightCurve.Average();

            float pitchRange = magRangeToPitchRange.map(range);
            //Debug.Log("Pitch Range: " + pitchRange);
            float pitchCenter = magMeanToPitchCenter.map(mean);
            //Debug.Log("Pitch Center: " + pitchCenter);

            List<float> pitchSequence = new List<float>();

            foreach (float mag in lightCurve)
            {
                float normalizedMag = (mag - min) / range;
                float thisPitch = normalizedMag * pitchRange + Mathf.Floor(pitchCenter) - pitchRange / 2;
                pitchSequence.Add(thisPitch);



            }
            //Debug.Log("pitch sequence: "+string.Join(", ", pitchSequence));
            return pitchSequence;



        }
    }
}
