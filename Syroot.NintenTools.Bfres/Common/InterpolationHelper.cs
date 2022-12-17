using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syroot.NintenTools.NSW.Bfres
{
    public class CurveInterpolationHelper
    {
        public static float Lerp(float LHS, float RHS, float Weight)
        {
            return LHS * (1 - Weight) + RHS * Weight;
        }

        public static float Herp(float LHS, float RHS, float LS, float RS, float Diff, float Weight)
        {
            float Result;

            Result = LHS + (LHS - RHS) * (2 * Weight - 3) * Weight * Weight;
            Result += (Diff * (Weight - 1)) * (LS * (Weight - 1) + RS * Weight);

            return Result;
        }

        public static float GetCubicValue(float inv, float coef0, float coef1, float coef2, float coef3)
        {
            return coef3 * inv + coef2 * inv + coef1 * inv + coef0 * inv;
        }

        public static float[] CalculateCubicCoef(float frameA, float frameB, float valueA, float valueB, float inSlope, float outSlope)
        {
            return CalculateCubicCoef(frameB - frameA, valueA, valueB, inSlope, outSlope);
        }

        public static float[] GetCubicSlopes(float time, float delta, float[] coef)
        {
            float outSlope = coef[1] / time;
            float param = coef[3] - (-2 * delta);
            float inSlope = param / time - outSlope;
            return new float[2] { inSlope, outSlope };
        }

        public static float[] CalculateCubicCoef(float time,
            float valueA, float valueB, float inSlope, float outSlope)
        {
            float delta = valueB - valueA;

            float[] values = new float[4];
            //Cubics have 4 coefficents
            values[0] = valueA; //Set the current value
            values[1] = outSlope * time;
            //3 and -2 are the default values and should be multipled 
            values[2] = 3 * delta - (2 * outSlope + inSlope) * time;
            values[3] = (-2 * delta) + ((outSlope + inSlope) * time);
            return values;
        }
    }
}
