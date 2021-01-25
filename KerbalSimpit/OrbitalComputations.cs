using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalSimpit.KerbalSimpit
{
	/** This class is mainly used to compute the time of the next morning given a 
	 * celestial body and a location. Inspired by the EarlyBird mod by taniwha*/
    class OrbitalComputations
    {

		public static double TimeToDaylight(double lat, double lon, CelestialBody body)
		{
			CelestialBody sun = Planetarium.fetch.Sun;

			double rotPeriod, localTime;

			localTime = GetLocalTime(lon, body, sun);
			rotPeriod = body.rotationPeriod;
			var orbit = body.orbit;
			while (orbit?.referenceBody != sun)
			{
				orbit = orbit.referenceBody.orbit;
			}
			if (orbit != null)
			{
				//Convert the absolute rotation period into a day lenght
				rotPeriod = orbit.period * rotPeriod / (orbit.period - rotPeriod);
			}

			double dayLength = GetDayLengthPercentage(lat, body, sun);
			double timeOfDawn = 0.5 - dayLength / 2;
			double timeToDaylight = rotPeriod * UtilMath.WrapAround(timeOfDawn - localTime, 0, 1);
			return timeToDaylight;
		}

		// returns the lenght of "daylight" in terms of body rotation period,
		// ranging from 0 (no daylight) to 1 (no night).
		// really, it's the amount of time the "sun" body is above the
		// spherical horizon of body at the given latitude.
		public static double GetDayLengthPercentage(double lat, CelestialBody body, CelestialBody sun)
		{
			// cos w = -tan p * tan d
			// w = hour angle, p = latitude, d = sun declination
			Vector3d sunPos = body.GetRelSurfacePosition(sun.position);
			double sunY = sunPos.y;
			sunPos.y = 0;
			double sunX = sunPos.magnitude;
			double tand = sunY / sunX;
			double tanp = Math.Tan(lat * Math.PI / 180);
			double cosw = -tanp * tand;
			if (cosw < -1)
			{
				return 1;
			}
			else if (cosw > 1)
			{
				return 0;
			}
			// however, acos is nasty, so...
			double thalf = Math.Sqrt((1 - cosw) / (1 + cosw));
			// the basic (acos) formula gives the angle of either sunrise
			// or sunset relative to noon, so need twice the angle to get
			// the angle swept by the sun through the day, but to avoid
			// acos, the half-angle was computed, so need 4x. Then to get
			// 0-1, divide by 2pi, so...
			return 2 * Math.Atan(thalf) / Math.PI;
		}

		public static double GetLocalTime(double lon, CelestialBody body, CelestialBody sun)
		{
			// latitude does not affect local time of day (it does affect sun visibility, though)
			Vector3d zenith_ra = body.GetRelSurfaceNVector(0, lon);
			Vector3d sunPos = body.GetRelSurfacePosition(sun.position);
			sunPos.y = 0;   // not interested in declination
			Vector3d sunPos_ra = sunPos.normalized;
			double sign = Vector3d.Cross(zenith_ra, sunPos_ra).y >= 0 ? 1 : -1;
			return sign * Angle(sunPos_ra, zenith_ra) / (2 * Math.PI) + 0.5;
		}

		// NOTE: loses sign, so only 0-pi
		public static double Angle(Vector3d a, Vector3d b)
		{
			Vector3d amb = a * b.magnitude;
			Vector3d bma = b * a.magnitude;
			double y = (amb - bma).magnitude;
			double x = (amb + bma).magnitude;
			return 2 * Math.Atan2(y, x);
		}
	}
}
