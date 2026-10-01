using System;
using UnityEngine;

namespace ModelViewer;

[Serializable]
public class ModelViewer_SkyParameters
{
	[Header("Sky")]
	[Range(0f, 1f)]
	public float skyBrightness = 1f;

	[Tooltip("Current hour of the day.")]
	[Header("                ")]
	[Header("Time of Day")]
	public float Hour = 9f;

	[Tooltip("Current day of the month.")]
	public int Day = 20;

	[Tooltip("Current month of the year.")]
	public int Month = 5;

	[Tooltip("Current year.")]
	[TOD_Range(1f, 9999f)]
	public int Year = 2000;

	[Range(-90f, 90f)]
	[Tooltip("Latitude of the current location in degrees.")]
	public float Latitude = -10f;

	[Tooltip("Longitude of the current location in degrees.")]
	[Range(-180f, 180f)]
	public float Longitude = -25f;

	[Range(-14f, 14f)]
	[Tooltip("UTC/GMT time zone of the current location in hours.")]
	public float UTC;

	[Header("Atmosphere")]
	[Header("                ")]
	public float skyContrast = 1.2f;

	public float skyFogginess = 0.2f;

	public float cloudCoverage;
}
