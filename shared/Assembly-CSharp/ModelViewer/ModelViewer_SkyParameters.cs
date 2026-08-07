using System;
using UnityEngine;

namespace ModelViewer;

[Serializable]
public class ModelViewer_SkyParameters
{
	[Range(0f, 1f)]
	[Header("Sky")]
	public float skyBrightness = 1f;

	[Header("Time of Day")]
	[Tooltip("Current hour of the day.")]
	[Header("                ")]
	public float Hour = 9f;

	[Tooltip("Current day of the month.")]
	public int Day = 20;

	[Tooltip("Current month of the year.")]
	public int Month = 5;

	[TOD_Range(1f, 9999f)]
	[Tooltip("Current year.")]
	public int Year = 2000;

	[Range(-90f, 90f)]
	[Tooltip("Latitude of the current location in degrees.")]
	public float Latitude = -10f;

	[Range(-180f, 180f)]
	[Tooltip("Longitude of the current location in degrees.")]
	public float Longitude = -25f;

	[Tooltip("UTC/GMT time zone of the current location in hours.")]
	[Range(-14f, 14f)]
	public float UTC;

	[Header("Atmosphere")]
	[Header("                ")]
	public float skyContrast = 1.2f;

	public float skyFogginess = 0.2f;

	public float cloudCoverage;
}
